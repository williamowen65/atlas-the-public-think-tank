using Atlas.Persistence;
using Atlas.Participants.Participants;

namespace Atlas.ConsoleApp.Storage;

/// <summary>Maps domain objects directly to EF Core rows in SQL Server.</summary>
public sealed class SqlParticipantRepository(Func<AtlasDataContext>? contextFactory = null) : SqlRepository(contextFactory), IParticipantRepository
{
    public IReadOnlyCollection<Participant> GetAll() => ReadRows<ParticipantRow>().Select(ToDomain).ToList();
    public Participant? GetById(ParticipantId id) => FindRow<ParticipantRow>(id.Value) is { } row ? ToDomain(row) : null;
    public void Save(Participant participant)
    {
        ArgumentNullException.ThrowIfNull(participant);
        if (ReadRows<ParticipantRow>().Any(row => row.Id != participant.Id.Value &&
            string.Equals(row.DisplayName, participant.DisplayName, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"A participant named '{participant.DisplayName}' already exists.");
        SaveRow(ToStorage(participant));
    }

    private static ParticipantRow ToStorage(Participant participant)
    {
        return new ParticipantRow
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
    private static Participant ToDomain(ParticipantRow storedParticipant)
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
