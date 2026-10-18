namespace Rms.Application.Matching;

public sealed record MatchSuggestion(
    Employee Employee,
    double Score,
    IReadOnlyList<string> MatchedSkills,
    IReadOnlyList<string> MissingSkills,
    int AvailablePercent);

/// <summary>Default: rewards covering required skills, then proficiency, then free capacity.</summary>
public sealed class SkillCoverageScoring : IMatchScoringStrategy
{
    public string Name => "Skill coverage 60% + proficiency 25% + availability 15%";

    public double Score(Employee employee, Project project, int availablePercent)
    {
        var requirements = project.Requirements;
        if (requirements.Count == 0)
        {
            return availablePercent;
        }

        var matched = requirements.Where(employee.Meets).ToList();
        var coverage = (double)matched.Count / requirements.Count;
        var proficiency = matched.Count == 0
            ? 0
            : matched.Average(r => (int)employee.LevelOf(r.Skill)) / 5.0;

        return Math.Round(100 * ((0.60 * coverage) + (0.25 * proficiency) + (0.15 * availablePercent / 100.0)), 1);
    }
}

/// <summary>Alternative: when you need people fast, free capacity matters most.</summary>
public sealed class AvailabilityFirstScoring : IMatchScoringStrategy
{
    public string Name => "Availability 70% + skill coverage 30%";

    public double Score(Employee employee, Project project, int availablePercent)
    {
        var requirements = project.Requirements;
        var coverage = requirements.Count == 0
            ? 1.0
            : (double)requirements.Count(employee.Meets) / requirements.Count;

        return Math.Round((0.70 * availablePercent) + (30 * coverage), 1);
    }
}
