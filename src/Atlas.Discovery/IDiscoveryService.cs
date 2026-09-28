namespace Atlas.Discovery;

/// <summary>Exposes Atlas content through ranked Discovery results.</summary>
public interface IDiscoveryService
{
    const int PageSize = 20;
    DiscoveryPage DiscoverPage(DiscoveryQuery query);
    DiscoveryPage DiscoverAuthoredPage(DiscoveryQuery query, IReadOnlyCollection<Guid> authoredNodeIds);
    IReadOnlyList<RankedDiscoveryItem> Discover(DiscoveryQuery query);
    IReadOnlyList<RankedDiscoveryItem> DiscoverAuthored(DiscoveryQuery query, IReadOnlyCollection<Guid> authoredNodeIds);
}
