using System.ComponentModel.DataAnnotations;
using Atlas.Application;
using Atlas.Graph.Nodes;
using Atlas.Identity;
using Atlas.Moderation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Atlas.Api.Controllers;

/// <summary>Separate member reporting/review from moderator-only queue and decision endpoints.</summary>
[ApiController, Authorize(Policy = AtlasPolicies.Member), Route("api/moderation")]
public sealed class ModerationController(ModerationService moderation, ModerationWorkflowService workflow, INodeRepository nodes, RequestActor actor) : ControllerBase
{
    [HttpPost("nodes/{id:guid}/reports")]
    public object Report(Guid id, ReportRequest request)
    {
        var node = nodes.GetById(new NodeId(id)) ?? throw new KeyNotFoundException();
        var report = moderation.ReportNode(id, actor.ParticipantId, node.Title.Value, request.Reason, request.Explanation, DateTimeOffset.UtcNow);
        return new { report.Id };
    }
    [HttpPost("nodes/{id:guid}/review")]
    public IActionResult Review(Guid id) { workflow.RequestReview(nodes.GetById(new NodeId(id)) ?? throw new KeyNotFoundException(), actor.ParticipantId); return NoContent(); }
    [Authorize(Policy = AtlasPolicies.GlobalModerator), HttpGet("queue")]
    public object Queue() => moderation.NodeQueue(actor.ParticipantId).Select(x => new { x.NodeId, x.ReportedTitle, count = x.PendingReports.Count, x.ReviewRequested });
    [Authorize(Policy = AtlasPolicies.GlobalModerator), HttpGet("nodes/{id:guid}/history")]
    public object History(Guid id) => moderation.NodeHistory(actor.ParticipantId, id).Select(x => new { x.Id, x.Reason, x.Explanation, x.CreatedAt, status = x.Status.ToString() });
    [Authorize(Policy = AtlasPolicies.GlobalModerator), HttpPost("nodes/{id:guid}/decision")]
    public IActionResult Decide(Guid id, DecisionRequest request) { workflow.Decide(id, actor.ParticipantId, request.Decision, request.Rationale, request.Reason); return NoContent(); }
    [Authorize(Policy = AtlasPolicies.GlobalModerator), HttpPost("nodes/{id:guid}/restore")]
    public IActionResult Restore(Guid id, TextRequest request) { workflow.Restore(id, actor.ParticipantId, request.Text); return NoContent(); }
}
public sealed record ReportRequest([Required, MaxLength(100)] string Reason, [MaxLength(10000)] string? Explanation);
public sealed record DecisionRequest(ModerationDecision Decision, [Required, MaxLength(10000)] string Rationale, PublicModerationReason Reason);
