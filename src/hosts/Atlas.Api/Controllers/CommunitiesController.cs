using System.ComponentModel.DataAnnotations;
using Atlas.Communities.Communities;
using Atlas.Communities.Nodes;
using Atlas.Communities.Memberships;
using Atlas.Graph.Nodes;
using Atlas.Application.Storage;
using Atlas.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Api.Controllers;

/// <summary>Translate community resource routes into the existing CommunityService operations.</summary>
// Explicit controller policy is visible to CodeQL and reviewers. GET/HEAD/OPTIONS
// remain readable; writes require the cookie/header token pair.
[AutoValidateAntiforgeryToken]
[ApiController, Route("api/communities")]
public sealed class CommunitiesController(
    ICommunityRepository communities,
    ICommunityNodeRepository associations,
    ICommunityMembershipRepository memberships,
    INodeRepository nodes,
    CommunityService workflow,
    RequestActor actor
) : ControllerBase
{
    /// <summary>Read a bounded community page in stable identifier order.</summary>
    [HttpGet]
    public object Page(int offset = 0, int size = 20)
    {
        // Validate the page window before reading community data.
        if (offset < 0 || size < 1 || size > 50)
            throw new ArgumentException();

        // Return stable public projections for the requested slice.
        return communities.GetAll().OrderBy(x => x.Id.Value).Skip(offset).Take(size).Select(View);
    }

    /// <summary>Load one community or report that it does not exist.</summary>
    [HttpGet("{id:guid}")]
    public object Get(Guid id) => View(Load(id));

    /// <summary>Community membership and ownership summaries for participant profiles.</summary>
    [HttpGet("participant/{participantId:guid}")]
    public object ParticipantCommunities(Guid participantId) => communities.GetAll()
        .Where(c => c.OwnerParticipantId == participantId || memberships.Get(c.Id, participantId)?.IsActive == true)
        .Select(c => View(c, participantId));

    /// <summary>Authenticated participant's current community memberships.</summary>
    [Authorize(Policy = AtlasPolicies.Member), HttpGet("mine")]
    public object Mine() => ParticipantCommunities(actor.ParticipantId);

    /// <summary>Expose the ownership cap and rolling creation allowance to the client.</summary>
    [Authorize(Policy = AtlasPolicies.Member), HttpGet("creation-allowance")]
    public object CreationAllowance() => new
    {
        maximumOwned = SqlCommunityRepository.MaximumOwnedCommunities,
        owned = communities.GetAll().Count(c => c.OwnerParticipantId == actor.ParticipantId),
        rolling = (communities as ICommunityCreationQuota)?.GetCreationAllowance(actor.ParticipantId)
    };

    /// <summary>Verify the community exists, then page its associated node identifiers.</summary>
    [HttpGet("{id:guid}/nodes")]
    public object Nodes(Guid id, int offset = 0, int size = 20)
    {
        // Require an existing community before paging its associations.
        Load(id);

        if (offset < 0 || size < 1 || size > 50)
            throw new ArgumentException();

        // Return node identifiers without loading every node payload.
        return associations.GetByCommunity(new CommunityId(id)).OrderBy(x => x.NodeId).Skip(offset).Take(size)
            .Select(
                x => new
                {
                    x.NodeId
                }
            );
    }

    /// <summary>Create a community for the verified participant through the shared workflow.</summary>
    [Authorize(Policy = AtlasPolicies.Member), HttpPost]
    public object Create(CommunityInput input) => View(workflow.Create(
        input.Name,
        input.Description,
        actor.ParticipantId,
        DateTimeOffset.UtcNow
    ));

    /// <summary>Rename a community after the workflow checks ownership.</summary>
    [Authorize(Policy = AtlasPolicies.Member), HttpPut("{id:guid}/name")]
    public IActionResult Rename(Guid id, TextRequest input)
    {
        workflow.Rename(
            Load(id),
            actor.ParticipantId,
            input.Text,
            DateTimeOffset.UtcNow
        );
        return NoContent();
    }

    /// <summary>Update a community description through its owner-checked workflow.</summary>
    [Authorize(Policy = AtlasPolicies.Member), HttpPut("{id:guid}/description")]
    public IActionResult Describe(Guid id, TextRequest input)
    {
        workflow.ChangeDescription(
            Load(id),
            actor.ParticipantId,
            input.Text,
            DateTimeOffset.UtcNow
        );
        return NoContent();
    }

    /// <summary>Archive the community through its current-state and ownership checks.</summary>
    [Authorize(Policy = AtlasPolicies.Member), HttpPost("{id:guid}/archive")]
    public IActionResult Archive(Guid id)
    {
        workflow.Archive(
            Load(id),
            actor.ParticipantId,
            DateTimeOffset.UtcNow
        );
        return NoContent();
    }

    /// <summary>Restore the owner’s archived community.</summary>
    [Authorize(Policy = AtlasPolicies.Member), HttpPost("{id:guid}/restore")]
    public IActionResult Restore(Guid id)
    {
        workflow.Restore(
            Load(id),
            actor.ParticipantId,
            DateTimeOffset.UtcNow
        );
        return NoContent();
    }

    /// <summary>Add the verified participant to the community through membership rules.</summary>
    [Authorize(Policy = AtlasPolicies.Member), HttpPut("{id:guid}/membership")]
    public IActionResult Join(Guid id)
    {
        workflow.Join(
            Load(id),
            actor.ParticipantId,
            DateTimeOffset.UtcNow
        );
        return NoContent();
    }

    /// <summary>Remove the verified participant’s own membership.</summary>
    [Authorize(Policy = AtlasPolicies.Member), HttpDelete("{id:guid}/membership")]
    public IActionResult Leave(Guid id)
    {
        workflow.Leave(
            Load(id),
            actor.ParticipantId,
            DateTimeOffset.UtcNow
        );
        return NoContent();
    }

    /// <summary>Associate a node through the community workflow and its permission checks.</summary>
    [Authorize(Policy = AtlasPolicies.Member), HttpPut("{id:guid}/nodes/{node:guid}")]
    public IActionResult Associate(Guid id, Guid node)
    {
        // The node author alone controls association with communities, even when
        // they are not a member of the destination community.
        var authoredNode = nodes.GetById(new NodeId(node)) ?? throw new KeyNotFoundException();
        if (authoredNode.AuthorId.Value != actor.ParticipantId) throw new UnauthorizedAccessException();
        workflow.AssociateNode(
            Load(id),
            node,
            actor.ParticipantId,
            DateTimeOffset.UtcNow
        );
        return NoContent();
    }

    /// <summary>Require community ownership before removing a node association.</summary>
    [Authorize(Policy = AtlasPolicies.Member), HttpDelete("{id:guid}/nodes/{node:guid}")]
    public IActionResult Disassociate(Guid id, Guid node)
    {
        // Node authors manage their own associations; community ownership alone
        // does not authorize changing someone else's node.
        var community = Load(id);
        var authoredNode = nodes.GetById(new NodeId(node)) ?? throw new KeyNotFoundException();
        if (authoredNode.AuthorId.Value != actor.ParticipantId) throw new UnauthorizedAccessException();

        // Remove only the association; the node itself remains.
        workflow.DisassociateNode(community, node, actor.ParticipantId);
        return NoContent();
    }

    private Community Load(Guid id) => communities.GetById(new CommunityId(id)) ?? throw new KeyNotFoundException();

    private object View(Community c, Guid? participantId = null) => new
    {
        id = c.Id.Value,
        c.Name,
        c.Description,
        c.OwnerParticipantId,
        status = c.Status.ToString(),
        c.CreatedAt,
        c.UpdatedAt,
        memberCount = memberships.GetByCommunity(c.Id).Count(m => m.IsActive),
        nodeCount = associations.GetByCommunity(c.Id).Count,
        isMember = (participantId ?? actor.Id) is { } participant && memberships.Get(c.Id, participant)?.IsActive == true
    };
}

public sealed record CommunityInput(
    [Required, MaxLength(100)] string Name,
    [Required, MaxLength(10000)] string Description
);
