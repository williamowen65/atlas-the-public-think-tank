namespace Atlas.Discovery;

/// <summary>Supplies the cross-boundary read model used by Discovery.</summary>
public interface IDiscoveryCandidateSource
{
    IReadOnlyCollection<DiscoveryCandidate> GetCandidates();
}
