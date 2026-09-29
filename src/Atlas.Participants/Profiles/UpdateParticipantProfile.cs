using Atlas.Participants.Participants;

namespace Atlas.Participants.Profiles;

/// <summary>Authorizes and coordinates the participant self-edit workflow.</summary>
public sealed class UpdateParticipantProfile
{
    private readonly IParticipantRepository _participants;

    /// <summary>Creates a validated update participant profile instance.</summary>
    public UpdateParticipantProfile(
        IParticipantRepository participants)
    {
        _participants = participants;
    }

    /// <summary>Authorizes and executes the participant profile update use case.</summary>
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
