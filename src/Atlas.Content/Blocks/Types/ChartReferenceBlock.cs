namespace Atlas.Content.Blocks;

/// <summary>A stable reference to deferred chart content plus its optional display title.</summary>
/// <remarks>
/// Chart creation is not implemented yet. The convenience constructor therefore
/// generates a temporary stand-in ChartId so block composition can be exercised.
/// Once a Chart domain exists, new reference blocks must receive the ID of a real
/// chart from that domain instead of generating the referenced entity's ID here.
/// </remarks>
public sealed class ChartReferenceBlock : ContentBlock
{
    public override string Kind => "chart-reference";
    public Guid ChartId { get; private set; }
    public string Title { get; private set; }

    public ChartReferenceBlock(Guid chartId, string? title, DateTimeOffset createdAt)
        : this(BlockId.New(), chartId, title, createdAt, createdAt) { }

    // Placeholder implementation: this ID does not currently identify a
    // persisted chart. Remove this overload when real chart creation is wired in.
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
