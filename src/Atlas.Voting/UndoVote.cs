using Atlas.Voting.Data;
using Atlas.Voting.Eligibility;
using Atlas.Voting.Target;
using Atlas.Voting.Votes;

namespace Atlas.Voting
{
    /// <summary>
    /// Removes an eligible participant's current vote when the target
    /// still permits interaction.
    /// </summary>
    public sealed class UndoVote
    {
        private readonly IVoteRepository _voteRepository;
        private readonly VoteMutationPolicy _mutationPolicy;

        public UndoVote(
            IVoteRepository voteRepository,
            VoteMutationPolicy mutationPolicy)
        {
            _voteRepository = voteRepository;
            _mutationPolicy = mutationPolicy;
        }

        /// <summary>
        /// Removes the participant-target vote when present.
        /// Returns false when the participant has no current vote.
        /// </summary>
        public bool Execute(
            VoteTarget target,
            ParticipantId participantId)
        {
            _mutationPolicy.EnsureAllowed(
                target,
                participantId);

            var existingVote =
                _voteRepository.GetByParticipantAndTarget(
                    participantId,
                    target);

            if (existingVote is null)
            {
                return false;
            }

            _voteRepository.Delete(existingVote.Id);

            return true;
        }

    }
}
