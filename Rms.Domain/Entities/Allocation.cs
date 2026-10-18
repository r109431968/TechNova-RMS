namespace Rms.Domain.Entities;

/// <summary>
/// Assignment of an employee to a project for a period at a percentage.
/// It is its own entity (not nested in Employee/Project) because rules span both sides.
/// </summary>
public sealed class Allocation
{
    private Allocation(int id, int employeeId, int projectId, DateRange period, Percentage percent, bool isReleased)
    {
        Id = id;
        EmployeeId = employeeId;
        ProjectId = projectId;
        Period = period;
        Percent = percent;
        IsReleased = isReleased;
    }

    public int Id { get; }
    public int EmployeeId { get; }
    public int ProjectId { get; }
    public DateRange Period { get; }
    public Percentage Percent { get; }
    public bool IsReleased { get; private set; }

    public static Allocation Create(int id, int employeeId, int projectId, DateRange period, Percentage percent) =>
        new(id, employeeId, projectId, period, percent, isReleased: false);

    public static Allocation Restore(int id, int employeeId, int projectId, DateRange period,
        Percentage percent, bool isReleased) =>
        new(id, employeeId, projectId, period, percent, isReleased);

    public Result Release()
    {
        if (IsReleased)
        {
            return DomainErrors.AlreadyReleased(Id);
        }

        IsReleased = true;
        return Result.Success();
    }
}
