using Atlas.Graph.Nodes;
using Atlas.Graph.Tags;

namespace Atlas.Graph.Tests;

/// <summary>Verifies reusable tag vocabulary and node-specific application rules.</summary>
[TestClass]
public class ReusableTagTests
{
    /// <summary>Verifies that case and whitespace variants share one comparison value.</summary>
    [TestMethod]
    public void Normalize_CollapsesWhitespaceAndIgnoresCase()
    {
        var first = TagDefinition.Normalize(" Tunnel   Vision ");
        var second = TagDefinition.Normalize("tunnel vision");

        Assert.AreEqual(first, second);
    }

    /// <summary>Verifies that applying equivalent text reuses both definition and association.</summary>
    [TestMethod]
    public void Apply_EquivalentText_ReusesExistingRecords()
    {
        var fixture = new TagFixture();
        var node = NodeTestFactory.Create();
        var actorId = node.AuthorId.Value;

        var first = fixture.Service.Apply(
            node,
            "Tunnel Vision",
            actorId,
            actorIsActive: true,
            DateTimeOffset.UtcNow);
        var second = fixture.Service.Apply(
            node,
            " TUNNEL   vision ",
            actorId,
            actorIsActive: true,
            DateTimeOffset.UtcNow.AddMinutes(1));

        Assert.AreEqual(first.Id, second.Id);
        Assert.HasCount(1, fixture.Definitions.GetAll());
        Assert.HasCount(1, fixture.NodeTags.GetActiveForNode(node.Id));
    }

    /// <summary>Verifies that one reusable definition creates independent targets on different nodes.</summary>
    [TestMethod]
    public void Apply_SameDefinitionToDifferentNodes_CreatesDistinctNodeTags()
    {
        var fixture = new TagFixture();
        var firstNode = NodeTestFactory.Create("First");
        var secondNode = NodeTestFactory.Create("Second");
        var actorId = Guid.NewGuid();

        var first = fixture.Service.Apply(
            firstNode,
            "Needs Evidence",
            actorId,
            actorIsActive: true,
            DateTimeOffset.UtcNow);
        var second = fixture.Service.Apply(
            secondNode,
            "needs evidence",
            actorId,
            actorIsActive: true,
            DateTimeOffset.UtcNow);

        Assert.AreNotEqual(first.Id, second.Id);
        Assert.AreEqual(first.TagDefinitionId, second.TagDefinitionId);
        Assert.HasCount(1, fixture.Definitions.GetAll());
    }

    /// <summary>Verifies that replacement preserves shared vocabulary used elsewhere.</summary>
    [TestMethod]
    public void Replace_ChangesOnlySelectedNodeAssociation()
    {
        var fixture = new TagFixture();
        var firstNode = NodeTestFactory.Create("First");
        var secondNode = NodeTestFactory.Create("Second");
        var actorId = firstNode.AuthorId.Value;
        var original = fixture.Service.Apply(
            firstNode,
            "Tunnel Vision",
            actorId,
            actorIsActive: true,
            DateTimeOffset.UtcNow);

        fixture.Service.Apply(
            secondNode,
            "Tunnel Vision",
            actorId,
            actorIsActive: true,
            DateTimeOffset.UtcNow);

        var replacement = fixture.Service.Replace(
            firstNode,
            original.Id,
            "Possible Tunnel Vision",
            actorId,
            actorIsActive: true,
            actorIsModerator: false,
            DateTimeOffset.UtcNow.AddMinutes(1));

        Assert.IsTrue(fixture.NodeTags.GetById(original.Id)!.IsRemoved);
        Assert.AreNotEqual(original.TagDefinitionId, replacement.TagDefinitionId);
        Assert.IsNotNull(
            fixture.NodeTags.GetActive(secondNode.Id, original.TagDefinitionId));
        Assert.AreEqual(
            "Tunnel Vision",
            fixture.Definitions.GetById(original.TagDefinitionId)!.Text);
    }

    /// <summary>Verifies that unrelated participants cannot remove another participant's tag.</summary>
    [TestMethod]
    public void Remove_ByUnrelatedParticipant_Throws()
    {
        var fixture = new TagFixture();
        var node = NodeTestFactory.Create();
        var applied = fixture.Service.Apply(
            node,
            "High Risk",
            Guid.NewGuid(),
            actorIsActive: true,
            DateTimeOffset.UtcNow);

        Assert.Throws<UnauthorizedAccessException>(() =>
            fixture.Service.Remove(
                node,
                applied.Id,
                Guid.NewGuid(),
                actorIsActive: true,
                actorIsModerator: false,
                DateTimeOffset.UtcNow.AddMinutes(1)));
    }

