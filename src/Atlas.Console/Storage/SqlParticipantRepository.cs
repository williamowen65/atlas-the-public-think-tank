using Atlas.Persistence;
using Atlas.Participants.Participants;

namespace Atlas.ConsoleApp.Storage;

/// <summary>Maps domain objects directly to EF Core rows in SQL Server.</summary>
public sealed class SqlParticipantRepository(Func<AtlasDataContext>? contextFactory = null) : SqlRepository(contextFactory), IParticipantRepository
{

    // Read profile rows and reconstitute saved Participants; a missing ID returns null.
    public IReadOnlyCollection<Participant> GetAll() => ReadRows<ParticipantRow>().Select(ToDomain).ToList();
    public Participant? GetById(ParticipantId id) => FindRow<ParticipantRow>(id.Value) is { } row ? ToDomain(row) : null;
    public void Save(Participant participant)
    {
        ArgumentNullException.ThrowIfNull(participant);

        // Reject a case-insensitive display-name collision with another Participant before saving.
        if (ReadRows<ParticipantRow>().Any(row => row.Id != participant.Id.Value &&
            string.Equals(
                row.DisplayName,
                participant.DisplayName,
                StringComparison.OrdinalIgnoreCase
            )))
            throw new InvalidOperationException($"A participant named '{participant.DisplayName}' already exists.");

        // Insert or update this profile. SaveRow starts a transaction or joins the caller's operation.
        SaveRow(ToStorage(participant));
    }


    // Copy profile fields, active state and timestamps into the persistence representation.
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

        // Rebuild and validate the saved profile without assigning a new identity or creation time.
        return Participant.Reconstitute(
            new ParticipantId(storedParticipant.Id),
            storedParticipant.DisplayName,
            storedParticipant.Bio,
            storedParticipant.IsActive,
            storedParticipant.CreatedAt,
            storedParticipant.UpdatedAt
        );
    }
}
