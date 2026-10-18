using Rms.Domain.Enums;
using Rms.Domain.ValueObjects;

namespace Rms.Infrastructure.Persistence;

// Storage shapes (DTOs). Keeping them separate from the domain entities means the domain
// needs no serializer attributes and the file format can change without touching business code.

internal sealed record EmployeeRecord(
    int Id,
    string Name,
    string Email,
    string Designation,
    Department Department,
    EmploymentType EmploymentType,
    decimal CostRate,
    bool IsActive,
    List<EmployeeSkill> Skills);

internal sealed record ProjectRecord(
    int Id,
    string Name,
    string Client,
    DateOnly Start,
    DateOnly End,
    ProjectStatus Status,
    List<SkillRequirement> Requirements);

internal sealed record AllocationRecord(
    int Id,
    int EmployeeId,
    int ProjectId,
    DateOnly Start,
    DateOnly End,
    int Percent,
    bool IsReleased);
