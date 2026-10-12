namespace Atlas.Persistence;

/// <summary>One ledger entry: a record of a successful Node creation that consumed quota.</summary>
/// <remarks>NodeCreationQuotaRows stores this quota history independently of Node lifecycle.
/// Trusted exempt creations add no entry; transaction rollback removes attempted consumption.
/// CreatedAt is the accounting timestamp, not a user-supplied Node timestamp.</remarks>
public sealed class NodeCreationQuotaRow
{
    public Guid NodeId { get; set; }
    public Guid ParticipantId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
