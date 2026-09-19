using Atlas.Graph.Nodes;
using Atlas.Graph.Nodes.NodeTypes;

namespace Atlas.Graph.Tests;

/// <summary>Verifies that author-owned node mutations are enforced by the Graph domain.</summary>
[TestClass]
public class NodeAuthorizationTests
{
    /// <summary>Verifies that a non-author cannot change scalar node state.</summary>
    [TestMethod]
    public void AuthorOwnedScalarMutations_ByNonAuthor_AreRejected()
    {
        AssertRejectedWithoutStateChange(
            (node, actorId) => node.Rename(
                new NodeTitle("Unauthorized title"),
                actorId,
                node.UpdatedAt.AddMinutes(1)));

        AssertRejectedWithoutStateChange(
            (node, actorId) => node.ChangeType(
                NodeTypeId.New(),
                actorId,
                node.UpdatedAt.AddMinutes(1)));

        AssertRejectedWithoutStateChange(
            (node, actorId) => node.Archive(
                actorId,
                node.UpdatedAt.AddMinutes(1)));

        var archivedNode = NodeTestFactory.Create();
        archivedNode.Archive(
            archivedNode.AuthorId.Value,
            archivedNode.UpdatedAt.AddMinutes(1));
        archivedNode.ClearDomainEvents();

        AssertRejectedWithoutStateChange(
            archivedNode,
            (node, actorId) => node.Restore(
                actorId,
                node.UpdatedAt.AddMinutes(1)));
    }

    /// <summary>Verifies that a non-author cannot change node relationships or requested types.</summary>
    [TestMethod]
    public void AuthorOwnedCollectionMutations_ByNonAuthor_AreRejected()
    {
        AssertRejectedWithoutStateChange(
            (node, actorId) => node.RequestSubNodeType(
                NodeTypeId.New(),
                actorId,
                node.UpdatedAt.AddMinutes(1)));

        var requestedTypeNode = NodeTestFactory.Create();
        var requestedTypeId = NodeTypeId.New();
        requestedTypeNode.RequestSubNodeType(
            requestedTypeId,
            requestedTypeNode.AuthorId.Value,
            requestedTypeNode.UpdatedAt.AddMinutes(1));
        requestedTypeNode.ClearDomainEvents();

        AssertRejectedWithoutStateChange(
            requestedTypeNode,
            (node, actorId) => node.StopRequestingSubNodeType(
                requestedTypeId,
                actorId,
                node.UpdatedAt.AddMinutes(1)));

        AssertRejectedWithoutStateChange(
            (node, actorId) => node.AttachToParent(
                NodeId.New(),
                actorId,
                node.UpdatedAt.AddMinutes(1)));

        var attachedNode = NodeTestFactory.Create();
        var parentId = NodeId.New();
        attachedNode.AttachToParent(
            parentId,
            attachedNode.AuthorId.Value,
            attachedNode.UpdatedAt.AddMinutes(1));
        attachedNode.ClearDomainEvents();

        AssertRejectedWithoutStateChange(
            attachedNode,
            (node, actorId) => node.DetachFromParent(
                parentId,
                actorId,
                node.UpdatedAt.AddMinutes(1)));
    }

    /// <summary>Verifies authorization happens even when a requested value would otherwise be a no-op.</summary>
    [TestMethod]
    public void AuthorOwnedMutation_ByNonAuthor_IsRejectedBeforeNoOpCheck()
    {
        var node = NodeTestFactory.Create();

        Assert.Throws<UnauthorizedAccessException>(
            () => node.Rename(
                node.Title,
                Guid.NewGuid(),
                node.UpdatedAt.AddMinutes(1)));
    }

    private static void AssertRejectedWithoutStateChange(
        Action<Node, Guid> mutation)
    {
        AssertRejectedWithoutStateChange(
            NodeTestFactory.Create(),
            mutation);
    }

    private static void AssertRejectedWithoutStateChange(
        Node node,
        Action<Node, Guid> mutation)
    {
        var title = node.Title;
        var descriptionId = node.DescriptionId;
        var typeId = node.TypeId;
        var status = node.Status;
        var requestedTypes = node.RequestedSubNodeTypes.ToArray();
        var parentIds = node.ParentNodeIds.ToArray();
        var updatedAt = node.UpdatedAt;
        var domainEvents = node.DomainEvents.ToArray();

        Assert.Throws<UnauthorizedAccessException>(
            () => mutation(node, Guid.NewGuid()));

        Assert.AreEqual(title, node.Title);
        Assert.AreEqual(descriptionId, node.DescriptionId);
        Assert.AreEqual(typeId, node.TypeId);
        Assert.AreEqual(status, node.Status);
        CollectionAssert.AreEqual(
            requestedTypes,
            node.RequestedSubNodeTypes.ToArray());
        CollectionAssert.AreEqual(
            parentIds,
            node.ParentNodeIds.ToArray());
        Assert.AreEqual(updatedAt, node.UpdatedAt);
        CollectionAssert.AreEqual(
            domainEvents,
            node.DomainEvents.ToArray());
    }
}
