using Atlas.ConsoleApp.Storage;
using Atlas.Content.Blocks;
using Atlas.Content.Documents;

namespace Atlas.ConsoleApp.Tests;

[TestClass]
public sealed class DocumentPersistenceTests
{
    [TestMethod]
    public void DocumentCompositionPreservesStableBlockIdsWhenReordered()
    {
        var createdAt = new DateTimeOffset(2026, 9, 19, 12, 0, 0, TimeSpan.Zero);
        var first = new MarkdownTextBlock("# Heading\n\nBody with **bold** text.", createdAt);
        var second = new ImageBlock("media/image-1", "A coastal landscape", "Optional caption", createdAt);
        var document = new Document([first.Id, second.Id], createdAt);
        var originalDocumentId = document.Id;

        document.MoveBlock(second.Id, 0);

        Assert.AreEqual(originalDocumentId, document.Id);
        CollectionAssert.AreEqual(
            new[] { second.Id, first.Id },
            document.BlockIds.ToArray());
    }

    [TestMethod]
    public void MixedBlocksRoundTripThroughSeparateJsonFiles()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"atlas-content-tests-{Guid.NewGuid():N}");
        var documentPath = Path.Combine(directory, "documents.json");
        var blockPath = Path.Combine(directory, "blocks.json");

        try
        {
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
            var repository = new JsonDocumentRepository(documentPath, blockPath);

            foreach (var block in blocks) repository.SaveBlock(block);
            repository.Save(document);

            var reloaded = new JsonDocumentRepository(documentPath, blockPath);
            var reloadedDocument = reloaded.GetById(document.Id);

            Assert.IsNotNull(reloadedDocument);
            CollectionAssert.AreEqual(document.BlockIds.ToArray(), reloadedDocument.BlockIds.ToArray());
            CollectionAssert.AreEqual(
                blocks.Select(block => block.GetType()).ToArray(),
                reloaded.GetBlocks(reloadedDocument).Select(block => block.GetType()).ToArray());
            Assert.AreEqual(blocks[0].Id, reloaded.GetBlocks(reloadedDocument).First().Id);
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        }
    }

    [TestMethod]
    public void BlockTypesEnforceTheirOwnValidation()
    {
        var now = DateTimeOffset.UtcNow;

        Assert.Throws<ArgumentException>(() => new ImageBlock("", "Alt", null, now));
        Assert.Throws<ArgumentException>(() => new ImageBlock("media/image", "", null, now));
        Assert.Throws<ArgumentException>(() => new LinkPreviewBlock("relative/path", "Title", null, now));
        Assert.Throws<ArgumentException>(() => new PollReferenceBlock(Guid.Empty, now));
        Assert.Throws<ArgumentException>(() => new ChartReferenceBlock(Guid.Empty, null, now));
    }

    [TestMethod]
    public void EditingMarkdownPreservesBlockIdentity()
    {
        var now = DateTimeOffset.UtcNow;
        var block = new MarkdownTextBlock("Original", now);
        var id = block.Id;

        block.Update("# Updated\n\n*Markdown*", now.AddMinutes(1));

        Assert.AreEqual(id, block.Id);
        Assert.AreEqual("# Updated\n\n*Markdown*", block.Markdown);
    }
}
