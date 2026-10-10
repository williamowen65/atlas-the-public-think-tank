using Atlas.Contracts.Notifications.V1;

namespace Atlas.Notifications;

public enum NotificationCategory { Discussion, Moderation }
public enum DeliveryChannel { Email, Push }
public enum DeliveryStatus { Simulated, Failed }

public sealed record DeliveryAttempt
{
    public DeliveryChannel Channel { get; }
    public DeliveryStatus Status { get; }
    public DateTimeOffset AttemptedAt { get; }
    public string? Error { get; }
    public DeliveryAttempt(DeliveryChannel channel, DeliveryStatus status, DateTimeOffset attemptedAt, string? error)
    {
        if (!Enum.IsDefined(channel) || !Enum.IsDefined(status)) throw new ArgumentException("Unsupported delivery state.");
        Channel = channel; Status = status; AttemptedAt = attemptedAt.ToUniversalTime(); Error = error;
    }
}

public sealed class Notification
{
    public Guid Id { get; }
    public Guid OccurrenceId { get; }
    public Guid RecipientParticipantId { get; }
    public NotificationCategory Category { get; }
    public bool InAppVisible { get; }
    public string Kind { get; }
    public string SubjectKind { get; }
    public Guid SubjectId { get; }
    public Guid ActorParticipantId { get; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset? ReadAt { get; private set; }
    public DateTimeOffset? DismissedAt { get; private set; }
    private readonly List<DeliveryAttempt> _deliveryAttempts;
    public IReadOnlyList<DeliveryAttempt> DeliveryAttempts => _deliveryAttempts.AsReadOnly();

    public Notification(Guid id, Guid occurrenceId, Guid recipientId, Guid actorId, string kind,
        string subjectKind, Guid subjectId, NotificationCategory category, bool inAppVisible,
        DateTimeOffset createdAt, IEnumerable<DeliveryAttempt>? attempts = null)
    {
        if (id == Guid.Empty) throw new ArgumentException("A notification ID is required.");
        var expectedCategory = ValidateRequest(new(occurrenceId, recipientId, actorId, kind, subjectKind, subjectId, createdAt));
        if (!Enum.IsDefined(category) || category != expectedCategory) throw new ArgumentException("Notification category does not match its kind.");
        Id = id; OccurrenceId = occurrenceId; RecipientParticipantId = recipientId; ActorParticipantId = actorId;
        Kind = kind; SubjectKind = subjectKind; SubjectId = subjectId; Category = category;
        InAppVisible = inAppVisible; CreatedAt = createdAt.ToUniversalTime();
        _deliveryAttempts = [];
        foreach (var attempt in attempts ?? []) RecordDelivery(attempt);
    }

    public static NotificationCategory ValidateRequest(NotificationRequestedV1 request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.OccurrenceId == Guid.Empty || request.RecipientParticipantId == Guid.Empty ||
            request.ActorParticipantId == Guid.Empty || request.SubjectId == Guid.Empty)
            throw new ArgumentException("Notification occurrence, recipient, actor and subject IDs are required.");
        if (request.SubjectKind != "Node") throw new ArgumentException("Only Node notification subjects are supported.");
        return request.Kind switch
        {
            "NodeCommented" or "CommentReplied" => NotificationCategory.Discussion,
            "NodeHidden" or "NodeRestored" or "ReportDecided" => NotificationCategory.Moderation,
            _ => throw new ArgumentException("Unsupported notification kind.")
        };
    }

    public void MarkRead(DateTimeOffset at)
    {
        if (ReadAt is not null) return;
        if (at < CreatedAt) throw new ArgumentException("Read time cannot precede creation.");
        ReadAt = at.ToUniversalTime();
    }
    public void Dismiss(DateTimeOffset at)
    {
        if (DismissedAt is not null) return;
        if (at < CreatedAt || at < ReadAt) throw new ArgumentException("Dismissal cannot precede creation or reading.");
        DismissedAt = at.ToUniversalTime();
    }
    public void RecordDelivery(DeliveryAttempt attempt)
    {
        ArgumentNullException.ThrowIfNull(attempt);
        if (attempt.AttemptedAt < CreatedAt || (_deliveryAttempts.Count > 0 && attempt.AttemptedAt < _deliveryAttempts[^1].AttemptedAt))
            throw new ArgumentException("Delivery time cannot precede recorded history.");
        _deliveryAttempts.Add(attempt);
    }
}

