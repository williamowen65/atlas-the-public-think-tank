using Atlas.Graph.Nodes;
using Atlas.Graph.Nodes.NodeTypes;

namespace Atlas.Graph.Tests;

/// <summary>Verifies node reconstitution behavior and boundary rules.</summary>
[TestClass]
public class NodeReconstitutionTests
{
    /// <summary>Verifies that reconstitute restores complete persisted state.</summary>
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

    /// <summary>Verifies that reconstitute does not record domain events.</summary>
    [TestMethod]
    public void Reconstitute_DoesNotRecordDomainEvents()
    {
        var node = NodeTestFactory.Reconstitute();

        Assert.IsEmpty(node.DomainEvents);
    }

    /// <summary>Verifies that reconstitute deduplicates requested types and parents.</summary>
    [TestMethod]
    public void Reconstitute_DeduplicatesRequestedTypesAndParents()
    {
        var requestedTypeId = NodeTypeId.New();
        var parentId = NodeId.New();

        var node = NodeTestFactory.Reconstitute(
            requestedTypeIds: [requestedTypeId, requestedTypeId],
            parentIds: [parentId, parentId]);

        Assert.HasCount(1, node.RequestedSubNodeTypes);
        Assert.HasCount(1, node.ParentNodeIds);
    }

    /// <summary>Verifies that reconstitute when updated precedes created throws.</summary>
    [TestMethod]
    public void Reconstitute_WhenUpdatedPrecedesCreated_Throws()
    {
        var createdAt = DateTimeOffset.UtcNow;

        Assert.Throws<ArgumentException>(
            () => NodeTestFactory.Reconstitute(
                createdAt: createdAt,
                updatedAt: createdAt.AddTicks(-1)));
    }

    /// <summary>Verifies that reconstitute with null requested types throws.</summary>
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

    /// <summary>Verifies that reconstitute with null parents throws.</summary>
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

    /// <summary>Verifies that reconstitute with self as parent throws.</summary>
    [TestMethod]
    public void Reconstitute_WithSelfAsParent_Throws()
    {
        var id = NodeId.New();

        Assert.Throws<InvalidOperationException>(
            () => NodeTestFactory.Reconstitute(
                id: id,
                parentIds: [id]));
    }

    /// <summary>Verifies that reconstitute with empty parent id throws.</summary>
    [TestMethod]
    public void Reconstitute_WithEmptyParentId_Throws()
    {
        Assert.Throws<ArgumentException>(
            () => NodeTestFactory.Reconstitute(
                parentIds: [new NodeId(Guid.Empty)]));
    }
}
