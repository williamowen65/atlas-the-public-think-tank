using Atlas.Contracts.Graph.V1;
using Atlas.Graph.Nodes;

namespace Atlas.Graph.Tests;

[TestClass]
public class NodeLifecycleBehaviorTests
{
    [TestMethod]
    public void Archive_ChangesStatusAndTimestamp()
    {
        var node = NodeTestFactory.Create();
        var archivedAt = node.CreatedAt.AddMinutes(1);

        node.Archive(archivedAt);

        Assert.AreEqual(NodeStatus.Archived, node.Status);
        Assert.AreEqual(archivedAt, node.UpdatedAt);
    }

    [TestMethod]
    public void Archive_WhenAlreadyArchived_PreservesTimestamp()
    {
        var node = NodeTestFactory.Create();
        node.Archive(node.CreatedAt.AddMinutes(1));
        var originalUpdatedAt = node.UpdatedAt;
        node.ClearDomainEvents();

        node.Archive(originalUpdatedAt.AddMinutes(1));

        Assert.AreEqual(originalUpdatedAt, node.UpdatedAt);
        Assert.IsEmpty(node.DomainEvents);
    }

    [TestMethod]
    public void Restore_ChangesStatusAndRecordsCompleteEvent()
    {
        var node = NodeTestFactory.Create();
        node.Archive(node.CreatedAt.AddMinutes(1));
        node.ClearDomainEvents();
        var restoredAt = node.UpdatedAt.AddMinutes(1);

        node.Restore(restoredAt);

        Assert.AreEqual(NodeStatus.Active, node.Status);
        Assert.AreEqual(restoredAt, node.UpdatedAt);

        var message = node.DomainEvents
            .OfType<NodeRestoredV1>()
            .Single();

        Assert.AreEqual(node.Id.Value, message.NodeId);
        Assert.AreEqual(node.DescriptionId.Value, message.DescriptionId);
        Assert.AreEqual(node.AuthorId.Value, message.AuthorId);
        Assert.AreEqual(restoredAt, message.OccurredAt);
    }

    [TestMethod]
    public void Restore_WhenAlreadyActive_IsNoOp()
    {
        var node = NodeTestFactory.Create();
        node.ClearDomainEvents();
        var originalUpdatedAt = node.UpdatedAt;

        node.Restore(originalUpdatedAt.AddMinutes(1));

        Assert.AreEqual(NodeStatus.Active, node.Status);
        Assert.AreEqual(originalUpdatedAt, node.UpdatedAt);
        Assert.IsEmpty(node.DomainEvents);
    }
}
