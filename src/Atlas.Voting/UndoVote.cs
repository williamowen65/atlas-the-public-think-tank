using Atlas.Voting.Data;
using Atlas.Voting.Eligibility;
using Atlas.Voting.Target;
using Atlas.Voting.Votes;

namespace Atlas.Voting
{
    /// <summary>
    /// Removes an eligible participant's current vote when the target
    /// still permits interaction.
    /// </summary>
    public sealed class UndoVote
    {
        private readonly IVoteRepository _voteRepository;
        private readonly IVotingParticipantEligibility _participantEligibility;
        private readonly IVoteTargetAvailability _targetAvailability;

        public UndoVote(
            IVoteRepository voteRepository,
            IVotingParticipantEligibility participantEligibility,
            IVoteTargetAvailability targetAvailability)
        {
            _voteRepository = voteRepository;
            _participantEligibility = participantEligibility;
            _targetAvailability = targetAvailability;
        }

        /// <summary>
        /// Removes the participant-target vote when present.
        /// Returns false when the participant has no current vote.
        /// </summary>
        public bool Execute(
            VoteTarget target,
            ParticipantId participantId)
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
                return false;
            }

            _voteRepository.Delete(existingVote.Id);

            return true;
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
