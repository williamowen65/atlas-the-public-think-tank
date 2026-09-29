using Atlas.Graph.Nodes;
using Atlas.Graph.Reactions;

namespace Atlas.Graph.Tests;

/// <summary>Verifies curated reaction vocabulary and node-specific application rules.</summary>
[TestClass]
public class ReusableReactionTests
{
    [TestMethod]
    public void Definition_RequiresEmojiAndDescription()
    {
        Assert.Throws<ArgumentException>(() => ReactionDefinition.Create(
            "Promising", "", "Shows potential.", Guid.NewGuid(), DateTimeOffset.UtcNow));
        Assert.Throws<ArgumentException>(() => ReactionDefinition.Create(
            "Promising", "🌱", "", Guid.NewGuid(), DateTimeOffset.UtcNow));
    }

    [TestMethod]
    public void Apply_UnknownReaction_IsRejected()
    {
        var fixture = new ReactionFixture("Promising");
        var node = NodeTestFactory.Create();

        Assert.Throws<InvalidOperationException>(() => fixture.Service.Apply(
            node, "Native plants", node.AuthorId.Value, true, DateTimeOffset.UtcNow));
    }

    [TestMethod]
    public void Apply_CatalogReaction_ReusesExistingAssociation()
    {
        var fixture = new ReactionFixture("Promising");
        var node = NodeTestFactory.Create();

        var first = fixture.Service.Apply(node, "Promising", node.AuthorId.Value,
            true, DateTimeOffset.UtcNow);
        var second = fixture.Service.Apply(node, " promising ", node.AuthorId.Value,
            true, DateTimeOffset.UtcNow.AddMinutes(1));

        Assert.AreEqual(first.Id, second.Id);
        Assert.HasCount(1, fixture.Reactions.GetActiveForNode(node.Id));
    }

    [TestMethod]
    public void Apply_SameReactionToDifferentNodes_CreatesContextualTargets()
    {
        var fixture = new ReactionFixture("Important");
        var firstNode = NodeTestFactory.Create("First");
        var secondNode = NodeTestFactory.Create("Second");

        var first = fixture.Service.Apply(firstNode, "Important", firstNode.AuthorId.Value,
            true, DateTimeOffset.UtcNow);
        var second = fixture.Service.Apply(secondNode, "Important", secondNode.AuthorId.Value,
            true, DateTimeOffset.UtcNow);

        Assert.AreNotEqual(first.Id, second.Id);
        Assert.AreEqual(first.ReactionDefinitionId, second.ReactionDefinitionId);
    }

    [TestMethod]
    public void Apply_TracksAuthorAndCommunityReactions()
    {
        var fixture = new ReactionFixture("Hopeful", "Concerning");
        var node = NodeTestFactory.Create();

        var authorReaction = fixture.Service.Apply(node, "Hopeful", node.AuthorId.Value,
            true, DateTimeOffset.UtcNow);
        var communityReaction = fixture.Service.Apply(node, "Concerning", Guid.NewGuid(),
            true, DateTimeOffset.UtcNow);

        Assert.AreEqual(NodeReactionDisposition.Endorsed, authorReaction.Disposition);
        Assert.AreEqual(NodeReactionDisposition.Community, communityReaction.Disposition);
    }

    [TestMethod]
    public void Apply_ToArchivedNode_IsRejected()
    {
        var fixture = new ReactionFixture("Urgent");
        var node = NodeTestFactory.Create();
        node.Archive(node.AuthorId.Value, DateTimeOffset.UtcNow);

        Assert.Throws<InvalidOperationException>(() => fixture.Service.Apply(
            node, "Urgent", node.AuthorId.Value, true, DateTimeOffset.UtcNow.AddMinutes(1)));
    }

    private sealed class ReactionFixture
    {
        public InMemoryReactionDefinitionRepository Definitions { get; } = new();
        public InMemoryNodeReactionRepository Reactions { get; } = new();
        public NodeReactionApplicationService Service { get; }

        public ReactionFixture(params string[] words)
        {
            foreach (var word in words)
            {
                Definitions.Save(ReactionDefinition.Create(
                    word, "✨", $"A curated {word.ToLowerInvariant()} reaction.",
                    Guid.NewGuid(), DateTimeOffset.UtcNow));
            }
            Service = new NodeReactionApplicationService(Definitions, Reactions);
        }
    }

    private sealed class InMemoryReactionDefinitionRepository : IReactionDefinitionRepository
    {
        private readonly List<ReactionDefinition> _items = [];
        public IReadOnlyCollection<ReactionDefinition> GetAll() => _items.ToList();
        public ReactionDefinition? GetById(ReactionDefinitionId id) =>
            _items.SingleOrDefault(item => item.Id == id);
        public ReactionDefinition? GetByNormalizedText(string normalizedText) =>
            _items.SingleOrDefault(item => item.NormalizedText == normalizedText);
        public void Save(ReactionDefinition definition) => _items.Add(definition);
    }

    private sealed class InMemoryNodeReactionRepository : INodeReactionRepository
    {
        private readonly List<NodeReaction> _items = [];
        public IReadOnlyCollection<NodeReaction> GetAll() => _items.ToList();
        public NodeReaction? GetById(NodeReactionId id) =>
            _items.SingleOrDefault(item => item.Id == id);
        public IReadOnlyCollection<NodeReaction> GetActiveForNode(NodeId nodeId) =>
            _items.Where(item => item.NodeId == nodeId && !item.IsRemoved).ToList();
        public NodeReaction? GetActive(NodeId nodeId, ReactionDefinitionId definitionId) =>
            _items.SingleOrDefault(item => item.NodeId == nodeId &&
                item.ReactionDefinitionId == definitionId && !item.IsRemoved);
        public void Save(NodeReaction reaction)
        {
            var index = _items.FindIndex(item => item.Id == reaction.Id);
            if (index >= 0) _items[index] = reaction;
            else _items.Add(reaction);
        }
    }
}
