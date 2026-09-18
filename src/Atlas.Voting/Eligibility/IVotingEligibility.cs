namespace Atlas.Voting.Eligibility
{
    /// <summary>
    /// Supplies Voting with the participant eligibility and target
    /// availability required to authorize vote mutations.
    /// </summary>
    public interface IVotingEligibility :
        IVotingParticipantEligibility,
        IVoteTargetAvailability
    {
    }
}
