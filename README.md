# TechNova RMS - Resource Management System (Week 1)

Week 1 of the 12-week RMS capstone: the **core domain and business engine** of a Resource Management System for TechNova Solutions, built as a **.NET 10 / C# 14 console application** with no UI framework and no database.

TechNova tracks employees, skills, client projects and allocations in spreadsheets today. That causes double bookings, people sitting unallocated and slow staffing decisions. This engine enforces the staffing rules in code and reports on them.

The design is layered (Clean Architecture style) so that later weeks can add an ASP.NET Core MVC front end, EF Core and a .NET MAUI app **without rewriting the business logic**. See [DESIGN.md](DESIGN.md) for the architecture, diagrams, ADRs and SOLID mapping.

## Features

**Employees**
- Add, update and deactivate employees (ID, name, email, designation, department, employment type, cost rate).
- Skills with proficiency 1-5 (`Beginner` ... `Expert`).
- Duplicate emails are rejected.

**Projects**
- Create client projects with name, client, start/end dates, status and the skills they need (with a minimum level per skill).

**Allocations**
- Allocate an employee to a project for a date range and a percentage.
- Release an allocation.
- Business rules enforced by the system:
  - An employee's total allocation never exceeds 100% on any day of an overlapping period (interns are capped at 50%).
  - Inactive employees cannot be allocated.
  - An allocation must fall within the project's dates.
  - Completed / on-hold projects cannot receive allocations.

**Matching**
- For a project, suggests active employees who match at least one required skill and have free capacity for the whole project, ranked by a pluggable scoring strategy.

**Reports**
- **Bench report:** active employees whose average allocation over the next 30 days is below 70%.
- **Project staffing summary:** headcount, FTE, team and skill gaps per project.
- **Department utilisation:** average allocation of active employees per department.

**Persistence**
- Data is stored as JSON files (`employees.json`, `projects.json`, `allocations.json`) behind repository interfaces and survives restarts.

**Notifications**
- Simulated notifications are printed when an employee is allocated or released.

