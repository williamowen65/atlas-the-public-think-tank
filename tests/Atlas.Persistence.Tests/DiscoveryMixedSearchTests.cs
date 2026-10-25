using Atlas.Discovery;
using Atlas.Persistence.Discovery;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Persistence.Tests;

[TestClass]
public sealed class DiscoveryMixedSearchTests
{
    [TestMethod]
    public void MixedQueryTranslatesWithoutConnectingToSqlServer()
    {
        using var db = new AtlasDataContext(new DbContextOptionsBuilder<AtlasDataContext>()
            .UseSqlServer("Server=localhost;Database=TranslationOnly;User Id=sa;Password=unused;").Options);
        var build = typeof(SqlPublicSearch).GetMethod("BuildHits", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;
        var query = (IQueryable)build.Invoke(null, [db, new DiscoveryQuery(SearchText: "salmon", MinimumVoteCount: 1), "all"])!;
        var sql = query.ToQueryString();
        StringAssert.Contains(sql, "FREETEXTTABLE");
        StringAssert.Contains(sql, "UNION ALL");
        StringAssert.Contains(sql, "CommentRows");
        var withoutText = (IQueryable)build.Invoke(null, [db, new DiscoveryQuery(), "comments"])!;
        Assert.IsFalse(withoutText.ToQueryString().Contains("FREETEXTTABLE", StringComparison.Ordinal));
    }

    [TestMethod]
    public void MixedHitsPageAsOneSequenceAndRespectVisibilityFiltersAndCommentAuthors()
    {
        using var fixture = SqlTestDatabase.Create(useCurrentModel: true);
        var at = DateTimeOffset.UtcNow;
        var node = new DiscoveryCandidate(Guid.NewGuid(), "Salmon protection", "Healthy rivers", Guid.NewGuid(), Guid.NewGuid(), false, at, at, 1, 7, [], []);
        var other = node with { NodeId = Guid.NewGuid(), Title = "Other discussion", SearchableContent = "Water", AuthorParticipantId = Guid.NewGuid() };
        var hidden = node with { NodeId = Guid.NewGuid(), IsModerationExcluded = true };
        var archived = node with { NodeId = Guid.NewGuid(), IsArchived = true };
        DiscoverySqlFixture.Seed(fixture, node, other, hidden, archived);
        var ids = Enumerable.Range(0, 7).Select(_ => Guid.NewGuid()).ToArray();
        using (var db = fixture.Open())
        {
            db.ParticipantRows.Single(p => p.Id == other.AuthorParticipantId).DisplayName = "Comment writer";
            foreach (var id in ids) db.CommentRows.Add(Comment(id, node.NodeId, other.AuthorParticipantId, at, new string('x', 320) + " salmon migration"));
            var removed = Comment(Guid.NewGuid(), node.NodeId, other.AuthorParticipantId, at, "salmon removed"); removed.Status = "RemovedByAuthor"; db.CommentRows.Add(removed);
            db.CommentRows.Add(Comment(Guid.NewGuid(), hidden.NodeId, other.AuthorParticipantId, at, "salmon hidden"));
            db.CommentRows.Add(Comment(Guid.NewGuid(), archived.NodeId, other.AuthorParticipantId, at, "salmon archived"));
            db.SaveChanges();
        }
        using var ready = fixture.OpenForDiscovery();
        var search = new SqlPublicSearch(fixture.Open);
        var first = search.Search(new(SearchText: "salmon"));
        var second = search.Search(new(SearchText: "salmon", Page: 2));
        var third = search.Search(new(SearchText: "salmon", Page: 3));
        Assert.AreEqual(8, first.TotalCount);
        Assert.AreEqual(3, first.Items.Count);
        Assert.IsTrue(first.HasNextPage); Assert.IsFalse(third.HasNextPage);
        var all = first.Items.Concat(second.Items).Concat(third.Items).ToArray();
        Assert.AreEqual(8, all.Select(i => i.Comment?.Id ?? i.NodeId).Distinct().Count());
        Assert.AreEqual(7, all.Count(i => i.Comment is not null));
        foreach (var hit in all.Where(i => i.Comment is not null))
        {
            Assert.AreEqual("Comment writer", hit.Comment!.AuthorName);
            Assert.AreEqual(other.AuthorParticipantId, hit.Comment.AuthorId);
            StringAssert.Contains(hit.Comment.Excerpt, "salmon");
            Assert.IsTrue(hit.Comment.Excerpt.StartsWith('…'));
        }
        Assert.AreEqual(7, search.Search(new(SearchText: "salmon", AuthorParticipantId: other.AuthorParticipantId), "comments").TotalCount);
        Assert.AreEqual(0, search.Search(new(SearchText: "salmon", AuthorParticipantId: node.AuthorParticipantId), "comments").TotalCount);
        Assert.AreEqual(1, search.Search(new(SearchText: "salmon"), "nodes").TotalCount);
        Assert.AreEqual(10, search.Search(new(SearchText: "salmon", IncludeArchived: true)).TotalCount); // archived node and its comment
        Assert.AreEqual(0, search.Search(new(SearchText: "salmon", NodeId: other.NodeId)).TotalCount);
        Assert.Throws<ArgumentException>(() => search.Search(new(SearchText: "salmon"), "invalid"));
    }

    [TestMethod]
    public void CommentPathIncludesRemovedAncestorsWithoutTheirBodiesAndRejectsOtherTargets()
    {
        using var fixture = SqlTestDatabase.Create(useCurrentModel: true);
        var at = DateTimeOffset.UtcNow;
        var node = new DiscoveryCandidate(Guid.NewGuid(), "Discussion", "Body", Guid.NewGuid(), Guid.NewGuid(), false, at, at, 0, null, [], []);
        DiscoverySqlFixture.Seed(fixture, node);
        var root = Comment(Guid.NewGuid(), node.NodeId, node.AuthorParticipantId, at, "removed secret"); root.Status = "RemovedByAuthor";
        var child = Comment(Guid.NewGuid(), node.NodeId, node.AuthorParticipantId, at, "Reply"); child.ParentCommentId = root.Id;
        var leaf = Comment(Guid.NewGuid(), node.NodeId, node.AuthorParticipantId, at, "Target"); leaf.ParentCommentId = child.Id;
        using (var db = fixture.Open()) { db.CommentRows.AddRange(root, child, leaf); db.SaveChanges(); }
        var search = new SqlPublicSearch(fixture.Open);
        var path = search.CommentPath(node.NodeId, leaf.Id);
        CollectionAssert.AreEqual(new[] { root.Id, child.Id, leaf.Id }, path.Select(c => c.Id).ToArray());
        Assert.IsTrue(path[0].IsRemoved); Assert.AreEqual("", path[0].Body);
        Assert.Throws<KeyNotFoundException>(() => search.CommentPath(Guid.NewGuid(), leaf.Id));
        Assert.Throws<KeyNotFoundException>(() => search.CommentPath(node.NodeId, root.Id));
        using (var db = fixture.Open()) { db.ModerationCaseRows.Add(new ModerationCaseRow { Id = Guid.NewGuid(), NodeId = node.NodeId, ReporterId = node.AuthorParticipantId, ReportedTitle = node.Title, Reason = "Spam", Status = 2, PublicReason = 1, CreatedAt = at }); db.SaveChanges(); }
        Assert.Throws<KeyNotFoundException>(() => search.CommentPath(node.NodeId, leaf.Id));
    }

    private static CommentRow Comment(Guid id, Guid node, Guid author, DateTimeOffset at, string body) => new()
    { Id = id, TargetKind = "Node", TargetId = node, AuthorParticipantId = author, Body = body, Status = "Active", CreatedAt = at, UpdatedAt = at };
}
