namespace Atlas.Content.Documents;

public sealed class Document
{
    public DocumentId Id { get; }
    public string Content { get; private set; }
    public DateTimeOffset CreatedAt { get; }

    public Document(
        string initialContent,
        DateTimeOffset createdAt)
    {
        Id = DocumentId.New();
        Content = initialContent?.Trim() ?? string.Empty;
        CreatedAt = createdAt;
    }

    private Document(
        DocumentId id,
        string content,
        DateTimeOffset createdAt)
    {
        Id = id;
        Content = content?.Trim() ?? string.Empty;
        CreatedAt = createdAt;
    }

    public static Document Reconstitute(
        DocumentId id,
        string content,
        DateTimeOffset createdAt)
    {
        return new Document(id, content, createdAt);
    }
}