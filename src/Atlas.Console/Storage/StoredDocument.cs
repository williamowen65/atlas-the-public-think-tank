namespace Atlas.ConsoleApp.Storage;

/// <summary>
/// Serialization DTO containing document metadata and ordered block references;
/// block payloads are deliberately stored in the separate block file.
/// </summary>
public sealed class StoredDocument
{
    public Guid Id { get; set; }
    public List<Guid> BlockIds { get; set; } = [];
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
