using Atlas.Graph.Nodes;

namespace Atlas.ConsoleApp.Discovery;

/// <summary>
/// Host-side projection input used only to build Discovery candidates. This is
/// intentionally not part of Graph's domain repository contract.
/// </summary>
public interface IDiscoveryNodeReader
{
    IReadOnlyCollection<Node> ReadNodesForDiscovery();
}
