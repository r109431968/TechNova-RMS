namespace Rms.Infrastructure.Persistence;

public sealed class JsonProjectRepository : IProjectRepository
{
    private readonly JsonFileStore<ProjectRecord> _store;
    private readonly List<Project> _items;

    public JsonProjectRepository(string filePath)
    {
        _store = new JsonFileStore<ProjectRecord>(filePath);
        _items = _store.Load()
            .Select(r => Project.Restore(r.Id, r.Name, r.Client, new DateRange(r.Start, r.End),
                r.Status, r.Requirements))
            .ToList();
    }

    public IReadOnlyList<Project> GetAll() => [.. _items];

    public Project? GetById(int id) => _items.FirstOrDefault(p => p.Id == id);

    public int NextId() => _items.Count == 0 ? 1 : _items.Max(p => p.Id) + 1;

    public void Add(Project project)
    {
        _items.Add(project);
        Persist();
    }

    public void Save(Project project) => Persist();

    private void Persist() => _store.Save(_items.Select(p => new ProjectRecord(
        p.Id, p.Name, p.Client, p.Dates.Start, p.Dates.End, p.Status, [.. p.Requirements])));
}
