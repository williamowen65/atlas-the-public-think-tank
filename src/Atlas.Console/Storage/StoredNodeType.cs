namespace Atlas.ConsoleApp.Storage;

public sealed class StoredNodeType
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? OwnerId { get; set; }
    public bool IsSystemDefined { get; set; }
    public bool IsArchived { get; set; }
    public bool? AutoPluralize { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
