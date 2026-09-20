using System.Text;

namespace Atlas.Graph.Reactions;

/// <summary>Represents one curated reaction available for use on nodes.</summary>
public sealed class ReactionDefinition
{
    public const int MaximumTextLength = 24;
    public const int MaximumDescriptionLength = 160;

    public ReactionDefinitionId Id { get; }
    public string Text { get; }
    public string Emoji { get; }
    public string Description { get; }
    public string NormalizedText { get; }
    public Guid CreatedByParticipantId { get; }
    public bool IsSuppressed { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private ReactionDefinition(
        ReactionDefinitionId id,
        string text,
        string emoji,
        string description,
        string normalizedText,
        Guid createdByParticipantId,
        bool isSuppressed,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException("A reaction definition ID is required.", nameof(id));
        }

        if (createdByParticipantId == Guid.Empty)
        {
            throw new ArgumentException("A reaction curator is required.", nameof(createdByParticipantId));
        }

        if (updatedAt < createdAt)
        {
            throw new ArgumentException("Updated time cannot precede created time.");
        }

        Text = CleanDisplayText(text);
        Emoji = CleanEmoji(emoji);
        Description = CleanDescription(description);
        var expectedNormalizedText = Normalize(Text);

        if (!string.Equals(normalizedText, expectedNormalizedText, StringComparison.Ordinal))
        {
            throw new ArgumentException("Normalized reaction text does not match its display text.", nameof(normalizedText));
        }

        Id = id;
        NormalizedText = normalizedText;
        CreatedByParticipantId = createdByParticipantId;
        IsSuppressed = isSuppressed;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    /// <summary>Creates a definition for the curated reaction catalog.</summary>
    public static ReactionDefinition Create(
        string text,
        string emoji,
        string description,
        Guid createdByParticipantId,
        DateTimeOffset createdAt)
    {
        var cleanedText = CleanDisplayText(text);

        return new ReactionDefinition(
            ReactionDefinitionId.New(),
            cleanedText,
            emoji,
            description,
            Normalize(cleanedText),
            createdByParticipantId,
            isSuppressed: false,
            createdAt,
            createdAt);
    }

    /// <summary>Rebuilds a persisted definition without replaying creation behavior.</summary>
    public static ReactionDefinition Reconstitute(
        ReactionDefinitionId id,
        string text,
        string emoji,
        string description,
        string normalizedText,
        Guid createdByParticipantId,
        bool isSuppressed,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        return new ReactionDefinition(
            id,
            text,
            emoji,
            description,
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
            throw new UnauthorizedAccessException("Reaction suppression requires moderator capability.");
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
            throw new ArgumentException("Reaction text is required.", nameof(text));
        }

        var cleaned = string.Join(
            " ",
            text.Split(
                (char[]?)null,
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

        if (cleaned.Length > MaximumTextLength)
        {
            throw new ArgumentException(
                $"Reaction text cannot exceed {MaximumTextLength} characters.",
                nameof(text));
        }

        return cleaned;
    }

    private static string CleanEmoji(string emoji)
    {
        if (string.IsNullOrWhiteSpace(emoji))
        {
            throw new ArgumentException("A reaction emoji is required.", nameof(emoji));
        }

        return emoji.Trim();
    }

    private static string CleanDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("A reaction description is required.", nameof(description));
        }

        var cleaned = description.Trim();
        if (cleaned.Length > MaximumDescriptionLength)
        {
            throw new ArgumentException(
                $"Reaction description cannot exceed {MaximumDescriptionLength} characters.",
                nameof(description));
        }

        return cleaned;
    }
}
