namespace Rms.Domain.ValueObjects;

/// <summary>A skill an employee has, with proficiency (1-5 via <see cref="Proficiency"/>).</summary>
public sealed record EmployeeSkill(string Skill, Proficiency Level);

/// <summary>A skill a project needs, with the minimum proficiency required.</summary>
public sealed record SkillRequirement(string Skill, Proficiency MinLevel);
