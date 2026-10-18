namespace Rms.Domain.Enums;

public enum EmploymentType
{
    FullTime,
    Contract,
    Intern
}

public enum Department
{
    Engineering,
    QualityAssurance,
    DevOps,
    DataAnalytics,
    Design
}

public enum ProjectStatus
{
    Planned,
    Active,
    OnHold,
    Completed
}

/// <summary>Skill proficiency. None means "does not have the skill".</summary>
public enum Proficiency
{
    None = 0,
    Beginner = 1,
    Basic = 2,
    Intermediate = 3,
    Advanced = 4,
    Expert = 5
}
