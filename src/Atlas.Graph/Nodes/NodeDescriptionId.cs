namespace Atlas.Graph.Nodes;

/// <summary>References a Content document without importing the Content model.</summary>
public sealed record NodeDescriptionId
{
    public Guid Value { get; }

    /// <summary>Creates a validated node description id instance.</summary>
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

    /// <summary>Returns the identifier's display value.</summary>
    public override string ToString() => Value.ToString();
}