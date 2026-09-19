namespace Atlas.Content.Blocks;

/// <summary>A video URL with an optional caption.</summary>
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
