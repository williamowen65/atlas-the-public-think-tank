namespace Atlas.Participants.Participants;

/// <summary>Represents a participant profile and its lifecycle within the Participants boundary.</summary>
public sealed class Participant
{
    public const int MaximumBioLength = 500;

    public ParticipantId Id { get; }
    public string DisplayName { get; private set; }
    public string Bio { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>Creates a validated participant instance.</summary>
    public Participant(
        string displayName,
        DateTimeOffset createdAt)
        : this(displayName, string.Empty, createdAt)
    {
    }

    /// <summary>Creates a validated participant instance.</summary>
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

    /// <summary>Creates a validated participant instance.</summary>
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

    /// <summary>Rebuilds the domain object from persisted state without replaying creation behavior.</summary>
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

    /// <summary>Changes the validated name and advances the modification timestamp when the value differs.</summary>
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

    /// <summary>Changes the validated participant biography when the value differs.</summary>
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

    /// <summary>Applies an atomic profile update after validating all proposed values.</summary>
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

    /// <summary>Moves the participant into the inactive lifecycle state.</summary>
    public void Deactivate(DateTimeOffset deactivatedAt)
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        UpdatedAt = deactivatedAt;
    }

    /// <summary>Validates and normalizes display name.</summary>
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

    /// <summary>Validates and normalizes bio.</summary>
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
