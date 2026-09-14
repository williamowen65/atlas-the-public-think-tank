using Atlas.Voting.Data;
using Atlas.Voting.Target;
using Atlas.Voting.Votes;

namespace Atlas.Voting
{
    /// <summary>
    /// Removes the acting participant's current vote for a target.
    /// </summary>
    public sealed class UndoVote
    {
        private readonly IVoteRepository _voteRepository;

        public UndoVote(IVoteRepository voteRepository)
        {
            _voteRepository = voteRepository;
        }

        /// <summary>
        /// Removes the participant-target vote when present.
        /// Returns false when the participant has no current vote.
        /// </summary>
        public bool Execute(
            VoteTarget target,
            ParticipantId participantId)
        {
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
