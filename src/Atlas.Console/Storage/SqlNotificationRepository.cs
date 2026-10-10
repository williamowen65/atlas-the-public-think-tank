using Atlas.Persistence;
using Atlas.Notifications;

namespace Atlas.ConsoleApp.Storage;

/// <summary>Maps domain objects directly to EF Core rows in SQL Server.</summary>
public sealed class SqlNotificationRepository(Func<AtlasDataContext>? contextFactory = null) : SqlRepository(contextFactory), INotificationRepository
{
    public Notification? Find(Guid id) => FindRow<NotificationRow>(id) is { } row ? ToDomain(row) : null;
    public Notification? FindOccurrence(Guid occurrenceId, Guid recipientId) =>
        QueryRows<NotificationRow>(row => row.OccurrenceId == occurrenceId && row.RecipientParticipantId == recipientId).Select(ToDomain).SingleOrDefault();
    public IReadOnlyList<Notification> Page(Guid recipientId, int offset, int limit) =>
        QueryRows<NotificationRow>(row => row.RecipientParticipantId == recipientId && row.InAppVisible && row.DismissedAt == null,
            query => query.OrderByDescending(row => row.CreatedAt).ThenByDescending(row => row.Id).Skip(offset).Take(limit)).Select(ToDomain).ToList();
    public void Save(Notification item) => SaveRow(new NotificationRow { Id = item.Id, OccurrenceId = item.OccurrenceId,
        RecipientParticipantId = item.RecipientParticipantId, Category = (int)item.Category, InAppVisible = item.InAppVisible, Kind = item.Kind,
        SubjectKind = item.SubjectKind, SubjectId = item.SubjectId, ActorParticipantId = item.ActorParticipantId, CreatedAt = item.CreatedAt,
        ReadAt = item.ReadAt, DismissedAt = item.DismissedAt, DeliveryAttempts = item.DeliveryAttempts.Select(attempt => new DeliveryAttemptRow
        { Channel = (int)attempt.Channel, Status = (int)attempt.Status, AttemptedAt = attempt.AttemptedAt, Error = attempt.Error }).ToList() });
    public NotificationPreferences Preferences(Guid id) => FindRow<NotificationPreferencesRow>(id) is { } row ? new NotificationPreferences(row.ParticipantId)
    { DiscussionInApp = row.DiscussionInApp, ModerationInApp = row.ModerationInApp,
        DiscussionEmail = row.DiscussionEmail, ModerationEmail = row.ModerationEmail, DiscussionPush = row.DiscussionPush, ModerationPush = row.ModerationPush }
        : new NotificationPreferences(id);
    public void SavePreferences(NotificationPreferences item) => SaveRow(new NotificationPreferencesRow { ParticipantId = item.ParticipantId,
        DiscussionInApp = item.DiscussionInApp, ModerationInApp = item.ModerationInApp, DiscussionEmail = item.DiscussionEmail,
        ModerationEmail = item.ModerationEmail, DiscussionPush = item.DiscussionPush, ModerationPush = item.ModerationPush });
    private static Notification ToDomain(NotificationRow row)
    {
        var item = new Notification(row.Id, row.OccurrenceId, row.RecipientParticipantId, row.ActorParticipantId,
            row.Kind, row.SubjectKind, row.SubjectId, (NotificationCategory)row.Category, row.InAppVisible, row.CreatedAt,
            row.DeliveryAttempts.OrderBy(attempt => attempt.Position).Select(attempt =>
                new DeliveryAttempt((DeliveryChannel)attempt.Channel, (DeliveryStatus)attempt.Status, attempt.AttemptedAt, attempt.Error)));
        if (row.ReadAt is { } readAt) item.MarkRead(readAt);
        if (row.DismissedAt is { } dismissedAt) item.Dismiss(dismissedAt);
        return item;
    }
}
