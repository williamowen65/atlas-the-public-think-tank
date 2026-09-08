using System.Text.Json;
using Atlas.Participants.Participants;

namespace Atlas.ConsoleApp.Storage;

/// <summary>Persists participant profiles as JSON and reconstitutes them as domain objects.</summary>
public sealed class JsonParticipantRepository : IParticipantRepository
{
    private readonly string _filePath;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    /// <summary>Initializes the JSON adapter and ensures its backing file is available.</summary>
    public JsonParticipantRepository(string filePath)
    {
        _filePath = filePath;
    }

    /// <summary>Loads all persisted domain objects.</summary>
    public IReadOnlyCollection<Participant> GetAll()
    {
        return ReadStoredParticipants()
            .Select(ToDomain)
            .ToList();
    }

    /// <summary>Loads a domain object by its boundary-owned identifier.</summary>
    public Participant? GetById(ParticipantId id)
    {
        var storedParticipant = ReadStoredParticipants()
            .SingleOrDefault(participant => participant.Id == id.Value);

        return storedParticipant is null
            ? null
            : ToDomain(storedParticipant);
    }

    /// <summary>Persists the current domain-object state.</summary>
    public void Save(Participant participant)
    {
        ArgumentNullException.ThrowIfNull(participant);

        var storedParticipants = ReadStoredParticipants();

        var duplicateName = storedParticipants.Any(existing =>
            existing.Id != participant.Id.Value &&
            string.Equals(
                existing.DisplayName,
                participant.DisplayName,
                StringComparison.OrdinalIgnoreCase));

        if (duplicateName)
        {
            throw new InvalidOperationException(
                $"A participant named '{participant.DisplayName}' already exists.");
        }

        var existingIndex = storedParticipants.FindIndex(
            existing => existing.Id == participant.Id.Value);

        var replacement = ToStorage(participant);

        if (existingIndex >= 0)
        {
            storedParticipants[existingIndex] = replacement;
        }
        else
        {
            storedParticipants.Add(replacement);
        }

        WriteStoredParticipants(storedParticipants);
    }

    /// <summary>Reads participant persistence records from JSON.</summary>
    private List<StoredParticipant> ReadStoredParticipants()
    {
        if (!File.Exists(_filePath))
        {
            return [];
        }

        var json = File.ReadAllText(_filePath);

        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        return JsonSerializer.Deserialize<List<StoredParticipant>>(
                   json,
                   _jsonOptions)
               ?? [];
    }

    /// <summary>Writes participant persistence records to JSON.</summary>
    private void WriteStoredParticipants(
        List<StoredParticipant> participants)
    {
        var directory = Path.GetDirectoryName(_filePath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(participants, _jsonOptions);
        File.WriteAllText(_filePath, json);
    }

    /// <summary>Maps a domain object to its data-only persistence representation.</summary>
    private static StoredParticipant ToStorage(Participant participant)
    {
        return new StoredParticipant
        {
            Id = participant.Id.Value,
            DisplayName = participant.DisplayName,
            Bio = participant.Bio,
            IsActive = participant.IsActive,
            CreatedAt = participant.CreatedAt,
            UpdatedAt = participant.UpdatedAt
        };
    }

    /// <summary>Reconstitutes a domain object from its data-only persistence representation.</summary>
    private static Participant ToDomain(StoredParticipant storedParticipant)
    {
        return Participant.Reconstitute(
            new ParticipantId(storedParticipant.Id),
            storedParticipant.DisplayName,
            storedParticipant.Bio,
            storedParticipant.IsActive,
            storedParticipant.CreatedAt,
            storedParticipant.UpdatedAt);
    }
}
