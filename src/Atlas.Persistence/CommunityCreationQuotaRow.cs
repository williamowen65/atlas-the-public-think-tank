namespace Atlas.Persistence;

/// <summary>Committed community creation at server accounting time, independent of community lifecycle.</summary>
/// <remarks>No community FK: archiving or deleting a community must not refund its slot.</remarks>
public sealed class CommunityCreationQuotaRow
{
    public Guid Id { get; set; }
    public Guid ParticipantId { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
}
