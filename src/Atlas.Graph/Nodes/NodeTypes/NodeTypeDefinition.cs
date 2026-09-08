namespace Atlas.Graph.Nodes.NodeTypes;

/// <summary>Defines a governed Graph node type, including naming, pluralization, ownership, and lifecycle rules.</summary>
public sealed class NodeTypeDefinition
{
    public NodeTypeId Id { get; }

    public string Name { get; private set; }

    public string Description { get; private set; }

    public string? OwnerId { get; }

    public bool IsSystemDefined { get; }

    public bool IsArchived { get; private set; }

    public bool AutoPluralize { get; private set; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>Creates a validated node type definition instance.</summary>
    private NodeTypeDefinition(
        NodeTypeId id,
        string name,
        string description,
        string? ownerId,
        bool isSystemDefined,
        bool isArchived,
        bool autoPluralize,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        Id = id;
        Name = ValidateName(name);
        Description = ValidateDescription(description);
        OwnerId = ownerId;
        IsSystemDefined = isSystemDefined;
        IsArchived = isArchived;
        AutoPluralize = autoPluralize;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    /// <summary>Creates custom during the current workflow.</summary>
    public static NodeTypeDefinition CreateCustom(
        string name,
        string description,
        string ownerId,
        DateTimeOffset createdAt,
        bool autoPluralize = true)
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
            autoPluralize,
            createdAt,
            createdAt);
    }

    /// <summary>Creates system defined during the current workflow.</summary>
    public static NodeTypeDefinition CreateSystemDefined(
        string name,
        string description,
        DateTimeOffset createdAt,
        bool autoPluralize = true)
    {
        return new NodeTypeDefinition(
            NodeTypeId.New(),
            name,
            description,
            ownerId: null,
            isSystemDefined: true,
            isArchived: false,
            autoPluralize,
            createdAt,
            createdAt);
    }

    /// <summary>Rebuilds the domain object from persisted state without replaying creation behavior.</summary>
    public static NodeTypeDefinition Reconstitute(
        NodeTypeId id,
        string name,
        string description,
        string? ownerId,
        bool isSystemDefined,
        bool isArchived,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        bool autoPluralize = true)
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
            autoPluralize,
            createdAt,
            updatedAt);
    }

    /// <summary>Changes the validated name and advances the modification timestamp when the value differs.</summary>
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

    /// <summary>Changes the description while enforcing node-type editing rules.</summary>
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


    /// <summary>Changes automatic pluralization while enforcing node-type editing rules.</summary>
    public void ChangeAutoPluralize(
        bool autoPluralize,
        string actorId,
        bool actorIsModerator,
        DateTimeOffset changedAt)
    {
        EnsureCanEdit(actorId, actorIsModerator);

        if (AutoPluralize == autoPluralize)
        {
            return;
        }

        AutoPluralize = autoPluralize;
        UpdatedAt = changedAt;
    }

    /// <summary>Moves the aggregate into its archived lifecycle state and records the transition when applicable.</summary>
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

    /// <summary>Enforces can edit before the operation continues.</summary>
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

    /// <summary>Validates and normalizes name.</summary>
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

    /// <summary>Validates and normalizes description.</summary>
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