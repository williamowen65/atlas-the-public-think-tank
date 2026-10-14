namespace Atlas.Persistence.Observability;

/// <summary>Append-only evidence of an important persisted transition. Payload is an allowlisted event, not an aggregate dump.</summary>
public sealed class AuditRecordRow
{
    public Guid Id { get; set; }
    public DateTimeOffset OccurredAtUtc { get; set; }
    public Guid ObservabilityOperationId { get; set; }
    public Guid? ActorId { get; set; }
    public Guid? TargetId { get; set; }
    public string EventName { get; set; } = "";
    public string PayloadJson { get; set; } = "";
}

/// <summary>Independent delivery state. Pending records are never deleted by file retention or retry limits.</summary>
public sealed class AuditOutboxRow
{
    public Guid Id { get; set; }
    public Guid AuditRecordId { get; set; }
    public string Destination { get; set; } = "local";
    public DateTimeOffset? DeliveredAtUtc { get; set; }
    public DateTimeOffset NextAttemptAtUtc { get; set; }
    public int Attempts { get; set; }
    public Guid? LeaseOwner { get; set; }
    public DateTimeOffset? LeaseUntilUtc { get; set; }
}
