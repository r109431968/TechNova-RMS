namespace Rms.ConsoleUI;

/// <summary>Console front end. It only talks to application services; no business rules live here.</summary>
public sealed class ConsoleMenu(
    EmployeeService employees,
    ProjectService projects,
    AllocationService allocations,
    MatchingService matching,
    ReportService reports)
{
    public void Run()
    {
        Console.WriteLine("==============================================");
        Console.WriteLine("  TechNova Resource Management System (RMS)");
        Console.WriteLine("==============================================");

        while (true)
        {
            ShowMenu();
            try
            {
                var choice = Prompt.ReadString("Choose an option");
                Console.WriteLine();
                if (choice == "0")
                {
                    Console.WriteLine("Goodbye!");
                    return;
                }

                Execute(choice);
            }
            catch (EndOfStreamException)
            {
                return;
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
            {
                Console.WriteLine($"  [X] {ex.Message}");
            }

            Console.WriteLine();
        }
    }

    private static void ShowMenu() => Console.WriteLine("""

        --- Employees ---          --- Projects & allocations ---
          1. List employees          6. List projects
          2. Add employee            7. Create project
          3. Update employee         8. Allocate employee to project
          4. Deactivate employee     9. Release allocation
          5. Set employee skill     10. List allocations
                                    11. Suggest employees for a project
        --- Reports ---
         12. Bench report
         13. Project staffing summary
         14. Department utilisation
          0. Exit
        """);

    private void Execute(string choice)
    {
        switch (choice)
        {
            case "1": ListEmployees(); break;
            case "2": AddEmployee(); break;
            case "3": UpdateEmployee(); break;
            case "4": DeactivateEmployee(); break;
            case "5": SetSkill(); break;
            case "6": ListProjects(); break;
            case "7": CreateProject(); break;
            case "8": Allocate(); break;
            case "9": Release(); break;
            case "10": ListAllocations(); break;
            case "11": SuggestMatches(); break;
            case "12": BenchReport(); break;
            case "13": ProjectStaffingReport(); break;
            case "14": DepartmentReport(); break;
            default: Console.WriteLine("Unknown option, please try again."); break;
        }
    }

    // ---------- Employees ----------

    private void ListEmployees()
    {
        Console.WriteLine($"{"ID",-4}{"Name",-18}{"Designation",-26}{"Department",-18}{"Type",-10}{"Cost/h",8}  {"Active",-7}Skills (level 1-5)");
        foreach (var e in employees.GetAll())
        {
            var skills = string.Join(", ", e.Skills.Select(s => $"{s.Skill}:{(int)s.Level}"));
            Console.WriteLine($"{e.Id,-4}{e.Name,-18}{e.Designation,-26}{e.Department,-18}{e.EmploymentType,-10}{e.CostRate,8:0.##}  {(e.IsActive ? "yes" : "NO"),-7}{skills}");
        }
    }

    private void AddEmployee()
    {
        var name = Prompt.ReadString("Name");
        var email = Prompt.ReadString("Email");
        var designation = Prompt.ReadString("Designation");
        var department = Prompt.ReadEnum<Department>("Department");
        var type = Prompt.ReadEnum<EmploymentType>("Employment type");
        var cost = Prompt.ReadDecimal("Cost rate (per hour)");

        var result = employees.Add(name, email, designation, department, type, cost);
        if (Succeeded(result))
        {
            Console.WriteLine($"  [OK] Employee added with ID {result.Value.Id}.");
        }
    }

    private void UpdateEmployee()
    {
        var id = Prompt.ReadInt("Employee ID to update");
        Console.WriteLine("Enter the new values for all fields:");
        var name = Prompt.ReadString("Name");
        var email = Prompt.ReadString("Email");
        var designation = Prompt.ReadString("Designation");
        var department = Prompt.ReadEnum<Department>("Department");
        var type = Prompt.ReadEnum<EmploymentType>("Employment type");
        var cost = Prompt.ReadDecimal("Cost rate (per hour)");

        if (Succeeded(employees.Update(id, name, email, designation, department, type, cost)))
        {
            Console.WriteLine("  [OK] Employee updated.");
        }
    }

    private void DeactivateEmployee()
    {
        var id = Prompt.ReadInt("Employee ID to deactivate");
        if (Succeeded(employees.Deactivate(id)))
        {
            Console.WriteLine("  [OK] Employee deactivated. They can no longer be allocated.");
        }
    }

    private void SetSkill()
    {
        var id = Prompt.ReadInt("Employee ID");
        var skill = Prompt.ReadString("Skill name");
        var level = Prompt.ReadProficiency("Proficiency (1=Beginner ... 5=Expert)");
        if (Succeeded(employees.SetSkill(id, skill, level)))
        {
            Console.WriteLine("  [OK] Skill saved.");
        }
    }

    // ---------- Projects & allocations ----------

    private void ListProjects()
    {
        Console.WriteLine($"{"ID",-4}{"Project",-26}{"Client",-20}{"Start",-12}{"End",-12}{"Status",-10}Required skills (min level)");
        foreach (var p in projects.GetAll())
        {
            var needs = string.Join(", ", p.Requirements.Select(r => $"{r.Skill}:{(int)r.MinLevel}"));
            Console.WriteLine($"{p.Id,-4}{p.Name,-26}{p.Client,-20}{p.Dates.Start:yyyy-MM-dd}  {p.Dates.End:yyyy-MM-dd}  {p.Status,-10}{needs}");
        }
    }

    private void CreateProject()
    {
        var name = Prompt.ReadString("Project name");
        var client = Prompt.ReadString("Client");
        var dates = Prompt.ReadDateRange("Start date", "End date");
        var status = Prompt.ReadEnum<ProjectStatus>("Status");

        var requirements = new List<SkillRequirement>();
        while (Prompt.ReadYesNo("Add a required skill?"))
        {
            var skill = Prompt.ReadString("  Skill name");
            var level = Prompt.ReadProficiency("  Minimum level (1-5)");
            requirements.Add(new SkillRequirement(skill, level));
        }

        var result = projects.Create(name, client, dates, status, requirements);
        if (Succeeded(result))
        {
            Console.WriteLine($"  [OK] Project created with ID {result.Value.Id}.");
        }
    }

    private void Allocate()
    {
        var employeeId = Prompt.ReadInt("Employee ID");
        var projectId = Prompt.ReadInt("Project ID");
        var period = Prompt.ReadDateRange("Allocation start date", "Allocation end date");
        var percent = Prompt.ReadPercentage("Allocation percentage (1-100)");

        var result = allocations.Allocate(employeeId, projectId, period, percent);
        if (Succeeded(result))
        {
            Console.WriteLine($"  [OK] Allocation #{result.Value.Id} created.");
        }
    }

    private void Release()
    {
        var id = Prompt.ReadInt("Allocation ID to release");
        if (Succeeded(allocations.Release(id)))
        {
            Console.WriteLine("  [OK] Allocation released.");
        }
    }

    private void ListAllocations()
    {
        var employeeNames = employees.GetAll().ToDictionary(e => e.Id, e => e.Name);
        var projectNames = projects.GetAll().ToDictionary(p => p.Id, p => p.Name);

        Console.WriteLine($"{"ID",-4}{"Employee",-18}{"Project",-26}{"Start",-12}{"End",-12}{"%",5}  Status");
        foreach (var a in allocations.GetAll())
        {
            Console.WriteLine($"{a.Id,-4}{employeeNames.GetValueOrDefault(a.EmployeeId, "?"),-18}{projectNames.GetValueOrDefault(a.ProjectId, "?"),-26}" +
                              $"{a.Period.Start:yyyy-MM-dd}  {a.Period.End:yyyy-MM-dd}  {a.Percent.Value,4}%  {(a.IsReleased ? "released" : "active")}");
        }
    }

    private void SuggestMatches()
    {
        var projectId = Prompt.ReadInt("Project ID");
        var result = matching.Suggest(projectId);
        if (!Succeeded(result))
        {
            return;
        }

        var suggestions = result.Value;
        if (suggestions.Count == 0)
        {
            Console.WriteLine("  No suitable available employees found.");
            return;
        }

        Console.WriteLine($"Scoring: {matching.StrategyName}");
        Console.WriteLine($"{"#",-3}{"Employee",-18}{"Score",7}{"Free%",7}  {"Matched skills",-28}Missing skills");
        for (var i = 0; i < suggestions.Count; i++)
        {
            var s = suggestions[i];
            Console.WriteLine($"{i + 1,-3}{s.Employee.Name,-18}{s.Score,7:0.0}{s.AvailablePercent,6}%  {string.Join(", ", s.MatchedSkills),-28}{string.Join(", ", s.MissingSkills)}");
        }
    }

    // ---------- Reports ----------

    private void BenchReport()
    {
        var window = reports.CurrentWindow();
        Console.WriteLine($"BENCH REPORT: active employees allocated below {reports.Settings.BenchThresholdPercent}% on average, {window.Start:yyyy-MM-dd} to {window.End:yyyy-MM-dd}");
        Console.WriteLine($"{"ID",-4}{"Employee",-18}{"Department",-18}{"Type",-10}{"Avg allocation",14}");
        foreach (var row in reports.Bench())
        {
            Console.WriteLine($"{row.Employee.Id,-4}{row.Employee.Name,-18}{row.Employee.Department,-18}{row.Employee.EmploymentType,-10}{row.AverageUtilisation,13:0.0}%");
        }
    }

    private void ProjectStaffingReport()
    {
        Console.WriteLine("PROJECT STAFFING SUMMARY");
        foreach (var row in reports.ProjectStaffing())
        {
            Console.WriteLine($"{row.Project.Name} ({row.Project.Client}) [{row.Project.Status}] - {row.HeadCount} people, {row.Fte:0.0} FTE");
            Console.WriteLine($"    Team: {(row.Team.Count == 0 ? "(nobody yet)" : string.Join(", ", row.Team))}");
            Console.WriteLine($"    Skill gaps: {(row.UncoveredSkills.Count == 0 ? "none" : string.Join(", ", row.UncoveredSkills))}");
        }
    }

    private void DepartmentReport()
    {
        var window = reports.CurrentWindow();
        Console.WriteLine($"DEPARTMENT UTILISATION: average allocation of active employees, {window.Start:yyyy-MM-dd} to {window.End:yyyy-MM-dd}");
        Console.WriteLine($"{"Department",-18}{"People",7}{"Utilisation",13}");
        foreach (var row in reports.DepartmentUtilisation())
        {
            Console.WriteLine($"{row.Department,-18}{row.HeadCount,7}{row.UtilisationPercent,12:0.0}%");
        }
    }

    private static bool Succeeded(Result result)
    {
        if (result.IsSuccess)
        {
            return true;
        }

        Console.WriteLine($"  [X] {result.Error.Message}");
        return false;
    }
}
