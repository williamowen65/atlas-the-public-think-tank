using Atlas.Graph.Nodes;
using Atlas.Graph.Nodes.NodeTypes;

namespace Atlas.Graph.Tests;

[TestClass]
public class NodeReconstitutionTests
{
    [TestMethod]
    public void Reconstitute_RestoresCompletePersistedState()
    {
        var id = NodeId.New();
        var descriptionId = new NodeDescriptionId(Guid.NewGuid());
        var typeId = NodeTypeId.New();
        var authorId = new NodeAuthorId(Guid.NewGuid());
        var requestedTypeId = NodeTypeId.New();
        var parentId = NodeId.New();
        var createdAt = DateTimeOffset.UtcNow.AddHours(-2);
        var updatedAt = createdAt.AddHours(1);

        var node = Node.Reconstitute(
            id,
            new NodeTitle("Climate adaptation"),
            descriptionId,
            typeId,
            authorId,
            NodeStatus.Archived,
            [requestedTypeId],
            [parentId],
            createdAt,
            updatedAt);

        Assert.AreEqual(id, node.Id);
        Assert.AreEqual(descriptionId, node.DescriptionId);
        Assert.AreEqual(typeId, node.TypeId);
        Assert.AreEqual(authorId, node.AuthorId);
        Assert.AreEqual(NodeStatus.Archived, node.Status);
        Assert.AreEqual(createdAt, node.CreatedAt);
        Assert.AreEqual(updatedAt, node.UpdatedAt);
        Assert.AreEqual(requestedTypeId, node.RequestedSubNodeTypes.Single().TypeId);
        Assert.AreEqual(parentId, node.ParentNodeIds.Single());
    }

    [TestMethod]
    public void Reconstitute_DoesNotRecordDomainEvents()
    {
        var node = NodeTestFactory.Reconstitute();

        Assert.AreEqual(0, node.DomainEvents.Count);
    }

    [TestMethod]
    public void Reconstitute_DeduplicatesRequestedTypesAndParents()
    {
        var requestedTypeId = NodeTypeId.New();
        var parentId = NodeId.New();

        var node = NodeTestFactory.Reconstitute(
            requestedTypeIds: [requestedTypeId, requestedTypeId],
            parentIds: [parentId, parentId]);

        Assert.AreEqual(1, node.RequestedSubNodeTypes.Count);
        Assert.AreEqual(1, node.ParentNodeIds.Count);
    }

    [TestMethod]
    public void Reconstitute_WhenUpdatedPrecedesCreated_Throws()
    {
        var createdAt = DateTimeOffset.UtcNow;

        Assert.Throws<ArgumentException>(
            () => NodeTestFactory.Reconstitute(
                createdAt: createdAt,
                updatedAt: createdAt.AddTicks(-1)));
    }

    [TestMethod]
    public void Reconstitute_WithNullRequestedTypes_Throws()
    {
        Assert.Throws<ArgumentNullException>(
            () => Node.Reconstitute(
                NodeId.New(),
                new NodeTitle("Climate adaptation"),
                new NodeDescriptionId(Guid.NewGuid()),
                NodeTypeId.New(),
                new NodeAuthorId(Guid.NewGuid()),
                NodeStatus.Active,
                null!,
                [],
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow));
    }

    [TestMethod]
    public void Reconstitute_WithNullParents_Throws()
    {
        Assert.Throws<ArgumentNullException>(
            () => Node.Reconstitute(
                NodeId.New(),
                new NodeTitle("Climate adaptation"),
                new NodeDescriptionId(Guid.NewGuid()),
                NodeTypeId.New(),
                new NodeAuthorId(Guid.NewGuid()),
                NodeStatus.Active,
                [],
                null!,
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow));
    }

    [TestMethod]
    public void Reconstitute_WithSelfAsParent_Throws()
    {
        var id = NodeId.New();

        Assert.Throws<InvalidOperationException>(
            () => NodeTestFactory.Reconstitute(
                id: id,
                parentIds: [id]));
    }

    [TestMethod]
    public void Reconstitute_WithEmptyParentId_Throws()
    {
        Assert.Throws<ArgumentException>(
            () => NodeTestFactory.Reconstitute(
                parentIds: [new NodeId(Guid.Empty)]));
    }
}
