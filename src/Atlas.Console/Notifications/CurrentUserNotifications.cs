using Atlas.Identity;
using Atlas.Notifications;

namespace Atlas.ConsoleApp.Notifications;

/// <summary>
/// Application/host workflow for notification operations that always belong to the
/// currently authenticated Atlas participant.
///
/// IAuthenticatedActor establishes the actor. NotificationService still receives the actor
/// explicitly so its ownership checks remain visible and independently testable.
/// A future request-scoped API workflow can use the same pattern.
/// </summary>
public sealed class CurrentUserNotifications(
    IAuthenticatedActor authenticatedActor,
    NotificationService notifications)
{
    public IReadOnlyList<Notification> Page(int offset, int limit) =>
        notifications.Page(authenticatedActor.ParticipantId, offset, limit);

    public NotificationPreferences Preferences() =>
        notifications.Preferences(authenticatedActor.ParticipantId, authenticatedActor.ParticipantId);

    public void SavePreferences(NotificationPreferences preferences) =>
        notifications.SavePreferences(authenticatedActor.ParticipantId, preferences);

    public void MarkRead(Guid notificationId) =>
        notifications.MarkRead(authenticatedActor.ParticipantId, notificationId);

    public void Dismiss(Guid notificationId) =>
        notifications.Dismiss(authenticatedActor.ParticipantId, notificationId);
}
