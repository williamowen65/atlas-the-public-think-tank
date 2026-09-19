namespace Atlas.Content.Blocks;

public abstract class ContentBlock
{
    public BlockId Id { get; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public abstract string Kind { get; }

    protected ContentBlock(BlockId id, DateTimeOffset createdAt, DateTimeOffset updatedAt)
    {
        if (updatedAt < createdAt)
        {
            throw new ArgumentException("Updated time cannot precede created time.");
        }

        Id = id;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    protected void ChangedAt(DateTimeOffset changedAt)
    {
        if (changedAt < CreatedAt)
        {
            throw new ArgumentException("Changed time cannot precede created time.", nameof(changedAt));
        }

        UpdatedAt = changedAt;
    }

    protected static string Required(string? value, string parameterName, int maximumLength)
    {
        var normalized = value?.Trim() ?? string.Empty;

        if (normalized.Length == 0)
        {
            throw new ArgumentException("A value is required.", parameterName);
        }

        if (normalized.Length > maximumLength)
        {
            throw new ArgumentException($"Value cannot exceed {maximumLength} characters.", parameterName);
        }

        return normalized;
    }

    protected static string Optional(string? value, int maximumLength, string parameterName)
    {
        var normalized = value?.Trim() ?? string.Empty;

        if (normalized.Length > maximumLength)
        {
            throw new ArgumentException($"Value cannot exceed {maximumLength} characters.", parameterName);
        }

        return normalized;
    }

    protected static Uri HttpUrl(string value, string parameterName)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new ArgumentException("An absolute HTTP or HTTPS URL is required.", parameterName);
        }

        return uri;
    }
}
