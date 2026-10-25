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
        StringAssert.Contains(sql, "FREETEXTTABLE(dbo.SearchDocuments");
        Assert.IsFalse(sql.Contains("FREETEXTTABLE(dbo.CommentRows", StringComparison.Ordinal));
        Assert.IsFalse(sql.Contains("FREETEXTTABLE(dbo.NodeRows", StringComparison.Ordinal));
        StringAssert.Contains(sql, "UNION ALL");
        StringAssert.Contains(sql, "ABS(");
        StringAssert.Contains(sql, "VoteRows");
        StringAssert.Contains(sql, "CommentRows");
        var withoutText = (IQueryable)build.Invoke(null, [db, new DiscoveryQuery(), "comments"])!;
        Assert.IsFalse(withoutText.ToQueryString().Contains("FREETEXTTABLE", StringComparison.Ordinal));
    }

    [TestMethod]
    public void MixedHitsPageAsOneSequenceAndRespectVisibilityFiltersAndCommentAuthors()
    {
        using var fixture = SqlTestDatabase.Create(useCurrentModel: true);
        var at = DateTimeOffset.UtcNow;
        var node = new DiscoveryCandidate(Guid.NewGuid(), "Salmon protection", "Salmon rivers", Guid.NewGuid(), Guid.NewGuid(), false, at, at, 1, 7, [], []);
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
        Assert.AreEqual(7, search.Search(new(), "comments").TotalCount);
        Assert.AreEqual(10, search.Search(new(SearchText: "salmon", IncludeArchived: true)).TotalCount); // archived node and its comment
        Assert.AreEqual(0, search.Search(new(SearchText: "salmon", NodeId: other.NodeId)).TotalCount);
        Assert.Throws<ArgumentException>(() => search.Search(new(SearchText: "salmon"), "invalid"));
    }

    [TestMethod]
    public void IdenticalNodeAndCommentDocumentsHaveEqualRanksAndSharePagination()
    {
        using var fixture = SqlTestDatabase.Create(useCurrentModel: true);
        var at = DateTimeOffset.UtcNow;
        var node = new DiscoveryCandidate(Guid.NewGuid(), "Silver Otter Meadow", "", Guid.NewGuid(), Guid.NewGuid(), false, at, at, 0, null, [], []);
        DiscoverySqlFixture.Seed(fixture, node);
        var ids = Enumerable.Range(0, 4).Select(_ => Guid.NewGuid()).ToArray();
        using (var db = fixture.Open())
        {
            // Use exactly the same document, including whitespace: any rank difference would
            // indicate that the result types have been sent to different ranking corpora.
            var text = db.Database.SqlQuery<string>($"SELECT SearchText AS [Value] FROM dbo.SearchDocuments WHERE ResultId = {node.NodeId} AND Kind = 0").Single();
            foreach (var id in ids) db.CommentRows.Add(Comment(id, node.NodeId, node.AuthorParticipantId, at, text));
            db.SaveChanges();
        }
        using var ready = fixture.OpenForDiscovery();
        var scores = ready.Database.SqlQuery<RankedDocument>($"""
            SELECT d.ResultId, hits.[RANK] AS Score FROM FREETEXTTABLE(dbo.SearchDocuments, SearchText, {"Silver Otter Meadow"}, LANGUAGE 1033) hits
            JOIN dbo.SearchDocuments d ON d.SearchKey = hits.[KEY]
            """).ToArray();
        Assert.AreEqual(5, scores.Length);
        Assert.AreEqual(1, scores.Select(r => r.Score).Distinct().Count(), "Identical documents must share a score regardless of result type.");
        var search = new SqlPublicSearch(fixture.Open);
        var first = search.Search(new(SearchText: "Silver Otter Meadow"));
        var second = search.Search(new(SearchText: "Silver Otter Meadow", Page: 2));
        Assert.AreEqual(5, first.TotalCount);
        Assert.AreEqual(node.NodeId, first.Items[0].NodeId);
        Assert.IsNull(first.Items[0].Comment, "The deterministic node tie-break must apply only after relevance.");
        Assert.AreEqual(5, first.Items.Concat(second.Items).Select(i => i.Comment?.Id ?? i.NodeId).Distinct().Count());
    }

    [TestMethod]
    public void CommentVotesAdjustSharedRelevanceAndTotalsWithoutChangingTheSearchIndex()
    {
        using var fixture = SqlTestDatabase.Create(useCurrentModel: true);
        var at = DateTimeOffset.UtcNow;
        var node = new DiscoveryCandidate(Guid.NewGuid(), "Silver Otter Meadow", "", Guid.NewGuid(), Guid.NewGuid(), false, at, at, 0, null, [], []);
        DiscoverySqlFixture.Seed(fixture, node);
        var up = Guid.NewGuid(); var down = Guid.NewGuid();
        using (var db = fixture.Open())
        {
            var text = db.Database.SqlQuery<string>($"SELECT SearchText AS [Value] FROM dbo.SearchDocuments WHERE ResultId = {node.NodeId} AND Kind = 0").Single();
            db.CommentRows.Add(Comment(up, node.NodeId, node.AuthorParticipantId, at, text));
            db.CommentRows.Add(Comment(down, node.NodeId, node.AuthorParticipantId, at, text));
            db.VoteRows.Add(new VoteRow { Id = Guid.NewGuid(), ParticipantId = node.AuthorParticipantId, TargetType = "Comment", TargetId = up, Value = 1, CreatedAt = at, UpdatedAt = at });
            db.VoteRows.Add(new VoteRow { Id = Guid.NewGuid(), ParticipantId = node.AuthorParticipantId, TargetType = "Comment", TargetId = down, Value = -1, CreatedAt = at, UpdatedAt = at });
            db.SaveChanges();
        }
        using var ready = fixture.OpenForDiscovery();
        var search = new SqlPublicSearch(fixture.Open);
        var page = search.Search(new(SearchText: "Silver Otter Meadow"));
        Assert.AreEqual(3, page.TotalCount);
        Assert.AreEqual(up, page.Items[0].Comment!.Id);
        Assert.IsNull(page.Items[1].Comment);
        Assert.AreEqual(down, page.Items[2].Comment!.Id);
        Assert.AreEqual(1, page.Items[0].Comment!.Score);
        Assert.AreEqual(-1, page.Items[2].Comment!.Score);
        var comments = search.Search(new(), "comments");
        Assert.AreEqual(up, comments.Items[0].Comment!.Id);
        using (var db = fixture.Open()) { db.VoteRows.RemoveRange(db.VoteRows.Where(v => v.TargetType == "Comment")); db.SaveChanges(); }
        Assert.IsNull(search.Search(new(SearchText: "Silver Otter Meadow")).Items[0].Comment);
    }

    [TestMethod]
    public void ProjectionTracksNodeBlockLinkAndCommentWritesAndSetupPreservesKeys()
    {
        using var fixture = SqlTestDatabase.Create(useCurrentModel: true);
        var at = DateTimeOffset.UtcNow;
        var node = new DiscoveryCandidate(Guid.NewGuid(), "Original title", "Original block", Guid.NewGuid(), Guid.NewGuid(), false, at, at, 0, null, [], []);
        DiscoverySqlFixture.Seed(fixture, node);
        var commentId = Guid.NewGuid();
        using var db = fixture.Open();
        db.CommentRows.Add(Comment(commentId, node.NodeId, node.AuthorParticipantId, at, "Original comment"));
        db.SaveChanges();
        long Key(Guid id) => db.Database.SqlQuery<long>($"SELECT SearchKey AS [Value] FROM dbo.SearchDocuments WHERE ResultId = {id}").Single();
        string Text(Guid id) => db.Database.SqlQuery<string>($"SELECT SearchText AS [Value] FROM dbo.SearchDocuments WHERE ResultId = {id}").Single();
        var key = Key(node.NodeId);
        StringAssert.Contains(Text(node.NodeId), "Original block");
        db.NodeRows.Single().Title = "Revised title";
        db.BlockRows.Single().Markdown = "Revised block";
        db.CommentRows.Single().Body = "Revised comment";
        db.SaveChanges();
        StringAssert.Contains(Text(node.NodeId), "Revised title");
        StringAssert.Contains(Text(node.NodeId), "Revised block");
        Assert.AreEqual("Revised comment", Text(commentId));
        db.DocumentRows.Include(d => d.Blocks).Single().Blocks.Clear();
        db.SaveChanges();
        Assert.IsFalse(Text(node.NodeId).Contains("Revised block", StringComparison.Ordinal));
        // Raw SQL writes (including seed/import paths) must be tracked too.
        db.Database.ExecuteSqlInterpolated($"UPDATE dbo.NodeRows SET ShortDescription = N'Raw import text' WHERE Id = {node.NodeId}");
        StringAssert.Contains(Text(node.NodeId), "Raw import text");
        AtlasDatabaseSetup.InstallSupplementalSchema(db);
        Assert.AreEqual(key, Key(node.NodeId), "Repeat setup must preserve the full-text document key.");
        db.CommentRows.Single().Status = "RemovedByAuthor";
        db.SaveChanges();
        Assert.AreEqual(0, db.Database.SqlQuery<int>($"SELECT COUNT(*) AS [Value] FROM dbo.SearchDocuments WHERE ResultId = {commentId}").Single());
        db.CommentRows.Single().Status = "Active";
        db.SaveChanges();
        Assert.AreEqual("Revised comment", Text(commentId));
        db.CommentRows.Remove(db.CommentRows.Single());
        db.SaveChanges();
        Assert.AreEqual(0, db.Database.SqlQuery<int>($"SELECT COUNT(*) AS [Value] FROM dbo.SearchDocuments WHERE ResultId = {commentId}").Single());
    }

    public sealed class RankedDocument { public Guid ResultId { get; set; } public int Score { get; set; } }

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
