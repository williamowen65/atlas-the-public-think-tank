using Atlas.Contracts.Graph.V1;
using Atlas.Graph.Nodes;
using Atlas.Graph.Nodes.NodeTypes;

namespace Atlas.Graph.Tests;

[TestClass]
public class NodeConstructionTests
{
    [TestMethod]
    public void Constructor_InitializesCompleteActiveNode()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var typeId = NodeTypeId.New();

        var node = new Node(
            new NodeTitle("Climate adaptation"),
            new NodeDescriptionId(Guid.NewGuid()),
            typeId,
            new NodeAuthorId(Guid.NewGuid()),
            createdAt);

        Assert.AreNotEqual(Guid.Empty, node.Id.Value);
        Assert.AreEqual(typeId, node.TypeId);
        Assert.AreEqual(NodeStatus.Active, node.Status);
        Assert.AreEqual(createdAt, node.CreatedAt);
        Assert.AreEqual(createdAt, node.UpdatedAt);
        Assert.IsEmpty(node.ParentNodeIds);
        Assert.IsEmpty(node.RequestedSubNodeTypes);
    }

    [TestMethod]
    public void Constructor_RecordsCompleteNodeCreatedEvent()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var node = NodeTestFactory.Create(createdAt: createdAt);

        var message = node.DomainEvents
            .OfType<NodeCreatedV1>()
            .Single();

        Assert.AreEqual(node.Id.Value, message.NodeId);
        Assert.AreEqual(node.DescriptionId.Value, message.DescriptionId);
        Assert.AreEqual(node.AuthorId.Value, message.AuthorId);
        Assert.AreEqual(createdAt, message.OccurredAt);
    }

    [TestMethod]
    public void Constructor_DeduplicatesRequestedSubNodeTypes()
    {
        var typeId = NodeTypeId.New();

        var node = new Node(
            new NodeTitle("Climate adaptation"),
            new NodeDescriptionId(Guid.NewGuid()),
            NodeTypeId.New(),
            new NodeAuthorId(Guid.NewGuid()),
            [typeId, typeId],
            DateTimeOffset.UtcNow);

        Assert.HasCount(1, node.RequestedSubNodeTypes);
        Assert.AreEqual(typeId, node.RequestedSubNodeTypes.Single().TypeId);
    }

    [TestMethod]
    public void Constructor_WithNullRequestedSubNodeTypes_Throws()
    {
        Assert.Throws<ArgumentNullException>(
            () => new Node(
                new NodeTitle("Climate adaptation"),
                new NodeDescriptionId(Guid.NewGuid()),
                NodeTypeId.New(),
                new NodeAuthorId(Guid.NewGuid()),
                null!,
                DateTimeOffset.UtcNow));
    }

    [TestMethod]
    public void ClearDomainEvents_RemovesRecordedEvents()
    {
        var node = NodeTestFactory.Create();

        node.ClearDomainEvents();

        Assert.IsEmpty(node.DomainEvents);
    }
}
