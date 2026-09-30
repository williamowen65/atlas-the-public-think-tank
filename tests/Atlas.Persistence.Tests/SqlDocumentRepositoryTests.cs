using Atlas.ConsoleApp.Storage;
using Atlas.Content.Blocks;
using Atlas.Content.Documents;
namespace Atlas.Persistence.Tests;
[TestClass]
public sealed class SqlDocumentRepositoryTests
{
    [TestMethod]
    public void Reordering_one_document_preserves_unrelated_document_and_child_order()
    {
        using var database = SqlTestDatabase.Create();
        var repository = new SqlDocumentRepository(database.Open);
        var now = DateTimeOffset.UtcNow;
        var first = new MarkdownTextBlock("First", now);
        var second = new MarkdownTextBlock("Second", now);
        repository.SaveBlock(first); repository.SaveBlock(second);
        var document = new Document([first.Id, second.Id], now);
        var unrelated = new Document([first.Id], now);
        repository.Save(document); repository.Save(unrelated);
        document.MoveBlock(second.Id, 0, now.AddMinutes(1));
        repository.Save(document);
        var reloaded = new SqlDocumentRepository(database.Open);
        CollectionAssert.AreEqual(new[] { second.Id, first.Id }, reloaded.GetById(document.Id)!.BlockIds.ToArray());
        CollectionAssert.AreEqual(new[] { first.Id }, reloaded.GetById(unrelated.Id)!.BlockIds.ToArray());
    }
    [TestMethod]
    public void Mixed_blocks_preserve_order_identity_types_and_timestamps()
    {
        using var database = SqlTestDatabase.Create();

            var createdAt = new DateTimeOffset(2026, 9, 19, 12, 0, 0, TimeSpan.Zero);
            var blocks = new ContentBlock[]
            {
                new MarkdownTextBlock("## Markdown heading", createdAt),
                new ImageBlock("media/image-1", "Alt text", "Caption", createdAt),
                new VideoBlock("media/video-1", "Video caption", createdAt),
                new LinkPreviewBlock("https://example.com/article", "Article", "Preview", createdAt),
                new PollReferenceBlock(Guid.NewGuid(), createdAt),
                new ChartReferenceBlock(Guid.NewGuid(), "Baseline chart", createdAt)
            };
            var document = new Document(blocks.Select(block => block.Id), createdAt);
            var repository = new SqlDocumentRepository(database.Open);

            foreach (var block in blocks) repository.SaveBlock(block);
            repository.Save(document);

            var reloaded = new SqlDocumentRepository(database.Open);
            var reloadedDocument = reloaded.GetById(document.Id);

            Assert.IsNotNull(reloadedDocument);
            CollectionAssert.AreEqual(document.BlockIds.ToArray(), reloadedDocument.BlockIds.ToArray());
            Assert.AreEqual(document.UpdatedAt, reloadedDocument.UpdatedAt);
            CollectionAssert.AreEqual(
                blocks.Select(block => block.GetType()).ToArray(),
                reloaded.GetBlocks(reloadedDocument).Select(block => block.GetType()).ToArray());
            Assert.AreEqual(blocks[0].Id, reloaded.GetBlocks(reloadedDocument).First().Id);
            }
}
