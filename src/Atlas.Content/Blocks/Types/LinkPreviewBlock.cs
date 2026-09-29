namespace Atlas.Content.Blocks;

/// <summary>A validated web link and the presentation metadata used for its preview.</summary>
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
