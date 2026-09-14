using Atlas.Voting.Data;
using Atlas.Voting.Target;
using Atlas.Voting.Votes;

namespace Atlas.Voting;

/// <summary>
/// Calculates the current voting summary for one target.
/// </summary>
public sealed class GetVoteSummary
{
    private readonly IVoteRepository _votes;

    public GetVoteSummary(IVoteRepository votes)
    {
        ArgumentNullException.ThrowIfNull(votes);
        _votes = votes;
    }

    /// <summary>
    /// Returns the target's vote count and arithmetic mean, together with
    /// the selected participant's current vote when a participant is supplied.
    /// </summary>
    public VoteSummary Execute(
        VoteTarget target,
        ParticipantId? participantId = null)
    {
        ArgumentNullException.ThrowIfNull(target);

        var targetVotes =
            _votes.GetTargetVotes(target);

        var averageVote = targetVotes.Count == 0
            ? (double?)null
            : targetVotes.Average(
                vote => vote.Value.Value);

        var currentParticipantVote = participantId is null
            ? null
            : _votes.GetByParticipantAndTarget(
                    participantId,
                    target)
                ?.Value.Value;

        return new VoteSummary(
            targetVotes.Count,
            averageVote,
            currentParticipantVote);
    }
}
