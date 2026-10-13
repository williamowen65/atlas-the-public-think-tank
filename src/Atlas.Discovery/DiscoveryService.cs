namespace Atlas.Discovery;

/// <summary>Validates Discovery requests before delegating to a read-query implementation.</summary>
public sealed class DiscoveryService : IDiscoveryService, IDiscoveryAuthorListing, IDiscoveryPersonalVotes
{
    private readonly IDiscoveryReadQuery _query;

    public DiscoveryService(IDiscoveryReadQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        _query = query;
    }

    public DiscoveryPage DiscoverAuthorPage(DiscoveryQuery query, Guid authorParticipantId)
    {
        Validate(query);
        if (authorParticipantId == Guid.Empty) throw new ArgumentException("An author is required.", nameof(authorParticipantId));
        return ((IDiscoveryAuthorListing)_query).DiscoverAuthorPage(query, authorParticipantId);
    }

    public IReadOnlyDictionary<Guid, int> ReadParticipantVotes(Guid participantId, IReadOnlyCollection<Guid> nodeIds) =>
        _query is IDiscoveryPersonalVotes votes ? votes.ReadParticipantVotes(participantId, nodeIds) : new Dictionary<Guid, int>();

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
