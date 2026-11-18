namespace Atlas.Content.Blocks;

/// <summary>An image URL with required accessible alternative text and an optional caption.</summary>
public sealed class ImageBlock : ContentBlock
{
    public override string Kind => "image";
    public string Url { get; private set; }
    public string AltText { get; private set; }
    public string Caption { get; private set; }
    public string Layout { get; private set; }

    public ImageBlock(
        string url,
        string altText,
        string? caption,
        DateTimeOffset createdAt,
        string? layout = null
    )
        : this(
            BlockId.New(),
            url,
            altText,
            caption,
            createdAt,
            createdAt,
            layout
        ) { }

    private ImageBlock(
        BlockId id,
        string url,
        string altText,
        string? caption,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        string? layout
    )
        : base(
            id,
            createdAt,
            updatedAt
        )
    {
        Layout = ValidateLayout(layout);
        Url = MediaUrl(
            url,
            nameof(url)
        );
        AltText = Required(
            altText,
            nameof(altText),
            2_000
        );
        Caption = Optional(
            caption,
            2_000,
            nameof(caption)
        );
    }

    public void Update(
        string url,
        string altText,
        string? caption,
        DateTimeOffset changedAt,
        string? layout = null
    )
    {
        var validatedUrl = MediaUrl(
            url,
            nameof(url)
        );
        var validatedAltText = Required(
            altText,
            nameof(altText),
            2_000
        );
        var validatedCaption = Optional(
            caption,
            2_000,
            nameof(caption)
        );
        var validatedLayout = layout is null ? Layout : ValidateLayout(layout);
        ChangedAt(changedAt);
        Layout = validatedLayout;
        Url = validatedUrl;
        AltText = validatedAltText;
        Caption = validatedCaption;
    }

    public static ImageBlock Reconstitute(
        BlockId id,
        string url,
        string altText,
        string? caption,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        string? layout = null
    ) =>
        new(
            id,
            url,
            altText,
            caption,
            createdAt,
            updatedAt,
            layout
        );
    private static string ValidateLayout(string? layout)
    {
        if (string.IsNullOrEmpty(layout)) return "half:left";
        var parts = layout.Split(':');
        if (parts.Length == 2
            && parts[0] is ("quarter" or "third" or "half")
            && parts[1] is ("left" or "center" or "right"))
            return layout;
        throw new ArgumentException("Invalid image layout.", nameof(layout));
    }
}
