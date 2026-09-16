using Atlas.Voting.Target;
using Atlas.Voting.Votes;

namespace Atlas.Voting.Data
{
    /// <summary>
    /// Stores current votes in memory for tests and local workflows.
    /// </summary>
    public sealed class InMemoryVoteRepository : IVoteRepository
    {
        private readonly object _gate = new();
        private readonly List<Vote> _votes = [];

        /// <summary>
        /// Atomically creates or changes one participant-target vote while
        /// preserving its identity and creation time.
        /// </summary>
        public Vote SetCurrentVote(
            VoteTarget target,
            ParticipantId participantId,
            int voteValue)
        {
            lock (_gate)
            {
                var existingVote =
                    FindByParticipantAndTarget(
                        participantId,
                        target);

                if (existingVote is null)
                {
                    var newVote = new Vote(
                        target,
                        participantId,
                        voteValue);

                    _votes.Add(newVote);

                    return newVote;
                }

                existingVote.ChangeValue(
                    voteValue,
                    NextChangedAt(existingVote));

                return existingVote;
            }
        }

        public void Save(Vote vote)
        {
            ArgumentNullException.ThrowIfNull(vote);

            lock (_gate)
            {
                var existingIndex = _votes.FindIndex(
                    existingVote => existingVote.Id == vote.Id);

                if (existingIndex >= 0)
                {
                    _votes[existingIndex] = vote;
                }
                else
                {
                    _votes.Add(vote);
                }
            }
        }

        public void Delete(VoteId id)
        {
            lock (_gate)
            {
                _votes.RemoveAll(
                    vote => vote.Id == id);
            }
        }

        public Vote? GetById(VoteId id)
        {
            lock (_gate)
            {
                return _votes.SingleOrDefault(
                    vote => vote.Id == id);
            }
        }

        public IReadOnlyCollection<Vote> GetTargetVotes(
            VoteTarget target)
        {
            lock (_gate)
            {
                return _votes
                    .Where(vote =>
                        SameTarget(
                            vote.Target,
                            target))
                    .ToArray();
            }
        }

        public Vote? GetByParticipantAndTarget(
            ParticipantId participantId,
            VoteTarget voteTarget)
        {
            lock (_gate)
            {
                return FindByParticipantAndTarget(
                    participantId,
                    voteTarget);
            }
        }

        private Vote? FindByParticipantAndTarget(
            ParticipantId participantId,
            VoteTarget voteTarget)
        {
            return _votes.SingleOrDefault(vote =>
                vote.ParticipantId.Id == participantId.Id &&
                SameTarget(
                    vote.Target,
                    voteTarget));
        }

        private static bool SameTarget(
            VoteTarget first,
            VoteTarget second)
        {
            return first.Id == second.Id &&
                   first.GetType() == second.GetType();
        }

        private static DateTimeOffset NextChangedAt(
            Vote existingVote)
        {
            var changedAt = DateTimeOffset.UtcNow;

            return changedAt > existingVote.UpdatedAt
                ? changedAt
                : existingVote.UpdatedAt.AddTicks(1);
        }
    }
}
