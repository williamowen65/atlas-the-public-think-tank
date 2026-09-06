using Atlas.Participants.Participants;

namespace Atlas.Participants.Profiles;

public sealed class UpdateParticipantProfile
{
    private readonly IParticipantRepository _participants;

    public UpdateParticipantProfile(
        IParticipantRepository participants)
    {
        _participants = participants;
    }

    public Participant Execute(
        ParticipantId actorId,
        ParticipantId profileId,
        string displayName,
        string bio,
        DateTimeOffset changedAt)
    {
        if (actorId != profileId)
        {
            throw new UnauthorizedAccessException(
                "Participants may only edit their own profiles.");
        }

        var participant = _participants.GetById(profileId)
            ?? throw new InvalidOperationException(
                "The participant profile was not found.");

        participant.UpdateProfile(
            displayName,
            bio,
            changedAt);

        _participants.Save(participant);
        return participant;
    }
}
