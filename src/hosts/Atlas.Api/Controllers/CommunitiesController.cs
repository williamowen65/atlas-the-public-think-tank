using System.ComponentModel.DataAnnotations;
using Atlas.Communities.Communities;
using Atlas.Communities.Nodes;
using Atlas.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Atlas.Api.Controllers;

/// <summary>Translate community resource routes into the existing CommunityService operations.</summary>
[ApiController, Route("api/communities")]
public sealed class CommunitiesController(ICommunityRepository communities, ICommunityNodeRepository associations,
    CommunityService workflow, RequestActor actor) : ControllerBase
{
    [HttpGet]
    public object Page(int offset = 0, int size = 20)
    {
        if (offset < 0 || size < 1 || size > 50) throw new ArgumentException();
        return communities.GetAll().OrderBy(x => x.Id.Value).Skip(offset).Take(size).Select(View);
    }
    [HttpGet("{id:guid}")]
    public object Get(Guid id) => View(Load(id));
    [HttpGet("{id:guid}/nodes")]
    public object Nodes(Guid id, int offset = 0, int size = 20)
    {
        Load(id);
        if (offset < 0 || size < 1 || size > 50) throw new ArgumentException();
        return associations.GetByCommunity(new CommunityId(id)).OrderBy(x => x.NodeId).Skip(offset).Take(size)
            .Select(x => new { x.NodeId });
    }
    [Authorize(Policy = AtlasPolicies.Member), HttpPost]
    public object Create(CommunityInput input) => View(workflow.Create(input.Name, input.Description, actor.ParticipantId, DateTimeOffset.UtcNow));
    [Authorize(Policy = AtlasPolicies.Member), HttpPut("{id:guid}/name")]
    public IActionResult Rename(Guid id, TextRequest input) { workflow.Rename(Load(id), actor.ParticipantId, input.Text, DateTimeOffset.UtcNow); return NoContent(); }
    [Authorize(Policy = AtlasPolicies.Member), HttpPut("{id:guid}/description")]
    public IActionResult Describe(Guid id, TextRequest input) { workflow.ChangeDescription(Load(id), actor.ParticipantId, input.Text, DateTimeOffset.UtcNow); return NoContent(); }
    [Authorize(Policy = AtlasPolicies.Member), HttpPost("{id:guid}/archive")]
    public IActionResult Archive(Guid id) { workflow.Archive(Load(id), actor.ParticipantId, DateTimeOffset.UtcNow); return NoContent(); }
    [Authorize(Policy = AtlasPolicies.Member), HttpPost("{id:guid}/restore")]
    public IActionResult Restore(Guid id) { workflow.Restore(Load(id), actor.ParticipantId, DateTimeOffset.UtcNow); return NoContent(); }
    [Authorize(Policy = AtlasPolicies.Member), HttpPut("{id:guid}/membership")]
    public IActionResult Join(Guid id) { workflow.Join(Load(id), actor.ParticipantId, DateTimeOffset.UtcNow); return NoContent(); }
    [Authorize(Policy = AtlasPolicies.Member), HttpDelete("{id:guid}/membership")]
    public IActionResult Leave(Guid id) { workflow.Leave(Load(id), actor.ParticipantId, DateTimeOffset.UtcNow); return NoContent(); }
    [Authorize(Policy = AtlasPolicies.Member), HttpPut("{id:guid}/nodes/{node:guid}")]
    public IActionResult Associate(Guid id, Guid node) { workflow.AssociateNode(Load(id), node, actor.ParticipantId, DateTimeOffset.UtcNow); return NoContent(); }
    [Authorize(Policy = AtlasPolicies.Member), HttpDelete("{id:guid}/nodes/{node:guid}")]
    public IActionResult Disassociate(Guid id, Guid node)
    {
        var community = Load(id);
        // The reusable service currently lacks this permission check; the API must
        // not expose its unrestricted repository removal capability to any member.
        if (community.OwnerParticipantId != actor.ParticipantId) throw new UnauthorizedAccessException();
        workflow.DisassociateNode(community, node, actor.ParticipantId); return NoContent();
    }
    private Community Load(Guid id) => communities.GetById(new CommunityId(id)) ?? throw new KeyNotFoundException();
    private static object View(Community c) => new { id = c.Id.Value, c.Name, c.Description, c.OwnerParticipantId, status = c.Status.ToString(), c.CreatedAt, c.UpdatedAt };
}
public sealed record CommunityInput([Required, MaxLength(100)] string Name, [Required, MaxLength(10000)] string Description);