    /// <summary>Verifies that a node author cannot erase another participant's application.</summary>
    [TestMethod]
    public void Remove_ByNodeAuthor_Throws()
    {
        var fixture = new TagFixture();
        var node = NodeTestFactory.Create();
        var applied = fixture.Service.Apply(
            node,
            "High Risk",
            Guid.NewGuid(),
            actorIsActive: true,
            DateTimeOffset.UtcNow);

        Assert.Throws<UnauthorizedAccessException>(() => fixture.Service.Remove(
            node, applied.Id, node.AuthorId.Value, actorIsActive: true,
            actorIsModerator: false, DateTimeOffset.UtcNow.AddMinutes(1)));
    }

    /// <summary>Verifies that moderator capability permits removal by a different participant.</summary>
    [TestMethod]
    public void Remove_ByModerator_Succeeds()
    {
        var fixture = new TagFixture();
        var node = NodeTestFactory.Create();
        var applied = fixture.Service.Apply(
            node,
            "High Risk",
            Guid.NewGuid(),
            actorIsActive: true,
            DateTimeOffset.UtcNow);

        fixture.Service.Remove(
            node,
            applied.Id,
            Guid.NewGuid(),
            actorIsActive: true,
            actorIsModerator: true,
            DateTimeOffset.UtcNow.AddMinutes(1));

        Assert.IsTrue(fixture.NodeTags.GetById(applied.Id)!.IsRemoved);
    }

    /// <summary>Verifies that inactive participants cannot apply tags.</summary>
    [TestMethod]
    public void Apply_ByInactiveParticipant_Throws()
    {
        var fixture = new TagFixture();
        var node = NodeTestFactory.Create();

        Assert.Throws<InvalidOperationException>(() =>
            fixture.Service.Apply(
                node,
                "Needs Evidence",
                Guid.NewGuid(),
                actorIsActive: false,
                DateTimeOffset.UtcNow));
    }

    /// <summary>Verifies that archived nodes reject new tag activity.</summary>
    [TestMethod]
    public void Apply_ToArchivedNode_Throws()
    {
        var fixture = new TagFixture();
        var node = NodeTestFactory.Create();
        node.Archive(DateTimeOffset.UtcNow);

        Assert.Throws<InvalidOperationException>(() =>
            fixture.Service.Apply(
                node,
                "Needs Evidence",
                node.AuthorId.Value,
                actorIsActive: true,
                DateTimeOffset.UtcNow.AddMinutes(1)));
    }

    /// <summary>Verifies that an author's own application begins as endorsed.</summary>
    [TestMethod]
    public void Apply_ByNodeAuthor_IsEndorsed()
    {
        var fixture = new TagFixture();
        var node = NodeTestFactory.Create();
        var applied = fixture.Service.Apply(node, "Useful", node.AuthorId.Value, true, DateTimeOffset.UtcNow);

        Assert.AreEqual(NodeTagDisposition.Endorsed, applied.Disposition);
        Assert.AreEqual(NodeTagLifecycleState.Active, applied.LifecycleState);
    }

    /// <summary>Verifies that only the node author may hide a community application.</summary>
    [TestMethod]
    public void SetDisposition_ByNodeAuthor_HidesWithoutRemoving()
    {
        var fixture = new TagFixture();
        var node = NodeTestFactory.Create();
        var applied = fixture.Service.Apply(node, "Unflattering", Guid.NewGuid(), true, DateTimeOffset.UtcNow);

        fixture.Service.SetDisposition(node, applied.Id, NodeTagDisposition.Hidden,
            node.AuthorId.Value, true);

        Assert.AreEqual(NodeTagDisposition.Hidden, applied.Disposition);
        Assert.IsFalse(applied.IsRemoved);
    }

    /// <summary>Verifies that a third party cannot decide how an author's node presents a tag.</summary>
    [TestMethod]
    public void SetDisposition_ByOtherParticipant_Throws()
    {
        var fixture = new TagFixture();
        var node = NodeTestFactory.Create();
        var applied = fixture.Service.Apply(node, "Questionable", Guid.NewGuid(), true, DateTimeOffset.UtcNow);

        Assert.Throws<UnauthorizedAccessException>(() => fixture.Service.SetDisposition(
            node, applied.Id, NodeTagDisposition.Disputed, Guid.NewGuid(), true));
    }

