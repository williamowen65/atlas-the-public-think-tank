namespace Atlas.Graph.Nodes;

public sealed record NodeDescriptionId
{
    public Guid Value { get; }

    public NodeDescriptionId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                "A description ID cannot be empty.",
                nameof(value));
        }

        Value = value;
    }

    public override string ToString() => Value.ToString();
}