namespace Atlas.ConsoleApp.Storage;

public sealed class StoredNode
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public Guid? TypeId { get; set; }

    // Retained temporarily so existing enum-based JSON can be read.
    public string? Type { get; set; }

    public string Status { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
