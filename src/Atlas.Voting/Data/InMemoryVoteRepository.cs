using Atlas.Voting.Target;
using Atlas.Voting.Votes;

namespace Atlas.Voting.Data
{
    public sealed class InMemoryVoteRepository : IVoteRepository
    {
        private readonly List<Vote> _votes = new();

        public void Save(Vote vote)
        {
            _votes.Add(vote);
        }

        public Vote? GetById(VoteId id)
        {
            return _votes.SingleOrDefault(
                vote => vote.Id == id);
        }

       public IReadOnlyCollection<Vote> GetTargetVotes(
       VoteTarget target)
        {
            return _votes
                .Where(vote =>
                    vote.Target.Id == target.Id &&
                    vote.Target.GetType() == target.GetType())
                .ToArray();
        }

        public Vote? GetByParticipantAndTarget(
            ParticipantId participantId,
            VoteTarget voteTarget)
        {
            return _votes.SingleOrDefault(vote =>
                vote.ParticipantId.Id == participantId.Id &&
                vote.Target.Id == voteTarget.Id &&
                vote.Target.GetType() == voteTarget.GetType());
        }
    }
}
