namespace Atlas.Content.Blocks;

/// <summary>Formatted prose stored as Markdown source.</summary>
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
