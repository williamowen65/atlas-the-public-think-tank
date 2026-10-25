using Atlas.Discovery;
using Atlas.Persistence.Discovery;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Persistence.Tests;

[TestClass]
public sealed class DiscoveryChildFeedTests
{
    [TestMethod]
    public void ParentAndTypeFilterPageIndependentlyAndRespectModeration()
    {
        using var fixture = SqlTestDatabase.Create(useCurrentModel: true);
        var author = Guid.NewGuid(); var type = Guid.NewGuid(); var otherType = Guid.NewGuid();
        var at = DateTimeOffset.UtcNow;
        DiscoveryCandidate Candidate(Guid kind) => new(Guid.NewGuid(), "Child", "Content", kind, author, false, at, at, 0, null, [], []);
        var parent = Candidate(type); var otherParent = Candidate(type);
        var children = Enumerable.Range(0, 6).Select(_ => Candidate(type)).ToArray();
        var differentType = Candidate(otherType);
        DiscoverySqlFixture.Seed(fixture, [parent, otherParent, .. children, differentType]);
        using (var db = fixture.Open())
        {
            foreach (var child in children.Append(differentType))
                db.NodeRows.Single(row => row.Id == child.NodeId).ParentNodeIds = [parent.NodeId];
            db.SaveChanges();
        }
        using var cache = new DiscoveryCache();
        var query = new SqlDiscoveryReadQuery(fixture.OpenForDiscovery, cache);
        var first = query.DiscoverPage(new(ParentNodeId: parent.NodeId, NodeTypeId: type));
        var second = query.DiscoverPage(new(ParentNodeId: parent.NodeId, NodeTypeId: type, Page: 2));
        Assert.AreEqual(6, first.TotalCount);
        Assert.IsTrue(first.HasNextPage); Assert.IsFalse(second.HasNextPage);
        CollectionAssert.AreEquivalent(children.Select(child => child.NodeId).ToArray(), first.Items.Concat(second.Items).Select(item => item.NodeId).ToArray());
        Assert.IsEmpty(query.DiscoverPage(new(ParentNodeId: otherParent.NodeId)).Items);
        Assert.AreEqual(differentType.NodeId, query.DiscoverPage(new(ParentNodeId: parent.NodeId, NodeTypeId: otherType)).Items.Single().NodeId);
        using (var db = fixture.Open())
        {
            db.ModerationCaseRows.Add(new ModerationCaseRow { Id = Guid.NewGuid(), NodeId = parent.NodeId, ReporterId = author, ReportedTitle = parent.Title, Reason = "Spam", Status = 2, PublicReason = 1, CreatedAt = at });
            db.SaveChanges();
        }
        Assert.IsEmpty(query.DiscoverPage(new(ParentNodeId: parent.NodeId, NodeTypeId: type)).Items);
        Assert.ThrowsExactly<ArgumentException>(() => DiscoveryQueryValidation.Validate(new(ParentNodeId: Guid.Empty)));
    }
}
