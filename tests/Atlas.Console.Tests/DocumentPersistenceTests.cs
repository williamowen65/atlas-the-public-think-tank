using Atlas.Content.Blocks;
using Atlas.Content.Documents;

namespace Atlas.ConsoleApp.Tests;

[TestClass]
public sealed class DocumentPersistenceTests
{
    [TestMethod]
    public void NewBlocksReceiveDistinctGeneratedGuids()
    {
        var now = DateTimeOffset.UtcNow;

        var first = new MarkdownTextBlock("First", now);
        var second = new MarkdownTextBlock("Second", now);

        Assert.AreNotEqual(Guid.Empty, first.Id.Value);
        Assert.AreNotEqual(Guid.Empty, second.Id.Value);
        Assert.AreNotEqual(first.Id, second.Id);
    }

    [TestMethod]
    public void PollAndChartReferenceIdsAreGeneratedByTheContentDomain()
    {
        var now = DateTimeOffset.UtcNow;

        var poll = new PollReferenceBlock(now);
        var chart = new ChartReferenceBlock("Participation", now);

        Assert.AreNotEqual(Guid.Empty, poll.PollId);
        Assert.AreNotEqual(Guid.Empty, chart.ChartId);
        Assert.AreNotEqual(poll.PollId, chart.ChartId);
    }

    [TestMethod]
    public void DocumentCompositionPreservesStableBlockIdsWhenReordered()
    {
        var createdAt = new DateTimeOffset(2026, 9, 19, 12, 0, 0, TimeSpan.Zero);
        var first = new MarkdownTextBlock("# Heading\n\nBody with **bold** text.", createdAt);
        var second = new ImageBlock("media/image-1", "A coastal landscape", "Optional caption", createdAt);
        var document = new Document([first.Id, second.Id], createdAt);
        var originalDocumentId = document.Id;

        var changedAt = createdAt.AddMinutes(1);
        document.MoveBlock(second.Id, 0, changedAt);

        Assert.AreEqual(originalDocumentId, document.Id);
        Assert.AreEqual(changedAt, document.UpdatedAt);
        CollectionAssert.AreEqual(
            new[] { second.Id, first.Id },
            document.BlockIds.ToArray());
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

    [TestMethod]
    public void CompositionChangesAdvanceDocumentUpdatedAt()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var first = new MarkdownTextBlock("First", createdAt);
        var second = new MarkdownTextBlock("Second", createdAt);
        var document = new Document([first.Id], createdAt);

        var addedAt = createdAt.AddMinutes(1);
        document.AddBlock(second.Id, addedAt);
        Assert.AreEqual(addedAt, document.UpdatedAt);

        var removedAt = addedAt.AddMinutes(1);
        document.RemoveBlock(first.Id, removedAt);
        Assert.AreEqual(removedAt, document.UpdatedAt);
    }
}
