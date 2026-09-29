namespace Atlas.Graph.Nodes.NodeTypes;

/// <summary>Identifies a node type within the Graph boundary.</summary>
public readonly record struct NodeTypeId(Guid Value)
{
    /// <summary>Creates a new identifier.</summary>
    public static NodeTypeId New()
    {
        return new NodeTypeId(Guid.NewGuid());
    }

    /// <summary>Returns the identifier's display value.</summary>
    public override string ToString()
    {
        return Value.ToString();
    }
}