namespace Atlas.Graph.NodeTypes;

public sealed class NodeTypeDefinition
{
    public NodeTypeId Id { get; }

    public string Name { get; private set; }

    public string Description { get; private set; }

    public string? OwnerId { get; }

    public bool IsSystemDefined { get; }

    public bool IsArchived { get; private set; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset UpdatedAt { get; private set; }

    private NodeTypeDefinition(
        NodeTypeId id,
        string name,
        string description,
        string? ownerId,
        bool isSystemDefined,
        bool isArchived,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        Id = id;
        Name = ValidateName(name);
        Description = ValidateDescription(description);
        OwnerId = ownerId;
        IsSystemDefined = isSystemDefined;
        IsArchived = isArchived;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public static NodeTypeDefinition CreateCustom(
        string name,
        string description,
        string ownerId,
        DateTimeOffset createdAt)
    {
        if (string.IsNullOrWhiteSpace(ownerId))
        {
            throw new ArgumentException(
                "A custom node type requires an owner.",
                nameof(ownerId));
        }

        return new NodeTypeDefinition(
            NodeTypeId.New(),
            name,
            description,
            ownerId,
            isSystemDefined: false,
            isArchived: false,
            createdAt,
            createdAt);
    }

    public static NodeTypeDefinition CreateSystemDefined(
        string name,
        string description,
        DateTimeOffset createdAt)
    {
        return new NodeTypeDefinition(
            NodeTypeId.New(),
            name,
            description,
            ownerId: null,
            isSystemDefined: true,
            isArchived: false,
            createdAt,
            createdAt);
    }

    public static NodeTypeDefinition Reconstitute(
        NodeTypeId id,
        string name,
        string description,
        string? ownerId,
        bool isSystemDefined,
        bool isArchived,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        if (updatedAt < createdAt)
        {
            throw new ArgumentException(
                "Updated time cannot precede created time.");
        }

        return new NodeTypeDefinition(
            id,
            name,
            description,
            ownerId,
            isSystemDefined,
            isArchived,
            createdAt,
            updatedAt);
    }

    public void Rename(
        string newName,
        string actorId,
        bool actorIsModerator,
        DateTimeOffset changedAt)
    {
        EnsureCanEdit(actorId, actorIsModerator);

        var validatedName = ValidateName(newName);

        if (Name == validatedName)
        {
            return;
        }

        Name = validatedName;
        UpdatedAt = changedAt;
    }

    public void ChangeDescription(
        string newDescription,
        string actorId,
        bool actorIsModerator,
        DateTimeOffset changedAt)
    {
        EnsureCanEdit(actorId, actorIsModerator);

        var validatedDescription =
            ValidateDescription(newDescription);

        if (Description == validatedDescription)
        {
            return;
        }

        Description = validatedDescription;
        UpdatedAt = changedAt;
    }

    public void Archive(
        string actorId,
        bool actorIsModerator,
        DateTimeOffset archivedAt)
    {
        EnsureCanEdit(actorId, actorIsModerator);

        if (IsArchived)
        {
            return;
        }

        IsArchived = true;
        UpdatedAt = archivedAt;
    }

    private void EnsureCanEdit(
        string actorId,
        bool actorIsModerator)
    {
        if (IsSystemDefined && !actorIsModerator)
        {
            throw new InvalidOperationException(
                "Only a moderator may edit a system-defined type.");
        }

        if (!actorIsModerator && OwnerId != actorId)
        {
            throw new UnauthorizedAccessException(
                "Only the type owner or a moderator may edit this type.");
        }
    }

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "A node type name is required.",
                nameof(name));
        }

        name = name.Trim();

        if (name.Length > 50)
        {
            throw new ArgumentException(
                "A node type name cannot exceed 50 characters.",
                nameof(name));
        }

        return name;
    }

    private static string ValidateDescription(
        string description)
    {
        description = description?.Trim() ?? string.Empty;

        if (description.Length > 500)
        {
            throw new ArgumentException(
                "A node type description cannot exceed 500 characters.",
                nameof(description));
        }

        return description;
    }
}