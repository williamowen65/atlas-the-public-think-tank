namespace Atlas.Discovery;

/// <summary>A fixed-size slice of ranked results and the total matching count.</summary>
public sealed record DiscoveryPage(
    IReadOnlyList<RankedDiscoveryItem> Items,
    int TotalCount,
    int Page,
    int PageSize)
{
    public bool HasNextPage => (long)Page * PageSize < TotalCount;
}