/// <summary>Each participant chooses channels per category; defaults favor a quiet in-app feed.</summary>
public sealed class NotificationPreferences
{
    public Guid ParticipantId { get; }
    public NotificationPreferences(Guid participantId)
    {
        if (participantId == Guid.Empty) throw new ArgumentException("A preferences owner is required.");
        ParticipantId = participantId;
    }
    public bool DiscussionInApp { get; set; } = true;
    public bool ModerationInApp { get; set; } = true;
    public bool DiscussionEmail { get; set; }
    public bool ModerationEmail { get; set; }
    public bool DiscussionPush { get; set; }
    public bool ModerationPush { get; set; }
    public bool Enabled(NotificationCategory category, DeliveryChannel? channel)
    {
        if (!Enum.IsDefined(category) || (channel is { } value && !Enum.IsDefined(value))) throw new ArgumentException("Unsupported preference category/channel.");
        return (category, channel) switch
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

/// <summary>Handles notification requests here because Notifications owns the feed, preferences, and delivery records; Console only wires the subscription.</summary>
public sealed class NotificationService(INotificationRepository repository, IEnumerable<INotificationDelivery> deliveries)
{
    public Notification? Handle(NotificationRequestedV1 request)
    {
        var category = Notification.ValidateRequest(request);
        ReferenceValidation.Require(repository, "Participant", request.ActorParticipantId);
        ReferenceValidation.Require(repository, "Participant", request.RecipientParticipantId);
        // Moderation notifications intentionally refer to hidden nodes; existence is still required.
        ReferenceValidation.Require(repository, "Node", request.SubjectId, category == NotificationCategory.Discussion);
        if (request.RecipientParticipantId == request.ActorParticipantId) return null;
        var created = false;
        var item = OperationBoundary.Execute(repository, () =>
        {
        var existing = repository.FindOccurrence(request.OccurrenceId, request.RecipientParticipantId);
        if (existing is not null) return existing;
        var preferences = repository.Preferences(request.RecipientParticipantId);
        if (!preferences.Enabled(category, null) && !Enum.GetValues<DeliveryChannel>()
                .Any(channel => preferences.Enabled(category, channel))) return null;
        var item = new Notification(Guid.NewGuid(), request.OccurrenceId, request.RecipientParticipantId,
            request.ActorParticipantId, request.Kind, request.SubjectKind, request.SubjectId,
            category, preferences.Enabled(category, null), request.OccurredAt);
        // Persist first so an external delivery failure cannot erase the in-app record.
        repository.Save(item);
        created = true;
        return item;
        });
        if (item is null) return null;
        // Existing notifications are returned without repeating external delivery.
        if (!created) return item;
        var preferences = repository.Preferences(request.RecipientParticipantId);
        foreach (var channel in deliveries)
        {
            if (!preferences.Enabled(category, channel.Channel)) continue;
            try
            {
                channel.Send(item);
                item.RecordDelivery(new(channel.Channel, DeliveryStatus.Simulated, AtlasTime.UtcNow, null));
            }
            catch (Exception exception)
            {
                item.RecordDelivery(new(channel.Channel, DeliveryStatus.Failed, AtlasTime.UtcNow, exception.Message));
            }
            repository.Save(item);
        }
        return item;
    }

    public IReadOnlyList<Notification> Page(Guid recipientId, int offset, int limit)
    {
        ReferenceValidation.Require(repository, "Participant", recipientId);
        if (offset < 0 || limit is < 1 or > 100) throw new ArgumentOutOfRangeException(nameof(limit));
        return repository.Page(recipientId, offset, limit);
    }

    public NotificationPreferences Preferences(Guid actorParticipantId, Guid participantId)
    {
        EnsureOwner(actorParticipantId, participantId);
        return repository.Preferences(participantId);
    }

    public void SavePreferences(Guid actorParticipantId, NotificationPreferences preferences)
    {
        ArgumentNullException.ThrowIfNull(preferences);
        EnsureOwner(actorParticipantId, preferences.ParticipantId);
        repository.SavePreferences(preferences);
    }

    public void MarkRead(Guid recipientId, Guid notificationId)
    {
        var item = Own(recipientId, notificationId);
        item.MarkRead(AtlasTime.UtcNow);
        repository.Save(item);
    }

    public void Dismiss(Guid recipientId, Guid notificationId)
    {
        var item = Own(recipientId, notificationId);
        item.Dismiss(AtlasTime.UtcNow);
        repository.Save(item);
    }

    private void EnsureOwner(Guid actorParticipantId, Guid participantId)
    {
        ReferenceValidation.Require(repository, "Participant", actorParticipantId);
        ReferenceValidation.Require(repository, "Participant", participantId);
        if (actorParticipantId != participantId)
            throw new UnauthorizedAccessException("Notification preferences are unavailable to this participant.");
    }

    private Notification Own(Guid recipientId, Guid id)
    {
        ReferenceValidation.Require(repository, "Participant", recipientId);
        if (id == Guid.Empty) throw new ArgumentException("A notification ID is required.");
        var item = repository.Find(id);
        if (item is null || item.RecipientParticipantId != recipientId)
            throw new UnauthorizedAccessException("Notification is unavailable to this participant.");
        return item;
    }
}
