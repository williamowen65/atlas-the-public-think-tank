namespace Atlas.Graph.Nodes;

/// <summary>References the participant who authored a Graph node without importing the Participants model.</summary>
public sealed record NodeAuthorId
{
    public Guid Value { get; }

    /// <summary>Creates a validated node author id instance.</summary>
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

    /// <summary>Returns the identifier's display value.</summary>
    public override string ToString() => Value.ToString();
}
