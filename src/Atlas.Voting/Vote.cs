using Atlas.Voting.Votes;
using Atlas.Voting.Value;
using Atlas.Voting.Target;

namespace Atlas.Voting
{
    public class Vote
    {
        public VoteId Id { get; }

        public IVoteValue Value { get; }

        public VoteTarget Target { get; }

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


            if (target is NodeVoteTarget) 
            {
                Value = new NodeRating(voteValue);
            }
            else
            {
                throw new ArgumentException(
                    "Unsupported vote target type.",
                    nameof(target));
            }
        }



    }


}
