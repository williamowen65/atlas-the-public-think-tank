using Atlas.Contracts.Notifications.V1;
using System.Text.Json.Serialization;

namespace Atlas.Notifications;

public enum NotificationCategory { Discussion, Moderation }
public enum DeliveryChannel { Email, Push }
public enum DeliveryStatus { Simulated, Failed }

public sealed record DeliveryAttempt(DeliveryChannel Channel, DeliveryStatus Status,
    DateTimeOffset AttemptedAt, string? Error);

public sealed class Notification
{
    public Guid Id { get; init; }
    public Guid OccurrenceId { get; init; }
    public Guid RecipientParticipantId { get; init; }
    public NotificationCategory Category { get; init; }
    public bool InAppVisible { get; init; }
    public string Kind { get; init; } = string.Empty;
    public string SubjectKind { get; init; } = string.Empty;
    public Guid SubjectId { get; init; }
    public Guid ActorParticipantId { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    [JsonInclude] public DateTimeOffset? ReadAt { get; private set; }
    [JsonInclude] public DateTimeOffset? DismissedAt { get; private set; }
    public List<DeliveryAttempt> DeliveryAttempts { get; init; } = [];

    public void MarkRead(DateTimeOffset at) => ReadAt ??= at;
    public void Dismiss(DateTimeOffset at) => DismissedAt ??= at;
}

/// <summary>Each participant chooses channels per category; defaults favor a quiet in-app feed.</summary>
public sealed class NotificationPreferences
{
    public Guid ParticipantId { get; init; }
    public bool DiscussionInApp { get; set; } = true;
    public bool ModerationInApp { get; set; } = true;
    public bool DiscussionEmail { get; set; }
    public bool ModerationEmail { get; set; }
    public bool DiscussionPush { get; set; }
    public bool ModerationPush { get; set; }
    public bool Enabled(NotificationCategory category, DeliveryChannel? channel) => (category, channel) switch
    {
        (NotificationCategory.Discussion, null) => DiscussionInApp,
        (NotificationCategory.Moderation, null) => ModerationInApp,
        (NotificationCategory.Discussion, DeliveryChannel.Email) => DiscussionEmail,
        (NotificationCategory.Moderation, DeliveryChannel.Email) => ModerationEmail,
        (NotificationCategory.Discussion, DeliveryChannel.Push) => DiscussionPush,
        (NotificationCategory.Moderation, DeliveryChannel.Push) => ModerationPush,
        _ => false
    };
}

public interface INotificationRepository
{
    Notification? Find(Guid id);
    Notification? FindOccurrence(Guid occurrenceId, Guid recipientId);
    IReadOnlyList<Notification> Page(Guid recipientId, int offset, int limit);
    void Save(Notification item);
    NotificationPreferences Preferences(Guid participantId);
    void SavePreferences(NotificationPreferences preferences);
}

public interface INotificationDelivery
{
    DeliveryChannel Channel { get; }
    void Send(Notification item);
}

/// <summary>Never contacts an external service; exercises the replaceable delivery boundary.</summary>
public sealed class SimulatedDelivery(DeliveryChannel channel) : INotificationDelivery
{
    public DeliveryChannel Channel { get; } = channel;
    public void Send(Notification item) { }
}

public sealed class NotificationService(INotificationRepository repository, IEnumerable<INotificationDelivery> deliveries)
{
    public Notification? Handle(NotificationRequestedV1 request)
    {
        if (request.OccurrenceId == Guid.Empty || request.RecipientParticipantId == Guid.Empty ||
            request.SubjectId == Guid.Empty || string.IsNullOrWhiteSpace(request.Kind))
            throw new ArgumentException("A notification needs an occurrence, recipient, kind and subject.");
        if (request.RecipientParticipantId == request.ActorParticipantId) return null;
        var category = request.Kind switch
        {
            "NodeCommented" or "CommentReplied" => NotificationCategory.Discussion,
            "NodeHidden" or "NodeRestored" or "ReportDecided" => NotificationCategory.Moderation,
            _ => throw new ArgumentException("Unsupported notification kind.")
        };
        var existing = repository.FindOccurrence(request.OccurrenceId, request.RecipientParticipantId);
        if (existing is not null) return existing;
        var preferences = repository.Preferences(request.RecipientParticipantId);
        if (!preferences.Enabled(category, null) && !Enum.GetValues<DeliveryChannel>()
                .Any(channel => preferences.Enabled(category, channel))) return null;
        var item = new Notification
        {
            Id = Guid.NewGuid(), OccurrenceId = request.OccurrenceId,
            RecipientParticipantId = request.RecipientParticipantId, ActorParticipantId = request.ActorParticipantId,
            Category = category, InAppVisible = preferences.Enabled(category, null),
            Kind = request.Kind, SubjectKind = request.SubjectKind,
            SubjectId = request.SubjectId, CreatedAt = request.OccurredAt
        };
        // Persist first so an external delivery failure cannot erase the in-app record.
        repository.Save(item);
        foreach (var channel in deliveries)
        {
            if (!preferences.Enabled(category, channel.Channel)) continue;
            try
            {
                channel.Send(item);
                item.DeliveryAttempts.Add(new(channel.Channel, DeliveryStatus.Simulated, DateTimeOffset.UtcNow, null));
            }
            catch (Exception exception)
            {
                item.DeliveryAttempts.Add(new(channel.Channel, DeliveryStatus.Failed, DateTimeOffset.UtcNow, exception.Message));
            }
            repository.Save(item);
        }
        return item;
    }

    public IReadOnlyList<Notification> Page(Guid recipientId, int offset, int limit)
    {
        if (offset < 0 || limit is < 1 or > 100) throw new ArgumentOutOfRangeException(nameof(limit));
        return repository.Page(recipientId, offset, limit);
    }

    public void MarkRead(Guid recipientId, Guid notificationId)
    {
        var item = Own(recipientId, notificationId);
        item.MarkRead(DateTimeOffset.UtcNow);
        repository.Save(item);
    }

    public void Dismiss(Guid recipientId, Guid notificationId)
    {
        var item = Own(recipientId, notificationId);
        item.Dismiss(DateTimeOffset.UtcNow);
        repository.Save(item);
    }

    private Notification Own(Guid recipientId, Guid id)
    {
        var item = repository.Find(id);
        if (item is null || item.RecipientParticipantId != recipientId)
            throw new UnauthorizedAccessException("Notification is unavailable to this participant.");
        return item;
    }
}
