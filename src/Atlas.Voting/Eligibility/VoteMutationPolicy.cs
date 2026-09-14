using Atlas.Voting.Target;
using Atlas.Voting.Votes;

namespace Atlas.Voting.Eligibility
{
    /// <summary>
    /// Enforces the shared eligibility rules for vote mutations.
    /// </summary>
    public sealed class VoteMutationPolicy
    {
        private readonly IVotingEligibility _votingEligibility;

        public VoteMutationPolicy(
            IVotingEligibility votingEligibility)
        {
            _votingEligibility = votingEligibility;
        }

        /// <summary>
        /// Rejects a mutation when its participant is ineligible or its
        /// target does not currently permit interaction.
        /// </summary>
        public void EnsureAllowed(
            VoteTarget target,
            ParticipantId participantId)
        {
            ArgumentNullException.ThrowIfNull(target);
            ArgumentNullException.ThrowIfNull(participantId);

            if (!_votingEligibility.IsEligible(participantId))
            {
                throw new InvalidOperationException(
                    "The acting participant is not eligible to vote.");
            }

            if (!_votingEligibility.IsAvailable(target))
            {
                throw new InvalidOperationException(
                    "The vote target is unavailable for interaction.");
            }
        }
    }
}
