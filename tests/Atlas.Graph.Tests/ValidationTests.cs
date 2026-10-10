using Atlas.Graph.Nodes;
using Atlas.Graph.Nodes.NodeTypes;
using Atlas.Graph.Reactions;

namespace Atlas.Graph.Tests;

[TestClass]
public sealed class ValidationTests
{
    [TestMethod]
    public void NodeReconstitutionRejectsUndefinedStatus()
    {
        var now = DateTimeOffset.UtcNow;
        Assert.Throws<ArgumentOutOfRangeException>(() => Node.Reconstitute(NodeId.New(),
            new NodeTitle("Title"), new NodeDescriptionId(Guid.NewGuid()), NodeTypeId.New(),
            new NodeAuthorId(Guid.NewGuid()), (NodeStatus)999, now, now));
    }

    [TestMethod]
    public void InvalidReactionDispositionDoesNotChangeStateOrAuditHistory()
    {
        var actor = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var reaction = NodeReaction.Create(NodeId.New(), ReactionDefinitionId.New(), actor, actor, now);
        var original = reaction.Disposition;
        var history = reaction.AuditHistory.ToArray();
        Assert.Throws<ArgumentOutOfRangeException>(() => reaction.SetDisposition(
            (NodeReactionDisposition)999, actor, actor, now.AddMinutes(1)));
        Assert.AreEqual(original, reaction.Disposition);
        CollectionAssert.AreEqual(history, reaction.AuditHistory.ToArray());
    }

    [TestMethod]
    public void ReactionAuditRejectsUndefinedEnums()
    {
        var now = DateTimeOffset.UtcNow;
        var actor = Guid.NewGuid();
        Assert.Throws<ArgumentOutOfRangeException>(() => new NodeReactionAuditEntry(
            (NodeReactionAuditAction)999, actor, now, NodeReactionLifecycleState.Active, NodeReactionDisposition.Community));
        Assert.Throws<ArgumentOutOfRangeException>(() => new NodeReactionAuditEntry(
            NodeReactionAuditAction.Applied, actor, now, (NodeReactionLifecycleState)999, NodeReactionDisposition.Community));
        Assert.Throws<ArgumentOutOfRangeException>(() => new NodeReactionAuditEntry(
            NodeReactionAuditAction.Applied, actor, now, NodeReactionLifecycleState.Active, (NodeReactionDisposition)999));
        Assert.Throws<ArgumentOutOfRangeException>(() => NodeReaction.Reconstitute(
            NodeReactionId.New(), NodeId.New(), ReactionDefinitionId.New(), actor,
            (NodeReactionLifecycleState)999, NodeReactionDisposition.Community, now, now));
    }
}
