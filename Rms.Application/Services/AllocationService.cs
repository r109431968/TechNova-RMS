namespace Rms.Application.Services;

public sealed class AllocationService(
    IEmployeeRepository employees,
    IProjectRepository projects,
    IAllocationRepository allocations,
    INotifier notifier)
{
    public IReadOnlyList<Allocation> GetAll() => allocations.GetAll();

    public Result<Allocation> Allocate(int employeeId, int projectId, DateRange period, Percentage percent)
    {
        var employee = employees.GetById(employeeId);
        if (employee is null)
        {
            return DomainErrors.NotFound("Employee", employeeId);
        }

        var project = projects.GetById(projectId);
        if (project is null)
        {
            return DomainErrors.NotFound("Project", projectId);
        }

        var check = AllocationPolicy.CanAllocate(employee, project, period, percent,
            allocations.GetForEmployee(employeeId));
        if (check.IsFailure)
        {
            return check.Error;
        }

        var allocation = Allocation.Create(allocations.NextId(), employeeId, projectId, period, percent);
        allocations.Add(allocation);
        notifier.EmployeeAllocated(employee, project, allocation);
        return allocation;
    }

    public Result Release(int allocationId)
    {
        var allocation = allocations.GetById(allocationId);
        if (allocation is null)
        {
            return DomainErrors.NotFound("Allocation", allocationId);
        }

        var released = allocation.Release();
        if (released.IsFailure)
        {
            return released;
        }

        allocations.Save(allocation);

        if (employees.GetById(allocation.EmployeeId) is { } employee
            && projects.GetById(allocation.ProjectId) is { } project)
        {
            notifier.EmployeeReleased(employee, project, allocation);
        }

        return Result.Success();
    }
}
