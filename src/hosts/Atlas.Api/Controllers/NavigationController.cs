using Atlas.Application;
using Atlas.Application.Queries;
using Atlas.Graph.Nodes;
using Atlas.Graph.Reactions;
using Atlas.Moderation;
using Atlas.Voting;
using Atlas.Voting.Data;
using Atlas.Voting.Target;
using Microsoft.AspNetCore.Mvc;
namespace Atlas.Api.Controllers;

/// <summary>Project Graph navigation and voting summaries without rendering frontend controls.</summary>
[ApiController, Route("api/nodes/{node:guid}")]
public sealed class NavigationController(INodeRepository nodes, ModerationService moderation, IVoteRepository votes,
    IReactionDefinitionRepository definitions, INodeReactionRepository reactions, RequestActor actor) : ControllerBase
{
    [HttpGet("children")]
    public object Children(Guid node, int offset = 0, int size = 20)
    {
        Read(node);
        if (offset < 0 || size < 1 || size > 50) throw new ArgumentException();
        return nodes.GetChildren(new NodeId(node)).OrderBy(x => x.CreatedAt).ThenBy(x => x.Id.Value)
            .Skip(offset).Take(size).Select(x => new { id = x.Id.Value,
                title = moderation.Visibility(x.Id.Value).IsHidden ? moderation.Visibility(x.Id.Value).Notice : x.Title.Value });
    }
    [HttpGet("ancestors")]
    public object Ancestors(Guid node, Guid? firstParent)
    {
        var segment = AncestorNavigation.ReadSegment(Read(node), nodes, [], firstParent is {} id ? new NodeId(id) : null);
        return new { items = segment.Items.Select(x => new { id = x.Id.Value,
                title = moderation.Visibility(x.Id.Value).IsHidden ? moderation.Visibility(x.Id.Value).Notice : x.Title.Value }),
            segment.HasMore, segment.HasAlternativeRoutes, segment.CycleDetected };
    }
    [HttpGet("votes")]
    public object Votes(Guid node) { Read(node); return new GetVoteSummary(votes).Execute(new NodeVoteTarget(node), actor.Id is {} voter ? new Atlas.Voting.Votes.ParticipantId(voter) : null); }
    [HttpGet("reactions")]
    public object Reactions(Guid node) => ReactionVotingService.BuildChoices(Read(node), definitions, reactions, votes,
        new Atlas.Voting.Votes.ParticipantId(actor.Id ?? Guid.NewGuid())).Select(x => new { id = x.Definition.Id.Value,
            associationId = x.Association?.Id.Value, x.Definition.Text, x.Definition.Emoji, x.Score, x.TotalVotes, x.CurrentParticipantVote });
    private Node Read(Guid id)
    {
        var node = nodes.GetById(new NodeId(id)) ?? throw new KeyNotFoundException();
        if (moderation.Visibility(id).IsHidden) throw new KeyNotFoundException();
        return node;
    }
}
