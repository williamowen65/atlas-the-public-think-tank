

using Atlas.Voting.Target;
using Atlas.Voting.Votes;

namespace Atlas.Voting.Data
{
    public interface IVoteRepository
    {

        /// <summary>Loads all persisted votes per domain vote target</summary>
        IReadOnlyCollection<Vote> GetTargetVotes(VoteTarget target);

        Vote? GetByParticipantAndTarget(
            ParticipantId participantId,
            VoteTarget voteTarget
         );

        /// <summary>Loads a domain object by its boundary-owned identifier.</summary>
        Vote? GetById(VoteId id);

        /// <summary>Persists the current domain-object state.</summary>
        void Save(Vote vote);
    }
}
