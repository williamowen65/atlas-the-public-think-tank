using Atlas.Graph.Nodes.NodeTypes;

namespace Atlas.Graph.Nodes;

/// <summary>Identifies a node type that responders may create beneath a node.</summary>
public sealed record RequestedSubNodeType
{
    public NodeTypeId TypeId { get; }

    /// <summary>Creates a validated requested sub node type instance.</summary>
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
