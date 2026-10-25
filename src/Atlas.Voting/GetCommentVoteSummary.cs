using Atlas.Voting.Data;
using Atlas.Voting.Target;
using Atlas.Voting.Votes;

namespace Atlas.Voting;

/// <summary>Calculates a comment's upvotes-minus-downvotes score.</summary>
public sealed class GetCommentVoteSummary
{
    private readonly IVoteRepository _votes;

    public GetCommentVoteSummary(IVoteRepository votes)
    {
        ArgumentNullException.ThrowIfNull(votes);
        _votes = votes;
    }

    public CommentVoteSummary Execute(
        CommentVoteTarget target,
        ParticipantId? participantId = null
    )
    {
        ArgumentNullException.ThrowIfNull(target);

        var targetVotes = _votes.GetTargetVotes(target);
        var score = targetVotes.Sum(vote => vote.Value.Value);

        var currentParticipantVote = participantId is null ? (int?)null
            : targetVotes.SingleOrDefault(vote => vote.ParticipantId.Id == participantId.Id)?.Value.Value;

        return new CommentVoteSummary(
            score,
            targetVotes.Count,
            currentParticipantVote
        );
    }
}
