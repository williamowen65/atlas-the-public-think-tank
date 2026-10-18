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

    protected ContentBlock(
        BlockId id,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt
    )
    {
        ArgumentNullException.ThrowIfNull(id);
        if (updatedAt < createdAt)
        {
            throw new ArgumentException("Updated time cannot precede created time.");
        }

        Id = id;
        CreatedAt = createdAt.ToUniversalTime();
        UpdatedAt = updatedAt.ToUniversalTime();
    }

    /// <summary>Records a successful domain change without changing block identity.</summary>
    protected void ChangedAt(DateTimeOffset changedAt)
    {
        if (changedAt < UpdatedAt)
        {
            throw new ArgumentException(
                "Changed time cannot precede the current updated time.",
                nameof(changedAt)
            );
        }

        UpdatedAt = changedAt.ToUniversalTime();
    }

    /// <summary>
    /// Normalizes and validates text that a concrete block requires.
    /// Keeping this invariant helper on the base class gives every subtype the
    /// same whitespace and maximum-length behavior.
    /// </summary>
    protected static string Required(
        string? value,
        string parameterName,
        int maximumLength
    )
    {
        var normalized = value?.Trim() ?? string.Empty;

        if (normalized.Length == 0)
        {
            throw new ArgumentException(
                "A value is required.",
                parameterName
            );
        }

        if (normalized.Length > maximumLength)
        {
            throw new ArgumentException(
                $"Value cannot exceed {maximumLength} characters.",
                parameterName
            );
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
    protected static string Optional(
        string? value,
        int maximumLength,
        string parameterName
    )
    {
        var normalized = value?.Trim() ?? string.Empty;

        if (normalized.Length > maximumLength)
        {
            throw new ArgumentException(
                $"Value cannot exceed {maximumLength} characters.",
                parameterName
            );
        }

        return normalized;
    }

    /// <summary>Accepts bounded absolute HTTP(S) links without credentials or ambiguous separators.</summary>
    /// <remarks>This checks URL form, not destination trust. It performs no DNS lookup or network fetch.</remarks>
    protected static Uri HttpUrl(string value, string parameterName)
    {
        var normalized = UrlText(value, parameterName);
        if (!Uri.TryCreate(normalized, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) ||
            string.IsNullOrWhiteSpace(uri.Host) || !string.IsNullOrEmpty(uri.UserInfo))
            throw new ArgumentException("An absolute HTTP or HTTPS URL without embedded credentials is required.", parameterName);
        return uri;
    }

    /// <summary>Accepts HTTP(S) media URLs or simple relative asset paths, never executable URI schemes.</summary>
    protected static string MediaUrl(string value, string parameterName)
    {
        var normalized = UrlText(value, parameterName);
        if (normalized.Contains(':'))
            return HttpUrl(normalized, parameterName).OriginalString;

        // Existing media references can be relative asset paths. Keep that format, but
        // exclude protocol-relative hosts, schemes, encoded separators and query syntax.
        if (normalized.StartsWith("//", StringComparison.Ordinal) ||
            !normalized.All(c => char.IsAsciiLetterOrDigit(c) || c is '/' or '.' or '-' or '_' or '~'))
            throw new ArgumentException("Media must be an HTTP(S) URL or a simple relative asset path.", parameterName);
        return normalized;
    }

    private static string UrlText(string value, string parameterName)
    {
        if (value is not null && value.Any(c => char.IsControl(c) || c == '\\'))
            throw new ArgumentException("URLs cannot contain control characters or backslashes.", parameterName);
        return Required(value, parameterName, 500);
    }
}
