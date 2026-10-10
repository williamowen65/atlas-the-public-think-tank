using Atlas.Content.Blocks;
using Atlas.Content.Documents;

namespace Atlas.ConsoleApp.Tests;

[TestClass]
public sealed class ContentValidationTests
{
    private static readonly DateTimeOffset Created = new(2026, 10, 3, 12, 0, 0, TimeSpan.Zero);

    [TestMethod]
    public void NullCompositionIdsAreRejectedBeforeChangingDocument()
    {
        Assert.Throws<ArgumentException>(() => new Document([null!], Created));
        Assert.Throws<ArgumentNullException>(() => Document.Reconstitute(null!, [], Created, Created));
        var document = new Document([], Created);
        Assert.Throws<ArgumentNullException>(() => document.AddBlock(null!, Created.AddMinutes(1)));
        Assert.AreEqual(0, document.BlockIds.Count);
        Assert.AreEqual(Created, document.UpdatedAt);
    }

    [TestMethod]
    public void InvalidPayloadDoesNotPartiallyUpdateBlocks()
    {
        var image = new ImageBlock("original", "alt", "caption", Created);
        Assert.Throws<ArgumentException>(() => image.Update("replacement", "", null, Created.AddMinutes(1)));
        Assert.AreEqual("original", image.Url);
        Assert.AreEqual("alt", image.AltText);
        Assert.AreEqual("caption", image.Caption);
        Assert.AreEqual(Created, image.UpdatedAt);

        var link = new LinkPreviewBlock("https://example.com/old", "title", "description", Created);
        Assert.Throws<ArgumentException>(() => link.UpdatePreview("https://example.com/new", "new", new string('x', 2001), Created.AddMinutes(1)));
        Assert.AreEqual("https://example.com/old", link.Url.AbsoluteUri);
        Assert.AreEqual("title", link.Title);
        Assert.AreEqual("description", link.Description);
        Assert.AreEqual(Created, link.UpdatedAt);

        var chartId = Guid.NewGuid();
        var chart = new ChartReferenceBlock(chartId, "title", Created);
        Assert.Throws<ArgumentException>(() => chart.Update(Guid.NewGuid(), new string('x', 501), Created.AddMinutes(1)));
        Assert.AreEqual(chartId, chart.ChartId);
        Assert.AreEqual("title", chart.Title);
        Assert.AreEqual(Created, chart.UpdatedAt);

        var video = new VideoBlock("original", "caption", Created);
        Assert.Throws<ArgumentException>(() => video.Update("new", new string('x', 2001), Created.AddMinutes(1)));
        Assert.AreEqual("original", video.Url);
        Assert.AreEqual("caption", video.Caption);
        Assert.AreEqual(Created, video.UpdatedAt);
    }

    [TestMethod]
    public void StaleTimeDoesNotChangeAnyBlockPayload()
    {
        var image = new ImageBlock("old", "alt", "caption", Created);
        var link = new LinkPreviewBlock("https://example.com/old", "title", "description", Created);
        var text = new MarkdownTextBlock("old", Created);
        var chart = new ChartReferenceBlock(Guid.NewGuid(), "old", Created);
        var poll = new PollReferenceBlock(Guid.NewGuid(), Created);
        var video = new VideoBlock("old", "caption", Created);
        var chartId = chart.ChartId;
        var pollId = poll.PollId;
        var changed = Created.AddMinutes(2);
        image.Update("old", "alt", "caption", changed);
        link.UpdatePreview("https://example.com/old", "title", "description", changed);
        text.Update("old", changed);
        chart.Update(chartId, "old", changed);
        poll.UpdateReference(pollId, changed);
        video.Update("old", "caption", changed);
        var stale = changed.AddMinutes(-1);

        Assert.Throws<ArgumentException>(() => image.Update("new", "new", "new", stale));
        Assert.Throws<ArgumentException>(() => link.UpdatePreview("https://example.com/new", "new", "new", stale));
        Assert.Throws<ArgumentException>(() => text.Update("new", stale));
        Assert.Throws<ArgumentException>(() => chart.Update(Guid.NewGuid(), "new", stale));
        Assert.Throws<ArgumentException>(() => poll.UpdateReference(Guid.NewGuid(), stale));
        Assert.Throws<ArgumentException>(() => video.Update("new", "new", stale));
        Assert.AreEqual("old", image.Url);
        Assert.AreEqual("alt", image.AltText);
        Assert.AreEqual("caption", image.Caption);
        Assert.AreEqual("https://example.com/old", link.Url.AbsoluteUri);
        Assert.AreEqual("title", link.Title);
        Assert.AreEqual("description", link.Description);
        Assert.AreEqual("old", text.Markdown);
        Assert.AreEqual(chartId, chart.ChartId);
        Assert.AreEqual("old", chart.Title);
        Assert.AreEqual(pollId, poll.PollId);
        Assert.AreEqual("old", video.Url);
        Assert.AreEqual("caption", video.Caption);
        foreach (var block in new ContentBlock[] { image, link, text, chart, poll, video })
            Assert.AreEqual(changed, block.UpdatedAt);
    }

    [TestMethod]
    public void StaleTimeDoesNotChangeDocumentComposition()
    {
        var first = BlockId.New();
        var second = BlockId.New();
        var third = BlockId.New();
        var changed = Created.AddMinutes(2);
        var document = Document.Reconstitute(DocumentId.New(), [first, second], Created, changed);
        var stale = changed.AddMinutes(-1);
        Assert.Throws<ArgumentException>(() => document.AddBlock(third, stale));
        Assert.Throws<ArgumentException>(() => document.MoveBlock(second, 0, stale));
        Assert.Throws<ArgumentException>(() => document.RemoveBlock(first, stale));
        CollectionAssert.AreEqual(new[] { first, second }, document.BlockIds.ToArray());
        Assert.AreEqual(changed, document.UpdatedAt);
    }
}
