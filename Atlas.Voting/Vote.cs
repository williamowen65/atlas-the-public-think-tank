using Atlas.Voting.Votes;
using Atlas.Voting.Votes.Value;
namespace Atlas.Voting
{
    public class Vote
    {
        public VoteId Id { get; }

        public IVoteValue Value { get; }

        public VoteTarget Target { get; private set; }

        public ParticipantId ParticipantId { get; }

        public DateTimeOffset CreatedAt { get; }
        public DateTimeOffset UpdatedAt { get; private set; }

        public Vote(
            VoteTarget target, 
            ParticipantId pId,
            int voteValue
        )
        {
            Id = VoteId.New();
            ParticipantId = pId;
            CreatedAt = DateTimeOffset.UtcNow;
            UpdatedAt = CreatedAt;
            Target = target;


            // if ( target is a Node)
                Value = new NodeRating(voteValue);
            // else if target is a NodeTag
                // Value = new NodeTagRating(voteValue)
        }



    }


}
