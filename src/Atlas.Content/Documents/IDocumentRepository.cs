using Atlas.Content.Blocks;

namespace Atlas.Content.Documents;

public interface IDocumentRepository
{
    IReadOnlyCollection<Document> GetAll();
    Document? GetById(DocumentId id);
    void Save(Document document);
    ContentBlock? GetBlockById(BlockId id);
    IReadOnlyCollection<ContentBlock> GetBlocks(Document document);
    void SaveBlock(ContentBlock block);
}
