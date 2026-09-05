using Atlas.Content.Documents;

namespace Atlas.ConsoleApp.Content;

public sealed class InMemoryDocumentRepository : IDocumentRepository
{
    private readonly List<Document> _documents = [];

    public IReadOnlyCollection<Document> GetAll()
    {
        return _documents.AsReadOnly();
    }

    public Document? GetById(DocumentId id)
    {
        return _documents.SingleOrDefault(
            document => document.Id == id);
    }

    public Document? GetByNodeId(Guid nodeId)
    {
        return _documents.SingleOrDefault(
            document => document.NodeId == nodeId);
    }

    public void Save(Document document)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (GetByNodeId(document.NodeId) is not null)
        {
            throw new InvalidOperationException(
                $"Node {document.NodeId} already has a content document.");
        }

        _documents.Add(document);
    }
}
