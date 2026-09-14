using Atlas.Voting.Votes;

namespace Atlas.Voting.Eligibility
{
    /// <summary>
    /// Supplies Voting with the target-independent eligibility of an
    /// authenticated participant reference.
    /// </summary>
    public interface IVotingParticipantEligibility
    {
        /// <summary>
        /// Returns true when the host-supplied participant may mutate votes.
        /// </summary>
        bool IsEligible(ParticipantId participantId);
    }
}
