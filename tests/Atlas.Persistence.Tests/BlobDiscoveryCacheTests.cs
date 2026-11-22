using System.Data.Common;
using Atlas.Discovery;
using Atlas.Persistence.Discovery;
using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Atlas.Persistence.Tests;

[TestClass]
public sealed class BlobDiscoveryCacheTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public void Blob_round_trip_survives_new_client_and_respects_expiration()
    {
        var container = CreateContainer();
        try
        {
            var id = Guid.NewGuid();
            var card = new DiscoveryCard(id, "Public title", "Idea", "Author", "Active",
                DateTimeOffset.UtcNow, "Text", "", "", "", "", 0, false)
            {
                AuthorId = Guid.NewGuid(),
                ChildTypes = [new(Guid.NewGuid(), "Evidence", 2)]
            };
            var page = new DiscoveryPage([new(id, 1, 3.5, 1, 7, card)], 1, 1, 3);
            var writer = OpenCache(container);
            writer.Write("current", page, TimeSpan.FromMinutes(1));
            writer.Write("expired", page, TimeSpan.FromSeconds(-1));

            var restarted = OpenCache(container);
            var restored = restarted.Read("current")!.Page.Items.Single().Card!;
            Assert.AreEqual(card.Title, restored.Title);
            Assert.AreEqual(card.AuthorId, restored.AuthorId);
            Assert.AreEqual(card.ChildTypes.Single(), restored.ChildTypes.Single());
            Assert.IsNull(restarted.Read("expired"));
            Assert.IsNull(restarted.Read("missing"));
        }
        finally { container.Delete(); }
    }

    [TestMethod]
    public void Restart_reuses_page_and_cards_until_a_committed_revision_changes()
    {
        var container = CreateContainer();
        try
        {
            using var fixture = SqlTestDatabase.Create(useCurrentModel: true, installFullText: false);
            using (var db = fixture.Open())
                db.Database.ExecuteSqlRaw("ALTER DATABASE CURRENT SET ALLOW_SNAPSHOT_ISOLATION ON");
            var candidate = new DiscoveryCandidate(Guid.NewGuid(), "Before edit", "evidence",
                Guid.NewGuid(), Guid.NewGuid(), false, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, 1, 8, [], []);
            DiscoverySqlFixture.Seed(fixture, candidate);
            var commands = new SqlReads();
            AtlasDataContext Open() => fixture.OpenWithInterceptor(commands);
            using (var original = new DiscoveryCache(external: OpenCache(container)))
            {
                new SqlDiscoveryReadQuery(Open, original).DiscoverPage(new());
                Assert.AreEqual(1, commands.RevisionChecks);
                Assert.IsGreaterThan(0, commands.OtherReads);
                RecordReads("Cold", commands);
            }

            using var restarted = new DiscoveryCache(external: OpenCache(container));
            var reader = new SqlDiscoveryReadQuery(Open, restarted);
            commands.Reset();
            Assert.AreEqual("Before edit", reader.DiscoverPage(new()).Items.Single().Card!.Title);
            Assert.AreEqual(1, commands.RevisionChecks);
            Assert.AreEqual(0, commands.OtherReads, "A restart hit should only issue the revision read.");
            RecordReads("Restart hit", commands);

            using (var db = fixture.Open())
            using (var transaction = db.Database.BeginTransaction())
            {
                db.NodeRows.Single().Title = "Rolled back";
                db.SaveChanges();
                transaction.Rollback();
            }
            using (var afterRollback = new DiscoveryCache(external: OpenCache(container)))
            {
                commands.Reset();
                Assert.AreEqual("Before edit", new SqlDiscoveryReadQuery(Open, afterRollback)
                    .DiscoverPage(new()).Items.Single().Card!.Title);
                Assert.AreEqual(1, commands.RevisionChecks);
                Assert.AreEqual(0, commands.OtherReads);
            }

            using (var db = fixture.Open())
            {
                db.NodeRows.Single().Title = "Committed edit";
                db.SaveChanges();
            }
            commands.Reset();
            Assert.AreEqual("Committed edit", reader.DiscoverPage(new()).Items.Single().Card!.Title);
            Assert.AreEqual(1, commands.RevisionChecks);
            Assert.IsGreaterThan(0, commands.OtherReads);
            RecordReads("After committed edit", commands);

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
            using (var afterHide = new DiscoveryCache(external: OpenCache(container)))
                Assert.IsEmpty(new SqlDiscoveryReadQuery(Open, afterHide).DiscoverPage(new()).Items);
            using (var db = fixture.Open())
            {
                db.ModerationCaseRows.Single().VisibilityRestoredAt = DateTimeOffset.UtcNow;
                db.SaveChanges();
            }

            container.Delete();
            using var unavailable = new DiscoveryCache(external: OpenCache(container));
            var fallback = new SqlDiscoveryReadQuery(Open, unavailable);
            Assert.AreEqual("Committed edit", fallback.DiscoverPage(new()).Items.Single().Card!.Title);
            commands.Reset();
            Assert.AreEqual("Committed edit", fallback.DiscoverPage(new()).Items.Single().Card!.Title);
            Assert.AreEqual(1, commands.RevisionChecks);
            Assert.AreEqual(0, commands.OtherReads);


        }
        finally { container.DeleteIfExists(); }
    }

    private void RecordReads(string phase, SqlReads commands) =>
        TestContext.WriteLine($"{phase}: {commands.RevisionChecks} revision reads ({commands.RevisionMs:F2} ms), " +
            $"{commands.OtherReads} rebuild reads ({commands.RebuildMs:F2} ms).");

    private static BlobContainerClient CreateContainer()
    {
        var client = new BlobContainerClient(ConnectionString(), $"discovery-{Guid.NewGuid():N}");
        client.Create();
        return client;
    }

    private static BlobDiscoveryCache OpenCache(BlobContainerClient container) =>
        BlobDiscoveryCache.ForEmulator(ConnectionString(), container.Name);

    private static string ConnectionString()
    {
        var connection = Environment.GetEnvironmentVariable("ATLAS_DISCOVERY_CACHE_TEST_EMULATOR");
        if (string.IsNullOrWhiteSpace(connection))
            Assert.Inconclusive("Set ATLAS_DISCOVERY_CACHE_TEST_EMULATOR to run Blob cache tests.");
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
