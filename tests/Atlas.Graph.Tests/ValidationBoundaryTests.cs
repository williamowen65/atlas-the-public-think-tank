using Atlas.Contracts.Operations;
using Atlas.Graph.Nodes;
using Atlas.Graph.Nodes.NodeTypes;
using Atlas.Graph.Reactions;

namespace Atlas.Graph.Tests;

[TestClass]
public sealed class ValidationBoundaryTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 10, 12, 0, 0, TimeSpan.Zero);

    [TestMethod]
    public void NodeConsumersRejectDefaultIdsAndRuntimeNulls()
    {
        Assert.Throws<ArgumentException>(() => Node.Reconstitute(default, new NodeTitle("Title"),
            new NodeDescriptionId(Guid.NewGuid()), NodeTypeId.New(), new NodeAuthorId(Guid.NewGuid()), NodeStatus.Active, Now, Now));
        Assert.Throws<ArgumentException>(() => new Node(new NodeTitle("Title"), new NodeDescriptionId(Guid.NewGuid()),
            default, new NodeAuthorId(Guid.NewGuid()), Now));
        Assert.Throws<ArgumentNullException>(() => new Node(null!, new NodeDescriptionId(Guid.NewGuid()),
            NodeTypeId.New(), new NodeAuthorId(Guid.NewGuid()), Now));
        var node = NodeTestFactory.Create(createdAt: Now);
        Assert.Throws<ArgumentException>(() => node.ChangeType(default, node.AuthorId.Value, Now));
        Assert.Throws<ArgumentNullException>(() => node.Rename(null!, node.AuthorId.Value, Now));
    }

    [TestMethod]
    public void RestoredNodeTypesRequireIdentityOwnerAndChronologicalHistory()
    {
        Assert.Throws<ArgumentException>(() => NodeTypeDefinition.Reconstitute(default, "Idea", "", null, true, false, Now, Now));
        Assert.Throws<ArgumentException>(() => NodeTypeDefinition.Reconstitute(NodeTypeId.New(), "Idea", "", null, false, false, Now, Now));
        Assert.Throws<ArgumentException>(() => NodeTypeDefinition.Reconstitute(NodeTypeId.New(), "Idea", "", "owner", true, false, Now, Now));
        var type = NodeTypeDefinition.CreateCustom("Idea", "", "owner", Now);
        Assert.Throws<ArgumentException>(() => type.Rename("Changed", "owner", false, Now.AddMinutes(-1)));
        Assert.AreEqual("Idea", type.Name);
        Assert.AreEqual(Now, type.UpdatedAt);
    }

    [TestMethod]
    public void EarlierMutationLeavesValuesCollectionsAndEventsUnchanged()
    {
        var node = NodeTestFactory.Create(createdAt: Now);
        var parent = NodeId.New();
        node.AttachToParent(parent, node.AuthorId.Value, Now.AddMinutes(1));
        var events = node.DomainEvents.ToArray();
        Assert.Throws<ArgumentException>(() => node.DetachFromParent(parent, node.AuthorId.Value, Now));
        Assert.Throws<ArgumentException>(() => node.Rename(new NodeTitle("Changed"), node.AuthorId.Value, Now));
        Assert.AreEqual("Climate adaptation", node.Title.Value);
        CollectionAssert.AreEqual(new[] { parent }, node.ParentNodeIds.ToArray());
        CollectionAssert.AreEqual(events, node.DomainEvents.ToArray());
        Assert.AreEqual(Now.AddMinutes(1), node.UpdatedAt);
    }

    [TestMethod]
    public void InvalidReactionAuditDoesNotRemoveOrSupersede()
    {
        var actor = Guid.NewGuid();
        var reaction = NodeReaction.Create(NodeId.New(), ReactionDefinitionId.New(), actor, actor, Now);
        var history = reaction.AuditHistory.ToArray();
        Assert.Throws<ArgumentException>(() => reaction.Supersede(Guid.Empty, NodeReactionId.New(), Now.AddMinutes(1)));
        Assert.Throws<ArgumentException>(() => reaction.Supersede(actor, reaction.Id, Now.AddMinutes(1)));
        Assert.Throws<ArgumentException>(() => reaction.Remove(actor, actor, true, false, Now.AddMinutes(-1)));
        Assert.AreEqual(NodeReactionLifecycleState.Active, reaction.LifecycleState);
        CollectionAssert.AreEqual(history, reaction.AuditHistory.ToArray());
    }

    [TestMethod]
    public void RestoredReactionRemovalMustAgreeWithAuditTime()
    {
        var actor = Guid.NewGuid();
        var history = new NodeReactionAuditEntry(NodeReactionAuditAction.Withdrawn, actor,
            Now.AddMinutes(1), NodeReactionLifecycleState.Withdrawn, NodeReactionDisposition.Community);
        Assert.Throws<ArgumentException>(() => NodeReaction.Reconstitute(NodeReactionId.New(), NodeId.New(),
            ReactionDefinitionId.New(), actor, NodeReactionLifecycleState.Withdrawn, NodeReactionDisposition.Community,
            Now, Now.AddMinutes(2), [history]));
    }

    [TestMethod]
    public void AncestryRejectsMissingParentsAndLongCyclesButAllowsSharedAncestors()
    {
        var a = NodeId.New(); var b = NodeId.New(); var c = NodeId.New(); var d = NodeId.New();
        var graph = new Dictionary<NodeId, IReadOnlyCollection<NodeId>> { [a] = [], [b] = [a], [c] = [b], [d] = [a] };
        IReadOnlyCollection<NodeId>? Parents(NodeId id) => graph.GetValueOrDefault(id);
        Assert.Throws<InvalidOperationException>(() => GraphRelationshipValidation.EnsureAcyclic(a, [c], Parents));
        Assert.Throws<InvalidOperationException>(() => GraphRelationshipValidation.EnsureAcyclic(a, [a], Parents));
        Assert.Throws<InvalidOperationException>(() => GraphRelationshipValidation.EnsureAcyclic(NodeId.New(), [NodeId.New()], Parents));
        GraphRelationshipValidation.EnsureAcyclic(NodeId.New(), [c, d], Parents);
    }

    [TestMethod]
    public void RelationshipServiceRejectsUnavailableReferenceBeforeMutationOrSave()
    {
        var node = NodeTestFactory.Create(createdAt: Now);
        var repository = new NodeRepository(node);
        var history = node.DomainEvents.ToArray();
        Assert.Throws<InvalidOperationException>(() => new NodeRelationshipService(repository)
            .Attach(node, NodeId.New(), node.AuthorId.Value, Now.AddMinutes(1)));
        Assert.AreEqual(0, repository.Saves);
        Assert.AreEqual(0, node.ParentNodeIds.Count);
        CollectionAssert.AreEqual(history, node.DomainEvents.ToArray());
    }

    [TestMethod]
    public void UnknownTypesLeaveTypeRequestsAndTimestampUnchanged()
    {
        var node = NodeTestFactory.Create(createdAt: Now);
        var type = node.TypeId;
        var repository = new NodeRepository(node);
        var service = new NodeRelationshipService(repository);
        Assert.Throws<InvalidOperationException>(() => service.ChangeType(node, NodeTypeId.New(), node.AuthorId.Value, Now.AddMinutes(1)));
        Assert.Throws<InvalidOperationException>(() => service.SetRequestedTypes(node, [NodeTypeId.New()], node.AuthorId.Value, Now.AddMinutes(1)));
        Assert.AreEqual(type, node.TypeId);
        Assert.AreEqual(0, node.RequestedSubNodeTypes.Count);
        Assert.AreEqual(Now, node.UpdatedAt);
        Assert.AreEqual(0, repository.Saves);
    }

    [TestMethod]
    public void SharedClockNormalizesUtcAndRestoresNestedScopes()
    {
        var outer = Now.ToOffset(TimeSpan.FromHours(5));
        using (AtlasTime.Use(new FixedClock(outer)))
        {
            Assert.AreEqual(TimeSpan.Zero, AtlasTime.UtcNow.Offset);
            Assert.AreEqual(Now, AtlasTime.UtcNow);
            using (AtlasTime.Use(new FixedClock(Now.AddHours(1)))) Assert.AreEqual(Now.AddHours(1), AtlasTime.UtcNow);
            Assert.AreEqual(Now, AtlasTime.UtcNow);
        }
    }

    private sealed class FixedClock(DateTimeOffset at) : TimeProvider { public override DateTimeOffset GetUtcNow() => at; }
    private sealed class NodeRepository(Node node) : INodeRepository, IReferenceLookup
    {
        public int Saves { get; private set; }
        public bool IsAvailable(string kind, Guid id, bool requireActive) => kind == "Participant" && id == node.AuthorId.Value || kind == "Node" && id == node.Id.Value;
        public Node? GetById(NodeId id) => id == node.Id ? node : null;
        public IReadOnlyCollection<Node> GetChildren(NodeId id) => [];
        public IReadOnlyCollection<Node> GetByAuthor(NodeAuthorId id) => [node];
        public IReadOnlyCollection<Node> GetParentCandidates(NodeId id, IReadOnlyCollection<NodeId> parents) => [];
        public void Save(Node value) => Saves++;
    }
}
