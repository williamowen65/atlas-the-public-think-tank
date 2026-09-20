namespace Atlas.ConsoleApp.Storage;

/// <summary>Defines the data-only JSON representation of a reusable reaction definition.</summary>
public sealed class StoredReactionDefinition
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public string Emoji { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string NormalizedText { get; set; } = string.Empty;
    public Guid CreatedByParticipantId { get; set; }
    public bool IsSuppressed { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
