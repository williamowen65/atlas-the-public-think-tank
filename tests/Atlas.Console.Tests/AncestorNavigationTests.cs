using Atlas.Application.Queries;
using Atlas.Graph.Nodes;
using Atlas.Graph.Nodes.NodeTypes;

namespace Atlas.ConsoleApp.Tests;

[TestClass]
public sealed class AncestorNavigationTests
{
    [TestMethod]
    public void Reads_three_nodes_per_segment_and_continues_above_last_item()
    {
        var ids = Enumerable.Range(0, 8).Select(_ => new NodeId(Guid.NewGuid())).ToArray();
        var nodes = ids.Select((id, index) => Make(id, index == ids.Length - 1 ? [] : [ids[index + 1]])).ToArray();
        var repository = new RecordingRepository(nodes);
        var visited = new HashSet<NodeId>();
        var first = AncestorNavigation.ReadSegment(nodes[0], repository, visited);
        Assert.HasCount(3, first.Items);
        Assert.AreEqual(3, repository.Reads);
        Assert.IsTrue(first.HasMore);
        CollectionAssert.AreEqual(ids.Skip(1).Take(3).ToArray(), first.Items.Select(node => node.Id).ToArray());
        var next = AncestorNavigation.ReadSegment(first.Items[^1], repository, visited);
        CollectionAssert.AreEqual(ids.Skip(4).Take(3).ToArray(), next.Items.Select(node => node.Id).ToArray());
        Assert.AreEqual(6, repository.Reads);
    }

    [TestMethod]
    public void Explicit_parent_choice_follows_only_that_route_and_cycle_stops()
    {
        var a = new NodeId(Guid.NewGuid());
        var b = new NodeId(Guid.NewGuid());
        var c = new NodeId(Guid.NewGuid());
        var origin = Make(a, [b, c]);
        var repository = new RecordingRepository([origin, Make(b, []), Make(c, [a])]);
        var page = AncestorNavigation.ReadSegment(origin, repository, new HashSet<NodeId>(), c);
        Assert.HasCount(1, page.Items);
        Assert.AreEqual(c, page.Items[0].Id);
        Assert.AreEqual(1, repository.Reads);
        Assert.IsTrue(page.HasAlternativeRoutes);
        Assert.IsTrue(page.CycleDetected);
        Assert.IsFalse(page.HasMore);
    }

    private static Node Make(NodeId id, NodeId[] parents) => Node.Reconstitute(
        id, new NodeTitle(id.ToString()), new NodeDescriptionId(Guid.NewGuid()),
        new NodeTypeId(Guid.NewGuid()), new NodeAuthorId(Guid.NewGuid()), NodeStatus.Active,
        [], parents, DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow);

    private sealed class RecordingRepository(IEnumerable<Node> nodes) : INodeRepository
    {
        private readonly Dictionary<NodeId, Node> _nodes = nodes.ToDictionary(node => node.Id);
        public int Reads { get; private set; }
        public Node? GetById(NodeId id) { Reads++; return _nodes.GetValueOrDefault(id); }
        public IReadOnlyCollection<Node> GetChildren(NodeId id) => throw new NotSupportedException();
        public IReadOnlyCollection<Node> GetByAuthor(NodeAuthorId id) => throw new NotSupportedException();
        public IReadOnlyCollection<Node> GetParentCandidates(NodeId id, IReadOnlyCollection<NodeId> parents) => throw new NotSupportedException();
        public void Save(Node node) => throw new NotSupportedException();
    }
}
