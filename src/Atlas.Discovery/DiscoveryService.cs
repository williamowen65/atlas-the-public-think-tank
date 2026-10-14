using Atlas.Contracts.Observability;

namespace Atlas.Discovery;

/// <summary>Validates Discovery requests before delegating to a read-query implementation.</summary>
public sealed class DiscoveryService : IDiscoveryService, IDiscoveryAuthorListing, IDiscoveryPersonalVotes
{
    private readonly IDiscoveryReadQuery _query;

    /// <summary>Accepts the storage query used after request validation.</summary>
    public DiscoveryService(IDiscoveryReadQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        _query = query;
    }

    /// <summary>Validates the author and delegates SQL-backed authored paging.</summary>
    public DiscoveryPage DiscoverAuthorPage(DiscoveryQuery query, Guid authorParticipantId)
    {
        Validate(query);
        if (authorParticipantId == Guid.Empty) throw new ArgumentException("An author is required.", nameof(authorParticipantId));
        return Telemetry.Observe(EventKind.DiscoveryRead, () => ((IDiscoveryAuthorListing)_query).DiscoverAuthorPage(query, authorParticipantId));
    }

    /// <summary>Reads personal ratings separately from the shared public card cache.</summary>
    public IReadOnlyDictionary<Guid, int> ReadParticipantVotes(Guid participantId, IReadOnlyCollection<Guid> nodeIds) =>
        _query is IDiscoveryPersonalVotes votes ? votes.ReadParticipantVotes(participantId, nodeIds) : new Dictionary<Guid, int>();

    /// <summary>Validates and requests one public result page.</summary>
    public DiscoveryPage DiscoverPage(DiscoveryQuery query)
    {
        Validate(query);
        return Telemetry.Observe(EventKind.DiscoveryRead, () => _query.DiscoverPage(query));
    }

    /// <summary>Validates and requests one page within the supplied authored IDs.</summary>
    public DiscoveryPage DiscoverAuthoredPage(DiscoveryQuery query, IReadOnlyCollection<Guid> authoredNodeIds)
    {
        ArgumentNullException.ThrowIfNull(authoredNodeIds);
        Validate(query);
        return Telemetry.Observe(EventKind.DiscoveryRead, () => _query.DiscoverAuthoredPage(query, authoredNodeIds));
    }

    /// <summary>Supports the existing unpaged contract; normal Console browsing uses pages.</summary>
    public IReadOnlyList<RankedDiscoveryItem> Discover(DiscoveryQuery query)
    {
        Validate(query);
        return Telemetry.Observe(EventKind.DiscoveryRead, () => _query.Discover(query));
    }

    /// <summary>Supports unpaged authored results for existing callers.</summary>
    public IReadOnlyList<RankedDiscoveryItem> DiscoverAuthored(DiscoveryQuery query, IReadOnlyCollection<Guid> authoredNodeIds)
    {
        ArgumentNullException.ThrowIfNull(authoredNodeIds);
        Validate(query);
        return Telemetry.Observe(EventKind.DiscoveryRead, () => _query.DiscoverAuthored(query, authoredNodeIds));
    }

    /// <summary>Rejects invalid requests before they reach storage.</summary>
    private static void Validate(DiscoveryQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        DiscoveryQueryValidation.Validate(query);
    }
}
