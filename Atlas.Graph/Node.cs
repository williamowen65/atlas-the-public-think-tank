using Atlas.Graph.NodeLifecycle;
using Atlas.Graph.Nodes.NodeTypes;

namespace Atlas.Graph.Nodes;

public sealed class Node
{

    private readonly List<object> _domainEvents = [];

    public IReadOnlyCollection<object> DomainEvents =>
     _domainEvents.AsReadOnly();


    public NodeId Id { get; }
    public NodeTitle Title { get; private set; }
    public NodeTypeId TypeId { get; private set; }
    public NodeStatus Status { get; private set; }

    public NodeDescriptionId DescriptionId { get; private set; }

    /*
     The interaction might be:

        Graph creates a node.
        Content creates a description document associated with that node.
        Content returns or publishes the document ID.
        Graph records that ID as its description reference.
        A UI-facing composition layer requests the node from Graph and its document from Content.
        The composition layer combines them into one screen model.
     */
    //public DocumentId DescriptionId { get; private set; }

    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public Node(
        NodeTitle title,
        NodeDescriptionId description,
        NodeTypeId typeId,
        DateTimeOffset createdAt)
    {
        Id = NodeId.New();
        Title = title;
        NodeDescriptionId = descriptionId;
        TypeId = typeId;
        Status = NodeStatus.Active;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;

        _domainEvents.Add(
          new NodeCreated(
              Id.Value,
              Description.Value,
              createdAt));
    }

    // create a new node
    private Node(
        NodeId id,
        NodeTitle title,
        NodeDescriptionId descriptionId,
        NodeTypeId typeId,
        NodeStatus status,
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
        Status = status;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    // restore an existing node
    public static Node Reconstitute(
        NodeId id,
        NodeTitle title,
        NodeDescriptionId descriptionId,
        NodeTypeId typeId,
        NodeStatus status,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        return new Node(
            id,
            title,
            descriptionId,
            typeId,
            status,
            createdAt,
            updatedAt);
    }

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

    public void Archive(DateTimeOffset archivedAt)
    {
        if (Status == NodeStatus.Archived)
        {
            return;
        }

        Status = NodeStatus.Archived;
        UpdatedAt = archivedAt;
    }

    public void Restore(DateTimeOffset restoredAt)
    {
        if (Status == NodeStatus.Active)
        {
            return;
        }

        Status = NodeStatus.Active;
        UpdatedAt = restoredAt;
    }

    public void ChangeDescription(
        NodeDescriptionId targetDescriptionId,
        DateTimeOffset changedAt)
    {
        if (DescriptionId == targetDescriptionId)
        {
            return;
        }

        DescriptionId = targetDescriptionId;
        UpdatedAt = changedAt;
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}