namespace Rms.Domain.Entities;

/// <summary>A client project with dates, a status and the skills it needs.</summary>
public sealed class Project
{
    private readonly List<SkillRequirement> _requirements = [];

    private Project(int id, string name, string client, DateRange dates, ProjectStatus status)
    {
        Id = id;
        Name = name;
        Client = client;
        Dates = dates;
        Status = status;
    }

    public int Id { get; }
    public string Name { get; }
    public string Client { get; }
    public DateRange Dates { get; }
    public ProjectStatus Status { get; private set; }
    public IReadOnlyList<SkillRequirement> Requirements => _requirements;

    public bool CanAcceptAllocations => Status is ProjectStatus.Planned or ProjectStatus.Active;

    public static Result<Project> Create(int id, string name, string client, DateRange dates,
        ProjectStatus status = ProjectStatus.Planned)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return DomainErrors.Validation("Project name is required.");
        }

        if (string.IsNullOrWhiteSpace(client))
        {
            return DomainErrors.Validation("Client is required.");
        }

        return new Project(id, name.Trim(), client.Trim(), dates, status);
    }

    public static Project Restore(int id, string name, string client, DateRange dates,
        ProjectStatus status, IEnumerable<SkillRequirement> requirements)
    {
        var project = new Project(id, name, client, dates, status);
        project._requirements.AddRange(requirements);
        return project;
    }

    public Result AddRequirement(string skill, Proficiency minimumLevel)
    {
        if (string.IsNullOrWhiteSpace(skill))
        {
            return DomainErrors.Validation("Skill name is required.");
        }

        if (minimumLevel == Proficiency.None || !Enum.IsDefined(minimumLevel))
        {
            return DomainErrors.Validation("Minimum proficiency must be between 1 and 5.");
        }

        var name = skill.Trim();
        var index = _requirements.FindIndex(r => r.Skill.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (index >= 0)
        {
            _requirements[index] = new SkillRequirement(_requirements[index].Skill, minimumLevel);
        }
        else
        {
            _requirements.Add(new SkillRequirement(name, minimumLevel));
        }

        return Result.Success();
    }

    public void ChangeStatus(ProjectStatus status) => Status = status;
}
