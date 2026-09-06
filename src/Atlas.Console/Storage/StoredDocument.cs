namespace Atlas.ConsoleApp.Storage;

public sealed class StoredDocument
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}
