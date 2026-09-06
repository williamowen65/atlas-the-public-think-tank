namespace Atlas.Participants.Participants;

public sealed class Participant
{
    public const int MaximumBioLength = 500;

    public ParticipantId Id { get; }
    public string DisplayName { get; private set; }
    public string Bio { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public Participant(
        string displayName,
        DateTimeOffset createdAt)
        : this(displayName, string.Empty, createdAt)
    {
    }

    public Participant(
        string displayName,
        string bio,
        DateTimeOffset createdAt)
    {
        Id = ParticipantId.New();
        DisplayName = ValidateDisplayName(displayName);
        Bio = ValidateBio(bio);
        IsActive = true;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    private Participant(
        ParticipantId id,
        string displayName,
        string bio,
        bool isActive,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        if (updatedAt < createdAt)
        {
            throw new ArgumentException(
                "Updated time cannot precede created time.");
        }

        Id = id;
        DisplayName = ValidateDisplayName(displayName);
        Bio = ValidateBio(bio);
        IsActive = isActive;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public static Participant Reconstitute(
        ParticipantId id,
        string displayName,
        string bio,
        bool isActive,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        return new Participant(
            id,
            displayName,
            bio,
            isActive,
            createdAt,
            updatedAt);
    }

    internal void Rename(
        string newDisplayName,
        DateTimeOffset changedAt)
    {
        var validatedName = ValidateDisplayName(newDisplayName);

        if (DisplayName == validatedName)
        {
            return;
        }

        DisplayName = validatedName;
        UpdatedAt = changedAt;
    }

    internal void ChangeBio(
        string newBio,
        DateTimeOffset changedAt)
    {
        var validatedBio = ValidateBio(newBio);

        if (Bio == validatedBio)
        {
            return;
        }

        Bio = validatedBio;
        UpdatedAt = changedAt;
    }

    internal void UpdateProfile(
        string newDisplayName,
        string newBio,
        DateTimeOffset changedAt)
    {
        var validatedName = ValidateDisplayName(newDisplayName);
        var validatedBio = ValidateBio(newBio);

        if (DisplayName == validatedName &&
            Bio == validatedBio)
        {
            return;
        }

        DisplayName = validatedName;
        Bio = validatedBio;
        UpdatedAt = changedAt;
    }

    public void Deactivate(DateTimeOffset deactivatedAt)
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        UpdatedAt = deactivatedAt;
    }

    private static string ValidateDisplayName(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException(
                "A participant display name is required.",
                nameof(displayName));
        }

        var trimmedName = displayName.Trim();

        if (trimmedName.Length > 80)
        {
            throw new ArgumentException(
                "A participant display name cannot exceed 80 characters.",
                nameof(displayName));
        }

        return trimmedName;
    }

    private static string ValidateBio(string bio)
    {
        var trimmedBio = bio?.Trim() ?? string.Empty;

        if (trimmedBio.Length > MaximumBioLength)
        {
            throw new ArgumentException(
                $"A participant bio cannot exceed " +
                $"{MaximumBioLength} characters.",
                nameof(bio));
        }

        return trimmedBio;
    }
}
