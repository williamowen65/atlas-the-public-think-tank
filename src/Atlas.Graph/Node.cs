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
    public NodeDescriptionId DescriptionId { get; private set; }
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
        Id = NodeId.New();
        Title = title;
        DescriptionId = descriptionId;
        TypeId = typeId;
        AuthorId = authorId;
        Status = NodeStatus.Active;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
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
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
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
        DateTimeOffset changedAt)
    {
        if (Title == newTitle)
        {
            return;
        }

        Title = newTitle;
        UpdatedAt = changedAt;
    }

    /// <summary>Changes the node type and advances the modification timestamp when the value differs.</summary>
    public void ChangeType(
        NodeTypeId newTypeId,
        DateTimeOffset changedAt)
    {
        if (TypeId == newTypeId)
        {
            return;
        }

        TypeId = newTypeId;
        UpdatedAt = changedAt;
    }

    /// <summary>Adds a requested response type when it is not already present.</summary>
    public void RequestSubNodeType(
        NodeTypeId typeId,
        DateTimeOffset changedAt)
    {
        var requestedType = new RequestedSubNodeType(typeId);

        if (_requestedSubNodeTypes.Contains(requestedType))
        {
            return;
        }

        _requestedSubNodeTypes.Add(requestedType);
        UpdatedAt = changedAt;
    }

    /// <summary>Removes a requested response type when it is present.</summary>
    public void StopRequestingSubNodeType(
        NodeTypeId typeId,
        DateTimeOffset changedAt)
    {
        var requestedType = new RequestedSubNodeType(typeId);

        if (!_requestedSubNodeTypes.Remove(requestedType))
        {
            return;
        }

        UpdatedAt = changedAt;
    }

    /// <summary>Attaches a parent relationship and records the corresponding integration event.</summary>
    public void AttachToParent(
        NodeId parentNodeId,
        DateTimeOffset attachedAt)
    {
        EnsureValidParentNodeId(parentNodeId, Id);

        if (_parentNodeIds.Contains(parentNodeId))
        {
            return;
        }

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
        DateTimeOffset detachedAt)
    {
        EnsureValidParentNodeId(parentNodeId, Id);

        if (!_parentNodeIds.Remove(parentNodeId))
        {
            return;
        }

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
    public void Archive(DateTimeOffset archivedAt)
    {
        if (Status == NodeStatus.Archived)
        {
            return;
        }

        Status = NodeStatus.Archived;
        UpdatedAt = archivedAt;

        _domainEvents.Add(
            new NodeArchivedV1(
                Id.Value,
                DescriptionId.Value,
                AuthorId.Value,
                archivedAt));
    }

    /// <summary>Returns the aggregate to its active lifecycle state and records the transition when applicable.</summary>
    public void Restore(DateTimeOffset restoredAt)
    {
        if (Status == NodeStatus.Active)
        {
            return;
        }

        Status = NodeStatus.Active;
        UpdatedAt = restoredAt;

        _domainEvents.Add(
            new NodeRestoredV1(
                Id.Value,
                DescriptionId.Value,
                AuthorId.Value,
                restoredAt));
    }

    /// <summary>Replaces the node's Content reference and advances its modification timestamp.</summary>
    public void ReplaceDescriptionReference(
        NodeDescriptionId newDescriptionId,
        DateTimeOffset changedAt)
    {
        if (DescriptionId == newDescriptionId)
        {
            return;
        }

        DescriptionId = newDescriptionId;
        UpdatedAt = changedAt;
    }

    /// <summary>Clears recorded events after the host has dispatched them.</summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
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
