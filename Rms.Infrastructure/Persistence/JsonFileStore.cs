using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rms.Infrastructure.Persistence;

/// <summary>Reads and writes a list of records as one JSON file (atomic replace on save).</summary>
internal sealed class JsonFileStore<T>(string path)
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public List<T> Load()
    {
        if (!File.Exists(path))
        {
            return [];
        }

        using var stream = File.OpenRead(path);
        return JsonSerializer.Deserialize<List<T>>(stream, Options) ?? [];
    }

    public void Save(IEnumerable<T> items)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var temp = path + ".tmp";
        using (var stream = File.Create(temp))
        {
            JsonSerializer.Serialize(stream, items.ToList(), Options);
        }

        File.Move(temp, path, overwrite: true);
    }
}
