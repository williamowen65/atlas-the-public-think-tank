using System.ComponentModel.DataAnnotations;
using Atlas.Application.Comments;
using Atlas.Comments.Comments;
using Atlas.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Api.Controllers;

/// <summary>Expose one sibling page at a time; delegate mutations and notification coordination to Application.</summary>
// Explicit controller policy is visible to CodeQL and reviewers. GET/HEAD/OPTIONS
// remain readable; writes require the cookie/header token pair.
[AutoValidateAntiforgeryToken]
[ApiController, Route("api")]
public sealed class CommentsController(
    CommentWorkflowService workflow,
    Atlas.Graph.Nodes.INodeRepository nodes,
    Atlas.Moderation.ModerationService moderation,
    RequestActor actor
) : ControllerBase
{
    /// <summary>Read roots or direct replies using a bounded keyset page and request cancellation.</summary>
    [HttpGet("nodes/{node:guid}/comments")]
    public async Task<object> Page(
        Guid node,
        Guid? parent,
        DateTimeOffset? afterAt,
        Guid? afterId,
        int size = 20,
        CancellationToken cancellationToken = default
    )
    {
        // Hide comments when their target node is unavailable or moderated away.
        if (nodes.GetById(new Atlas.Graph.Nodes.NodeId(node)) is null || moderation.Visibility(node).IsHidden)
            throw new KeyNotFoundException();

        // A keyset cursor requires both its timestamp and identifier.
        if (afterAt.HasValue != afterId.HasValue)
            throw new ArgumentException();

        // Read one sibling page, propagating cancellation to the application boundary.
        var page = await workflow.GetPageAsync(
            new(
                new CommentTarget("Node", node),
                parent is { } p ? new CommentId(p) : null,
                afterAt is { } at && afterId is { } id ? new CommentCursor(at, new CommentId(id)) : null,
                size
            ),
            cancellationToken
        );

        // Expose the next cursor separately from the comment projections.
        return new
        {
            items = page.Items.Select(View),
            next = page.Next is { } next ? new
            {
                afterAt = next.CreatedAt,
                afterId = next.Id.Value
            } : null
        };
    }

    /// <summary>Create a root comment and coordinate its notification through Application.</summary>
    [Authorize(Policy = AtlasPolicies.Member), HttpPost("nodes/{node:guid}/comments")]
    public object Add(Guid node, CommentBody request) => View(workflow.AddTopLevel(
        new("Node", node),
        actor.ParticipantId,
        request.Body,
        DateTimeOffset.UtcNow
    ));

    /// <summary>Reply to an existing comment without accepting an author identity from JSON.</summary>
    [Authorize(Policy = AtlasPolicies.Member), HttpPost("comments/{id:guid}/replies")]
    public object Reply(Guid id, CommentBody request) => View(workflow.Reply(
        new CommentId(id),
        actor.ParticipantId,
        request.Body,
        DateTimeOffset.UtcNow
    ));

    /// <summary>Delegate to Comments, which checks the current actor’s editing permission.</summary>
    [Authorize(Policy = AtlasPolicies.Member), HttpPut("comments/{id:guid}")]
    public IActionResult Edit(Guid id, CommentBody request)
    {
        workflow.Edit(
            new CommentId(id),
            actor.ParticipantId,
            request.Body,
            DateTimeOffset.UtcNow
        );
        return NoContent();
    }

    /// <summary>Remove through Comments; subsequent reads retain a placeholder rather than the body.</summary>
    [Authorize(Policy = AtlasPolicies.Member), HttpDelete("comments/{id:guid}")]
    public IActionResult Remove(Guid id)
    {
        workflow.Remove(
            new CommentId(id),
            actor.ParticipantId,
            DateTimeOffset.UtcNow
        );
        return NoContent();
    }

    private static object View(Comment comment) => new
    {
        id = comment.Id.Value,
        parentId = comment.ParentCommentId?.Value,
        authorId = comment.AuthorParticipantId,
        body = comment.IsRemoved ? null : comment.Body,
        status = comment.Status.ToString(),
        comment.CreatedAt,
        comment.UpdatedAt
    };
}

public sealed record CommentBody([Required, MaxLength(10000)] string Body);
