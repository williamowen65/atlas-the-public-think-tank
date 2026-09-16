using Atlas.Voting.Data;
using Atlas.Voting.Eligibility;
using Atlas.Voting.Target;
using Atlas.Voting.Votes;

namespace Atlas.Voting
{
    /// <summary>
    /// Creates or changes a current vote after checking actor eligibility
    /// and target availability through Voting-owned ports.
    /// </summary>
    public sealed class CastVote
    {
        private readonly IVoteRepository _voteRepository;
        private readonly VoteMutationPolicy _mutationPolicy;

        public CastVote(
            IVoteRepository voteRepository,
            VoteMutationPolicy mutationPolicy)
        {
            _voteRepository = voteRepository;
            _mutationPolicy = mutationPolicy;
        }

        /// <summary>
        /// Validates whether the mutation is allowed and delegates the complete
        /// participant-target mutation to the repository's atomic operation.
        /// </summary>
        public Vote Execute(
            VoteTarget target,
            ParticipantId participantId,
            int voteValue)
        {
            _mutationPolicy.EnsureAllowed(
                target,
                participantId);

            return _voteRepository.SetCurrentVote(
                target,
                participantId,
                voteValue);
        }
    }
}
