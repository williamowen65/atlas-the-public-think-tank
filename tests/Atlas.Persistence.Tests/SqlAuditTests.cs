using Atlas.Contracts.Observability;
using Atlas.Persistence.Observability;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Atlas.Persistence.Tests;

/// <summary>Fresh-model fixtures test the proposed mappings without generating the developer-owned migration.</summary>
[TestClass]
public sealed class SqlAuditTests
{
    [TestMethod]
    public async Task Audit_and_outbox_commit_together_and_delivery_retries_keep_event_identity()
    {
        await WithDatabase(async open =>
        {
            Guid auditId;
            using (var operation = Telemetry.BeginOperation(Guid.NewGuid()))
            using (var db = open())
            {
                db.Roles.Add(new IdentityRole<Guid> { Id = Guid.NewGuid(), Name = "AuditTest", NormalizedName = "AUDITTEST" });
                await db.SaveChangesAsync();
                var audit = await db.AuditRecords.SingleAsync();
                auditId = audit.Id;
                Assert.IsNotNull(audit.ActorId);
                Assert.AreEqual(auditId, (await db.AuditOutbox.SingleAsync()).AuditRecordId);
            }
            var broken = new Destination(fail: true);
            Assert.IsTrue(await new AuditDelivery(open, broken).DeliverOneAsync());
            using (var db = open())
            {
                var pending = await db.AuditOutbox.SingleAsync();
                Assert.IsNull(pending.DeliveredAtUtc);
                Assert.AreEqual(1, pending.Attempts);
                pending.NextAttemptAtUtc = DateTimeOffset.UtcNow.AddSeconds(-1);
                await db.SaveChangesAsync();
            }
            var working = new Destination(fail: false);
            Assert.IsTrue(await new AuditDelivery(open, working).DeliverOneAsync());
            using (var db = open())
            {
                Assert.IsNotNull((await db.AuditOutbox.SingleAsync()).DeliveredAtUtc);
                Assert.AreEqual(auditId, working.Received.Single());
                Assert.AreEqual(1, await db.AuditRecords.CountAsync());
            }
        });
    }

    [TestMethod]
    public async Task Outer_rollback_removes_business_change_audit_and_delivery_intent()
    {
        await WithDatabase(async open =>
        {
            var id = Guid.NewGuid();
            using (var db = open())
            {
                await using var transaction = await db.Database.BeginTransactionAsync();
                db.Roles.Add(new IdentityRole<Guid> { Id = id, Name = "RollbackTest", NormalizedName = "ROLLBACKTEST" });
                await db.SaveChangesAsync();
                Assert.AreEqual(1, await db.AuditRecords.CountAsync());
                await transaction.RollbackAsync();
            }
            using var check = open();
            Assert.IsFalse(await check.Roles.AnyAsync(role => role.Id == id));
            Assert.AreEqual(0, await check.AuditRecords.CountAsync());
            Assert.AreEqual(0, await check.AuditOutbox.CountAsync());
        });
    }

    [TestMethod]
    public async Task Audit_insert_failure_prevents_business_commit()
    {
        await WithDatabase(async open =>
        {
            using var db = open();
            await db.Database.ExecuteSqlRawAsync("CREATE TRIGGER RejectAudit ON AuditRecords INSTEAD OF INSERT AS THROW 51001, 'Test audit failure', 1;");
            var id = Guid.NewGuid();
            db.Roles.Add(new IdentityRole<Guid> { Id = id, Name = "RejectedTest", NormalizedName = "REJECTEDTEST" });
            await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
            using var check = open();
            Assert.IsFalse(await check.Roles.AnyAsync(role => role.Id == id));
            Assert.AreEqual(0, await check.AuditOutbox.CountAsync());
        });
    }

    [TestMethod]
    public async Task Local_acknowledgement_does_not_acknowledge_cloud_and_workers_do_not_change_actor()
    {
        await WithDatabase(async open =>
        {
            var actor = Guid.NewGuid();
            using (var operation = Telemetry.BeginOperation(actor))
            using (var db = open())
            {
                db.RequireCloudAuditDelivery = true;
                db.Roles.Add(new IdentityRole<Guid> { Id = Guid.NewGuid(), Name = "secret-canary", NormalizedName = "SECRET-CANARY" });
                await db.SaveChangesAsync();
            }
            var destination = new Destination(fail: false);
            await new AuditDelivery(open, destination).DeliverOneAsync();
            using var check = open();
            Assert.AreEqual(actor, (await check.AuditRecords.SingleAsync()).ActorId);
            Assert.DoesNotContain("canary", (await check.AuditRecords.SingleAsync()).PayloadJson);
            Assert.IsNotNull((await check.AuditOutbox.SingleAsync(row => row.Destination == "local")).DeliveredAtUtc);
            Assert.IsNull((await check.AuditOutbox.SingleAsync(row => row.Destination == "cloud")).DeliveredAtUtc);
        });
    }

