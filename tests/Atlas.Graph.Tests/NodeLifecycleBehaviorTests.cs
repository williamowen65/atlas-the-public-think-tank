using Atlas.Contracts.Graph.V1;
using Atlas.Graph.Nodes;

namespace Atlas.Graph.Tests;

/// <summary>Verifies node lifecycle behavior behavior and boundary rules.</summary>
[TestClass]
public class NodeLifecycleBehaviorTests
{
    /// <summary>Verifies that archive changes status and timestamp.</summary>
    [TestMethod]
    public void Archive_ChangesStatusAndTimestamp()
    {
        var node = NodeTestFactory.Create();
        var archivedAt = node.CreatedAt.AddMinutes(1);

        node.Archive(archivedAt);

        Assert.AreEqual(NodeStatus.Archived, node.Status);
        Assert.AreEqual(archivedAt, node.UpdatedAt);
    }

    /// <summary>Verifies that archive when already archived preserves timestamp.</summary>
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

    /// <summary>Verifies that restore changes status and records complete event.</summary>
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

    /// <summary>Verifies that restore when already active is no op.</summary>
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
