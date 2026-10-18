namespace Rms.Domain.Repositories;

// Interfaces live in the Domain so Application/Infrastructure depend inwards.
// Specific (not generic) interfaces, so each exposes only the queries its aggregate really needs.

public interface IEmployeeRepository
{
    IReadOnlyList<Employee> GetAll();
    Employee? GetById(int id);
    int NextId();
    void Add(Employee employee);
    void Save(Employee employee);
}

public interface IProjectRepository
{
    IReadOnlyList<Project> GetAll();
    Project? GetById(int id);
    int NextId();
    void Add(Project project);
    void Save(Project project);
}

public interface IAllocationRepository
{
    IReadOnlyList<Allocation> GetAll();
    Allocation? GetById(int id);
    IReadOnlyList<Allocation> GetForEmployee(int employeeId);
    IReadOnlyList<Allocation> GetForProject(int projectId);
    int NextId();
    void Add(Allocation allocation);
    void Save(Allocation allocation);
}
