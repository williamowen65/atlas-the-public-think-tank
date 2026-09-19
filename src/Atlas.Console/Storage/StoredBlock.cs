namespace Atlas.ConsoleApp.Storage;

/// <summary>
/// Serialization DTO for heterogeneous block records. Nullable properties are
/// omitted from JSON, so each stored record contains only its concrete payload.
/// Domain validation remains in the Content block classes.
/// </summary>
public sealed class StoredBlock
{
    public Guid Id { get; set; }
    public string Kind { get; set; } = string.Empty;
    public string? Markdown { get; set; }
    public string? AltText { get; set; }
    public string? Caption { get; set; }
    public string? Url { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public Guid? ReferenceId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
