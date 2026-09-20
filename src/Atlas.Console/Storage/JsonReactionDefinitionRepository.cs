using System.Text.Json;
using Atlas.Graph.Reactions;

namespace Atlas.ConsoleApp.Storage;

/// <summary>Persists reusable Graph reaction definitions in a dedicated JSON file.</summary>
public sealed class JsonReactionDefinitionRepository : IReactionDefinitionRepository
{
    private readonly string _filePath;
    private readonly object _gate = new();
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    /// <summary>Initializes the adapter for its dedicated data file.</summary>
    public JsonReactionDefinitionRepository(string filePath)
    {
        _filePath = filePath;
    }

    /// <summary>Loads all reusable definitions.</summary>
    public IReadOnlyCollection<ReactionDefinition> GetAll()
    {
        lock (_gate)
        {
            return ReadStored().Select(ToDomain).ToList();
        }
    }

    /// <summary>Loads a definition by identifier.</summary>
    public ReactionDefinition? GetById(ReactionDefinitionId id)
    {
        lock (_gate)
        {
            var stored = ReadStored().SingleOrDefault(item => item.Id == id.Value);
            return stored is null ? null : ToDomain(stored);
        }
    }

    /// <summary>Loads the unique definition matching normalized text.</summary>
    public ReactionDefinition? GetByNormalizedText(string normalizedText)
    {
        lock (_gate)
        {
            var stored = ReadStored().SingleOrDefault(item =>
                string.Equals(item.NormalizedText, normalizedText, StringComparison.Ordinal));
            return stored is null ? null : ToDomain(stored);
        }
    }

    /// <summary>Saves a definition while enforcing normalized uniqueness within this adapter.</summary>
    public void Save(ReactionDefinition definition)
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
                    $"The reaction '{duplicate.Text}' already exists.");
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

    private List<StoredReactionDefinition> ReadStored()
    {
        if (!File.Exists(_filePath))
        {
            return [];
        }

        var json = File.ReadAllText(_filePath);
        return string.IsNullOrWhiteSpace(json)
            ? []
            : JsonSerializer.Deserialize<List<StoredReactionDefinition>>(json, _jsonOptions) ?? [];
    }

    private void WriteStored(List<StoredReactionDefinition> definitions)
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

    private static StoredReactionDefinition ToStorage(ReactionDefinition definition) => new()
    {
        Id = definition.Id.Value,
        Text = definition.Text,
        Emoji = definition.Emoji,
        Description = definition.Description,
        NormalizedText = definition.NormalizedText,
        CreatedByParticipantId = definition.CreatedByParticipantId,
        IsSuppressed = definition.IsSuppressed,
        CreatedAt = definition.CreatedAt,
        UpdatedAt = definition.UpdatedAt
    };

    private static ReactionDefinition ToDomain(StoredReactionDefinition stored) =>
        ReactionDefinition.Reconstitute(
            new ReactionDefinitionId(stored.Id),
            stored.Text,
            stored.Emoji,
            stored.Description,
            stored.NormalizedText,
            stored.CreatedByParticipantId,
            stored.IsSuppressed,
            stored.CreatedAt,
            stored.UpdatedAt);
}
