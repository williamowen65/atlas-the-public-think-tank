namespace Atlas.Graph.Nodes;

public sealed record NodeTitle
{
    public const int MaximumLength = 200;

    public string Value { get; }

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

    public override string ToString() => Value;
}