using System.Text.Json;
using Atlas.Graph.Tags;

namespace Atlas.ConsoleApp.Storage;

/// <summary>Persists reusable Graph tag definitions in a dedicated JSON file.</summary>
public sealed class JsonTagDefinitionRepository : ITagDefinitionRepository
{
    private readonly string _filePath;
    private readonly object _gate = new();
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    /// <summary>Initializes the adapter for its dedicated data file.</summary>
    public JsonTagDefinitionRepository(string filePath)
    {
        _filePath = filePath;
    }

    /// <summary>Loads all reusable definitions.</summary>
    public IReadOnlyCollection<TagDefinition> GetAll()
    {
        lock (_gate)
        {
            return ReadStored().Select(ToDomain).ToList();
        }
    }

    /// <summary>Loads a definition by identifier.</summary>
    public TagDefinition? GetById(TagDefinitionId id)
    {
        lock (_gate)
        {
            var stored = ReadStored().SingleOrDefault(item => item.Id == id.Value);
            return stored is null ? null : ToDomain(stored);
        }
    }

    /// <summary>Loads the unique definition matching normalized text.</summary>
    public TagDefinition? GetByNormalizedText(string normalizedText)
    {
        lock (_gate)
        {
            var stored = ReadStored().SingleOrDefault(item =>
                string.Equals(item.NormalizedText, normalizedText, StringComparison.Ordinal));
            return stored is null ? null : ToDomain(stored);
        }
    }

    /// <summary>Saves a definition while enforcing normalized uniqueness within this adapter.</summary>
    public void Save(TagDefinition definition)
    {
        lock (_gate)
        {
            var stored = ReadStored();
            var duplicate = stored.FirstOrDefault(item =>
                item.Id != definition.Id.Value &&
                string.Equals(
                    item.NormalizedText,
                    definition.NormalizedText,
                    StringComparison.Ordinal));

            if (duplicate is not null)
            {
                throw new InvalidOperationException(
                    $"The tag '{duplicate.Text}' already exists.");
            }

            var index = stored.FindIndex(item => item.Id == definition.Id.Value);
            var replacement = ToStorage(definition);

            if (index >= 0)
            {
                stored[index] = replacement;
            }
            else
            {
                stored.Add(replacement);
            }

            WriteStored(stored);
        }
    }

    private List<StoredTagDefinition> ReadStored()
    {
        if (!File.Exists(_filePath))
        {
            return [];
        }

        var json = File.ReadAllText(_filePath);
        return string.IsNullOrWhiteSpace(json)
            ? []
            : JsonSerializer.Deserialize<List<StoredTagDefinition>>(json, _jsonOptions) ?? [];
    }

    private void WriteStored(List<StoredTagDefinition> definitions)
    {
        var directory = Path.GetDirectoryName(_filePath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(
            _filePath,
            JsonSerializer.Serialize(definitions, _jsonOptions));
    }

    private static StoredTagDefinition ToStorage(TagDefinition definition) => new()
    {
        Id = definition.Id.Value,
        Text = definition.Text,
        NormalizedText = definition.NormalizedText,
        CreatedByParticipantId = definition.CreatedByParticipantId,
        IsSuppressed = definition.IsSuppressed,
        CreatedAt = definition.CreatedAt,
        UpdatedAt = definition.UpdatedAt
    };

    private static TagDefinition ToDomain(StoredTagDefinition stored) =>
        TagDefinition.Reconstitute(
            new TagDefinitionId(stored.Id),
            stored.Text,
            stored.NormalizedText,
            stored.CreatedByParticipantId,
            stored.IsSuppressed,
            stored.CreatedAt,
            stored.UpdatedAt);
}
