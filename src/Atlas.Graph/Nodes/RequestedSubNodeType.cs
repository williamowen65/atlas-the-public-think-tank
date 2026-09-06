using Atlas.Graph.Nodes.NodeTypes;

namespace Atlas.Graph.Nodes;

public sealed record RequestedSubNodeType
{
    public NodeTypeId TypeId { get; }

    public RequestedSubNodeType(NodeTypeId typeId)
    {
        if (typeId.Value == Guid.Empty)
        {
            throw new ArgumentException(
                "A requested sub-node type ID is required.",
                nameof(typeId));
        }

        TypeId = typeId;
    }
}
