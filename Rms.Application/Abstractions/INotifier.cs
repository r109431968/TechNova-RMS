namespace Rms.Application.Abstractions;

/// <summary>
/// Outbound notification port. The console version prints; later it can be email, push, etc.
/// </summary>
public interface INotifier
{
    void EmployeeAllocated(Employee employee, Project project, Allocation allocation);
    void EmployeeReleased(Employee employee, Project project, Allocation allocation);
}
