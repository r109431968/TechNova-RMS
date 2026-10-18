namespace Rms.Application.Abstractions;

/// <summary>Strategy: how to score a candidate for a project. Swap it without touching MatchingService.</summary>
public interface IMatchScoringStrategy
{
    string Name { get; }

    double Score(Employee employee, Project project, int availablePercent);
}
