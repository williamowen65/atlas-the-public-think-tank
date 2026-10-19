namespace Atlas.Persistence;

/// <summary>Successful voting-action consumption, independent of the current vote's lifetime.</summary>
/// <remarks>No vote/target payload or cascading vote FK: undo must not erase consumption.
/// OccurredAt is server accounting time, never a supplied vote timestamp.</remarks>
public sealed class VoteActionQuotaRow
{
    public Guid Id { get; set; }
    public Guid ParticipantId { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
}
