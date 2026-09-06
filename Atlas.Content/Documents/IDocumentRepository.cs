namespace Atlas.Content.Documents;

public interface IDocumentRepository
{
    IReadOnlyCollection<Document> GetAll();
    Document? GetById(DocumentId id);
    void Save(Document document);
}