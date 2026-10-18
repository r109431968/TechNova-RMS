namespace Rms.Domain.Common;

/// <summary>Central catalogue of business errors, so messages and codes stay consistent.</summary>
public static class DomainErrors
{
    public static Error Validation(string message) => new("validation", message);

    public static Error NotFound(string entity, int id) =>
        new("not_found", $"{entity} with ID {id} was not found.");

    public static Error DuplicateEmail(string email) =>
        new("employee.duplicate_email", $"An employee with email {email} already exists.");

    public static Error EmployeeInactive(string name) =>
        new("allocation.employee_inactive", $"{name} is inactive and cannot be allocated.");

    public static Error ProjectNotOpen(string project, ProjectStatus status) =>
        new("allocation.project_not_open", $"Project '{project}' is {status} and cannot accept allocations.");

    public static Error OutsideProjectDates(DateRange projectDates) =>
        new("allocation.outside_project_dates",
            $"Allocation must fall within the project dates ({projectDates.Start:yyyy-MM-dd} to {projectDates.End:yyyy-MM-dd}).");

    public static Error OverAllocated(DateOnly day, int currentLoad, int requested, int limit) =>
        new("allocation.over_allocated",
            $"On {day:yyyy-MM-dd} the employee is already at {currentLoad}%; adding {requested}% would exceed the {limit}% limit.");

    public static Error AlreadyReleased(int allocationId) =>
        new("allocation.already_released", $"Allocation {allocationId} has already been released.");
}
