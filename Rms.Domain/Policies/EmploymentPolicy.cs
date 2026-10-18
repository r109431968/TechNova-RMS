namespace Rms.Domain.Policies;

/// <summary>Rules that differ by employment type, expressed as data (composition over inheritance).</summary>
public sealed record EmploymentPolicy(int MaxTotalAllocation, string Description);

public static class EmploymentPolicies
{
    public static EmploymentPolicy For(EmploymentType type) => type switch
    {
        EmploymentType.FullTime => new EmploymentPolicy(100, "Full-time: up to 100% allocation."),
        EmploymentType.Contract => new EmploymentPolicy(100, "Contract: up to 100% allocation."),
        EmploymentType.Intern => new EmploymentPolicy(50, "Intern: capped at 50% allocation."),
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown employment type.")
    };
}
