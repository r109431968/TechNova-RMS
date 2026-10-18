namespace Rms.Application.Services;

/// <summary>Bench threshold and the look-ahead window used by the reports.</summary>
public sealed record ReportSettings(int BenchThresholdPercent = 70, int WindowDays = 30);

public sealed record BenchRow(Employee Employee, double AverageUtilisation);

public sealed record ProjectStaffingRow(
    Project Project,
    int HeadCount,
    double Fte,
    IReadOnlyList<string> Team,
    IReadOnlyList<string> UncoveredSkills);

public sealed record DepartmentUtilisationRow(Department Department, int HeadCount, double UtilisationPercent);

public sealed class ReportService(
    IEmployeeRepository employees,
    IProjectRepository projects,
    IAllocationRepository allocations,
    ReportSettings settings,
    TimeProvider clock)
{
    public ReportSettings Settings => settings;

    /// <summary>Window from today for WindowDays days (TimeProvider makes this testable).</summary>
    public DateRange CurrentWindow()
    {
        var today = DateOnly.FromDateTime(clock.GetLocalNow().DateTime);
        return new DateRange(today, today.AddDays(settings.WindowDays - 1));
    }

    /// <summary>Active employees whose average allocation in the window is below the threshold.</summary>
    public IReadOnlyList<BenchRow> Bench()
    {
        var window = CurrentWindow();
        return
        [
            .. employees.GetAll().ActiveOnly()
                .Select(e => new BenchRow(e, Utilisation(e.Id, window)))
                .Where(r => r.AverageUtilisation < settings.BenchThresholdPercent)
                .OrderBy(r => r.AverageUtilisation)
                .ThenBy(r => r.Employee.Name)
        ];
    }

    public IReadOnlyList<ProjectStaffingRow> ProjectStaffing()
    {
        var employeesById = employees.GetAll().ToDictionary(e => e.Id);
        var rows = new List<ProjectStaffingRow>();

        foreach (var project in projects.GetAll())
        {
            var active = allocations.GetForProject(project.Id)
                .Where(a => !a.IsReleased && employeesById.ContainsKey(a.EmployeeId))
                .ToList();
            var members = active.Select(a => employeesById[a.EmployeeId]).DistinctBy(e => e.Id).ToList();
            var uncovered = project.Requirements
                .Where(r => !members.Any(m => m.Meets(r)))
                .Select(r => r.Skill)
                .ToList();
            var team = active.Select(a => $"{employeesById[a.EmployeeId].Name} ({a.Percent})").ToList();

            rows.Add(new ProjectStaffingRow(project, members.Count, active.Sum(a => a.Percent.Value) / 100.0, team, uncovered));
        }

        return rows;
    }

    /// <summary>Average utilisation of active employees per department over the window.</summary>
    public IReadOnlyList<DepartmentUtilisationRow> DepartmentUtilisation()
    {
        var window = CurrentWindow();
        return
        [
            .. employees.GetAll().ActiveOnly()
                .GroupBy(e => e.Department)
                .Select(g => new DepartmentUtilisationRow(g.Key, g.Count(), g.Average(e => Utilisation(e.Id, window))))
                .OrderBy(r => r.Department)
        ];
    }

    // Allocated person-days (weighted by %) divided by the days in the window.
    private double Utilisation(int employeeId, DateRange window) =>
        allocations.GetForEmployee(employeeId)
            .Where(a => !a.IsReleased && a.Period.Overlaps(window))
            .Sum(a => a.Percent.Value * a.Period.OverlapDays(window)) / (double)window.Days;
}
