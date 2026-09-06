using System.Text.Json;
using Atlas.Participants.Participants;

namespace Atlas.ConsoleApp.Storage;

public sealed class JsonParticipantRepository : IParticipantRepository
{
    private readonly string _filePath;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    public JsonParticipantRepository(string filePath)
    {
        _filePath = filePath;
    }

    public IReadOnlyCollection<Participant> GetAll()
    {
        return ReadStoredParticipants()
            .Select(ToDomain)
            .ToList();
    }

    public Participant? GetById(ParticipantId id)
    {
        var storedParticipant = ReadStoredParticipants()
            .SingleOrDefault(participant => participant.Id == id.Value);

        return storedParticipant is null
            ? null
            : ToDomain(storedParticipant);
    }

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

    private static StoredParticipant ToStorage(Participant participant)
    {
        return new StoredParticipant
        {
            Id = participant.Id.Value,
            DisplayName = participant.DisplayName,
            IsActive = participant.IsActive,
            CreatedAt = participant.CreatedAt,
            UpdatedAt = participant.UpdatedAt
        };
    }

    private static Participant ToDomain(StoredParticipant storedParticipant)
    {
        return Participant.Reconstitute(
            new ParticipantId(storedParticipant.Id),
            storedParticipant.DisplayName,
            storedParticipant.IsActive,
            storedParticipant.CreatedAt,
            storedParticipant.UpdatedAt);
    }
}
