using System.ComponentModel.DataAnnotations;
using Atlas.Application;
using Atlas.Graph.Nodes;
using Atlas.Identity;
using Atlas.Moderation;
using Atlas.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Api.Controllers;

/// <summary>Separate member reporting/review from moderator-only queue and decision endpoints.</summary>
// Explicit controller policy is visible to CodeQL and reviewers. GET/HEAD/OPTIONS
// remain readable; writes require the cookie/header token pair.
[AutoValidateAntiforgeryToken]
[ApiController, Authorize(Policy = AtlasPolicies.Member), Route("api/moderation")]
public sealed class ModerationController(
    ModerationService moderation,
    ModerationWorkflowService workflow,
    INodeRepository nodes,
    AtlasDataContext database,
    RequestActor actor
) : ControllerBase
{
    /// <summary>Load the current node title and submit the participant’s report.</summary>
    [HttpPost("nodes/{id:guid}/reports")]
    public object Report(Guid id, ReportRequest request)
    {
        // Capture the current title from storage rather than trusting a submitted title.
        var node = nodes.GetById(new NodeId(id)) ?? throw new KeyNotFoundException();

        // Submit the report through moderation’s eligibility and quota checks.
        var report = moderation.ReportNode(
            id,
            actor.ParticipantId,
            node.Title.Value,
            request.Reason,
            request.Explanation,
            DateTimeOffset.UtcNow
        );
        return new
        {
            report.Id
        };
    }

    /// <summary>Request review of an existing node through the author-checked workflow.</summary>
    [HttpPost("nodes/{id:guid}/review")]
    public IActionResult Review(Guid id)
    {
        workflow.RequestReview(
            nodes.GetById(new NodeId(id)) ?? throw new KeyNotFoundException(),
            actor.ParticipantId
        );
        return NoContent();
    }

    /// <summary>Reveal a hidden-node decision only to its author or a global moderator.</summary>
    [HttpGet("nodes/{id:guid}/notice")]
    public object Notice(Guid id)
    {
        var node = nodes.GetById(new NodeId(id)) ?? throw new KeyNotFoundException();
        if (!moderation.CanViewHiddenOriginal(actor.ParticipantId, node.AuthorId.Value, id))
            return NotFound();

        var decisions = moderation.HiddenDecisionDetails(actor.ParticipantId, node.AuthorId.Value, id);
        return new
        {
            nodeId = id,
            reasons = decisions.Select(x => new
            {
                publicReason = x.PublicReason.ToString(),
                rationale = x.DecisionReason,
                decidedAt = x.DecidedAt,
                reviewRequested = x.ReviewRequestedAt is not null
            })
        };
    }

    /// <summary>Project the pending moderation queue for a global moderator.</summary>
    [Authorize(Policy = AtlasPolicies.GlobalModerator), HttpGet("queue")]
    public object Queue([FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery] string? search = null, [FromQuery] bool reviewOnly = false,
        [FromQuery] string sort = "reports")
    {
        const int pageSize = 20;
        var candidates = moderation.NodeQueue(actor.ParticipantId)
            .Where(x => (!reviewOnly || x.ReviewRequested) &&
                (string.IsNullOrWhiteSpace(search) || x.ReportedTitle.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase)));
        var ordered = sort == "title"
            ? candidates.OrderBy(x => x.ReportedTitle).ThenBy(x => x.NodeId)
            : candidates.OrderByDescending(x => x.PendingReports.Count).ThenBy(x => x.ReportedTitle).ThenBy(x => x.NodeId);
        var totalCount = ordered.Count();
        return new
        {
            items = ordered.Skip((int)Math.Min(((long)page - 1) * pageSize, int.MaxValue)).Take(pageSize).Select(x => new
            {
                x.NodeId, x.ReportedTitle, count = x.PendingReports.Count, x.ReviewRequested
            }).ToArray(),
            page, pageSize, totalCount, hasNextPage = (long)page * pageSize < totalCount
        };
    }

    /// <summary>Project a node’s moderation decisions for a global moderator.</summary>
    [Authorize(Policy = AtlasPolicies.GlobalModerator), HttpGet("nodes/{id:guid}/history")]
    public object History(Guid id) => moderation.NodeHistory(actor.ParticipantId, id).Select(
        x => new
        {
            x.Id,
            x.Reason,
            x.Explanation,
            x.CreatedAt,
            status = x.Status.ToString()
        }
    );

    /// <summary>Read the prior persisted text revision for a hidden node. Moderator-only.</summary>
    [Authorize(Policy = AtlasPolicies.GlobalModerator), HttpGet("nodes/{id:guid}/revisions")]
    public IActionResult Revisions(Guid id)
    {
        // Do not expose history for ordinary public nodes through this moderation surface.
        if (!moderation.Visibility(id).IsHidden) return NotFound();
        var current = database.NodeRows.AsNoTracking().SingleOrDefault(row => row.Id == id);
        if (current is null) return NotFound();
        var previous = database.NodeRows.TemporalAll().AsNoTracking()
            .Where(row => row.Id == id && EF.Property<DateTime>(row, "PeriodEnd") < DateTime.MaxValue)
            .OrderByDescending(row => EF.Property<DateTime>(row, "PeriodEnd"))
            .Select(row => new { row.Title, row.ShortDescription })
            .FirstOrDefault();
        // Content is independently versioned: a block edit need not modify its node row.
        var blockId = database.DocumentRows.AsNoTracking()
            .Where(row => row.Id == current.DescriptionId)
            .SelectMany(row => row.Blocks.OrderBy(link => link.Position).Select(link => link.BlockId))
            .FirstOrDefault();
        var block = blockId == Guid.Empty ? null : database.BlockRows.AsNoTracking().SingleOrDefault(row => row.Id == blockId);
        var oldBlock = blockId == Guid.Empty ? null : database.BlockRows.TemporalAll().AsNoTracking()
            .Where(row => row.Id == blockId && EF.Property<DateTime>(row, "PeriodEnd") < DateTime.MaxValue)
            .OrderByDescending(row => EF.Property<DateTime>(row, "PeriodEnd"))
            .Select(row => new { row.Markdown }).FirstOrDefault();
        return Ok(new
        {
            reported = previous is null && oldBlock is null ? null : new {
                title = previous?.Title ?? current.Title,
                shortDescription = previous?.ShortDescription ?? current.ShortDescription,
                markdown = oldBlock?.Markdown ?? block?.Markdown ?? ""
            },
            revised = new {
                current.Title,
                current.ShortDescription,
                markdown = block?.Markdown ?? ""
            }
        });
    }

    /// <summary>Apply a moderator decision through the shared moderation workflow.</summary>
    [Authorize(Policy = AtlasPolicies.GlobalModerator), HttpPost("nodes/{id:guid}/decision")]
    public IActionResult Decide(Guid id, DecisionRequest request)
    {
        workflow.Decide(
            id,
            actor.ParticipantId,
            request.Decision,
            request.Rationale,
            request.Reason
        );
        return NoContent();
    }

    /// <summary>Restore visibility through the moderator-only workflow.</summary>
    [Authorize(Policy = AtlasPolicies.GlobalModerator), HttpPost("nodes/{id:guid}/restore")]
    public IActionResult Restore(Guid id, TextRequest request)
    {
        workflow.Restore(id, actor.ParticipantId, request.Text);
        return NoContent();
    }
}

public sealed record ReportRequest(
    [Required, MaxLength(100)] string Reason,
    [MaxLength(10000)] string? Explanation
);

public sealed record DecisionRequest(
    ModerationDecision Decision,
    [Required, MaxLength(10000)] string Rationale,
    PublicModerationReason Reason
);
