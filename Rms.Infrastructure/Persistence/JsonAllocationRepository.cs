namespace Rms.Infrastructure.Persistence;

public sealed class JsonAllocationRepository : IAllocationRepository
{
    private readonly JsonFileStore<AllocationRecord> _store;
    private readonly List<Allocation> _items;

    public JsonAllocationRepository(string filePath)
    {
        _store = new JsonFileStore<AllocationRecord>(filePath);
        _items = _store.Load()
            .Select(r => Allocation.Restore(r.Id, r.EmployeeId, r.ProjectId, new DateRange(r.Start, r.End),
                new Percentage(r.Percent), r.IsReleased))
            .ToList();
    }

    public IReadOnlyList<Allocation> GetAll() => [.. _items];

    public Allocation? GetById(int id) => _items.FirstOrDefault(a => a.Id == id);

    public IReadOnlyList<Allocation> GetForEmployee(int employeeId) =>
        [.. _items.Where(a => a.EmployeeId == employeeId)];

    public IReadOnlyList<Allocation> GetForProject(int projectId) =>
        [.. _items.Where(a => a.ProjectId == projectId)];

    public int NextId() => _items.Count == 0 ? 1 : _items.Max(a => a.Id) + 1;

    public void Add(Allocation allocation)
    {
        _items.Add(allocation);
        Persist();
    }

    public void Save(Allocation allocation) => Persist();

    private void Persist() => _store.Save(_items.Select(a => new AllocationRecord(
        a.Id, a.EmployeeId, a.ProjectId, a.Period.Start, a.Period.End, a.Percent.Value, a.IsReleased)));
}
