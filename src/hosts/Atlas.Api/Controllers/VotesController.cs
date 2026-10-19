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

[ApiController, Authorize(Policy = AtlasPolicies.Member), Route("api")]
public sealed class VotesController(CastVote cast, UndoVote undo, ReactionVotingService reactions, INodeRepository nodes, RequestActor actor) : ControllerBase
{
    [HttpPut("nodes/{node:guid}/vote")]
    public IActionResult Vote(Guid node, VoteInput input) { cast.Execute(new NodeVoteTarget(node), new Voter(actor.ParticipantId), input.Value); return NoContent(); }
    [HttpDelete("nodes/{node:guid}/vote")]
    public IActionResult Undo(Guid node) { undo.Execute(new NodeVoteTarget(node), new Voter(actor.ParticipantId)); return NoContent(); }
    [HttpPut("nodes/{node:guid}/reactions/vote")]
    public IActionResult Reaction(Guid node, ReactionVoteInput input)
    { reactions.Vote(nodes.GetById(new NodeId(node)) ?? throw new KeyNotFoundException(), input.Text, actor.ParticipantId, input.Value); return NoContent(); }
    [HttpDelete("reactions/{association:guid}/vote")]
    public IActionResult UndoReaction(Guid association) { undo.Execute(new NodeReactionVoteTarget(association), new Voter(actor.ParticipantId)); return NoContent(); }
}
public sealed record VoteInput(int Value);
public sealed record ReactionVoteInput(string Text, int Value);
