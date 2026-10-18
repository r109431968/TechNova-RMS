namespace Rms.Domain.Entities;

/// <summary>
/// An employee (resource). Invariants are protected here: state changes only through methods,
/// and expected failures come back as <see cref="Result"/>.
/// </summary>
public sealed class Employee
{
    private readonly List<EmployeeSkill> _skills = [];

    private Employee(int id, string name, Email email, string designation, Department department,
        EmploymentType employmentType, decimal costRate, bool isActive)
    {
        Id = id;
        Name = name;
        Email = email;
        Designation = designation;
        Department = department;
        EmploymentType = employmentType;
        CostRate = costRate;
        IsActive = isActive;
    }

    public int Id { get; }
    public string Name { get; private set; }
    public Email Email { get; private set; }
    public string Designation { get; private set; }
    public Department Department { get; private set; }
    public EmploymentType EmploymentType { get; private set; }

    /// <summary>Cost per hour. C# 14 <c>field</c> keyword keeps the validation next to the property.</summary>
    public decimal CostRate
    {
        get;
        private set => field = value >= 0
            ? value
            : throw new ArgumentOutOfRangeException(nameof(value), "Cost rate cannot be negative.");
    }

    public bool IsActive { get; private set; }
    public IReadOnlyList<EmployeeSkill> Skills => _skills;

    /// <summary>Employment rules are composed in via a policy object, not via subclasses.</summary>
    public EmploymentPolicy Policy => EmploymentPolicies.For(EmploymentType);

    public static Result<Employee> Create(int id, string name, string email, string designation,
        Department department, EmploymentType employmentType, decimal costRate)
    {
        var validated = Validate(name, email, designation, costRate);
        if (validated.IsFailure)
        {
            return validated.Error;
        }

        return new Employee(id, name.Trim(), validated.Value, designation.Trim(), department,
            employmentType, costRate, isActive: true);
    }

    /// <summary>Rebuilds an employee from storage (data was validated when first saved).</summary>
    public static Employee Restore(int id, string name, string email, string designation,
        Department department, EmploymentType employmentType, decimal costRate, bool isActive,
        IEnumerable<EmployeeSkill> skills)
    {
        var employee = new Employee(id, name, new Email(email), designation, department,
            employmentType, costRate, isActive);
        employee._skills.AddRange(skills);
        return employee;
    }

    public Result Update(string name, string email, string designation, Department department,
        EmploymentType employmentType, decimal costRate)
    {
        var validated = Validate(name, email, designation, costRate);
        if (validated.IsFailure)
        {
            return validated.Error;
        }

        Name = name.Trim();
        Email = validated.Value;
        Designation = designation.Trim();
        Department = department;
        EmploymentType = employmentType;
        CostRate = costRate;
        return Result.Success();
    }

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;

    public Result SetSkill(string skill, Proficiency level)
    {
        if (string.IsNullOrWhiteSpace(skill))
        {
            return DomainErrors.Validation("Skill name is required.");
        }

        if (level == Proficiency.None || !Enum.IsDefined(level))
        {
            return DomainErrors.Validation("Proficiency must be between 1 and 5.");
        }

        var name = skill.Trim();
        var index = _skills.FindIndex(s => s.Skill.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (index >= 0)
        {
            _skills[index] = new EmployeeSkill(_skills[index].Skill, level);
        }
        else
        {
            _skills.Add(new EmployeeSkill(name, level));
        }

        return Result.Success();
    }

    public Proficiency LevelOf(string skill) =>
        _skills.FirstOrDefault(s => s.Skill.Equals(skill, StringComparison.OrdinalIgnoreCase))?.Level
        ?? Proficiency.None;

    public bool Meets(SkillRequirement requirement) => LevelOf(requirement.Skill) >= requirement.MinLevel;

    private static Result<Email> Validate(string name, string email, string designation, decimal costRate)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return DomainErrors.Validation("Name is required.");
        }

        if (string.IsNullOrWhiteSpace(designation))
        {
            return DomainErrors.Validation("Designation is required.");
        }

        if (costRate < 0)
        {
            return DomainErrors.Validation("Cost rate cannot be negative.");
        }

        return Email.Create(email);
    }
}
