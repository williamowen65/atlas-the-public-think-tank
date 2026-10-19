using Atlas.Identity;
using Atlas.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Atlas.Api.Controllers;

[ApiController, Authorize(Policy = AtlasPolicies.Member), Route("api/notifications")]
public sealed class NotificationsController(NotificationService workflow, RequestActor actor) : ControllerBase
{
    [HttpGet]
    public object Page(int offset = 0, int size = 20) { if (offset < 0 || size < 1 || size > 50) throw new ArgumentException(); return workflow.Page(actor.ParticipantId, offset, size).Select(n => new
    { n.Id, n.Kind, n.SubjectKind, n.SubjectId, n.ActorParticipantId, n.CreatedAt, n.ReadAt }); }
    [HttpPut("{id:guid}/read")]
    public IActionResult Read(Guid id) { workflow.MarkRead(actor.ParticipantId, id); return NoContent(); }
    [HttpDelete("{id:guid}")]
    public IActionResult Dismiss(Guid id) { workflow.Dismiss(actor.ParticipantId, id); return NoContent(); }
    [HttpGet("preferences")]
    public object Preferences() { var p = workflow.Preferences(actor.ParticipantId, actor.ParticipantId); return new { p.DiscussionInApp, p.ModerationInApp, p.DiscussionEmail, p.ModerationEmail, p.DiscussionPush, p.ModerationPush }; }
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
