namespace Atlas.Content.Blocks;

public sealed class MarkdownTextBlock : ContentBlock
{
    public override string Kind => "markdown";
    public string Markdown { get; private set; }

    public MarkdownTextBlock(string markdown, DateTimeOffset createdAt)
        : this(BlockId.New(), markdown, createdAt, createdAt) { }

    private MarkdownTextBlock(BlockId id, string markdown, DateTimeOffset createdAt, DateTimeOffset updatedAt)
        : base(id, createdAt, updatedAt)
    {
        Markdown = Optional(markdown, 100_000, nameof(markdown));
    }

    public void Update(string markdown, DateTimeOffset changedAt)
    {
        Markdown = Optional(markdown, 100_000, nameof(markdown));
        ChangedAt(changedAt);
    }

    public static MarkdownTextBlock Reconstitute(BlockId id, string markdown, DateTimeOffset createdAt, DateTimeOffset updatedAt) =>
        new(id, markdown, createdAt, updatedAt);
}

public sealed class ImageBlock : ContentBlock
{
    public override string Kind => "image";
    public string Url { get; private set; }
    public string AltText { get; private set; }
    public string Caption { get; private set; }

    public ImageBlock(string url, string altText, string? caption, DateTimeOffset createdAt)
        : this(BlockId.New(), url, altText, caption, createdAt, createdAt) { }

    private ImageBlock(BlockId id, string url, string altText, string? caption, DateTimeOffset createdAt, DateTimeOffset updatedAt)
        : base(id, createdAt, updatedAt)
    {
        Url = Required(url, nameof(url), 500);
        AltText = Required(altText, nameof(altText), 2_000);
        Caption = Optional(caption, 2_000, nameof(caption));
    }

    public void Update(string url, string altText, string? caption, DateTimeOffset changedAt)
    {
        Url = Required(url, nameof(url), 500);
        AltText = Required(altText, nameof(altText), 2_000);
        Caption = Optional(caption, 2_000, nameof(caption));
        ChangedAt(changedAt);
    }

    public static ImageBlock Reconstitute(BlockId id, string url, string altText, string? caption, DateTimeOffset createdAt, DateTimeOffset updatedAt) =>
        new(id, url, altText, caption, createdAt, updatedAt);
}

public sealed class VideoBlock : ContentBlock
{
    public override string Kind => "video";
    public string Url { get; private set; }
    public string Caption { get; private set; }

    public VideoBlock(string url, string? caption, DateTimeOffset createdAt)
        : this(BlockId.New(), url, caption, createdAt, createdAt) { }

    private VideoBlock(BlockId id, string url, string? caption, DateTimeOffset createdAt, DateTimeOffset updatedAt)
        : base(id, createdAt, updatedAt)
    {
        Url = Required(url, nameof(url), 500);
        Caption = Optional(caption, 2_000, nameof(caption));
    }

    public void Update(string url, string? caption, DateTimeOffset changedAt)
    {
        Url = Required(url, nameof(url), 500);
        Caption = Optional(caption, 2_000, nameof(caption));
        ChangedAt(changedAt);
    }

    public static VideoBlock Reconstitute(BlockId id, string url, string? caption, DateTimeOffset createdAt, DateTimeOffset updatedAt) =>
        new(id, url, caption, createdAt, updatedAt);
}

public sealed class LinkPreviewBlock : ContentBlock
{
    public override string Kind => "link-preview";
    public Uri Url { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }

    public LinkPreviewBlock(string url, string title, string? description, DateTimeOffset createdAt)
        : this(BlockId.New(), url, title, description, createdAt, createdAt) { }

    private LinkPreviewBlock(BlockId id, string url, string title, string? description, DateTimeOffset createdAt, DateTimeOffset updatedAt)
        : base(id, createdAt, updatedAt)
    {
        Url = HttpUrl(url, nameof(url));
        Title = Required(title, nameof(title), 500);
        Description = Optional(description, 2_000, nameof(description));
    }

    public void UpdatePreview(string url, string title, string? description, DateTimeOffset changedAt)
    {
        Url = HttpUrl(url, nameof(url));
        Title = Required(title, nameof(title), 500);
        Description = Optional(description, 2_000, nameof(description));
        ChangedAt(changedAt);
    }

    public static LinkPreviewBlock Reconstitute(BlockId id, string url, string title, string? description, DateTimeOffset createdAt, DateTimeOffset updatedAt) =>
        new(id, url, title, description, createdAt, updatedAt);
}

public sealed class PollReferenceBlock : ContentBlock
{
    public override string Kind => "poll-reference";
    public Guid PollId { get; private set; }

    public PollReferenceBlock(Guid pollId, DateTimeOffset createdAt)
        : this(BlockId.New(), pollId, createdAt, createdAt) { }

    public PollReferenceBlock(DateTimeOffset createdAt)
        : this(BlockId.New(), Guid.NewGuid(), createdAt, createdAt) { }

    private PollReferenceBlock(BlockId id, Guid pollId, DateTimeOffset createdAt, DateTimeOffset updatedAt)
        : base(id, createdAt, updatedAt)
    {
        PollId = pollId != Guid.Empty ? pollId : throw new ArgumentException("A poll ID is required.", nameof(pollId));
    }

    public void UpdateReference(Guid pollId, DateTimeOffset changedAt)
    {
        PollId = pollId != Guid.Empty ? pollId : throw new ArgumentException("A poll ID is required.", nameof(pollId));
        ChangedAt(changedAt);
    }

    public static PollReferenceBlock Reconstitute(BlockId id, Guid pollId, DateTimeOffset createdAt, DateTimeOffset updatedAt) =>
        new(id, pollId, createdAt, updatedAt);
}

public sealed class ChartReferenceBlock : ContentBlock
{
    public override string Kind => "chart-reference";
    public Guid ChartId { get; private set; }
    public string Title { get; private set; }

    public ChartReferenceBlock(Guid chartId, string? title, DateTimeOffset createdAt)
        : this(BlockId.New(), chartId, title, createdAt, createdAt) { }

    public ChartReferenceBlock(string? title, DateTimeOffset createdAt)
        : this(BlockId.New(), Guid.NewGuid(), title, createdAt, createdAt) { }

    private ChartReferenceBlock(BlockId id, Guid chartId, string? title, DateTimeOffset createdAt, DateTimeOffset updatedAt)
        : base(id, createdAt, updatedAt)
    {
        ChartId = chartId != Guid.Empty ? chartId : throw new ArgumentException("A chart ID is required.", nameof(chartId));
        Title = Optional(title, 500, nameof(title));
    }

    public void Update(Guid chartId, string? title, DateTimeOffset changedAt)
    {
        ChartId = chartId != Guid.Empty ? chartId : throw new ArgumentException("A chart ID is required.", nameof(chartId));
        Title = Optional(title, 500, nameof(title));
        ChangedAt(changedAt);
    }

    public static ChartReferenceBlock Reconstitute(BlockId id, Guid chartId, string? title, DateTimeOffset createdAt, DateTimeOffset updatedAt) =>
        new(id, chartId, title, createdAt, updatedAt);
}
