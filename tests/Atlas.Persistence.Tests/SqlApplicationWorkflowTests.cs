using Atlas.Application;
using Atlas.ConsoleApp.Storage;
using Atlas.Content.Blocks;
using Atlas.Content.Documents;
using Atlas.Contracts.Operations;
using Atlas.Graph.Nodes;
using Atlas.Graph.Nodes.NodeTypes;
using Creation = Atlas.Application.NodeCreationService;

namespace Atlas.Persistence.Tests;

[TestClass]
public sealed class SqlApplicationWorkflowTests
{
    [TestMethod]
    public void DescriptionChangesRequireOwnerAndDoNotLeaveOrphanBlocksOnFailedSave()
    {
        using var database = SqlTestDatabase.Create();
        var author = database.AddParticipant();
        var stranger = database.AddParticipant();
        var types = new SqlNodeTypeRepository(database.Open);
        var type = new NodeTypeCreationService(types).Create("Proposal", "Proposals", author.ToString(), AtlasTime.UtcNow);
        var nodes = new SqlNodeRepository(database.Open);
        var documents = new SqlDocumentRepository(database.Open);
        var node = new Creation(nodes, documents).Create("Title", "Original", type.Id, author, []);
        var service = new NodeDescriptionService(nodes, documents);
        Assert.Throws<UnauthorizedAccessException>(() => service.Add(node.Id, stranger, new MarkdownInput("Forbidden")));
        using (var check = database.Open()) Assert.AreEqual(1, check.BlockRows.Count());

        // Content saves the new block, then this adapter fails the document save.
        // The outer node operation must roll back the earlier block insert as well.
        var failing = new NodeDescriptionService(nodes, new FailingDocumentSave(documents));
        Assert.Throws<InvalidOperationException>(() => failing.Add(node.Id, author, new MarkdownInput("Must roll back")));
        using (var check = database.Open()) Assert.AreEqual(1, check.BlockRows.Count());
        Assert.HasCount(1, documents.GetById(new DocumentId(node.DescriptionId.Value))!.BlockIds);

        var added = service.Add(node.Id, author, new MarkdownInput("Added"), 0);
        service.Update(node.Id, author, added.Id, new MarkdownInput("Updated"));
        service.Move(node.Id, author, added.Id, 1);
        Assert.AreEqual(added.Id, documents.GetById(new DocumentId(node.DescriptionId.Value))!.BlockIds.Last());
        service.Remove(node.Id, author, added.Id);
        Assert.IsNotNull(documents.GetBlockById(added.Id));
        Assert.HasCount(1, documents.GetById(new DocumentId(node.DescriptionId.Value))!.BlockIds);
    }

    private sealed class FailingDocumentSave(SqlDocumentRepository inner) : IDocumentRepository, IOperationBoundary
    {
        public T Execute<T>(Func<T> operation) => inner.Execute(operation);
        public IReadOnlyCollection<Document> GetAll() => inner.GetAll();
        public Document? GetById(DocumentId id) => inner.GetById(id);
        public void Save(Document document) => throw new InvalidOperationException("Injected document failure");
        public ContentBlock? GetBlockById(BlockId id) => inner.GetBlockById(id);
        public IReadOnlyCollection<ContentBlock> GetBlocks(Document document) => inner.GetBlocks(document);
        public void SaveBlock(ContentBlock block) => inner.SaveBlock(block);
    }
}
