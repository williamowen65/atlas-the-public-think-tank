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
    public object History(Guid id)
    {
        // Resolve display names only after the global-moderator authorization check.
        // Never include reporter identity in the author-facing notice endpoint.
        var cases = moderation.NodeHistory(actor.ParticipantId, id);
        var ids = cases.SelectMany(x => new Guid?[] { x.ReporterId, x.ReviewerId, x.RestoredBy })
            .Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToArray();
        var names = database.ParticipantRows.AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .ToDictionary(x => x.Id, x => x.DisplayName);
        string Name(Guid? participantId) =>
            participantId is null ? "Unknown participant"
            : names.GetValueOrDefault(participantId.Value, "Unknown participant");
        return cases.Select(x => new
        {
            x.Id,
            x.Reason,
            x.Explanation,
            x.CreatedAt,
            status = x.Status.ToString(),
            reporter = Name(x.ReporterId),
            reviewer = x.ReviewerId is null ? null : Name(x.ReviewerId),
            x.DecisionReason,
            x.DecidedAt,
            publicReason = x.PublicReason.ToString(),
            x.ReviewRequestedAt,
            x.VisibilityRestoredAt,
            restoredBy = x.RestoredBy is null ? null : Name(x.RestoredBy),
            x.RestorationReason
        }).ToArray();
    }

    /// <summary>Read the prior persisted text revision for a hidden node. Moderator-only.</summary>
    [Authorize(Policy = AtlasPolicies.GlobalModerator), HttpGet("nodes/{id:guid}/revisions")]
    public IActionResult Revisions(Guid id)
    {
        // Do not expose history for ordinary public nodes through this moderation surface.
        if (!moderation.Visibility(id).IsHidden) return NotFound();
        var current = database.NodeRows.AsNoTracking().SingleOrDefault(row => row.Id == id);
        if (current is null) return NotFound();
        // Anchor the comparison to the earliest report, not the last author edit.
        // SQL temporal timestamps are database-generated UTC values. Demo reports
        // have synthetic dates, so fall back to the earliest retained revision.
        var reportedAt = database.ModerationCaseRows.AsNoTracking()
            .Where(row => row.NodeId == id)
            .OrderBy(row => row.CreatedAt)
            .Select(row => (DateTimeOffset?)row.CreatedAt)
            .FirstOrDefault();
        var reportTime = reportedAt?.UtcDateTime;
        var nodeVersions = database.NodeRows.TemporalAll().AsNoTracking()
            .Where(row => row.Id == id);
        var atReport = reportTime is null ? null : nodeVersions
            .Where(row => EF.Property<DateTime>(row, "PeriodStart") <= reportTime.Value
                && EF.Property<DateTime>(row, "PeriodEnd") > reportTime.Value)
            .Select(row => new { row.Title, row.ShortDescription })
            .FirstOrDefault();
        var firstNode = nodeVersions
            .OrderBy(row => EF.Property<DateTime>(row, "PeriodStart"))
            .Select(row => new { row.Title, row.ShortDescription })
            .FirstOrDefault();
        // Blocks have independent temporal periods: resolve each at the same
        // report timestamp rather than selecting the latest historical block.
        var blockId = database.Set<DocumentBlockRow>().AsNoTracking()
            .Where(row => row.DocumentId == current.DescriptionId)
            .OrderBy(row => row.Position)
            .Select(row => row.BlockId)
            .FirstOrDefault();
        var block = blockId == Guid.Empty ? null : database.BlockRows.AsNoTracking().SingleOrDefault(row => row.Id == blockId);
        var blockVersions = database.BlockRows.TemporalAll().AsNoTracking()
            .Where(row => row.Id == blockId);
        var atReportBlock = reportTime is null ? null : blockVersions
            .Where(row => EF.Property<DateTime>(row, "PeriodStart") <= reportTime.Value
                && EF.Property<DateTime>(row, "PeriodEnd") > reportTime.Value)
            .Select(row => new { row.Markdown })
            .FirstOrDefault();
        var firstBlock = blockVersions
            .OrderBy(row => EF.Property<DateTime>(row, "PeriodStart"))
            .Select(row => new { row.Markdown })
            .FirstOrDefault();
        var reportedNode = atReport ?? firstNode;
        var reportedBlock = atReportBlock ?? firstBlock;
        return Ok(new
        {
            reported = reportedNode is null && reportedBlock is null ? null : new {
                title = reportedNode?.Title ?? current.Title,
                shortDescription = reportedNode?.ShortDescription ?? current.ShortDescription,
                markdown = reportedBlock?.Markdown ?? block?.Markdown ?? ""
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
