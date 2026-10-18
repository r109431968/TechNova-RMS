namespace Rms.Application.Services;

public sealed class ProjectService(IProjectRepository projects)
{
    public IReadOnlyList<Project> GetAll() => projects.GetAll();

    public Result<Project> Create(string name, string client, DateRange dates, ProjectStatus status,
        IEnumerable<SkillRequirement> requirements)
    {
        var created = Project.Create(projects.NextId(), name, client, dates, status);
        if (created.IsFailure)
        {
            return created;
        }

        foreach (var requirement in requirements)
        {
            var added = created.Value.AddRequirement(requirement.Skill, requirement.MinLevel);
            if (added.IsFailure)
            {
                return added.Error;
            }
        }

        projects.Add(created.Value);
        return created;
    }
}