**Seed data** (created on the first run)
- 17 employees (1 inactive, 2 interns, 2 contractors), 5 projects and 15 allocations. Dates are relative to "today", so the reports always show meaningful data.

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (C# 14)
- No other tools or third-party packages. Only `Microsoft.Extensions.DependencyInjection` is used (plus xUnit for tests).

Check your SDK with:

```
dotnet --version
```

## How to run

```bash
git clone https://github.com/r109431968/TechNova-RMS.git
cd TechNova-RMS

dotnet build
dotnet run --project Rms.Console
```

Type a menu number and press Enter. Use `0` to exit.

**Run the tests**

```bash
dotnet test
```

**Where is the data?** JSON files are written to a `data` folder next to the compiled app, for example `Rms.Console/bin/Debug/net10.0/data/`. The first run creates the seed data. To start fresh, delete that `data` folder.

## Project structure

```
Rms.sln
Directory.Build.props net10.0, nullable enabled, warnings as errors
Rms.Domain/ entities, value objects, rules, repository interfaces (no dependencies)
Rms.Application/ use-case services, matching strategies, ports (INotifier)
Rms.Infrastructure/ JSON repositories, console notifier, seed data
Rms.Console/ Program.cs (composition root only) and the console menu
Rms.Tests/ xUnit tests for allocation rules, value objects and matching
```

Dependencies point inwards: `Console -> Application -> Domain`, and `Infrastructure -> Application/Domain`.

## Menu

```
--- Employees --- --- Projects & allocations ---
  1. List employees 6. List projects
  2. Add employee 7. Create project
  3. Update employee 8. Allocate employee to project
  4. Deactivate employee 9. Release allocation
  5. Set employee skill 10. List allocations
                            11. Suggest employees for a project
--- Reports ---
 12. Bench report
 13. Project staffing summary
 14. Department utilisation
  0. Exit
```

## Sample console session

<!-- TODO before submitting: replace this block with the real output of your own run (copy it from the console). -->

The session below was run on 2026-10-19 with the seed data. Seed dates are relative to the day you run the app, so dates and some percentages will differ on other days. Valid input is re-asked until it is correct; business-rule failures are printed as `[X]` and the menu continues.

```text
==============================================
  TechNova Resource Management System (RMS)
==============================================

--- Employees --- --- Projects & allocations ---
  1. List employees 6. List projects
  2. Add employee 7. Create project
  3. Update employee 8. Allocate employee to project
  4. Deactivate employee 9. Release allocation
  5. Set employee skill 10. List allocations
                            11. Suggest employees for a project
--- Reports ---
 12. Bench report
 13. Project staffing summary
 14. Department utilisation
  0. Exit
Choose an option: 1

ID Name Designation Department Type Cost/h Active Skills (level 1-5)
1 Aarav Sharma Senior Engineer Engineering FullTime 2500 yes C#:5, SQL:4, Azure:3
2 Diya Patel Software Engineer Engineering FullTime 1800 yes C#:4, SQL:3, React:2
3 Rohan Mehta Tech Lead Engineering FullTime 3200 yes C#:5, Azure:4, DevOps:3
4 Ananya Iyer QA Engineer QualityAssurance FullTime 1500 yes Testing:4, C#:2
5 Vikram Singh DevOps Engineer DevOps FullTime 2200 yes DevOps:5, Azure:4
6 Sneha Reddy Data Engineer DataAnalytics FullTime 2400 yes Python:4, Data Engineering:4, SQL:4
7 Karan Malhotra UI Developer Engineering FullTime 1900 yes React:5, UX Design:3
8 Isha Gupta UX Designer Design FullTime 1700 yes UX Design:5, React:2
9 Arjun Nair Contract Developer Engineering Contract 2000 yes C#:4, SQL:3
10 Meera Joshi Contract Data Analyst DataAnalytics Contract 1900 yes Python:3, SQL:4, Data Engineering:3
11 Rahul Verma Cloud Architect DevOps FullTime 3500 yes Azure:5, DevOps:4, C#:3
12 Pooja Kulkarni Test Automation Engineer QualityAssurance FullTime 1600 yes Testing:5, C#:3, DevOps:2
13 Siddharth Rao Intern Developer Engineering Intern 600 yes C#:2, React:2, Testing:2
14 Nisha Bansal Intern Designer Design Intern 500 yes UX Design:3
15 Aditya Chauhan Senior Developer Engineering FullTime 2300 yes React:4, C#:3, Testing:3
16 Lakshmi Pillai Data Scientist DataAnalytics FullTime 2600 yes Python:5, Data Engineering:3
17 Manish Desai Developer Engineering FullTime 1700 NO C#:3

(menu is printed again before every prompt; omitted below)

Choose an option: 11

Project ID: 4
Scoring: Skill coverage 60% + proficiency 25% + availability 15%
# Employee Score Free% Matched skills Missing skills
1 Rahul Verma 86.0 40% Azure, DevOps, C#           
2 Vikram Singh 67.0 30% Azure, DevOps C#
3 Pooja Kulkarni 50.0 100% C# Azure, DevOps
4 Diya Patel 43.0 20% C# Azure, DevOps
5 Aditya Chauhan 42.5 50% C# Azure, DevOps

Choose an option: 8

Employee ID: 1
Project ID: 2
Allocation start date (yyyy-MM-dd): 2026-10-19
Allocation end date (yyyy-MM-dd): 2026-10-28
Allocation percentage (1-100): 20
  [X] On 2026-10-19 the employee is already at 100%; adding 20% would exceed the 100% limit.

Choose an option: 8

Employee ID: 17
Project ID: 2
Allocation start date (yyyy-MM-dd): 2026-10-19
Allocation end date (yyyy-MM-dd): 2026-10-28
Allocation percentage (1-100): 30
  [X] Manish Desai is inactive and cannot be allocated.

Choose an option: 8

Employee ID: 10
Project ID: 4
Allocation start date (yyyy-MM-dd): 2026-10-19
Allocation end date (yyyy-MM-dd): 2026-11-18
Allocation percentage (1-100): 50
  [X] Allocation must fall within the project dates (2026-10-29 to 2027-04-17).

Choose an option: 8

Employee ID: 10
Project ID: 3
Allocation start date (yyyy-MM-dd): 2026-10-19
Allocation end date (yyyy-MM-dd): 2026-11-17
Allocation percentage (1-100): 50
  [NOTIFY] Meera Joshi allocated to 'Orion Data Platform' at 50% (2026-10-19 to 2026-11-17).
  [OK] Allocation #16 created.

Choose an option: 9

Allocation ID to release: 16
  [NOTIFY] Meera Joshi released from 'Orion Data Platform' (allocation #16).
  [OK] Allocation released.

Choose an option: 12

BENCH REPORT: active employees allocated below 70% on average, 2026-10-19 to 2026-11-17
ID Employee Department Type Avg allocation
10 Meera Joshi DataAnalytics Contract 0.0%
14 Nisha Bansal Design Intern 0.0%
12 Pooja Kulkarni QualityAssurance FullTime 0.0%
8 Isha Gupta Design FullTime 40.0%
11 Rahul Verma DevOps FullTime 40.0%
5 Vikram Singh DevOps FullTime 46.7%
15 Aditya Chauhan Engineering FullTime 50.0%
4 Ananya Iyer QualityAssurance FullTime 50.0%
16 Lakshmi Pillai DataAnalytics FullTime 50.0%
13 Siddharth Rao Engineering Intern 50.0%

Choose an option: 13

PROJECT STAFFING SUMMARY
Phoenix Banking Portal (Northwind Bank) [Active] - 3 people, 1.9 FTE
    Team: Aarav Sharma (60%), Diya Patel (80%), Rohan Mehta (50%)
    Skill gaps: none
Atlas Retail Platform (Contoso Retail) [Active] - 4 people, 2.4 FTE
    Team: Aarav Sharma (40%), Arjun Nair (100%), Siddharth Rao (50%), Aditya Chauhan (50%)
    Skill gaps: none
Orion Data Platform (Fabrikam Logistics) [Active] - 2 people, 1.5 FTE
    Team: Sneha Reddy (100%), Lakshmi Pillai (50%)
    Skill gaps: none
Helios Cloud Migration (Tailspin Energy) [Planned] - 3 people, 1.8 FTE
    Team: Rohan Mehta (50%), Vikram Singh (70%), Rahul Verma (60%)
    Skill gaps: none
Nimbus Customer Portal (Adventure Works) [Active] - 3 people, 1.7 FTE
    Team: Ananya Iyer (50%), Karan Malhotra (80%), Isha Gupta (40%)
    Skill gaps: none

Choose an option: 14

DEPARTMENT UTILISATION: average allocation of active employees, 2026-10-19 to 2026-11-17
Department People Utilisation
Engineering 7 77.6%
QualityAssurance 2 25.0%
DevOps 2 43.3%
DataAnalytics 3 50.0%
Design 2 20.0%

Choose an option: 0

Goodbye!
```

## Things to try (demo cheat sheet)

| Try this | Expected result |
|---|---|
| Option 8: employee 1 (Aarav, already at 100%), project 2, 20% | Rejected: would exceed the 100% limit |
| Option 8: employee 17 (Manish) | Rejected: employee is inactive |
| Option 8: employee 10, project 4, start date before the project starts | Rejected: outside project dates |
| Option 8: employee 13 (intern, already 50%), project 1, 10% | Rejected: intern cap is 50% |
| Option 2 with `aarav.sharma@technova.example` | Rejected: duplicate email |
| Option 11 with project 4 | Ranked suggestions with matched and missing skills |
| Exit, run again, option 10 | Allocations are still there (JSON persistence) |

## Design decisions in one minute

- **Clean/layered architecture** with the domain at the centre, so MVC, EF Core and MAUI plug in later.
- **Allocation is its own entity**, because the rules span both Employee and Project.
- **Employment types use composition** (an enum plus a policy record), not inheritance.
- **Cross-object rules live in a domain service** (`AllocationPolicy`); single-object rules live in the entities and value objects.
- **Expected failures use a `Result` type**, not exceptions.
- **Strategy pattern** for match scoring; **Repository** for storage; **`INotifier`** port for notifications.
- **Singleton** repositories (shared state and file), **transient** services, composition root in `Program.cs`.

Details, diagrams and 9 ADRs are in [DESIGN.md](DESIGN.md).

## Known limitations

- All operations are synchronous (EF Core and MAUI would prefer async; this is planned for the database week).
- Every change rewrites the whole JSON file. Fine for 400 employees, not for concurrent users.
- Deactivating an employee does not release their existing allocations automatically.
- No locking: run only one instance of the app at a time.
