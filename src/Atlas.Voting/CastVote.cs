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
        private readonly IVotingParticipantEligibility _participantEligibility;
        private readonly IVoteTargetAvailability _targetAvailability;

        public CastVote(
            IVoteRepository voteRepository,
            IVotingParticipantEligibility participantEligibility,
            IVoteTargetAvailability targetAvailability)
        {
            _voteRepository = voteRepository;
            _participantEligibility = participantEligibility;
            _targetAvailability = targetAvailability;
        }

        public Vote Execute(
            VoteTarget target,
            ParticipantId participantId,
            int voteValue)
        {
            ArgumentNullException.ThrowIfNull(target);
            ArgumentNullException.ThrowIfNull(participantId);

            EnsureMutationIsAllowed(
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
                    voteValue);

                _voteRepository.Save(newVote);

                return newVote;
            }

            var changedAt = DateTimeOffset.UtcNow;

            if (changedAt <= existingVote.UpdatedAt)
            {
                changedAt = existingVote.UpdatedAt.AddTicks(1);
            }

            existingVote.ChangeValue(
                voteValue,
                changedAt);

            _voteRepository.Save(existingVote);

            return existingVote;
        }

        private void EnsureMutationIsAllowed(
            VoteTarget target,
            ParticipantId participantId)
        {
            if (!_participantEligibility.IsEligible(participantId))
            {
                throw new InvalidOperationException(
                    "The acting participant is not eligible to vote.");
            }

            if (!_targetAvailability.IsAvailable(target))
            {
                throw new InvalidOperationException(
                    "The vote target is unavailable for interaction.");
            }
        }
    }
}
