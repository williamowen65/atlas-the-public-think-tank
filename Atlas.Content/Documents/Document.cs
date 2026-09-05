namespace Atlas.Content.Documents;

public sealed class Document
{
    public DocumentId Id { get; }
    public Guid NodeId { get; }
    public string Content { get; private set; }
    public DateTimeOffset CreatedAt { get; }

    public Document(
        Guid nodeId,
        string initialContent,
        DateTimeOffset createdAt)
    {
        if (nodeId == Guid.Empty)
        {
            throw new ArgumentException(
                "A document must reference a node.",
                nameof(nodeId));
        }

        Id = DocumentId.New();
        NodeId = nodeId;
        Content = initialContent?.Trim() ?? string.Empty;
        CreatedAt = createdAt;
    }
}