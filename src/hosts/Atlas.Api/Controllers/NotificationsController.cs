using Atlas.Identity;
using Atlas.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Atlas.Api.Controllers;

/// <summary>Scope notification reads and changes to the current participant, never a supplied recipient ID.</summary>
// Explicit controller policy is visible to CodeQL and reviewers. GET/HEAD/OPTIONS
// remain readable; writes require the cookie/header token pair.
[AutoValidateAntiforgeryToken]
[ApiController, Authorize(Policy = AtlasPolicies.Member), Route("api/notifications")]
public sealed class NotificationsController(NotificationService workflow, RequestActor actor) : ControllerBase
{
    /// <summary>Read a bounded page belonging to the authenticated recipient.</summary>
    [HttpGet]
    public object Page(int offset = 0, int size = 20) { if (offset < 0 || size < 1 || size > 50) throw new ArgumentException(); return workflow.Page(actor.ParticipantId, offset, size).Select(n => new
    { n.Id, n.Kind, n.SubjectKind, n.SubjectId, n.ActorParticipantId, n.CreatedAt, n.ReadAt }); }
    /// <summary>Mark only the current participant’s notification as read.</summary>
    [HttpPut("{id:guid}/read")]
    public IActionResult Read(Guid id) { workflow.MarkRead(actor.ParticipantId, id); return NoContent(); }
    /// <summary>Dismiss only the current participant’s notification.</summary>
    [HttpDelete("{id:guid}")]
    public IActionResult Dismiss(Guid id) { workflow.Dismiss(actor.ParticipantId, id); return NoContent(); }
    /// <summary>Read or save preferences for the verified participant; clients cannot select another owner.</summary>
    [HttpGet("preferences")]
    public object Preferences() { var p = workflow.Preferences(actor.ParticipantId, actor.ParticipantId); return new { p.DiscussionInApp, p.ModerationInApp, p.DiscussionEmail, p.ModerationEmail, p.DiscussionPush, p.ModerationPush }; }
    /// <summary>Read or save preferences for the verified participant; clients cannot select another owner.</summary>
    [HttpPut("preferences")]
    public IActionResult Preferences(PreferencesRequest r)
    {
        workflow.SavePreferences(actor.ParticipantId, new NotificationPreferences(actor.ParticipantId)
        { DiscussionInApp = r.DiscussionInApp, ModerationInApp = r.ModerationInApp, DiscussionEmail = r.DiscussionEmail,
            ModerationEmail = r.ModerationEmail, DiscussionPush = r.DiscussionPush, ModerationPush = r.ModerationPush });
        return NoContent();
    }
}
public sealed record PreferencesRequest(bool DiscussionInApp, bool ModerationInApp, bool DiscussionEmail, bool ModerationEmail, bool DiscussionPush, bool ModerationPush);
