using Atlas.Persistence;
using Atlas.Notifications;

namespace Atlas.Application.Storage;

/// <summary>Maps domain objects directly to EF Core rows in SQL Server.</summary>
public sealed class SqlNotificationRepository(Func<AtlasDataContext>? contextFactory = null) : SqlRepository(contextFactory), INotificationRepository
{

    // Read by notification ID, or locate one recipient's notification for an occurrence.
    public Notification? Find(Guid id) => FindRow<NotificationRow>(id) is { } row ? ToDomain(row) : null;
    public Notification? FindOccurrence(
        Guid occurrenceId,
        Guid recipientId
    ) =>
        QueryRows<NotificationRow>(row => row.OccurrenceId == occurrenceId && row.RecipientParticipantId == recipientId).Select(ToDomain).SingleOrDefault();

    // Select visible, undismissed notifications for this recipient. Sort newest first with an ID tie-breaker,
    // then apply offset/limit in SQL before rebuilding domain objects.
    public IReadOnlyList<Notification> Page(
        Guid recipientId,
        int offset,
        int limit
    ) =>
        QueryRows<NotificationRow>(
            row => row.RecipientParticipantId == recipientId && row.InAppVisible && row.DismissedAt == null,
            query => query.OrderByDescending(row => row.CreatedAt).ThenByDescending(row => row.Id).Skip(offset).Take(limit)
        ).Select(ToDomain).ToList();
    public void Save(Notification item) => Execute(() =>
    {
        ArgumentNullException.ThrowIfNull(item);

        // New notifications require active actor/recipient references. Discussion notifications also need
        // an available Node; moderation notifications only require that Node to exist.
        var isNew = FindRow<NotificationRow>(item.Id) is null;
        ReferenceValidation.Require(
            this,
            "Participant",
            item.RecipientParticipantId,
            isNew
        );
        ReferenceValidation.Require(
            this,
            "Participant",
            item.ActorParticipantId,
            isNew
        );
        ReferenceValidation.Require(
            this,
            "Node",
            item.SubjectId,
            isNew && item.Category == NotificationCategory.Discussion
        );

        // Persist notification state and delivery-attempt history together in the operation transaction.
        SaveRow(new NotificationRow
        {
            Id = item.Id,
            OccurrenceId = item.OccurrenceId,
            RecipientParticipantId = item.RecipientParticipantId,
            Category = (int)item.Category,
            InAppVisible = item.InAppVisible,
            Kind = item.Kind,
            SubjectKind = item.SubjectKind,
            SubjectId = item.SubjectId,
            ActorParticipantId = item.ActorParticipantId,
            CreatedAt = item.CreatedAt,
            ReadAt = item.ReadAt,
            DismissedAt = item.DismissedAt,
            DeliveryAttempts = item.DeliveryAttempts
                .Select(attempt => new DeliveryAttemptRow
                {
                    Channel = (int)attempt.Channel,
                    Status = (int)attempt.Status,
                    AttemptedAt = attempt.AttemptedAt,
                    Error = attempt.Error
                })
                .ToList()
        });
        return true;
    });

    // Use saved preferences when present; otherwise return the domain defaults for this participant.
    public NotificationPreferences Preferences(Guid id) => FindRow<NotificationPreferencesRow>(id) is { } row ? new NotificationPreferences(row.ParticipantId)
    {
        DiscussionInApp = row.DiscussionInApp,
        ModerationInApp = row.ModerationInApp,
        DiscussionEmail = row.DiscussionEmail,
        ModerationEmail = row.ModerationEmail,
        DiscussionPush = row.DiscussionPush,
        ModerationPush = row.ModerationPush
    }
        : new NotificationPreferences(id);
    public void SavePreferences(NotificationPreferences item) => Execute(() =>
    {
        ArgumentNullException.ThrowIfNull(item);

        // Require an active participant before persisting their channel/category preferences.
        ReferenceValidation.Require(
            this,
            "Participant",
            item.ParticipantId
        );
        SaveRow(new NotificationPreferencesRow {
            ParticipantId = item.ParticipantId,
            DiscussionInApp = item.DiscussionInApp,
            ModerationInApp = item.ModerationInApp,
            DiscussionEmail = item.DiscussionEmail,
            ModerationEmail = item.ModerationEmail,
            DiscussionPush = item.DiscussionPush,
            ModerationPush = item.ModerationPush
        });
        return true;
    });

    // Rebuild the notification and delivery attempts in stored order, then restore read/dismissed state.
    private static Notification ToDomain(NotificationRow row)
    {
        var item = new Notification(
            row.Id,
            row.OccurrenceId,
            row.RecipientParticipantId,
            row.ActorParticipantId,
            row.Kind,
            row.SubjectKind,
            row.SubjectId,
            (NotificationCategory)row.Category,
            row.InAppVisible,
            row.CreatedAt,
            row.DeliveryAttempts.OrderBy(attempt => attempt.Position).Select(attempt =>
                new DeliveryAttempt(
                    (DeliveryChannel)attempt.Channel,
                    (DeliveryStatus)attempt.Status,
                    attempt.AttemptedAt,
                    attempt.Error
                ))
        );

        // Each non-null timestamp is unwrapped by the pattern, like HasValue followed by Value.
        if (row.ReadAt is { } readAt) item.MarkRead(readAt);
        if (row.DismissedAt is { } dismissedAt) item.Dismiss(dismissedAt);
        return item;
    }
}
