using System.Text;

namespace Atlas.Graph.Tags;

/// <summary>Represents immutable, reusable tag vocabulary owned by Graph.</summary>
public sealed class TagDefinition
{
    public const int MaximumTextLength = 80;

    public TagDefinitionId Id { get; }
    public string Text { get; }
    public string NormalizedText { get; }
    public Guid CreatedByParticipantId { get; }
    public bool IsSuppressed { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private TagDefinition(
        TagDefinitionId id,
        string text,
        string normalizedText,
        Guid createdByParticipantId,
        bool isSuppressed,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException("A tag definition ID is required.", nameof(id));
        }

        if (createdByParticipantId == Guid.Empty)
        {
            throw new ArgumentException("A tag creator is required.", nameof(createdByParticipantId));
        }

        if (updatedAt < createdAt)
        {
            throw new ArgumentException("Updated time cannot precede created time.");
        }

        Text = CleanDisplayText(text);
        var expectedNormalizedText = Normalize(Text);

        if (!string.Equals(normalizedText, expectedNormalizedText, StringComparison.Ordinal))
        {
            throw new ArgumentException("Normalized tag text does not match its display text.", nameof(normalizedText));
        }

        Id = id;
        NormalizedText = normalizedText;
        CreatedByParticipantId = createdByParticipantId;
        IsSuppressed = isSuppressed;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    /// <summary>Creates a reusable definition from participant-supplied text.</summary>
    public static TagDefinition Create(
        string text,
        Guid createdByParticipantId,
        DateTimeOffset createdAt)
    {
        var cleanedText = CleanDisplayText(text);

        return new TagDefinition(
            TagDefinitionId.New(),
            cleanedText,
            Normalize(cleanedText),
            createdByParticipantId,
            isSuppressed: false,
            createdAt,
            createdAt);
    }

    /// <summary>Rebuilds a persisted definition without replaying creation behavior.</summary>
    public static TagDefinition Reconstitute(
        TagDefinitionId id,
        string text,
        string normalizedText,
        Guid createdByParticipantId,
        bool isSuppressed,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        return new TagDefinition(
            id,
            text,
            normalizedText,
            createdByParticipantId,
            isSuppressed,
            createdAt,
            updatedAt);
    }

    /// <summary>Suppresses the reusable definition while retaining its identity and history.</summary>
    public void Suppress(bool actorIsModerator, DateTimeOffset suppressedAt)
    {
        if (!actorIsModerator)
        {
            throw new UnauthorizedAccessException("Tag suppression requires moderator capability.");
        }

        if (IsSuppressed)
        {
            return;
        }

        IsSuppressed = true;
        UpdatedAt = suppressedAt;
    }

    /// <summary>Produces the stable comparison form used for lookup and duplicate detection.</summary>
    public static string Normalize(string text)
    {
        var cleaned = CleanDisplayText(text)
            .Normalize(NormalizationForm.FormKC);

        return cleaned.ToUpperInvariant();
    }

    private static string CleanDisplayText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Tag text is required.", nameof(text));
        }

        var cleaned = string.Join(
            " ",
            text.Split(
                (char[]?)null,
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

        if (cleaned.Length > MaximumTextLength)
        {
            throw new ArgumentException(
                $"Tag text cannot exceed {MaximumTextLength} characters.",
                nameof(text));
        }

        return cleaned;
    }
}
