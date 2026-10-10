namespace Atlas.Communities.Communities;

/// <summary>Represents a public, open Atlas community and its owner-managed lifecycle.</summary>
public sealed class Community
{
    public const int MaximumNameLength = 100;
    public const int MaximumDescriptionLength = 1000;

    public CommunityId Id { get; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public Guid OwnerParticipantId { get; }
    public CommunityStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public Community(string name, string description, Guid ownerParticipantId, DateTimeOffset createdAt)
        : this(CommunityId.New(), name, description, ownerParticipantId, CommunityStatus.Active, createdAt, createdAt)
    {
    }

    private Community(
        CommunityId id,
        string name,
        string description,
        Guid ownerParticipantId,
        CommunityStatus status,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        if (!Enum.IsDefined(status)) throw new ArgumentOutOfRangeException(nameof(status));
        if (ownerParticipantId == Guid.Empty)
        {
            throw new ArgumentException("A community owner is required.", nameof(ownerParticipantId));
        }

        if (id.Value == Guid.Empty) throw new ArgumentException("A community ID is required.", nameof(id));
        if (updatedAt < createdAt)
        {
            throw new ArgumentException("Updated time cannot precede created time.");
        }

        Id = id;
        Name = ValidateName(name);
        Description = ValidateDescription(description);
        OwnerParticipantId = ownerParticipantId;
        Status = status;
        CreatedAt = createdAt.ToUniversalTime();
        UpdatedAt = updatedAt.ToUniversalTime();
    }

    public static Community Reconstitute(
        CommunityId id,
        string name,
        string description,
        Guid ownerParticipantId,
        CommunityStatus status,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt) =>
        new(id, name, description, ownerParticipantId, status, createdAt, updatedAt);

    public void Rename(Guid actorParticipantId, string name, DateTimeOffset changedAt)
    {
        EnsureOwner(actorParticipantId);
        var validated = ValidateName(name);
        if (Name == validated) return;
        EnsureTime(changedAt);
        Name = validated;
        UpdatedAt = changedAt.ToUniversalTime();
    }

    public void ChangeDescription(Guid actorParticipantId, string description, DateTimeOffset changedAt)
    {
        EnsureOwner(actorParticipantId);
        var validated = ValidateDescription(description);
        if (Description == validated) return;
        EnsureTime(changedAt);
        Description = validated;
        UpdatedAt = changedAt.ToUniversalTime();
    }

    public void Archive(Guid actorParticipantId, DateTimeOffset changedAt)
    {
        EnsureOwner(actorParticipantId);
        if (Status == CommunityStatus.Archived) return;
        EnsureTime(changedAt);
        Status = CommunityStatus.Archived;
        UpdatedAt = changedAt.ToUniversalTime();
    }

    public void Restore(Guid actorParticipantId, DateTimeOffset changedAt)
    {
        EnsureOwner(actorParticipantId);
        if (Status == CommunityStatus.Active) return;
        EnsureTime(changedAt);
        Status = CommunityStatus.Active;
        UpdatedAt = changedAt.ToUniversalTime();
    }

    private void EnsureTime(DateTimeOffset at)
    {
        if (at < UpdatedAt) throw new ArgumentException("Change time cannot precede the current update time.", nameof(at));
    }

    private void EnsureOwner(Guid actorParticipantId)
    {
        if (actorParticipantId != OwnerParticipantId)
        {
            throw new InvalidOperationException("Only the community owner may change this community.");
        }
    }

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("A community name is required.", nameof(name));
        var trimmed = name.Trim();
        if (trimmed.Length > MaximumNameLength) throw new ArgumentException($"A community name cannot exceed {MaximumNameLength} characters.", nameof(name));
        return trimmed;
    }

    private static string ValidateDescription(string description)
    {
        var trimmed = description?.Trim() ?? string.Empty;
        if (trimmed.Length > MaximumDescriptionLength) throw new ArgumentException($"A community description cannot exceed {MaximumDescriptionLength} characters.", nameof(description));
        return trimmed;
    }
}
