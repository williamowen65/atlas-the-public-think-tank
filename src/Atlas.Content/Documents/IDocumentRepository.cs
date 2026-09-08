namespace Atlas.Content.Documents;

/// <summary>Defines the persistence operations required by the Content boundary.</summary>
public interface IDocumentRepository
{
    /// <summary>Loads all persisted domain objects.</summary>
    IReadOnlyCollection<Document> GetAll();
    /// <summary>Loads a domain object by its boundary-owned identifier.</summary>
    Document? GetById(DocumentId id);
    /// <summary>Persists the current domain-object state.</summary>
    void Save(Document document);
}