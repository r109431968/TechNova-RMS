namespace Rms.Tests;

public sealed class MatchingServiceTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), $"rms-tests-{Guid.NewGuid():N}");
    private readonly JsonEmployeeRepository _employees;
    private readonly JsonProjectRepository _projects;
    private readonly JsonAllocationRepository _allocations;

    public MatchingServiceTests()
    {
        _employees = new JsonEmployeeRepository(Path.Combine(_directory, "employees.json"));
        _projects = new JsonProjectRepository(Path.Combine(_directory, "projects.json"));
        _allocations = new JsonAllocationRepository(Path.Combine(_directory, "allocations.json"));
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    private void AddEmployee(string name, bool active, params (string Skill, Proficiency Level)[] skills)
    {
        var employee = Employee.Create(_employees.NextId(), name, $"{name.Replace(' ', '.')}@technova.example",
            "Developer", Department.Engineering, EmploymentType.FullTime, 1000m).Value;
        foreach (var (skill, level) in skills)
        {
            employee.SetSkill(skill, level);
        }

        if (!active)
        {
            employee.Deactivate();
        }

        _employees.Add(employee);
    }

    private Project AddProject()
    {
        var dates = new DateRange(new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31));
        var project = Project.Create(_projects.NextId(), "Portal", "Contoso", dates, ProjectStatus.Active).Value;
        project.AddRequirement("C#", Proficiency.Advanced);
        project.AddRequirement("SQL", Proficiency.Intermediate);
        _projects.Add(project);
        return project;
    }

    [Fact]
    public void Suggests_free_matching_employees_best_match_first()
    {
        var project = AddProject();
        AddEmployee("Both Skills", active: true, ("C#", Proficiency.Expert), ("SQL", Proficiency.Advanced)); // id 1
        AddEmployee("One Skill", active: true, ("C#", Proficiency.Advanced));                                  // id 2
        AddEmployee("Fully Booked", active: true, ("C#", Proficiency.Expert), ("SQL", Proficiency.Advanced)); // id 3
        AddEmployee("Inactive", active: false, ("C#", Proficiency.Expert));                                    // id 4
        AddEmployee("No Skills", active: true);                                                                // id 5
        AddEmployee("Too Junior", active: true, ("C#", Proficiency.Basic));                                    // id 6

        _allocations.Add(Allocation.Create(_allocations.NextId(), 3, 99, project.Dates, new Percentage(100)));

        var service = new MatchingService(_employees, _projects, _allocations, new SkillCoverageScoring());
        var result = service.Suggest(project.Id);

        Assert.True(result.IsSuccess);
        var names = result.Value.Select(s => s.Employee.Name).ToArray();
        Assert.Equal(new[] { "Both Skills", "One Skill" }, names);
        Assert.Contains("SQL", result.Value[1].MissingSkills);
    }

    [Fact]
    public void Unknown_project_returns_not_found()
    {
        var service = new MatchingService(_employees, _projects, _allocations, new SkillCoverageScoring());

        var result = service.Suggest(42);

        Assert.True(result.IsFailure);
        Assert.Equal("not_found", result.Error.Code);
    }

    [Fact]
    public void Scoring_strategy_can_be_swapped_without_changing_the_service()
    {
        var project = AddProject();
        AddEmployee("Skilled but busy", active: true, ("C#", Proficiency.Expert), ("SQL", Proficiency.Expert));
        AddEmployee("Free but less skilled", active: true, ("C#", Proficiency.Advanced));
        _allocations.Add(Allocation.Create(_allocations.NextId(), 1, 99, project.Dates, new Percentage(80)));

        var bySkill = new MatchingService(_employees, _projects, _allocations, new SkillCoverageScoring())
            .Suggest(project.Id).Value;
        var byAvailability = new MatchingService(_employees, _projects, _allocations, new AvailabilityFirstScoring())
            .Suggest(project.Id).Value;

        Assert.Equal("Skilled but busy", bySkill[0].Employee.Name);
        Assert.Equal("Free but less skilled", byAvailability[0].Employee.Name);
    }
}
