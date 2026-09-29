
namespace Atlas.Graph.Nodes;

/// <summary>Identifies a node within the Graph boundary.</summary>
public readonly record struct NodeId(Guid Value)
{
    /// <summary>Creates a new identifier.</summary>
    public static NodeId New()
    {
        return new NodeId(Guid.NewGuid());
    }
}