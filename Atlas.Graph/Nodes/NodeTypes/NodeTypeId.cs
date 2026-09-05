namespace Atlas.Graph.NodeTypes;

public readonly record struct NodeTypeId(Guid Value)
{
    public static NodeTypeId New()
    {
        return new NodeTypeId(Guid.NewGuid());
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}