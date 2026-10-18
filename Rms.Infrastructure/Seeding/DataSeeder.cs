namespace Rms.Infrastructure.Seeding;

/// <summary>
/// Creates demo data on first run: 17 employees (1 inactive), 5 projects, 15 allocations.
/// Dates are relative to today so the reports are always meaningful.
/// Allocations are checked with <see cref="AllocationPolicy"/>, so seed data obeys the same rules.
/// </summary>
public sealed class DataSeeder(
    IEmployeeRepository employees,
    IProjectRepository projects,
    IAllocationRepository allocations,
    TimeProvider clock)
{
    public void SeedIfEmpty()
    {
        if (employees.GetAll().Count > 0 || projects.GetAll().Count > 0)
        {
            return;
        }

        var today = DateOnly.FromDateTime(clock.GetLocalNow().DateTime);
        DateOnly Day(int offset) => today.AddDays(offset);

        Employee AddEmployee(string name, string designation, Department department, EmploymentType type,
            decimal cost, params (string Skill, Proficiency Level)[] skills)
        {
            var email = $"{name.ToLowerInvariant().Replace(' ', '.')}@technova.example";
            var employee = Employee.Create(employees.NextId(), name, email, designation, department, type, cost).Value;
            foreach (var (skill, level) in skills)
            {
                employee.SetSkill(skill, level);
            }

            employees.Add(employee);
            return employee;
        }

        void AddProject(string name, string client, int startOffset, int endOffset, ProjectStatus status,
            params (string Skill, Proficiency Min)[] requirements)
        {
            var project = Project.Create(projects.NextId(), name, client,
                new DateRange(Day(startOffset), Day(endOffset)), status).Value;
            foreach (var (skill, min) in requirements)
            {
                project.AddRequirement(skill, min);
            }

            projects.Add(project);
        }

        void Allocate(int employeeId, int projectId, int percent)
        {
            var employee = employees.GetById(employeeId)!;
            var project = projects.GetById(projectId)!;
            var share = new Percentage(percent);
            var check = AllocationPolicy.CanAllocate(employee, project, project.Dates, share,
                allocations.GetForEmployee(employeeId));
            if (check.IsFailure)
            {
                throw new InvalidOperationException($"Invalid seed data: {check.Error.Message}");
            }

            allocations.Add(Allocation.Create(allocations.NextId(), employeeId, projectId, project.Dates, share));
        }

        // Employees (IDs 1..17)
        AddEmployee("Aarav Sharma", "Senior Engineer", Department.Engineering, EmploymentType.FullTime, 2500,
            ("C#", Proficiency.Expert), ("SQL", Proficiency.Advanced), ("Azure", Proficiency.Intermediate));
        AddEmployee("Diya Patel", "Software Engineer", Department.Engineering, EmploymentType.FullTime, 1800,
            ("C#", Proficiency.Advanced), ("SQL", Proficiency.Intermediate), ("React", Proficiency.Basic));
        AddEmployee("Rohan Mehta", "Tech Lead", Department.Engineering, EmploymentType.FullTime, 3200,
            ("C#", Proficiency.Expert), ("Azure", Proficiency.Advanced), ("DevOps", Proficiency.Intermediate));
        AddEmployee("Ananya Iyer", "QA Engineer", Department.QualityAssurance, EmploymentType.FullTime, 1500,
            ("Testing", Proficiency.Advanced), ("C#", Proficiency.Basic));
        AddEmployee("Vikram Singh", "DevOps Engineer", Department.DevOps, EmploymentType.FullTime, 2200,
            ("DevOps", Proficiency.Expert), ("Azure", Proficiency.Advanced));
        AddEmployee("Sneha Reddy", "Data Engineer", Department.DataAnalytics, EmploymentType.FullTime, 2400,
            ("Python", Proficiency.Advanced), ("Data Engineering", Proficiency.Advanced), ("SQL", Proficiency.Advanced));
        AddEmployee("Karan Malhotra", "UI Developer", Department.Engineering, EmploymentType.FullTime, 1900,
            ("React", Proficiency.Expert), ("UX Design", Proficiency.Intermediate));
        AddEmployee("Isha Gupta", "UX Designer", Department.Design, EmploymentType.FullTime, 1700,
            ("UX Design", Proficiency.Expert), ("React", Proficiency.Basic));
        AddEmployee("Arjun Nair", "Contract Developer", Department.Engineering, EmploymentType.Contract, 2000,
            ("C#", Proficiency.Advanced), ("SQL", Proficiency.Intermediate));
        AddEmployee("Meera Joshi", "Contract Data Analyst", Department.DataAnalytics, EmploymentType.Contract, 1900,
            ("Python", Proficiency.Intermediate), ("SQL", Proficiency.Advanced), ("Data Engineering", Proficiency.Intermediate));
        AddEmployee("Rahul Verma", "Cloud Architect", Department.DevOps, EmploymentType.FullTime, 3500,
            ("Azure", Proficiency.Expert), ("DevOps", Proficiency.Advanced), ("C#", Proficiency.Intermediate));
        AddEmployee("Pooja Kulkarni", "Test Automation Engineer", Department.QualityAssurance, EmploymentType.FullTime, 1600,
            ("Testing", Proficiency.Expert), ("C#", Proficiency.Intermediate), ("DevOps", Proficiency.Basic));
        AddEmployee("Siddharth Rao", "Intern Developer", Department.Engineering, EmploymentType.Intern, 600,
            ("C#", Proficiency.Basic), ("React", Proficiency.Basic), ("Testing", Proficiency.Basic));
        AddEmployee("Nisha Bansal", "Intern Designer", Department.Design, EmploymentType.Intern, 500,
            ("UX Design", Proficiency.Intermediate));
        AddEmployee("Aditya Chauhan", "Senior Developer", Department.Engineering, EmploymentType.FullTime, 2300,
            ("React", Proficiency.Advanced), ("C#", Proficiency.Intermediate), ("Testing", Proficiency.Intermediate));
        AddEmployee("Lakshmi Pillai", "Data Scientist", Department.DataAnalytics, EmploymentType.FullTime, 2600,
            ("Python", Proficiency.Expert), ("Data Engineering", Proficiency.Intermediate));

        // An inactive employee, to show the "inactive cannot be allocated" rule.
        var inactive = AddEmployee("Manish Desai", "Developer", Department.Engineering, EmploymentType.FullTime, 1700,
            ("C#", Proficiency.Intermediate));
        inactive.Deactivate();
        employees.Save(inactive);

        // Projects (IDs 1..5)
        AddProject("Phoenix Banking Portal", "Northwind Bank", -60, 120, ProjectStatus.Active,
            ("C#", Proficiency.Advanced), ("SQL", Proficiency.Intermediate), ("Azure", Proficiency.Intermediate));
        AddProject("Atlas Retail Platform", "Contoso Retail", -30, 150, ProjectStatus.Active,
            ("C#", Proficiency.Advanced), ("React", Proficiency.Advanced), ("Testing", Proficiency.Intermediate));
        AddProject("Orion Data Platform", "Fabrikam Logistics", -15, 200, ProjectStatus.Active,
            ("Python", Proficiency.Advanced), ("Data Engineering", Proficiency.Advanced), ("SQL", Proficiency.Intermediate));
        AddProject("Helios Cloud Migration", "Tailspin Energy", 10, 180, ProjectStatus.Planned,
            ("Azure", Proficiency.Advanced), ("DevOps", Proficiency.Advanced), ("C#", Proficiency.Intermediate));
        AddProject("Nimbus Customer Portal", "Adventure Works", -45, 90, ProjectStatus.Active,
            ("React", Proficiency.Advanced), ("UX Design", Proficiency.Intermediate), ("Testing", Proficiency.Intermediate));

        // Allocations: (employeeId, projectId, percent), each for the full project period.
        Allocate(1, 1, 60);
        Allocate(1, 2, 40);   // Aarav is exactly at 100%
        Allocate(2, 1, 80);
        Allocate(3, 1, 50);
        Allocate(3, 4, 50);   // Rohan is at 100% while both projects overlap
        Allocate(4, 5, 50);
        Allocate(5, 4, 70);
        Allocate(6, 3, 100);
        Allocate(7, 5, 80);
        Allocate(8, 5, 40);
        Allocate(9, 2, 100);
        Allocate(11, 4, 60);
        Allocate(13, 2, 50);  // intern, capped at 50%
        Allocate(15, 2, 50);
        Allocate(16, 3, 50);
    }
}
