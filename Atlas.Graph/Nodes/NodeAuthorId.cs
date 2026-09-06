namespace Atlas.Graph.Nodes;

public sealed record NodeAuthorId
{
    public Guid Value { get; }

    public NodeAuthorId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                "A node author ID cannot be empty.",
                nameof(value));
        }

        Value = value;
    }

    public override string ToString() => Value.ToString();
}