    [TestMethod]
    public async Task Audit_history_cannot_be_overwritten_through_the_context()
    {
        await WithDatabase(async open =>
        {
            using var db = open();
            db.Roles.Add(new IdentityRole<Guid> { Id = Guid.NewGuid(), Name = "AppendOnly", NormalizedName = "APPENDONLY" });
            await db.SaveChangesAsync();
            var record = await db.AuditRecords.SingleAsync();
            record.PayloadJson = "{}";
            await Assert.ThrowsAsync<InvalidOperationException>(() => db.SaveChangesAsync());
        });
    }

    [TestMethod]
    public async Task Moderation_transition_keeps_public_reason_and_identifiers_but_excludes_private_content()
    {
        await WithDatabase(async open =>
        {
            var actor = Guid.NewGuid();
            var node = Guid.NewGuid();
            var now = DateTimeOffset.UtcNow;
            using var operation = Telemetry.BeginOperation(actor);
            using var db = open();
            db.ParticipantRows.Add(new ParticipantRow { Id = actor, DisplayName = "Audit reporter", IsActive = true, CreatedAt = now, UpdatedAt = now });
            db.NodeRows.Add(new NodeRow { Id = node, Title = "private-canary-title", Status = "Active", CreatedAt = now, UpdatedAt = now });
            var report = new ModerationCaseRow
            {
                Id = Guid.NewGuid(), NodeId = node, ReporterId = actor, Reason = "Spam",
                Explanation = "private-canary-report", ReportedTitle = "private-canary-title", CreatedAt = now
            };
            db.ModerationCaseRows.Add(report);
            await db.SaveChangesAsync();
            report.Status = 2;
            report.PublicReason = 2;
            report.ReviewerId = actor;
            report.DecidedAt = now;
            report.DecisionReason = "private-canary-rationale";
            await db.SaveChangesAsync();
            var records = await db.AuditRecords.Where(row => row.EventName == "moderation.changed").ToListAsync();
            Assert.HasCount(2, records);
            var hidden = records.Single(row => row.PayloadJson.Contains("hidden", StringComparison.Ordinal));
            Assert.AreEqual(actor, hidden.ActorId);
            using var json = System.Text.Json.JsonDocument.Parse(hidden.PayloadJson);
            Assert.AreEqual("harassment", json.RootElement.GetProperty("moderation_reason").GetString());
            Assert.AreEqual(node, json.RootElement.GetProperty("related_target_id").GetGuid());
            foreach (var record in records) Assert.DoesNotContain("canary", record.PayloadJson);
        });
    }

    [TestMethod]
    public async Task Competing_workers_share_a_lease_and_expired_leases_recover()
    {
        await WithDatabase(async open =>
        {
            using (var db = open())
            {
                db.Roles.Add(new IdentityRole<Guid> { Id = Guid.NewGuid(), Name = "LeaseTest", NormalizedName = "LEASETEST" });
                await db.SaveChangesAsync();
                var pending = await db.AuditOutbox.SingleAsync();
                pending.LeaseOwner = Guid.NewGuid();
                pending.LeaseUntilUtc = DateTimeOffset.UtcNow.AddMinutes(-1);
                await db.SaveChangesAsync();
            }
            var destination = new Destination(fail: false);
            await Task.WhenAll(new AuditDelivery(open, destination).DeliverOneAsync(), new AuditDelivery(open, destination).DeliverOneAsync());
            Assert.HasCount(1, destination.Received);
            using var check = open();
            Assert.IsNotNull((await check.AuditOutbox.SingleAsync()).DeliveredAtUtc);
        });
    }

    private static async Task WithDatabase(Func<Func<AtlasDataContext>, Task> test)
    {
        var config = new ConfigurationBuilder().AddUserSecrets<SqlAuditTests>(optional: true).AddEnvironmentVariables().Build();
        var server = config["ATLAS_SQL_TEST_CONNECTION_STRING"];
        if (string.IsNullOrWhiteSpace(server)) Assert.Inconclusive("Set ATLAS_SQL_TEST_CONNECTION_STRING to run SQL audit tests.");
        var connection = new SqlConnectionStringBuilder(server) { InitialCatalog = "AtlasAuditTests_" + Guid.NewGuid().ToString("N") }.ConnectionString;
        var options = new DbContextOptionsBuilder<AtlasDataContext>().UseSqlServer(connection).Options;
        AtlasDataContext Open() => new(options);
        try
        {
            using var db = Open();
            await db.Database.EnsureCreatedAsync();
            await test(Open);
        }
        finally
        {
            using var db = Open();
            await db.Database.EnsureDeletedAsync();
        }
    }

    private sealed class Destination(bool fail) : IAuditDestination
    {
        public string Name => "local";
        public System.Collections.Concurrent.ConcurrentBag<Guid> Received { get; } = [];
        public Task DeliverAsync(AuditRecordRow record, CancellationToken cancellationToken)
        {
            if (fail) throw new IOException("secret-canary");
            Received.Add(record.Id);
            return Task.CompletedTask;
        }
    }
}
