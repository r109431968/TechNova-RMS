namespace Rms.Domain.Policies;

/// <summary>
/// Domain service holding the business rules that span several objects:
/// inactive employees, project dates, and the 100% cap on overlapping periods.
/// Pure functions, no I/O, so it is trivial to unit test.
/// </summary>
public static class AllocationPolicy
{
    public static Result CanAllocate(Employee employee, Project project, DateRange period,
        Percentage percent, IEnumerable<Allocation> employeeAllocations)
    {
        if (!employee.IsActive)
        {
            return DomainErrors.EmployeeInactive(employee.Name);
        }

        if (!project.CanAcceptAllocations)
        {
            return DomainErrors.ProjectNotOpen(project.Name, project.Status);
        }

        if (!project.Dates.Contains(period))
        {
            return DomainErrors.OutsideProjectDates(project.Dates);
        }

        var limit = employee.Policy.MaxTotalAllocation;
        var existing = Relevant(employeeAllocations, period);

        // Load only changes on certain dates, so checking those dates is enough.
        foreach (var day in ChangePoints(period, existing))
        {
            var load = LoadOn(day, existing);
            if (load + percent.Value > limit)
            {
                return DomainErrors.OverAllocated(day, load, percent.Value, limit);
            }
        }

        return Result.Success();
    }

    /// <summary>Highest total allocation the employee has on any day of the period.</summary>
    public static int PeakLoad(DateRange period, IEnumerable<Allocation> employeeAllocations)
    {
        var existing = Relevant(employeeAllocations, period);
        return ChangePoints(period, existing).Max(day => LoadOn(day, existing));
    }

    /// <summary>Percentage still free for the whole period (never negative).</summary>
    public static int AvailableCapacity(Employee employee, DateRange period, IEnumerable<Allocation> employeeAllocations) =>
        Math.Max(0, employee.Policy.MaxTotalAllocation - PeakLoad(period, employeeAllocations));

    private static List<Allocation> Relevant(IEnumerable<Allocation> allocations, DateRange period) =>
        allocations.Where(a => !a.IsReleased && a.Period.Overlaps(period)).ToList();

    private static IEnumerable<DateOnly> ChangePoints(DateRange period, List<Allocation> existing)
    {
        yield return period.Start;

        foreach (var allocation in existing)
        {
            if (period.Contains(allocation.Period.Start))
            {
                yield return allocation.Period.Start;
            }

            var dayAfter = allocation.Period.End.AddDays(1);
            if (period.Contains(dayAfter))
            {
                yield return dayAfter;
            }
        }
    }

    private static int LoadOn(DateOnly day, List<Allocation> existing) =>
        existing.Where(a => a.Period.Contains(day)).Sum(a => a.Percent.Value);
}
