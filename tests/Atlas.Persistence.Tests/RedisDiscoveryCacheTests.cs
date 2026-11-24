using System.Data.Common;
using Atlas.Discovery;
using Atlas.Persistence.Discovery;
using StackExchange.Redis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Atlas.Persistence.Tests;

[TestClass]
public sealed class RedisDiscoveryCacheTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public void Redis_entries_persist_without_expiration_and_only_match_the_requested_revision()
    {
        var connection = ConnectionString();
        var key = $"atlas:test:{Guid.NewGuid():N}";
        using (var writer = new RedisDiscoveryCache(connection))
            writer.Write(key, 1, "test value");

        using var reader = new RedisDiscoveryCache(connection);
        Assert.AreEqual("test value", reader.Read<string>([key], 1).Single());
        using var inspection = ConnectionMultiplexer.Connect(connection);
        var database = inspection.GetDatabase();
        Assert.IsNull(database.KeyTimeToLive(key));
        Assert.IsNull(reader.Read<string>([key], 2).Single());
        reader.Write(key, 2, "updated value");
        Assert.IsNull(reader.Read<string>([key], 1).Single());
        Assert.AreEqual("updated value", reader.Read<string>([key], 2).Single());
        database.KeyDelete(key);
        Assert.IsNull(reader.Read<string>([key], 1).Single());
    }

    [TestMethod]
    public void Restart_reuses_filtered_page_and_cards_until_a_committed_revision_changes()
    {
        var connection = ConnectionString();
        using var fixture = SqlTestDatabase.Create(useCurrentModel: true, installFullText: false);
        using (var db = fixture.Open())
            db.Database.ExecuteSqlRaw("ALTER DATABASE CURRENT SET ALLOW_SNAPSHOT_ISOLATION ON");
        var candidate = new DiscoveryCandidate(Guid.NewGuid(), "Before edit", "evidence",
            Guid.NewGuid(), Guid.NewGuid(), false, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, 1, 8, [], []);
        DiscoverySqlFixture.Seed(fixture, candidate);
        var query = new DiscoveryQuery(NodeId: candidate.NodeId);
        var commands = new SqlReads();
        AtlasDataContext Open() => fixture.OpenWithInterceptor(commands);
        DiscoveryCache OpenCache() => new(external: new RedisDiscoveryCache(connection));

        using (var original = OpenCache())
        {
            var card = new SqlDiscoveryReadQuery(Open, original).DiscoverPage(query).Items.Single().Card!;
            Assert.AreEqual(candidate.SearchableContent, card.PreviewBlocks.Single().Content.Markdown);
            Assert.AreEqual(1, commands.RevisionChecks);
            Assert.IsGreaterThan(0, commands.OtherReads);
            RecordReads("Cold", commands);
        }

        using (var restarted = OpenCache())
        {
            commands.Reset();
            var card = new SqlDiscoveryReadQuery(Open, restarted).DiscoverPage(query).Items.Single().Card!;
            Assert.AreEqual("Before edit", card.Title);
            Assert.AreEqual(candidate.SearchableContent, card.PreviewBlocks.Single().Content.Markdown);
            Assert.AreEqual(1, commands.RevisionChecks);
            Assert.AreEqual(0, commands.OtherReads);
            RecordReads("Restart hit", commands);
        }

        using (var db = fixture.Open())
        using (var transaction = db.Database.BeginTransaction())
        {
            db.NodeRows.Single().Title = "Rolled back";
            db.SaveChanges();
            transaction.Rollback();
        }
        using (var afterRollback = OpenCache())
        {
            commands.Reset();
            Assert.AreEqual("Before edit", new SqlDiscoveryReadQuery(Open, afterRollback)
                .DiscoverPage(query).Items.Single().Card!.Title);
            Assert.AreEqual(1, commands.RevisionChecks);
            Assert.AreEqual(0, commands.OtherReads);
        }

        using (var db = fixture.Open())
        {
            db.VoteRows.Single().Value = 6;
            db.SaveChanges();
        }
        using (var afterVote = OpenCache())
        {
            commands.Reset();
            Assert.AreEqual("Before edit", new SqlDiscoveryReadQuery(Open, afterVote)
                .DiscoverPage(query).Items.Single().Card!.Title);
            Assert.AreEqual(1, commands.RevisionChecks);
            Assert.AreEqual(2, commands.OtherReads, "A vote must rebuild ranking while reusing public cards.");
            RecordReads("After vote", commands);
        }

        using (var db = fixture.Open())
        {
            db.NodeRows.Single().Title = "Committed edit";
            db.SaveChanges();
        }
        using (var afterEdit = OpenCache())
        {
            commands.Reset();
            Assert.AreEqual("Committed edit", new SqlDiscoveryReadQuery(Open, afterEdit)
                .DiscoverPage(query).Items.Single().Card!.Title);
            Assert.IsGreaterThan(2, commands.OtherReads);
            RecordReads("After committed edit", commands);
        }

        using var offline = new DiscoveryCache(external: new RedisDiscoveryCache("127.0.0.1:1"));
        commands.Reset();
        Assert.AreEqual("Committed edit", new SqlDiscoveryReadQuery(Open, offline)
            .DiscoverPage(query).Items.Single().Card!.Title);
        Assert.AreEqual(1, commands.RevisionChecks);
        Assert.IsGreaterThan(2, commands.OtherReads);

        using (var db = fixture.Open())
        {
            db.ModerationCaseRows.Add(new ModerationCaseRow
            {
                Id = Guid.NewGuid(), NodeId = candidate.NodeId, ReporterId = candidate.AuthorParticipantId,
                ReportedTitle = "Committed edit", Reason = "Spam", Status = 2, PublicReason = 1,
                CreatedAt = DateTimeOffset.UtcNow
            });
            db.SaveChanges();
        }
        using var afterHide = OpenCache();
        Assert.IsEmpty(new SqlDiscoveryReadQuery(Open, afterHide).DiscoverPage(query).Items);
    }

    private void RecordReads(string phase, SqlReads commands) =>
        TestContext.WriteLine($"{phase}: {commands.RevisionChecks} revision reads ({commands.RevisionMs:F2} ms), " +
            $"{commands.OtherReads} rebuild reads ({commands.RebuildMs:F2} ms).");

    private static string ConnectionString()
    {
        var connection = Environment.GetEnvironmentVariable("ATLAS_DISCOVERY_REDIS_TEST_CONNECTION");
        if (string.IsNullOrWhiteSpace(connection))
            Assert.Inconclusive("Set ATLAS_DISCOVERY_REDIS_TEST_CONNECTION to run Redis cache tests.");
        return connection!;
    }

    private sealed class SqlReads : DbCommandInterceptor
    {
        public int RevisionChecks { get; private set; }
        public int OtherReads { get; private set; }
        public double RevisionMs { get; private set; }
        public double RebuildMs { get; private set; }

        public override DbDataReader ReaderExecuted(
            DbCommand command, CommandExecutedEventData eventData, DbDataReader result)
        {
            if (command.CommandText.Contains("Discovery revision check", StringComparison.Ordinal))
            {
                RevisionChecks++;
                RevisionMs += eventData.Duration.TotalMilliseconds;
            }
            else
            {
                OtherReads++;
                RebuildMs += eventData.Duration.TotalMilliseconds;
            }
            return result;
        }

        public void Reset() => (RevisionChecks, OtherReads, RevisionMs, RebuildMs) = (0, 0, 0, 0);
    }
}
