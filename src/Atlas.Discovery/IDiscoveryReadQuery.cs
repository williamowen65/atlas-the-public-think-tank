namespace Atlas.Discovery;

/// <summary>
/// Storage-independent ranked read port. Implementations must filter and rank before
/// paging, return the complete matching total and retain global ranks across pages.
/// Public queries exclude hidden nodes; authored queries retain scoped placeholders
/// but must never match hidden text. Authored IDs are a listing scope, not authorization.
/// Implementations must preserve all Discovery filters and deterministic tie-breakers.
/// Page methods must not call the unpaged methods in a database implementation.
/// </summary>
public interface IDiscoveryReadQuery
{
    DiscoveryPage DiscoverPage(DiscoveryQuery query);
    DiscoveryPage DiscoverAuthoredPage(DiscoveryQuery query, IReadOnlyCollection<Guid> authoredNodeIds);
    IReadOnlyList<RankedDiscoveryItem> Discover(DiscoveryQuery query);
    IReadOnlyList<RankedDiscoveryItem> DiscoverAuthored(DiscoveryQuery query, IReadOnlyCollection<Guid> authoredNodeIds);
}
