using Atlas.Contracts.Graph.V1;
using Atlas.Graph.Nodes;

namespace Atlas.Graph.Tests;

/// <summary>Verifies node parent behavior behavior and boundary rules.</summary>
[TestClass]
public class NodeParentBehaviorTests
{
    /// <summary>Verifies that attach to parent updates timestamp and records complete event.</summary>
    [TestMethod]
    public void AttachToParent_UpdatesTimestampAndRecordsCompleteEvent()
    {
        var node = NodeTestFactory.Create();
        node.ClearDomainEvents();
        var parentId = NodeId.New();
        var attachedAt = node.CreatedAt.AddMinutes(1);

        node.AttachToParent(parentId, attachedAt);

        Assert.AreEqual(attachedAt, node.UpdatedAt);

        var message = node.DomainEvents
            .OfType<NodeParentAttachedV1>()
            .Single();

        Assert.AreEqual(node.Id.Value, message.NodeId);
        Assert.AreEqual(parentId.Value, message.ParentNodeId);
        Assert.AreEqual(node.DescriptionId.Value, message.DescriptionId);
        Assert.AreEqual(node.AuthorId.Value, message.AuthorId);
        Assert.AreEqual(attachedAt, message.OccurredAt);
    }

    /// <summary>Verifies that attach to parent when already attached preserves timestamp.</summary>
    [TestMethod]
    public void AttachToParent_WhenAlreadyAttached_PreservesTimestamp()
    {
        var node = NodeTestFactory.Create();
        var parentId = NodeId.New();
        node.AttachToParent(parentId, node.CreatedAt.AddMinutes(1));
        var originalUpdatedAt = node.UpdatedAt;
        node.ClearDomainEvents();

        node.AttachToParent(parentId, originalUpdatedAt.AddMinutes(1));

        Assert.AreEqual(originalUpdatedAt, node.UpdatedAt);
        Assert.IsEmpty(node.DomainEvents);
    }

    /// <summary>Verifies that detach from parent updates timestamp and records complete event.</summary>
    [TestMethod]
    public void DetachFromParent_UpdatesTimestampAndRecordsCompleteEvent()
    {
        var node = NodeTestFactory.Create();
        var parentId = NodeId.New();
        node.AttachToParent(parentId, node.CreatedAt.AddMinutes(1));
        node.ClearDomainEvents();
        var detachedAt = node.UpdatedAt.AddMinutes(1);

        node.DetachFromParent(parentId, detachedAt);

        Assert.AreEqual(detachedAt, node.UpdatedAt);

        var message = node.DomainEvents
            .OfType<NodeParentDetachedV1>()
            .Single();

        Assert.AreEqual(node.Id.Value, message.NodeId);
        Assert.AreEqual(parentId.Value, message.ParentNodeId);
        Assert.AreEqual(node.DescriptionId.Value, message.DescriptionId);
        Assert.AreEqual(node.AuthorId.Value, message.AuthorId);
        Assert.AreEqual(detachedAt, message.OccurredAt);
    }

    /// <summary>Verifies that detach from parent when not attached is no op.</summary>
    [TestMethod]
    public void DetachFromParent_WhenNotAttached_IsNoOp()
    {
        var node = NodeTestFactory.Create();
        node.ClearDomainEvents();
        var originalUpdatedAt = node.UpdatedAt;

        node.DetachFromParent(
            NodeId.New(),
            originalUpdatedAt.AddMinutes(1));

        Assert.AreEqual(originalUpdatedAt, node.UpdatedAt);
        Assert.IsEmpty(node.DomainEvents);
    }

    /// <summary>Verifies that detach from parent with empty id throws.</summary>
    [TestMethod]
    public void DetachFromParent_WithEmptyId_Throws()
    {
        var node = NodeTestFactory.Create();

        Assert.Throws<ArgumentException>(
            () => node.DetachFromParent(
                new NodeId(Guid.Empty),
                DateTimeOffset.UtcNow));
    }

    /// <summary>Verifies that detach from parent with own id throws.</summary>
    [TestMethod]
    public void DetachFromParent_WithOwnId_Throws()
    {
        var node = NodeTestFactory.Create();

        Assert.Throws<InvalidOperationException>(
            () => node.DetachFromParent(
                node.Id,
                DateTimeOffset.UtcNow));
    }
}
