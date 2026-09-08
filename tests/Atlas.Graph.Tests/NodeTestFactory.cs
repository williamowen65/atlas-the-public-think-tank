using Atlas.Graph.Nodes;
using Atlas.Graph.Nodes.NodeTypes;

namespace Atlas.Graph.Tests;

/// <summary>Creates Graph aggregates in consistent states for focused tests.</summary>
internal static class NodeTestFactory
{
    /// <summary>Coordinates creation and persistence for this workflow.</summary>
    public static Node Create(
        string title = "Climate adaptation",
        DateTimeOffset? createdAt = null)
    {
        return new Node(
            new NodeTitle(title),
            new NodeDescriptionId(Guid.NewGuid()),
            NodeTypeId.New(),
            new NodeAuthorId(Guid.NewGuid()),
            createdAt ?? DateTimeOffset.UtcNow);
    }

    /// <summary>Rebuilds the domain object from persisted state without replaying creation behavior.</summary>
    public static Node Reconstitute(
        NodeId? id = null,
        IEnumerable<NodeTypeId>? requestedTypeIds = null,
        IEnumerable<NodeId>? parentIds = null,
        DateTimeOffset? createdAt = null,
        DateTimeOffset? updatedAt = null)
    {
        var created = createdAt ?? DateTimeOffset.UtcNow.AddHours(-1);

        return Node.Reconstitute(
            id ?? NodeId.New(),
            new NodeTitle("Climate adaptation"),
            new NodeDescriptionId(Guid.NewGuid()),
            NodeTypeId.New(),
            new NodeAuthorId(Guid.NewGuid()),
            NodeStatus.Active,
            requestedTypeIds ?? [],
            parentIds ?? [],
            created,
            updatedAt ?? created);
    }
}
