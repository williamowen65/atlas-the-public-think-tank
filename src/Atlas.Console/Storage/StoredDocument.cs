namespace Atlas.ConsoleApp.Storage;

public sealed class StoredDocument
{
    public Guid Id { get; set; }
    public List<Guid> BlockIds { get; set; } = [];
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
