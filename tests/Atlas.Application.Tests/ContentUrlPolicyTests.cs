using Atlas.Content.Blocks;

namespace Atlas.Application.Tests;

/// <summary>URL form checks apply to creation, edits and persisted-state reconstitution.</summary>
[TestClass]
public sealed class ContentUrlPolicyTests
{
    private static readonly DateTimeOffset At = DateTimeOffset.UtcNow;

    [TestMethod]
    [DataRow("javascript:alert(1)")]
    [DataRow("data:text/html,<script>alert(1)</script>")]
    [DataRow("vbscript:msgbox(1)")]
    [DataRow("file:///etc/passwd")]
    [DataRow("ftp://example.com/file")]
    [DataRow("//example.com/path")]
    [DataRow("https://user:password@example.com/path")]
    [DataRow("https://example.com/\npath")]
    [DataRow("https:\\example.com\\path")]
    [DataRow("%2f%2fexample.com")]
    public void UnsafeUrlFormsAreRejectedWithoutChangingExistingBlocks(string url)
    {
        Assert.Throws<ArgumentException>(() => new LinkPreviewBlock(url, "Link", null, At));
        Assert.Throws<ArgumentException>(() => new ImageBlock(url, "Alt", null, At));
        Assert.Throws<ArgumentException>(() => new VideoBlock(url, null, At));
        var link = new LinkPreviewBlock("https://example.com/original", "Original", null, At);
        var image = new ImageBlock("media/original", "Original", null, At);
        var video = new VideoBlock("media/original", null, At);
        Assert.Throws<ArgumentException>(() => link.UpdatePreview(url, "Changed", null, At.AddMinutes(1)));
        Assert.Throws<ArgumentException>(() => image.Update(url, "Changed", null, At.AddMinutes(1)));
        Assert.Throws<ArgumentException>(() => video.Update(url, "Changed", At.AddMinutes(1)));
        Assert.AreEqual("https://example.com/original", link.Url.AbsoluteUri);
        Assert.AreEqual("Original", link.Title);
        Assert.AreEqual("media/original", image.Url);
        Assert.AreEqual("Original", image.AltText);
        Assert.AreEqual("media/original", video.Url);
        Assert.AreEqual(string.Empty, video.Caption);
        Assert.AreEqual(At, link.UpdatedAt);
        Assert.AreEqual(At, image.UpdatedAt);
        Assert.AreEqual(At, video.UpdatedAt);
    }

    [TestMethod]
    [DataRow("https://example.com/media?q=1#part")]
    [DataRow("http://example.com/media")]
    public void AbsoluteWebUrlsRemainSupported(string url)
    {
        Assert.AreEqual(url, new LinkPreviewBlock(url, "Link", null, At).Url.OriginalString);
        Assert.AreEqual(url, new ImageBlock(url, "Alt", null, At).Url);
        Assert.AreEqual(url, new VideoBlock(url, null, At).Url);
    }

    [TestMethod]
    [DataRow("media/image-1.png")]
    [DataRow("/media/image-1.png")]
    public void RelativeMediaPathsRemainSupportedButLinkPreviewsRequireAbsoluteUrls(string path)
    {
        Assert.AreEqual(path, new ImageBlock(path, "Alt", null, At).Url);
        Assert.AreEqual(path, new VideoBlock(path, null, At).Url);
        Assert.Throws<ArgumentException>(() => new LinkPreviewBlock(path, "Link", null, At));
    }

    [TestMethod]
    public void ReconstitutionCannotBypassUrlPolicyAndOversizedUrlsAreRejected()
    {
        Assert.Throws<ArgumentException>(() => LinkPreviewBlock.Reconstitute(BlockId.New(), "javascript:bad", "Link", null, At, At));
        Assert.Throws<ArgumentException>(() => ImageBlock.Reconstitute(BlockId.New(), "data:image/svg+xml,bad", "Alt", null, At, At));
        Assert.Throws<ArgumentException>(() => VideoBlock.Reconstitute(BlockId.New(), "file:///tmp/video", null, At, At));
        var oversized = "https://example.com/" + new string('x', 500);
        Assert.Throws<ArgumentException>(() => new LinkPreviewBlock(oversized, "Link", null, At));
        Assert.Throws<ArgumentException>(() => new ImageBlock(oversized, "Alt", null, At));
        Assert.Throws<ArgumentException>(() => new VideoBlock(oversized, null, At));
    }
}
