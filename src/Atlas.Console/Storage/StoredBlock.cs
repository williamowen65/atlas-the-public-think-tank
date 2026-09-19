namespace Atlas.ConsoleApp.Storage;

public sealed class StoredBlock
{
    public Guid Id { get; set; }
    public string Kind { get; set; } = string.Empty;
    public string? Markdown { get; set; }
    public string? ResourceId { get; set; }
    public string? AltText { get; set; }
    public string? Caption { get; set; }
    public string? Url { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public Guid? ReferenceId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
