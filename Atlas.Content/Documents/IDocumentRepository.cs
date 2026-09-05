namespace Atlas.Content.Documents;

public interface IDocumentRepository
{
    IReadOnlyCollection<Document> GetAll();
    Document? GetById(DocumentId id);
    Document? GetByNodeId(Guid nodeId);
    void Save(Document document);
}