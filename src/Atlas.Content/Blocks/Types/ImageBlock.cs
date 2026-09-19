namespace Atlas.Content.Blocks;

/// <summary>An image URL with required accessible alternative text and an optional caption.</summary>
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
