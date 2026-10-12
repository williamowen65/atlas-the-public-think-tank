namespace Atlas.Discovery;

/// <summary>Validates Discovery requests before delegating to a read-query implementation.</summary>
public sealed class DiscoveryService : IDiscoveryService
{
    private readonly IDiscoveryReadQuery _query;

    public DiscoveryService(IDiscoveryCandidateSource source)
        : this(new InMemoryDiscoveryReadQuery(source)) { }

    private DiscoveryService(IDiscoveryReadQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        _query = query;
    }

    /// <summary>Composes a database-backed read query without requiring a candidate source.</summary>
    public static DiscoveryService FromReadQuery(IDiscoveryReadQuery query) => new(query);

    public DiscoveryPage DiscoverPage(DiscoveryQuery query)
    {
        Validate(query);
        return _query.DiscoverPage(query);
    }

    public DiscoveryPage DiscoverAuthoredPage(DiscoveryQuery query, IReadOnlyCollection<Guid> authoredNodeIds)
    {
        ArgumentNullException.ThrowIfNull(authoredNodeIds);
        Validate(query);
        return _query.DiscoverAuthoredPage(query, authoredNodeIds);
    }

    public IReadOnlyList<RankedDiscoveryItem> Discover(DiscoveryQuery query)
    {
        Validate(query);
        return _query.Discover(query);
    }

    public IReadOnlyList<RankedDiscoveryItem> DiscoverAuthored(DiscoveryQuery query, IReadOnlyCollection<Guid> authoredNodeIds)
    {
        ArgumentNullException.ThrowIfNull(authoredNodeIds);
        Validate(query);
        return _query.DiscoverAuthored(query, authoredNodeIds);
    }

    private static void Validate(DiscoveryQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        DiscoveryQueryValidation.Validate(query);
    }
}
