using Atlas.Voting.Data;
using Atlas.Voting.Eligibility;
using Atlas.Voting.Target;
using Atlas.Voting.Votes;

namespace Atlas.Voting
{
    /// <summary>
    /// Creates or changes a current vote after checking actor eligibility
    /// and target availability through Voting-owned ports.
    /// </summary>
    public sealed class CastVote
    {
        private readonly IVoteRepository _voteRepository;
        private readonly VoteMutationPolicy _mutationPolicy;

        public CastVote(
            IVoteRepository voteRepository,
            VoteMutationPolicy mutationPolicy)
        {
            _voteRepository = voteRepository;
            _mutationPolicy = mutationPolicy;
        }

        public Vote Execute(
            VoteTarget target,
            ParticipantId participantId,
            int voteValue)
        {
            return OperationBoundary.Execute(_voteRepository, () =>
            {
                var now = AtlasTime.UtcNow;
                _mutationPolicy.EnsureAllowed(
                    target,
                    participantId);

                var existingVote =
                    _voteRepository.GetByParticipantAndTarget(
                        participantId,
                        target);

                if (existingVote is null)
                {
                    var newVote = new Vote(
                        target,
                        participantId,
                        voteValue, now);

                    _voteRepository.Save(newVote);

                    return newVote;
                }

                var changedAt = now;

                if (changedAt <= existingVote.UpdatedAt)
                {
                    changedAt = existingVote.UpdatedAt.AddTicks(1);
                }

                existingVote.ChangeValue(
                    voteValue,
                    changedAt);

                _voteRepository.Save(existingVote);

                return existingVote;
            });
        }

    }
}
