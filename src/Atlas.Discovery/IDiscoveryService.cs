namespace Atlas.Discovery;

/// <summary>Exposes Atlas content through ranked Discovery results.</summary>
public interface IDiscoveryService
{
    IReadOnlyList<RankedDiscoveryItem> Discover(DiscoveryQuery query);
}