    /// <summary>Verifies endorsement transfers withdrawal control away from the proposer.</summary>
    [TestMethod]
    public void Remove_EndorsedTagByOriginalProposer_Throws()
    {
        var fixture = new TagFixture();
        var node = NodeTestFactory.Create();
        var proposerId = Guid.NewGuid();
        var applied = fixture.Service.Apply(node, "Useful", proposerId, true, DateTimeOffset.UtcNow);
        fixture.Service.SetDisposition(node, applied.Id, NodeTagDisposition.Endorsed,
            node.AuthorId.Value, true);

        Assert.Throws<UnauthorizedAccessException>(() => fixture.Service.Remove(
            node, applied.Id, proposerId, true, false, DateTimeOffset.UtcNow.AddMinutes(1)));
    }

    /// <summary>Verifies the node author may withdraw an endorsed association.</summary>
    [TestMethod]
    public void Remove_EndorsedTagByNodeAuthor_Succeeds()
    {
        var fixture = new TagFixture();
        var node = NodeTestFactory.Create();
        var applied = fixture.Service.Apply(node, "Useful", Guid.NewGuid(), true, DateTimeOffset.UtcNow);
        fixture.Service.SetDisposition(node, applied.Id, NodeTagDisposition.Endorsed,
            node.AuthorId.Value, true);

        fixture.Service.Remove(node, applied.Id, node.AuthorId.Value, true, false,
            DateTimeOffset.UtcNow.AddMinutes(1));

        Assert.AreEqual(NodeTagLifecycleState.Withdrawn, applied.LifecycleState);
    }

    /// <summary>Verifies replacement records supersession rather than erasing history.</summary>
    [TestMethod]
    public void Replace_PreservesSupersededLifecycle()
    {
        var fixture = new TagFixture();
        var node = NodeTestFactory.Create();
        var original = fixture.Service.Apply(node, "Typoo", node.AuthorId.Value, true, DateTimeOffset.UtcNow);

        fixture.Service.Replace(node, original.Id, "Typo", node.AuthorId.Value, true, false,
            DateTimeOffset.UtcNow.AddMinutes(1));

        Assert.AreEqual(NodeTagLifecycleState.Superseded,
            fixture.NodeTags.GetById(original.Id)!.LifecycleState);
    }

    private sealed class TagFixture
    {
        public InMemoryTagDefinitionRepository Definitions { get; } = new();
        public InMemoryNodeTagRepository NodeTags { get; } = new();
        public NodeTagApplicationService Service { get; }

        public TagFixture()
        {
            Service = new NodeTagApplicationService(Definitions, NodeTags);
        }
    }

    private sealed class InMemoryTagDefinitionRepository : ITagDefinitionRepository
    {
        private readonly List<TagDefinition> _items = [];

        public IReadOnlyCollection<TagDefinition> GetAll() => _items.ToList();

        public TagDefinition? GetById(TagDefinitionId id) =>
            _items.SingleOrDefault(item => item.Id == id);

        public TagDefinition? GetByNormalizedText(string normalizedText) =>
            _items.SingleOrDefault(item => item.NormalizedText == normalizedText);

        public void Save(TagDefinition definition)
        {
            var index = _items.FindIndex(item => item.Id == definition.Id);

            if (index >= 0)
            {
                _items[index] = definition;
            }
            else
            {
                _items.Add(definition);
            }
        }
    }

    private sealed class InMemoryNodeTagRepository : INodeTagRepository
    {
        private readonly List<NodeTag> _items = [];

        public IReadOnlyCollection<NodeTag> GetAll() => _items.ToList();

        public NodeTag? GetById(NodeTagId id) =>
            _items.SingleOrDefault(item => item.Id == id);

        public IReadOnlyCollection<NodeTag> GetActiveForNode(NodeId nodeId) =>
            _items.Where(item => item.NodeId == nodeId && !item.IsRemoved).ToList();

        public NodeTag? GetActive(NodeId nodeId, TagDefinitionId tagDefinitionId) =>
            _items.SingleOrDefault(item =>
                item.NodeId == nodeId &&
                item.TagDefinitionId == tagDefinitionId &&
                !item.IsRemoved);

        public void Save(NodeTag nodeTag)
        {
            var index = _items.FindIndex(item => item.Id == nodeTag.Id);

            if (index >= 0)
            {
                _items[index] = nodeTag;
            }
            else
            {
                _items.Add(nodeTag);
            }
        }
    }
}
