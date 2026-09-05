namespace Atlas.Graph.Nodes;

public sealed record NodeDescription
{
    public const int MaximumLength = 2_000;

    public string Value { get; }

    public NodeDescription(string value)
    {
        value = value?.Trim() ?? string.Empty;

        if (value.Length > MaximumLength)
        {
            throw new ArgumentException(
                $"A node description cannot exceed " +
                $"{MaximumLength} characters.",
                nameof(value));
        }

        Value = value;
    }

    public override string ToString()
    {
        return Value;
    }
}