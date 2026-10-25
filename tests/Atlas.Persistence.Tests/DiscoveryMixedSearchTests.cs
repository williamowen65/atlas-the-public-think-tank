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
    public void WeightedRankingTranslatesAndOrdersBeforePaging()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => DiscoveryQueryValidation.Validate(new(VotingWeight: -1)));
        Assert.Throws<ArgumentOutOfRangeException>(() => DiscoveryQueryValidation.Validate(new(VotingWeight: 101)));
        using var db = new AtlasDataContext(new DbContextOptionsBuilder<AtlasDataContext>()
            .UseSqlServer("Server=localhost;Database=TranslationOnly;User Id=sa;Password=unused;").Options);
        var build = typeof(SqlPublicSearch).GetMethod("BuildHits", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;
        foreach (var weight in new[] { 0, 40, 100 })
        {
            var query = (IQueryable)build.Invoke(null, [db, new DiscoveryQuery(SearchText: "testing", VotingWeight: weight), "all"])!;
            var sql = query.ToQueryString();
            StringAssert.Contains(sql, "LOG(");
            StringAssert.Contains(sql, "ORDER BY");
        }
    }

    [TestMethod]
    public void VotingWeightOrdersTheWholeCandidateSetBeforeSelectingPages()
    {
        using var fixture = SqlTestDatabase.Create(useCurrentModel: true);
        var at = DateTimeOffset.UtcNow;
        var nodes = Enumerable.Range(0, 6).Select(i => new DiscoveryCandidate(Guid.NewGuid(),
            "Silver Otter Meadow", "", Guid.NewGuid(), Guid.NewGuid(), false, at.AddMinutes(-i),
            at.AddMinutes(-i), i == 0 ? 0 : 1, i == 0 ? null : i * 2.0, [], [])).ToArray();
        DiscoverySqlFixture.Seed(fixture, nodes);
        using var ready = fixture.OpenForDiscovery();
        var search = new SqlPublicSearch(fixture.Open);
        Assert.AreEqual(nodes[0].NodeId, search.Search(new(SearchText: "otter", VotingWeight: 0), "nodes").Items[0].NodeId);
        foreach (var weight in new[] { 40, 100 })
        {
            var first = search.Explain(new(SearchText: "otter", VotingWeight: weight), "nodes");
            var second = search.Explain(new(SearchText: "otter", VotingWeight: weight, Page: 2), "nodes");
            CollectionAssert.AreEqual(nodes.Reverse().Select(n => n.NodeId).ToArray(),
                first.Page.Items.Concat(second.Page.Items).Select(i => i.NodeId).ToArray());
            Assert.AreEqual(6, first.Page.TotalCount);
            Assert.IsTrue(first.Evidence[0].FinalScore > second.Evidence.Last().FinalScore);
        }
    }

    [TestMethod]
    public void CommonStopwordsDoNotBroadenSearchToOtherwiseUnrelatedNodes()
    {
        using var fixture = SqlTestDatabase.Create(useCurrentModel: true);
        var at = DateTimeOffset.UtcNow;
        var target = new DiscoveryCandidate(Guid.NewGuid(), "Otter sanctuary", "", Guid.NewGuid(), Guid.NewGuid(), false, at, at, 0, null, [], []);
        var unrelated = target with { NodeId = Guid.NewGuid(), Title = "Pebbles and stones" };
        DiscoverySqlFixture.Seed(fixture, target, unrelated);
        using var ready = fixture.OpenForDiscovery();
        Assert.AreEqual(0, ready.Database.SqlQuery<int>($"SELECT COALESCE(stoplist_id, -1) AS Value FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID(N'dbo.SearchDocuments')").Single());
        var results = new SqlPublicSearch(fixture.Open).Search(new(SearchText: "otter and", VotingWeight: 40), "nodes");
        Assert.AreEqual(1, results.TotalCount);
        Assert.AreEqual(target.NodeId, results.Items.Single().NodeId);
    }

    [TestMethod]
    public void OrderedPassagesAndStrictnessApplyBeforeVotingAndPagination()
    {
        using var fixture = SqlTestDatabase.Create(useCurrentModel: true);
        var at = DateTimeOffset.UtcNow;
        const string phrase = "Measure housing retention and wellbeing and ask participants which services actually helped";
        var exact = new DiscoveryCandidate(Guid.NewGuid(), "Passage", phrase, Guid.NewGuid(), Guid.NewGuid(), false, at, at, 0, null, [], []);
        var reversed = exact with { NodeId = Guid.NewGuid(), SearchableContent = string.Join(" ", phrase.Split(' ').Reverse()), VoteCount = 5, AverageVote = 10 };
        var weak = reversed with { NodeId = Guid.NewGuid(), Title = "Measure", SearchableContent = "An unrelated discussion" };
        DiscoverySqlFixture.Seed(fixture, exact, reversed, weak);
        using var ready = fixture.OpenForDiscovery();
        var search = new SqlPublicSearch(fixture.Open);
        const string query = "Measure housing retention and wellbeing, and ask participants which services actually helped.";
        var broad = search.Explain(new(SearchText: query, VotingWeight: 0, MatchStrictness: 0), "nodes");
        Assert.AreEqual(3, broad.Page.TotalCount);
        Assert.AreEqual(exact.NodeId, broad.Page.Items[0].NodeId);
        var direct = broad.Evidence.Single(e => e.Id == exact.NodeId);
        var scrambled = broad.Evidence.Single(e => e.Id == reversed.NodeId);
        Assert.IsTrue(direct.PhraseMatch);
        Assert.IsFalse(scrambled.PhraseMatch);
        Assert.IsTrue(direct.TextSignal > scrambled.TextSignal);
        var strict = search.Explain(new(SearchText: query, VotingWeight: 100, MatchStrictness: 100), "nodes");
        Assert.IsTrue(strict.Page.TotalCount < broad.Page.TotalCount, "Voting must not rescue the weak match.");
        Assert.IsTrue(strict.Evidence.All(e => e.TextRank == broad.BestTextRank));
        Assert.AreEqual(broad.BestTextRank!.Value, strict.MinimumTextRank!.Value, 0.00001);
        Assert.AreEqual(0, search.Search(new(SearchText: query, VotingWeight: 100, MatchStrictness: 100, Page: 99), "nodes").Items.Count);
    }

    [TestMethod]
    public void PassageInputIsLiteralBoundedAndDoesNotEnableSingleWordBonuses()
    {
        Assert.IsNull(SqlPublicSearch.OrderedPhrase("otter"));
        Assert.IsNull(SqlPublicSearch.OrderedPhrase(new string('a', 3998) + " b"));
        Assert.AreEqual(" Measure housing retention ", SqlPublicSearch.OrderedPhrase("  Measure housing,\nretention! "));
        Assert.AreEqual(" x OR y ", SqlPublicSearch.OrderedPhrase("x\" OR \"y"));
        Assert.Throws<ArgumentOutOfRangeException>(() => DiscoveryQueryValidation.Validate(new(MatchStrictness: -1)));
        Assert.Throws<ArgumentOutOfRangeException>(() => DiscoveryQueryValidation.Validate(new(MatchStrictness: 101)));
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
        // Both kinds use votes: one 10/10 node rating outranks a +1 equally relevant comment.
        using (var db = fixture.Open())
        {
            db.VoteRows.Add(new VoteRow { Id = Guid.NewGuid(), ParticipantId = node.AuthorParticipantId, TargetType = "Node", TargetId = node.NodeId, Value = 10, CreatedAt = at, UpdatedAt = at });
            db.SaveChanges();
        }
        var rated = search.Search(new(SearchText: "Silver Otter Meadow"));
        Assert.IsNull(rated.Items[0].Comment);
        Assert.AreEqual(10.0, rated.Items[0].AverageVote);
        Assert.AreEqual(1, rated.Items[0].VoteCount);
        Assert.AreEqual(up, rated.Items[1].Comment!.Id);
        var comments = search.Search(new(), "comments");
        Assert.AreEqual(up, comments.Items[0].Comment!.Id);
        using (var db = fixture.Open()) { db.VoteRows.RemoveRange(db.VoteRows.Where(v => v.TargetType == "Comment")); db.SaveChanges(); }
        Assert.IsNull(search.Search(new(SearchText: "Silver Otter Meadow")).Items[0].Comment);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task SearchReadsCommittedStateWhileVoteOrContentWriteRemainsOpen(bool editContent)
    {
        using var fixture = SqlTestDatabase.Create(useCurrentModel: true);
        var at = DateTimeOffset.UtcNow;
        var node = new DiscoveryCandidate(Guid.NewGuid(), "Silver Otter Meadow", "", Guid.NewGuid(), Guid.NewGuid(), false, at, at, 1, 7, [], []);
        DiscoverySqlFixture.Seed(fixture, node);
        using var ready = fixture.OpenForDiscovery();
        var search = new SqlPublicSearch(fixture.Open);
        var discovery = new SqlDiscoveryReadQuery(fixture.Open);
        Assert.AreEqual(1, search.Search(new(SearchText: "otter")).TotalCount);
        Assert.AreEqual(1, discovery.DiscoverPage(new()).TotalCount);
        using var writer = fixture.Open();
        using var transaction = writer.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);
        if (editContent) writer.NodeRows.Single(n => n.Id == node.NodeId).Title = "Uncommitted replacement";
        else writer.VoteRows.Single(v => v.TargetType == "Node" && v.TargetId == node.NodeId).Value = 10;
        writer.SaveChanges(); // Holds data/projection/revision locks until rollback below.
        var searchTask = Task.Run(() => search.Search(new(SearchText: "otter")));
        var feedTask = Task.Run(() => discovery.DiscoverPage(new()));
        var all = Task.WhenAll(searchTask, feedTask);
        var completedBeforeRollback = await Task.WhenAny(all, Task.Delay(TimeSpan.FromSeconds(5))) == all;
        transaction.Rollback(); // Always release the writer, including if the reader regresses.
        await all.WaitAsync(TimeSpan.FromSeconds(15));
        Assert.IsTrue(completedBeforeRollback, "Search/feed waited for an open writer transaction.");
        Assert.AreEqual("Silver Otter Meadow", searchTask.Result.Items.Single().Card!.Title);
        Assert.AreEqual(7.0, searchTask.Result.Items.Single().AverageVote);
        Assert.AreEqual("Silver Otter Meadow", feedTask.Result.Items.Single().Card!.Title);
        Assert.AreEqual(7.0, feedTask.Result.Items.Single().AverageVote);
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
