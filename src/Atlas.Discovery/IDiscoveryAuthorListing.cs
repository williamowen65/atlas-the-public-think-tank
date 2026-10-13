namespace Atlas.Discovery;

/// <summary>Pages one author's public listing without first loading every authored aggregate.</summary>
public interface IDiscoveryAuthorListing
{
    DiscoveryPage DiscoverAuthorPage(DiscoveryQuery query, Guid authorParticipantId);
}

public interface IDiscoveryPersonalVotes
{
    IReadOnlyDictionary<Guid, int> ReadParticipantVotes(Guid participantId, IReadOnlyCollection<Guid> nodeIds);
}
