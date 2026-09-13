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

            Id = VoteId.New();
            ParticipantId = pId;
            CreatedAt = DateTimeOffset.UtcNow;
            UpdatedAt = CreatedAt;
            Target = target;


        }

        private Vote(
            VoteId id,
            VoteTarget target,
            ParticipantId pId,
            int voteValue,
            DateTimeOffset createdAt,
            DateTimeOffset updatedAt
            )
        {
            if (updatedAt < createdAt)
            {
                throw new ArgumentException(
                    "Updated time cannot precede created time.");
            }
            Id = id;
            Target = target;
            ParticipantId = pId;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
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

        public static Vote Reconstitute(
            VoteId id,
            VoteTarget target,
            ParticipantId pId,
            int voteValue,
            DateTimeOffset createdAt,
            DateTimeOffset updatedAt
            ) 
        {
            return new Vote(
                id,
                target,
                pId,
                voteValue,
                createdAt,
                updatedAt
                );
        }


    }


}
