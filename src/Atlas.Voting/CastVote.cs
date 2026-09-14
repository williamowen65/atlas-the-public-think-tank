using Atlas.Voting.Data;
using Atlas.Voting.Target;
using Atlas.Voting.Votes;

namespace Atlas.Voting
{
    public sealed class CastVote
    {
        private readonly IVoteRepository _voteRepository;

        public CastVote(IVoteRepository voteRepository)
        {
            _voteRepository = voteRepository;
        }


        public Vote Execute(
            VoteTarget target,
            ParticipantId participantId,
            int voteValue)
        {
            var existingVote =
                _voteRepository.GetByParticipantAndTarget(
                    participantId,
                    target);

            if (existingVote is not null)
            {
                throw new InvalidOperationException(
                    "The participant has already voted on this target.");
            }

            var vote = new Vote(
                target,
                participantId,
                voteValue);

            _voteRepository.Save(vote);

            return vote;
        }
    }
}