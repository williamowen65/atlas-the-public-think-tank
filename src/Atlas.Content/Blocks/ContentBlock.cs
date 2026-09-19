namespace Atlas.Content.Blocks;

/// <summary>
/// Base entity for every item that can appear in a description document.
/// It centralizes identity, timestamps, and validation behavior shared by all
/// concrete block types while each subtype owns its particular payload.
/// </summary>
public abstract class ContentBlock
{
    public BlockId Id { get; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; private set; }
    /// <summary>Discriminator used to restore the correct concrete type from storage.</summary>
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

    /// <summary>Records a successful domain change without changing block identity.</summary>
    protected void ChangedAt(DateTimeOffset changedAt)
    {
        if (changedAt < CreatedAt)
        {
            throw new ArgumentException("Changed time cannot precede created time.", nameof(changedAt));
        }

        UpdatedAt = changedAt;
    }

    /// <summary>
    /// Normalizes and validates text that a concrete block requires.
    /// Keeping this invariant helper on the base class gives every subtype the
    /// same whitespace and maximum-length behavior.
    /// </summary>
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

    /// <summary>
    /// Normalizes and length-checks text that may be omitted, representing an
    /// absent value as an empty domain string. This is a protected base-class
    /// validation helper: concrete blocks supply their limits and reuse the
    /// shared invariant behavior. It resembles the helper/hook portion of the
    /// Template Method pattern, although it is not a complete Template Method.
    /// </summary>
    protected static string Optional(string? value, int maximumLength, string parameterName)
    {
        var normalized = value?.Trim() ?? string.Empty;

        if (normalized.Length > maximumLength)
        {
            throw new ArgumentException($"Value cannot exceed {maximumLength} characters.", parameterName);
        }

        return normalized;
    }

    /// <summary>Accepts only absolute HTTP or HTTPS addresses.</summary>
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
