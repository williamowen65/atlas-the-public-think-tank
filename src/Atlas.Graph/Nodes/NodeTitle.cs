namespace Atlas.Graph.Nodes;

/// <summary>Represents a validated node title.</summary>
public sealed record NodeTitle
{
    public const int MaximumLength = 200;

    public string Value { get; }

    /// <summary>Creates a validated node title instance.</summary>
    public NodeTitle(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "A node title is required.",
                nameof(value));
        }

        value = value.Trim();

        if (value.Length > MaximumLength)
        {
            throw new ArgumentException(
                $"A node title cannot exceed {MaximumLength} characters.",
                nameof(value));
        }

        Value = value;
    }

    /// <summary>Returns the identifier's display value.</summary>
    public override string ToString() => Value;
}