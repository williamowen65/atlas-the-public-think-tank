using Atlas.Application;
using Atlas.Graph.Nodes;
using Atlas.Identity;
using Atlas.Voting;
using Atlas.Voting.Data;
using Atlas.Voting.Target;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Voter = Atlas.Voting.Votes.ParticipantId;

namespace Atlas.Api.Controllers;

/// <summary>Use the verified request actor for cast/change/undo; Voting owns eligibility and quotas.</summary>
// Explicit controller policy is visible to CodeQL and reviewers. GET/HEAD/OPTIONS
// remain readable; writes require the cookie/header token pair.
[AutoValidateAntiforgeryToken]
[ApiController, Authorize(Policy = AtlasPolicies.Member), Route("api")]
public sealed class VotesController(
    CastVote cast,
    UndoVote undo,
    ReactionVotingService reactions,
    INodeRepository nodes,
    RequestActor actor
) : ControllerBase
{
    /// <summary>Cast or change a node rating through Voting’s eligibility and action-quota boundary.</summary>
    [HttpPut("nodes/{node:guid}/vote")]
    public IActionResult Vote(Guid node, VoteInput input)
    {
        cast.Execute(
            new NodeVoteTarget(node),
            new Voter(actor.ParticipantId),
            input.Value
        );
        return NoContent();
    }

    /// <summary>Undo the current rating; this action does not refund previously consumed quota.</summary>
    [HttpDelete("nodes/{node:guid}/vote")]
    public IActionResult Undo(Guid node)
    {
        undo.Execute(
            new NodeVoteTarget(node),
            new Voter(actor.ParticipantId)
        );
        return NoContent();
    }

    /// <summary>Cast/change a comment vote using shared eligibility, quotas and participant locking.</summary>
    [HttpPut("comments/{id:guid}/vote")]
    public IActionResult Comment(Guid id, VoteInput input)
    {
        cast.Execute(new CommentVoteTarget(id), new Voter(actor.ParticipantId), input.Value);
        return NoContent();
    }

    [HttpDelete("comments/{id:guid}/vote")]
    public IActionResult UndoComment(Guid id)
    {
        undo.Execute(new CommentVoteTarget(id), new Voter(actor.ParticipantId));
        return NoContent();
    }

    /// <summary>Coordinate applying a reaction and casting/changing its vote atomically.</summary>
    [HttpPut("nodes/{node:guid}/reactions/vote")]
    public IActionResult Reaction(Guid node, ReactionVoteInput input)
    {
        reactions.Vote(
            nodes.GetById(new NodeId(node)) ?? throw new KeyNotFoundException(),
            input.Text,
            actor.ParticipantId,
            input.Value
        );
        return NoContent();
    }

    /// <summary>Undo the actor’s vote on a reaction association through Voting.</summary>
    [HttpDelete("reactions/{association:guid}/vote")]
    public IActionResult UndoReaction(Guid association)
    {
        undo.Execute(
            new NodeReactionVoteTarget(association),
            new Voter(actor.ParticipantId)
        );
        return NoContent();
    }
}

public sealed record VoteInput(int Value);

public sealed record ReactionVoteInput(string Text, int Value);
