namespace Rms.Infrastructure.Persistence;

public sealed class JsonEmployeeRepository : IEmployeeRepository
{
    private readonly JsonFileStore<EmployeeRecord> _store;
    private readonly List<Employee> _items;

    public JsonEmployeeRepository(string filePath)
    {
        _store = new JsonFileStore<EmployeeRecord>(filePath);
        _items = _store.Load()
            .Select(r => Employee.Restore(r.Id, r.Name, r.Email, r.Designation, r.Department,
                r.EmploymentType, r.CostRate, r.IsActive, r.Skills))
            .ToList();
    }

    public IReadOnlyList<Employee> GetAll() => [.. _items];

    public Employee? GetById(int id) => _items.FirstOrDefault(e => e.Id == id);

    public int NextId() => _items.Count == 0 ? 1 : _items.Max(e => e.Id) + 1;

    public void Add(Employee employee)
    {
        _items.Add(employee);
        Persist();
    }

    public void Save(Employee employee) => Persist();

    private void Persist() => _store.Save(_items.Select(e => new EmployeeRecord(
        e.Id, e.Name, e.Email.Value, e.Designation, e.Department, e.EmploymentType,
        e.CostRate, e.IsActive, [.. e.Skills])));
}
