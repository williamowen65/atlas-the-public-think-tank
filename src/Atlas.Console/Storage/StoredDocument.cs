namespace Atlas.ConsoleApp.Storage;

/// <summary>Defines the data-only JSON representation of a Content document.</summary>
public sealed class StoredDocument
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}
