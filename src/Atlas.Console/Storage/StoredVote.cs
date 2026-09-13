namespace Atlas.ConsoleApp.Storage;

/// <summary>Defines the data-only JSON representation of a vote.</summary>
public sealed class StoredVote
{
    public Guid Id { get; set; }

    public Guid ParticipantId { get; set; }

    public Guid TargetId { get; set; }

    public string TargetType { get; set; } = string.Empty;

    public int Value { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}