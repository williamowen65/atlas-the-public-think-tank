namespace Atlas.Participants.Participants;

/// <summary>Defines the persistence operations required by the Participants boundary.</summary>
public interface IParticipantRepository
{
    /// <summary>Loads all persisted domain objects.</summary>
    IReadOnlyCollection<Participant> GetAll();
    /// <summary>Loads a domain object by its boundary-owned identifier.</summary>
    Participant? GetById(ParticipantId id);
    /// <summary>Persists the current domain-object state.</summary>
    void Save(Participant participant);
}
