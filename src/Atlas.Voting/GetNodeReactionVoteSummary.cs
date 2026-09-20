using Atlas.Voting.Data;
using Atlas.Voting.Target;
using Atlas.Voting.Votes;

namespace Atlas.Voting;

/// <summary>Calculates a node reaction's upvotes-minus-downvotes score.</summary>
public sealed class GetNodeReactionVoteSummary
{
    private readonly IVoteRepository _votes;

    public GetNodeReactionVoteSummary(IVoteRepository votes)
    {
        ArgumentNullException.ThrowIfNull(votes);
        _votes = votes;
    }

    public NodeReactionVoteSummary Execute(
        NodeReactionVoteTarget target,
        ParticipantId? participantId = null)
    {
        ArgumentNullException.ThrowIfNull(target);

        var score = _votes
            .GetTargetVotes(target)
            .Sum(vote => vote.Value.Value);

        var currentParticipantVote = participantId is null
            ? null
            : _votes.GetByParticipantAndTarget(participantId, target)
                ?.Value.Value;

        return new NodeReactionVoteSummary(score, currentParticipantVote);
    }
}
