namespace Rms.Tests;

public class AllocationPolicyTests
{
    private static DateRange Range(int fromMonth, int fromDay, int toMonth, int toDay) =>
        new(new DateOnly(2026, fromMonth, fromDay), new DateOnly(2026, toMonth, toDay));

    private static readonly DateRange ProjectDates = Range(1, 1, 12, 31);

    private static Employee NewEmployee(EmploymentType type = EmploymentType.FullTime, bool active = true)
    {
        var employee = Employee.Create(1, "Test User", "test@technova.example", "Developer",
            Department.Engineering, type, 1000m).Value;
        if (!active)
        {
            employee.Deactivate();
        }

        return employee;
    }

    private static Project NewProject(ProjectStatus status = ProjectStatus.Active) =>
        Project.Create(1, "Portal", "Contoso", ProjectDates, status).Value;

    private static Allocation Existing(int percent, DateRange period, bool released = false)
    {
        var allocation = Allocation.Create(1, 1, 99, period, new Percentage(percent));
        if (released)
        {
            allocation.Release();
        }

        return allocation;
    }

    private static Result Try(Employee employee, Project project, DateRange period, int percent,
        params Allocation[] existing) =>
        AllocationPolicy.CanAllocate(employee, project, period, new Percentage(percent), existing);

    [Fact]
    public void Allows_allocation_that_reaches_exactly_100_percent()
    {
        var result = Try(NewEmployee(), NewProject(), Range(1, 1, 6, 30), 40,
            Existing(60, Range(1, 1, 6, 30)));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Rejects_allocation_that_goes_above_100_percent_on_overlap()
    {
        var result = Try(NewEmployee(), NewProject(), Range(6, 1, 6, 30), 50,
            Existing(60, Range(1, 1, 6, 30)));

        Assert.True(result.IsFailure);
        Assert.Equal("allocation.over_allocated", result.Error.Code);
    }

    [Fact]
    public void Allows_allocations_that_do_not_overlap_in_time()
    {
        var result = Try(NewEmployee(), NewProject(), Range(4, 1, 6, 30), 60,
            Existing(60, Range(1, 1, 3, 31)));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Checks_the_busiest_day_not_just_the_total_of_all_overlaps()
    {
        var existing = new[]
        {
            Existing(50, Range(1, 1, 6, 30)),
            Existing(40, Range(3, 1, 4, 30))   // busiest period: 90% in March-April
        };

        var tooMuch = Try(NewEmployee(), NewProject(), Range(1, 1, 12, 31), 20, existing);
        var justRight = Try(NewEmployee(), NewProject(), Range(1, 1, 12, 31), 10, existing);

        Assert.True(tooMuch.IsFailure);
        Assert.True(justRight.IsSuccess);
    }

    [Fact]
    public void Released_allocations_do_not_count()
    {
        var result = Try(NewEmployee(), NewProject(), Range(1, 1, 6, 30), 100,
            Existing(100, Range(1, 1, 6, 30), released: true));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Rejects_inactive_employee()
    {
        var result = Try(NewEmployee(active: false), NewProject(), Range(1, 1, 6, 30), 50);

        Assert.True(result.IsFailure);
        Assert.Equal("allocation.employee_inactive", result.Error.Code);
    }

    [Fact]
    public void Rejects_allocation_outside_project_dates()
    {
        var project = Project.Create(1, "Short", "Contoso", Range(3, 1, 5, 31), ProjectStatus.Active).Value;

        var result = Try(NewEmployee(), project, Range(2, 1, 4, 30), 50);

        Assert.True(result.IsFailure);
        Assert.Equal("allocation.outside_project_dates", result.Error.Code);
    }

    [Fact]
    public void Rejects_allocation_to_a_completed_project()
    {
        var result = Try(NewEmployee(), NewProject(ProjectStatus.Completed), Range(1, 1, 6, 30), 50);

        Assert.True(result.IsFailure);
        Assert.Equal("allocation.project_not_open", result.Error.Code);
    }

    [Fact]
    public void Intern_policy_caps_allocation_at_50_percent()
    {
        var intern = NewEmployee(EmploymentType.Intern);

        Assert.True(Try(intern, NewProject(), Range(1, 1, 6, 30), 50).IsSuccess);
        Assert.True(Try(intern, NewProject(), Range(1, 1, 6, 30), 60).IsFailure);
    }

    [Fact]
    public void Available_capacity_is_what_is_left_at_the_busiest_day()
    {
        var capacity = AllocationPolicy.AvailableCapacity(NewEmployee(), Range(1, 1, 6, 30),
            [Existing(50, Range(1, 1, 6, 30)), Existing(30, Range(3, 1, 3, 31))]);

        Assert.Equal(20, capacity);
    }
}
