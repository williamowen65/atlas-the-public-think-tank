using Atlas.Discovery;
using Atlas.Persistence.Discovery;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Persistence.Tests;

[TestClass]
public sealed class DiscoveryBreadcrumbTests
{
    [TestMethod]
    public void Deep_chain_returns_three_ancestors_and_resumes_without_skips()
    {
        using var fixture = SqlTestDatabase.Create(useCurrentModel: true);
        var now = DateTimeOffset.UtcNow;
        var seed = new DiscoveryCandidate(Guid.NewGuid(), "Node 0", "", Guid.NewGuid(), Guid.NewGuid(), false, now, now, 0, null, [], []);
        var chain = Enumerable.Range(0, 20).Select(index => seed with { NodeId = Guid.NewGuid(), Title = $"Node {index}" }).ToArray();
        DiscoverySqlFixture.Seed(fixture, chain);
        using var db = fixture.Open();
        for (var index = 0; index < chain.Length - 1; index++)
        {
            db.Set<NodeParentRow>().Add(new NodeParentRow { NodeId = chain[index].NodeId, ParentNodeId = chain[index + 1].NodeId, Position = 0 });
        }
        db.SaveChanges();
        var page = DiscoveryBreadcrumbReader.ReadPage(db, chain[0].NodeId);
        CollectionAssert.AreEqual(chain.Skip(1).Take(3).Select(node => node.NodeId).ToArray(), page.Items.Select(item => item.NodeId).ToArray());
        Assert.AreEqual(chain[3].NodeId, page.ContinuationNodeId);
        StringAssert.Contains(page.DisplayText, "…");
        Assert.IsFalse(page.DisplayText.Contains("Node 4", StringComparison.Ordinal));
        var next = DiscoveryBreadcrumbReader.ReadPage(db, page.ContinuationNodeId!.Value,
            new[] { chain[0].NodeId }.Concat(page.Items.Select(item => item.NodeId)).ToArray());
        CollectionAssert.AreEqual(chain.Skip(4).Take(3).Select(node => node.NodeId).ToArray(), next.Items.Select(item => item.NodeId).ToArray());
    }

    [TestMethod]
    public void Branches_follow_one_stored_route_and_redact_hidden_ancestors()
    {
        using var fixture = SqlTestDatabase.Create(useCurrentModel: true);
        var now = DateTimeOffset.UtcNow;
        var seed = new DiscoveryCandidate(Guid.NewGuid(), "Child", "", Guid.NewGuid(), Guid.NewGuid(), false, now, now, 0, null, [], []);
        var first = seed with { NodeId = Guid.NewGuid(), Title = "Hidden secret", IsModerationExcluded = true };
        var alternative = seed with { NodeId = Guid.NewGuid(), Title = "Alternate root" };
        DiscoverySqlFixture.Seed(fixture, seed, first, alternative);
        using var db = fixture.Open();
        db.Set<NodeParentRow>().AddRange(
            new NodeParentRow { NodeId = seed.NodeId, ParentNodeId = first.NodeId, Position = 0 },
            new NodeParentRow { NodeId = seed.NodeId, ParentNodeId = alternative.NodeId, Position = 1 });
        db.SaveChanges();
        var page = DiscoveryBreadcrumbReader.ReadPage(db, seed.NodeId);
        Assert.HasCount(1, page.Items);
        Assert.IsTrue(page.HasAlternativeRoutes);
        Assert.IsNull(page.ContinuationNodeId);
        StringAssert.Contains(page.DisplayText, "Hidden by moderator");
        Assert.IsFalse(page.DisplayText.Contains("Hidden secret", StringComparison.Ordinal));
        Assert.IsFalse(page.DisplayText.Contains("Alternate root", StringComparison.Ordinal));
        Assert.AreEqual("Root node", DiscoveryBreadcrumbReader.ReadPage(db, alternative.NodeId).DisplayText);
    }

    [TestMethod]
    public void Cycle_stops_and_continuation_respects_already_visited_nodes()
    {
        using var fixture = SqlTestDatabase.Create(useCurrentModel: true);
        var now = DateTimeOffset.UtcNow;
        var a = new DiscoveryCandidate(Guid.NewGuid(), "A", "", Guid.NewGuid(), Guid.NewGuid(), false, now, now, 0, null, [], []);
        var b = a with { NodeId = Guid.NewGuid(), Title = "B" };
        DiscoverySqlFixture.Seed(fixture, a, b);
        using var db = fixture.Open();
        db.Set<NodeParentRow>().AddRange(
            new NodeParentRow { NodeId = a.NodeId, ParentNodeId = b.NodeId, Position = 0 },
            new NodeParentRow { NodeId = b.NodeId, ParentNodeId = a.NodeId, Position = 0 });
        db.SaveChanges();
        var page = DiscoveryBreadcrumbReader.ReadPage(db, a.NodeId);
        Assert.HasCount(1, page.Items);
        Assert.IsTrue(page.CycleDetected);
        Assert.IsNull(page.ContinuationNodeId);
        Assert.IsTrue(DiscoveryBreadcrumbReader.ReadPage(db, b.NodeId, [a.NodeId, b.NodeId]).CycleDetected);
    }
}
