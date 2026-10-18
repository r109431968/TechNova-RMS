namespace Rms.Domain.Entities;

public static class EmployeeCollectionExtensions
{
    // C# 14 extension members.
    extension(IEnumerable<Employee> employees)
    {
        public IEnumerable<Employee> ActiveOnly() => employees.Where(e => e.IsActive);
    }
}
