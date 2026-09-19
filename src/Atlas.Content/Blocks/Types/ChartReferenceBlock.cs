namespace Atlas.Content.Blocks;

/// <summary>A stable reference to deferred chart content plus its optional display title.</summary>
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
