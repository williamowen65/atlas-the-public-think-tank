namespace Atlas.Content.Documents;

/// <summary>Represents a Content-owned description document referenced by a Graph node.</summary>
public sealed class Document
{
    public DocumentId Id { get; }
    public string Content { get; private set; }
    public DateTimeOffset CreatedAt { get; }

    /// <summary>Creates a validated document instance.</summary>
    public Document(
        string initialContent,
        DateTimeOffset createdAt)
    {
        Id = DocumentId.New();
        Content = initialContent?.Trim() ?? string.Empty;
        CreatedAt = createdAt;
    }

    /// <summary>Creates a validated document instance.</summary>
    private Document(
        DocumentId id,
        string content,
        DateTimeOffset createdAt)
    {
        Id = id;
        Content = content?.Trim() ?? string.Empty;
        CreatedAt = createdAt;
    }

    /// <summary>Rebuilds the domain object from persisted state without replaying creation behavior.</summary>
    public static Document Reconstitute(
        DocumentId id,
        string content,
        DateTimeOffset createdAt)
    {
        return new Document(id, content, createdAt);
    }
}