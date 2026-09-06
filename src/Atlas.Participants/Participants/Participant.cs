namespace Atlas.Participants.Participants;

public sealed class Participant
{
    public ParticipantId Id { get; }
    public string DisplayName { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public Participant(
        string displayName,
        DateTimeOffset createdAt)
    {
        Id = ParticipantId.New();
        DisplayName = ValidateDisplayName(displayName);
        IsActive = true;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    private Participant(
        ParticipantId id,
        string displayName,
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
        IsActive = isActive;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public static Participant Reconstitute(
        ParticipantId id,
        string displayName,
        bool isActive,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        return new Participant(
            id,
            displayName,
            isActive,
            createdAt,
            updatedAt);
    }

    public void Rename(
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
}
