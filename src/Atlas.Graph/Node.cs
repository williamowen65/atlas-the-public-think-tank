using Atlas.Contracts.Graph.V1;
using Atlas.Graph.Nodes.NodeTypes;

namespace Atlas.Graph.Nodes;

/// <summary>Represents the Graph aggregate that owns node identity, lifecycle, relationships, and recorded domain events.</summary>
public sealed class Node
{
    private readonly List<object> _domainEvents = [];
    private readonly List<RequestedSubNodeType> _requestedSubNodeTypes;
    private readonly List<NodeId> _parentNodeIds;

    public IReadOnlyCollection<object> DomainEvents =>
        _domainEvents.AsReadOnly();

    public IReadOnlyCollection<RequestedSubNodeType> RequestedSubNodeTypes =>
        _requestedSubNodeTypes.AsReadOnly();

    public IReadOnlyCollection<NodeId> ParentNodeIds =>
        _parentNodeIds.AsReadOnly();

    public NodeId Id { get; }
    public NodeTitle Title { get; private set; }
    public NodeTypeId TypeId { get; private set; }
    public NodeStatus Status { get; private set; }
    public NodeDescriptionId DescriptionId { get; }
    public NodeAuthorId AuthorId { get; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>Creates a validated node instance.</summary>
    public Node(
        NodeTitle title,
        NodeDescriptionId descriptionId,
        NodeTypeId typeId,
        NodeAuthorId authorId,
        DateTimeOffset createdAt)
        : this(
            title,
            descriptionId,
            typeId,
            authorId,
            [],
            createdAt)
    {
    }

    /// <summary>Creates a validated node instance.</summary>
    public Node(
        NodeTitle title,
        NodeDescriptionId descriptionId,
        NodeTypeId typeId,
        NodeAuthorId authorId,
        IEnumerable<NodeTypeId> requestedSubNodeTypeIds,
        DateTimeOffset createdAt)
    {
        ValidateValues(title, descriptionId, typeId, authorId);
        Id = NodeId.New();
        Title = title;
        DescriptionId = descriptionId;
        TypeId = typeId;
        AuthorId = authorId;
        Status = NodeStatus.Active;
        CreatedAt = createdAt.ToUniversalTime();
        UpdatedAt = createdAt.ToUniversalTime();
        _requestedSubNodeTypes =
            CreateRequestedSubNodeTypes(requestedSubNodeTypeIds);
        _parentNodeIds = [];

        _domainEvents.Add(
            new NodeCreatedV1(
                Id.Value,
                DescriptionId.Value,
                AuthorId.Value,
                createdAt));
    }

    /// <summary>Creates a validated node instance.</summary>
    private Node(
        NodeId id,
        NodeTitle title,
        NodeDescriptionId descriptionId,
        NodeTypeId typeId,
        NodeAuthorId authorId,
        NodeStatus status,
        IEnumerable<NodeTypeId> requestedSubNodeTypeIds,
        IEnumerable<NodeId> parentNodeIds,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        if (id.Value == Guid.Empty) throw new ArgumentException("A node ID is required.", nameof(id));
        ValidateValues(title, descriptionId, typeId, authorId);
        if (!Enum.IsDefined(status)) throw new ArgumentOutOfRangeException(nameof(status));

        if (updatedAt < createdAt)
        {
            throw new ArgumentException(
                "Updated time cannot precede created time.");
        }

        Id = id;
        Title = title;
        DescriptionId = descriptionId;
        TypeId = typeId;
        AuthorId = authorId;
        Status = status;
        CreatedAt = createdAt.ToUniversalTime();
        UpdatedAt = updatedAt.ToUniversalTime();
        _requestedSubNodeTypes =
            CreateRequestedSubNodeTypes(requestedSubNodeTypeIds);
        _parentNodeIds = CreateParentNodeIds(parentNodeIds, Id);
    }

    /// <summary>Rebuilds the domain object from persisted state without replaying creation behavior.</summary>
    public static Node Reconstitute(
        NodeId id,
        NodeTitle title,
        NodeDescriptionId descriptionId,
        NodeTypeId typeId,
        NodeAuthorId authorId,
        NodeStatus status,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        return Reconstitute(
            id,
            title,
            descriptionId,
            typeId,
            authorId,
            status,
            [],
            [],
            createdAt,
            updatedAt);
    }

    /// <summary>Rebuilds the domain object from persisted state without replaying creation behavior.</summary>
    public static Node Reconstitute(
        NodeId id,
        NodeTitle title,
        NodeDescriptionId descriptionId,
        NodeTypeId typeId,
        NodeAuthorId authorId,
        NodeStatus status,
        IEnumerable<NodeTypeId> requestedSubNodeTypeIds,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        return Reconstitute(
            id,
            title,
            descriptionId,
            typeId,
            authorId,
            status,
            requestedSubNodeTypeIds,
            [],
            createdAt,
            updatedAt);
    }

    /// <summary>Rebuilds the domain object from persisted state without replaying creation behavior.</summary>
    public static Node Reconstitute(
        NodeId id,
        NodeTitle title,
        NodeDescriptionId descriptionId,
        NodeTypeId typeId,
        NodeAuthorId authorId,
        NodeStatus status,
        IEnumerable<NodeTypeId> requestedSubNodeTypeIds,
        IEnumerable<NodeId> parentNodeIds,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        return new Node(
            id,
            title,
            descriptionId,
            typeId,
            authorId,
            status,
            requestedSubNodeTypeIds,
            parentNodeIds,
            createdAt,
            updatedAt);
    }

    /// <summary>Changes the validated name and advances the modification timestamp when the value differs.</summary>
    public void Rename(
        NodeTitle newTitle,
        Guid actorParticipantId,
        DateTimeOffset changedAt)
    {
        EnsureAuthoredBy(actorParticipantId);
        ArgumentNullException.ThrowIfNull(newTitle);

        if (Title == newTitle)
        {
            return;
        }

        EnsureTime(changedAt);
        Title = newTitle;
        UpdatedAt = changedAt.ToUniversalTime();
    }

    /// <summary>Changes the node type and advances the modification timestamp when the value differs.</summary>
    public void ChangeType(
        NodeTypeId newTypeId,
        Guid actorParticipantId,
        DateTimeOffset changedAt)
    {
        EnsureAuthoredBy(actorParticipantId);
        if (newTypeId.Value == Guid.Empty) throw new ArgumentException("A node type is required.", nameof(newTypeId));

        if (TypeId == newTypeId)
        {
            return;
        }

        EnsureTime(changedAt);
        TypeId = newTypeId;
        UpdatedAt = changedAt.ToUniversalTime();
    }

    /// <summary>Adds a requested response type when it is not already present.</summary>
    public void RequestSubNodeType(
        NodeTypeId typeId,
        Guid actorParticipantId,
        DateTimeOffset changedAt)
    {
        EnsureAuthoredBy(actorParticipantId);

        var requestedType = new RequestedSubNodeType(typeId);

        if (_requestedSubNodeTypes.Contains(requestedType))
        {
            return;
        }

        EnsureTime(changedAt);
        _requestedSubNodeTypes.Add(requestedType);
        UpdatedAt = changedAt.ToUniversalTime();
    }

    /// <summary>Removes a requested response type when it is present.</summary>
    public void StopRequestingSubNodeType(
        NodeTypeId typeId,
        Guid actorParticipantId,
        DateTimeOffset changedAt)
    {
        EnsureAuthoredBy(actorParticipantId);

        var requestedType = new RequestedSubNodeType(typeId);

        if (!_requestedSubNodeTypes.Contains(requestedType))
        {
            return;
        }

        EnsureTime(changedAt);
        _requestedSubNodeTypes.Remove(requestedType);
        UpdatedAt = changedAt.ToUniversalTime();
    }

    /// <summary>Attaches a parent relationship and records the corresponding integration event.</summary>
    public void AttachToParent(
        NodeId parentNodeId,
        Guid actorParticipantId,
        DateTimeOffset attachedAt)
    {
        EnsureAuthoredBy(actorParticipantId);
        EnsureValidParentNodeId(parentNodeId, Id);

        if (_parentNodeIds.Contains(parentNodeId))
        {
            return;
        }

        EnsureTime(attachedAt);
        _parentNodeIds.Add(parentNodeId);
        UpdatedAt = attachedAt;

        _domainEvents.Add(
            new NodeParentAttachedV1(
                Id.Value,
                parentNodeId.Value,
                DescriptionId.Value,
                AuthorId.Value,
                attachedAt));
    }

    /// <summary>Detaches a parent relationship and records the corresponding integration event.</summary>
    public void DetachFromParent(
        NodeId parentNodeId,
        Guid actorParticipantId,
        DateTimeOffset detachedAt)
    {
        EnsureAuthoredBy(actorParticipantId);
        EnsureValidParentNodeId(parentNodeId, Id);

        if (!_parentNodeIds.Contains(parentNodeId))
        {
            return;
        }

        EnsureTime(detachedAt);
        _parentNodeIds.Remove(parentNodeId);
        UpdatedAt = detachedAt;

        _domainEvents.Add(
            new NodeParentDetachedV1(
                Id.Value,
                parentNodeId.Value,
                DescriptionId.Value,
                AuthorId.Value,
                detachedAt));
    }

    /// <summary>Moves the aggregate into its archived lifecycle state and records the transition when applicable.</summary>
    public void Archive(
        Guid actorParticipantId,
        DateTimeOffset archivedAt)
    {
        EnsureAuthoredBy(actorParticipantId);

        if (Status == NodeStatus.Archived)
        {
            return;
        }

        EnsureTime(archivedAt);
        Status = NodeStatus.Archived;
        UpdatedAt = archivedAt.ToUniversalTime();

        _domainEvents.Add(
            new NodeArchivedV1(
                Id.Value,
                DescriptionId.Value,
                AuthorId.Value,
                archivedAt));
    }

    /// <summary>Returns the aggregate to its active lifecycle state and records the transition when applicable.</summary>
    public void Restore(
        Guid actorParticipantId,
        DateTimeOffset restoredAt)
    {
        EnsureAuthoredBy(actorParticipantId);

        if (Status == NodeStatus.Active)
        {
            return;
        }

        EnsureTime(restoredAt);
        Status = NodeStatus.Active;
        UpdatedAt = restoredAt.ToUniversalTime();

        _domainEvents.Add(
            new NodeRestoredV1(
                Id.Value,
                DescriptionId.Value,
                AuthorId.Value,
                restoredAt));
    }

    /// <summary>Ensures an actor is the node author before an author-owned workflow continues.</summary>
    public void EnsureAuthoredBy(Guid actorParticipantId)
    {
        if (actorParticipantId == Guid.Empty)
        {
            throw new ArgumentException(
                "An acting participant ID is required.",
                nameof(actorParticipantId));
        }

        if (actorParticipantId != AuthorId.Value)
        {
            throw new UnauthorizedAccessException(
                "Only the node author may modify this node.");
        }
    }

    /// <summary>Clears recorded events after the host has dispatched them.</summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    private static void ValidateValues(NodeTitle title, NodeDescriptionId descriptionId, NodeTypeId typeId, NodeAuthorId authorId)
    {
        ArgumentNullException.ThrowIfNull(title);
        ArgumentNullException.ThrowIfNull(descriptionId);
        ArgumentNullException.ThrowIfNull(authorId);
        if (typeId.Value == Guid.Empty) throw new ArgumentException("A node type is required.", nameof(typeId));
    }

    private void EnsureTime(DateTimeOffset at)
    {
        if (at < UpdatedAt) throw new ArgumentException("Change time cannot precede the current update time.", nameof(at));
    }

    private static List<RequestedSubNodeType>
        CreateRequestedSubNodeTypes(
            IEnumerable<NodeTypeId> typeIds)
    {
        ArgumentNullException.ThrowIfNull(typeIds);

        return typeIds
            .Select(typeId => new RequestedSubNodeType(typeId))
            .Distinct()
            .ToList();
    }

    /// <summary>Creates parent node ids during the current workflow.</summary>
    private static List<NodeId> CreateParentNodeIds(
        IEnumerable<NodeId> parentNodeIds,
        NodeId nodeId)
    {
        ArgumentNullException.ThrowIfNull(parentNodeIds);

        var parents = parentNodeIds.Distinct().ToList();

        foreach (var parentNodeId in parents)
        {
            EnsureValidParentNodeId(parentNodeId, nodeId);
        }

        return parents;
    }

    /// <summary>Enforces valid parent node id before the operation continues.</summary>
    private static void EnsureValidParentNodeId(
        NodeId parentNodeId,
        NodeId nodeId)
    {
        if (parentNodeId.Value == Guid.Empty)
        {
            throw new ArgumentException(
                "A parent node ID is required.",
                nameof(parentNodeId));
        }

        if (parentNodeId == nodeId)
        {
            throw new InvalidOperationException(
                "A node cannot be its own parent.");
        }
    }
}
