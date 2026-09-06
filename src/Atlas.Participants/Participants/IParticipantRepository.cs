namespace Atlas.Participants.Participants;

public interface IParticipantRepository
{
    IReadOnlyCollection<Participant> GetAll();
    Participant? GetById(ParticipantId id);
    void Save(Participant participant);
}
