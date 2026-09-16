using Atlas.Voting.Target;
using Atlas.Voting.Votes;

namespace Atlas.Voting.Data
{
    /// <summary>
    /// Provides persistence operations for current vote records.
    /// </summary>
    public interface IVoteRepository
    {
        /// <summary>
        /// Atomically creates or changes the one current vote owned by a
        /// participant for a target.
        /// </summary>
        /// <remarks>
        /// Implementations must treat participant ID, target type, and target
        /// ID as one uniqueness key for the complete read-modify-write
        /// operation.
        /// </remarks>
        Vote SetCurrentVote(
            VoteTarget target,
            ParticipantId participantId,
            int voteValue);

        /// <summary>Loads all current votes for a domain vote target.</summary>
        IReadOnlyCollection<Vote> GetTargetVotes(VoteTarget target);

        /// <summary>Loads the current vote owned by a participant for a target.</summary>
        Vote? GetByParticipantAndTarget(
            ParticipantId participantId,
            VoteTarget voteTarget);

        /// <summary>Loads a domain object by its boundary-owned identifier.</summary>
        Vote? GetById(VoteId id);

        /// <summary>
        /// Creates or replaces a record by VoteId for persistence support.
        /// CastVote uses SetCurrentVote so participant-target uniqueness is
        /// enforced atomically.
        /// </summary>
        void Save(Vote vote);

        /// <summary>Physically removes a vote from current persistence.</summary>
        void Delete(VoteId id);
    }
}
