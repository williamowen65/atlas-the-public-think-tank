namespace Atlas.Contracts.Notifications.V1;

/// <summary>Stable, recipient-specific integration request; the source owns the occurrence ID.</summary>
public sealed record NotificationRequestedV1(
    Guid OccurrenceId, Guid RecipientParticipantId, Guid ActorParticipantId,
    string Kind, string SubjectKind, Guid SubjectId, DateTimeOffset OccurredAt);
