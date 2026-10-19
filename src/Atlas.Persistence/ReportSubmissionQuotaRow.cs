namespace Atlas.Persistence;

/// <summary>Committed report submission at server accounting time, independent of moderation outcome.</summary>
/// <remarks>No case FK: resolving or deleting a report must not refund submission capacity.</remarks>
public sealed class ReportSubmissionQuotaRow
{
    public Guid Id { get; set; }
    public Guid ParticipantId { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
}
