namespace Rms.Application.Services;

public sealed class EmployeeService(IEmployeeRepository employees)
{
    public IReadOnlyList<Employee> GetAll() => employees.GetAll();

    public Result<Employee> Add(string name, string email, string designation, Department department,
        EmploymentType employmentType, decimal costRate)
    {
        var created = Employee.Create(employees.NextId(), name, email, designation, department, employmentType, costRate);
        if (created.IsFailure)
        {
            return created;
        }

        if (employees.GetAll().Any(e => e.Email == created.Value.Email))
        {
            return DomainErrors.DuplicateEmail(created.Value.Email.Value);
        }

        employees.Add(created.Value);
        return created;
    }

    public Result Update(int id, string name, string email, string designation, Department department,
        EmploymentType employmentType, decimal costRate)
    {
        var employee = employees.GetById(id);
        if (employee is null)
        {
            return DomainErrors.NotFound("Employee", id);
        }

        if (employees.GetAll().Any(e => e.Id != id
                && string.Equals(e.Email.Value, email.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            return DomainErrors.DuplicateEmail(email.Trim());
        }

        var result = employee.Update(name, email, designation, department, employmentType, costRate);
        if (result.IsSuccess)
        {
            employees.Save(employee);
        }

        return result;
    }

    public Result Deactivate(int id)
    {
        var employee = employees.GetById(id);
        if (employee is null)
        {
            return DomainErrors.NotFound("Employee", id);
        }

        employee.Deactivate();
        employees.Save(employee);
        return Result.Success();
    }

    public Result SetSkill(int id, string skill, Proficiency level)
    {
        var employee = employees.GetById(id);
        if (employee is null)
        {
            return DomainErrors.NotFound("Employee", id);
        }

        var result = employee.SetSkill(skill, level);
        if (result.IsSuccess)
        {
            employees.Save(employee);
        }

        return result;
    }
}
