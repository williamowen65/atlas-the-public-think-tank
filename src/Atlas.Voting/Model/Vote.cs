using Atlas.Voting.Target;
using Atlas.Voting.Value;
using Atlas.Voting.Votes;

namespace Atlas.Voting
{
    /// <summary>
    /// Represents one participant's current vote for one target.
    /// </summary>
    public class Vote
    {
        public VoteId Id { get; }

        public IVoteValue Value { get; private set; }

        public VoteTarget Target { get; }

        public ParticipantId ParticipantId { get; }

        public DateTimeOffset CreatedAt { get; }

        public DateTimeOffset UpdatedAt { get; private set; }

        public Vote(
            VoteTarget target,
            ParticipantId pId,
            int voteValue)
        {
            Value = CreateValue(target, voteValue);
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
            DateTimeOffset updatedAt)
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
            Value = CreateValue(target, voteValue);
        }

        /// <summary>
        /// Replaces the current value while preserving vote identity
        /// and creation time.
        /// </summary>
        public void ChangeValue(
            int voteValue,
            DateTimeOffset changedAt)
        {
            if (changedAt <= UpdatedAt)
            {
                throw new ArgumentException(
                    "Change time must be later than the current update time.",
                    nameof(changedAt));
            }

            var replacement = CreateValue(
                Target,
                voteValue);

            Value = replacement;
            UpdatedAt = changedAt;
        }

        /// <summary>
        /// Reconstitutes a persisted vote without generating new identity
        /// or timestamps.
        /// </summary>
        public static Vote Reconstitute(
            VoteId id,
            VoteTarget target,
            ParticipantId pId,
            int voteValue,
            DateTimeOffset createdAt,
            DateTimeOffset updatedAt)
        {
            return new Vote(
                id,
                target,
                pId,
                voteValue,
                createdAt,
                updatedAt);
        }

        private static IVoteValue CreateValue(
            VoteTarget target,
            int voteValue)
        {
            return target switch
            {
                NodeVoteTarget => new NodeRating(voteValue),
                NodeTagVoteTarget => new NodeTagVote(voteValue),

                _ => throw new ArgumentException(
                    "Unsupported vote target type.",
                    nameof(target))
            };
        }
    }
}
