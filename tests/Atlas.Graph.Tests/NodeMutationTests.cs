using Atlas.Graph.Nodes;
using Atlas.Graph.Nodes.NodeTypes;

namespace Atlas.Graph.Tests;

[TestClass]
public class NodeMutationTests
{
    [TestMethod]
    public void Rename_UpdatesTitleAndTimestamp()
    {
        var node = NodeTestFactory.Create();
        var changedAt = node.CreatedAt.AddMinutes(1);

        node.Rename(new NodeTitle("Updated title"), changedAt);

        Assert.AreEqual(new NodeTitle("Updated title"), node.Title);
        Assert.AreEqual(changedAt, node.UpdatedAt);
    }

    [TestMethod]
    public void Rename_ToSameTitle_IsNoOp()
    {
        var node = NodeTestFactory.Create();
        var originalUpdatedAt = node.UpdatedAt;

        node.Rename(node.Title, originalUpdatedAt.AddMinutes(1));

        Assert.AreEqual(originalUpdatedAt, node.UpdatedAt);
    }

    [TestMethod]
    public void ChangeType_UpdatesTypeAndTimestamp()
    {
        var node = NodeTestFactory.Create();
        var typeId = NodeTypeId.New();
        var changedAt = node.CreatedAt.AddMinutes(1);

        node.ChangeType(typeId, changedAt);

        Assert.AreEqual(typeId, node.TypeId);
        Assert.AreEqual(changedAt, node.UpdatedAt);
    }

    [TestMethod]
    public void ChangeType_ToSameType_IsNoOp()
    {
        var node = NodeTestFactory.Create();
        var originalUpdatedAt = node.UpdatedAt;

        node.ChangeType(node.TypeId, originalUpdatedAt.AddMinutes(1));

        Assert.AreEqual(originalUpdatedAt, node.UpdatedAt);
    }

    [TestMethod]
    public void ReplaceDescriptionReference_UpdatesReferenceAndTimestamp()
    {
        var node = NodeTestFactory.Create();
        var descriptionId = new NodeDescriptionId(Guid.NewGuid());
        var changedAt = node.CreatedAt.AddMinutes(1);

        node.ReplaceDescriptionReference(descriptionId, changedAt);

        Assert.AreEqual(descriptionId, node.DescriptionId);
        Assert.AreEqual(changedAt, node.UpdatedAt);
    }

    [TestMethod]
    public void ReplaceDescriptionReference_WithSameReference_IsNoOp()
    {
        var node = NodeTestFactory.Create();
        var originalUpdatedAt = node.UpdatedAt;

        node.ReplaceDescriptionReference(
            node.DescriptionId,
            originalUpdatedAt.AddMinutes(1));

        Assert.AreEqual(originalUpdatedAt, node.UpdatedAt);
    }

    [TestMethod]
    public void RequestSubNodeType_AddsRequestAndUpdatesTimestamp()
    {
        var node = NodeTestFactory.Create();
        var typeId = NodeTypeId.New();
        var changedAt = node.CreatedAt.AddMinutes(1);

        node.RequestSubNodeType(typeId, changedAt);

        Assert.AreEqual(typeId, node.RequestedSubNodeTypes.Single().TypeId);
        Assert.AreEqual(changedAt, node.UpdatedAt);
    }

    [TestMethod]
    public void RequestSubNodeType_WhenAlreadyRequested_IsNoOp()
    {
        var node = NodeTestFactory.Create();
        var typeId = NodeTypeId.New();
        node.RequestSubNodeType(typeId, node.CreatedAt.AddMinutes(1));
        var originalUpdatedAt = node.UpdatedAt;

        node.RequestSubNodeType(typeId, originalUpdatedAt.AddMinutes(1));

        Assert.HasCount(1, node.RequestedSubNodeTypes);
        Assert.AreEqual(originalUpdatedAt, node.UpdatedAt);
    }

    [TestMethod]
    public void StopRequestingSubNodeType_RemovesRequestAndUpdatesTimestamp()
    {
        var node = NodeTestFactory.Create();
        var typeId = NodeTypeId.New();
        node.RequestSubNodeType(typeId, node.CreatedAt.AddMinutes(1));
        var changedAt = node.UpdatedAt.AddMinutes(1);

        node.StopRequestingSubNodeType(typeId, changedAt);

        Assert.IsEmpty(node.RequestedSubNodeTypes);
        Assert.AreEqual(changedAt, node.UpdatedAt);
    }

    [TestMethod]
    public void StopRequestingSubNodeType_WhenNotRequested_IsNoOp()
    {
        var node = NodeTestFactory.Create();
        var originalUpdatedAt = node.UpdatedAt;

        node.StopRequestingSubNodeType(
            NodeTypeId.New(),
            originalUpdatedAt.AddMinutes(1));

        Assert.AreEqual(originalUpdatedAt, node.UpdatedAt);
    }
}
