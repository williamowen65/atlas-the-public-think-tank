using Atlas.Voting.Data;
using Atlas.Voting.Target;
using Atlas.Voting.Votes;

namespace Atlas.Voting
{
    /// <summary>
    /// Creates a participant-target vote or changes its current value.
    /// </summary>
    public sealed class CastVote
    {
        private readonly IVoteRepository _voteRepository;

        public CastVote(IVoteRepository voteRepository)
        {
            _voteRepository = voteRepository;
        }

        public Vote Execute(
            VoteTarget target,
            ParticipantId participantId,
            int voteValue)
        {
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
    }
}
