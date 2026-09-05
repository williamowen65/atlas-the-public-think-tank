
namespace Atlas.Graph.Nodes;

public readonly record struct NodeId(Guid Value)
{
    public static NodeId New()
    {
        return new NodeId(Guid.NewGuid());
    }
}