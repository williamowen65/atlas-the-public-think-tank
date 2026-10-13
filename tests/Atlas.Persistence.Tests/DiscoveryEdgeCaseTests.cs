using Atlas.Discovery;
using Atlas.Persistence.Discovery;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Persistence.Tests;

[TestClass]
public sealed class DiscoveryEdgeCaseTests
{
    [TestMethod]
    public void Accent_title_ties_use_dotnet_guid_order_and_dates_keep_the_timestamp_calendar_day()
    {
        using var fixture = SqlTestDatabase.Create(useCurrentModel: true);
        var created = new DateTimeOffset(2026, 10, 1, 0, 30, 0, TimeSpan.FromHours(12));
        var type = Guid.NewGuid(); var author = Guid.NewGuid();
        var a = new DiscoveryCandidate(Guid.Parse("00000001-0000-0000-0000-ffffffffffff"), "Café", "", type, author, false, created, created, 1, 5, [], []);
        var b = a with { NodeId = Guid.Parse("00000002-0000-0000-0000-000000000000"), Title = "cafe" };
        DiscoverySqlFixture.Seed(fixture, b, a);
        var query = new SqlDiscoveryReadQuery(fixture.Open);
        var page = query.DiscoverPage(new(CreatedFrom: new(2026, 10, 1), CreatedThrough: new(2026, 10, 1)));
        CollectionAssert.AreEqual(new[] { a.NodeId, b.NodeId }, page.Items.Select(item => item.NodeId).ToArray());
        Assert.AreEqual(5 + Math.Log10(2) / 100, page.Items[0].RankingScore, 0.0000001);
        Assert.IsEmpty(query.DiscoverPage(new(CreatedThrough: new(2026, 9, 30))).Items);
        Assert.AreEqual(2, query.DiscoverPage(new(CreatedFrom: DateOnly.MinValue, CreatedThrough: DateOnly.MaxValue)).TotalCount);
        Assert.Throws<ArgumentNullException>(() => query.DiscoverAuthoredPage(new(), null!));
        Assert.AreEqual(2, query.DiscoverAuthorPage(new(), author).TotalCount);
        Assert.IsEmpty(query.DiscoverAuthorPage(new(AuthorParticipantId: Guid.NewGuid()), author).Items);
    }

    [TestMethod]
    public void Search_covers_ordered_block_fields_and_literal_wildcards_without_matching_poll_ids()
    {
        using var fixture = SqlTestDatabase.Create(useCurrentModel: true);
        var candidate = new DiscoveryCandidate(Guid.NewGuid(), "100% coast", "climate", Guid.NewGuid(), Guid.NewGuid(), false, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, 0, null, [], []);
        DiscoverySqlFixture.Seed(fixture, candidate);
        using (var db = fixture.Open())
        {
            var document = db.DocumentRows.Include(row => row.Blocks).Single();
            var kinds = new[] { ("markdown", "adaptation"), ("image", "résilience"), ("video", "orcas"), ("link-preview", "ocean"), ("chart-reference", "trend"), ("poll-reference", "secret-poll") };
            for (var i = 0; i < kinds.Length; i++)
            {
                var block = new BlockRow { Id = Guid.NewGuid(), Kind = kinds[i].Item1, Markdown = kinds[i].Item2, AltText = kinds[i].Item2, Caption = kinds[i].Item2, Title = kinds[i].Item2, Description = kinds[i].Item2, Url = "https://example.test/whales", CreatedAt = candidate.CreatedAt, UpdatedAt = candidate.UpdatedAt };
                db.BlockRows.Add(block);
                document.Blocks.Add(new DocumentBlockRow { DocumentId = document.Id, BlockId = block.Id, Position = i + 1 });
            }
            db.SaveChanges();
        }
        var query = new SqlDiscoveryReadQuery(fixture.Open);
        foreach (var text in new[] { "%", "climate adaptation", "resilience", "orcas", "ocean", "trend", "whales" })
            Assert.AreEqual(candidate.NodeId, query.DiscoverPage(new(SearchText: text)).Items.Single().NodeId, text);
        Assert.IsEmpty(query.DiscoverPage(new(SearchText: "secret-poll")).Items);
        Assert.IsEmpty(query.DiscoverPage(new(SearchText: "100_")).Items);
    }
    [TestMethod]
    public void Batched_cards_preserve_ancestor_paths_child_pluralization_and_hidden_ancestor_notices()
    {
        using var fixture = SqlTestDatabase.Create(useCurrentModel: true);
        var now = DateTimeOffset.UtcNow;
        var root = new DiscoveryCandidate(Guid.NewGuid(), "Root", "", Guid.NewGuid(), Guid.NewGuid(), false, now, now, 0, null, [], []);
        var parent = root with { NodeId = Guid.NewGuid(), Title = "Parent" };
        var child = root with { NodeId = Guid.NewGuid(), Title = "Child one" };
        var other = root with { NodeId = Guid.NewGuid(), Title = "Child two" };
        DiscoverySqlFixture.Seed(fixture, root, parent, child, other);
        using (var db = fixture.Open())
        {
            db.Set<NodeParentRow>().AddRange(
                new NodeParentRow { NodeId = parent.NodeId, ParentNodeId = root.NodeId, Position = 0 },
                new NodeParentRow { NodeId = child.NodeId, ParentNodeId = parent.NodeId, Position = 0 },
                new NodeParentRow { NodeId = other.NodeId, ParentNodeId = parent.NodeId, Position = 0 });
            db.SaveChanges();
        }
        using var cache = new DiscoveryCache();
        var query = new SqlDiscoveryReadQuery(fixture.Open, cache);
        Assert.AreEqual("2 Ideas", query.DiscoverPage(new(SearchText: "Parent")).Items.Single().Card!.SubNodes);
        Assert.AreEqual("Idea · Parent → Idea · Root", query.DiscoverPage(new(SearchText: "Child one")).Items.Single().Card!.Breadcrumb);
        using (var db = fixture.Open())
        {
            db.ModerationCaseRows.Add(new ModerationCaseRow { Id = Guid.NewGuid(), NodeId = root.NodeId, ReporterId = root.AuthorParticipantId, Reason = "Spam", ReportedTitle = root.Title, Status = 2, PublicReason = 1, CreatedAt = now });
            db.SaveChanges();
        }
        var breadcrumb = query.DiscoverPage(new(SearchText: "Child one")).Items.Single().Card!.Breadcrumb;
        StringAssert.Contains(breadcrumb, "Hidden by moderator: Spam");
        Assert.IsFalse(breadcrumb.Contains("Root", StringComparison.Ordinal));
    }

}
