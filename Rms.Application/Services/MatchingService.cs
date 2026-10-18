namespace Rms.Application.Services;

public sealed class MatchingService(
    IEmployeeRepository employees,
    IProjectRepository projects,
    IAllocationRepository allocations,
    IMatchScoringStrategy scoring)
{
    public string StrategyName => scoring.Name;

    /// <summary>Active employees who match at least one requirement and have free capacity for the whole project.</summary>
    public Result<IReadOnlyList<MatchSuggestion>> Suggest(int projectId, int minimumAvailablePercent = 10)
    {
        var project = projects.GetById(projectId);
        if (project is null)
        {
            return DomainErrors.NotFound("Project", projectId);
        }

        var suggestions = new List<MatchSuggestion>();
        foreach (var employee in employees.GetAll().ActiveOnly())
        {
            var matched = project.Requirements.Where(employee.Meets).ToList();
            if (project.Requirements.Count > 0 && matched.Count == 0)
            {
                continue;
            }

            var available = AllocationPolicy.AvailableCapacity(employee, project.Dates,
                allocations.GetForEmployee(employee.Id));
            if (available < minimumAvailablePercent)
            {
                continue;
            }

            var missing = project.Requirements.Except(matched).Select(r => r.Skill).ToList();
            suggestions.Add(new MatchSuggestion(
                employee,
                scoring.Score(employee, project, available),
                [.. matched.Select(r => r.Skill)],
                missing,
                available));
        }

        IReadOnlyList<MatchSuggestion> ordered =
            [.. suggestions.OrderByDescending(s => s.Score).ThenBy(s => s.Employee.Name)];
        return Result<IReadOnlyList<MatchSuggestion>>.Success(ordered);
    }
}
