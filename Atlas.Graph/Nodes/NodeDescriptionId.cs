namespace Atlas.Graph.Nodes;

public sealed record NodeDescriptionId(Guid Value)
{
    public NodeDescriptionId(Guid value) : this()
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                "A description ID cannot be empty.",
                nameof(value));
        }

        Value = value;
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}