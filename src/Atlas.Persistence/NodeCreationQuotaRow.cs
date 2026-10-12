namespace Atlas.Persistence;

/// <summary>Durable successful-creation accounting, independent of Node archival/deletion.</summary>
public sealed class NodeCreationQuotaRow
{
    public Guid NodeId { get; set; }
    public Guid ParticipantId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
