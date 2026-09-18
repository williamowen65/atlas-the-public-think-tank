using Atlas.Voting.Target;
using Atlas.Voting.Votes;

namespace Atlas.Voting.Data
{
    /// <summary>
    /// Provides persistence operations for current vote records.
    /// </summary>
    public interface IVoteRepository
    {
        /// <summary>Loads all current votes for a domain vote target.</summary>
        IReadOnlyCollection<Vote> GetTargetVotes(VoteTarget target);

        /// <summary>Loads the current vote owned by a participant for a target.</summary>
        Vote? GetByParticipantAndTarget(
            ParticipantId participantId,
            VoteTarget voteTarget);

        /// <summary>Loads a domain object by its boundary-owned identifier.</summary>
        Vote? GetById(VoteId id);

        /// <summary>Creates or replaces the current state of a vote.</summary>
        void Save(Vote vote);

        /// <summary>Physically removes a vote from current persistence.</summary>
        void Delete(VoteId id);
    }
}
