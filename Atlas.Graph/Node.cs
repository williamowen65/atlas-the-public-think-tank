namespace Atlas.Graph.Nodes;

public sealed class Node
{
    public NodeId Id { get; }
    public NodeTitle Title { get; private set; }
    public NodeType Type { get; private set; }
    public NodeStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public Node(
        NodeId id,
        NodeTitle title,
        NodeType type,
        DateTimeOffset createdAt)
    {
        Id = id;
        Title = title;
        Type = type;
        Status = NodeStatus.Active;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
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
        NodeType newType,
        DateTimeOffset changedAt)
    {
        if (Type == newType)
        {
            return;
        }

        Type = newType;
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
}