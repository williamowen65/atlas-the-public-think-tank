using Atlas.Application.Storage;
using Atlas.Communities.Communities;
using Atlas.Contracts.Operations;

namespace Atlas.Persistence.Tests;

/// <summary>Fresh-model quota evidence; migration/upgrade validation remains developer-owned.</summary>
[TestClass]
public sealed class SqlCommunityCreationQuotaTests
{
    [TestMethod]
    public void CreationIsDurableParticipantScopedAndExpiresAfter24Hours()
    {
        using var database = SqlTestDatabase.Create(useCurrentModel: true);
        var at = DateTimeOffset.Parse("2026-10-19T12:00:00Z");
        using var clock = AtlasTime.Use(new Clock(at));
        var owner = database.AddParticipant();
        var other = database.AddParticipant();
        var repo = new SqlCommunityRepository(database.Open);
        var service = Service(repo, database);
        var first = service.Create("First", "Description", owner, at.AddYears(-1));
        service.Archive(first, owner, at); // Archiving is allowed while exhausted; no refund.
        service.ChangeDescription(first, owner, "Edited", at.AddSeconds(1));
        var reopened = new SqlCommunityRepository(database.Open);
        var rejected = Assert.ThrowsExactly<CommunityCreationQuotaExceededException>(() =>
            Service(reopened, database).Create("Blocked", "Description", owner, at.AddYears(-1)));
        Assert.AreEqual("Community creation limit reached: 1 per 24 hours. " +
            "Try again at 2026-10-20 12:00:00 UTC.", rejected.Message);
        Console.WriteLine(rejected.Message);
        Assert.AreEqual(0, reopened.GetCreationAllowance(owner).Remaining);
        service.Create("Other owner", "Description", other, at);
        using (var check = database.Open())
        {
            Assert.AreEqual(2, check.CommunityRows.Count());
            Assert.AreEqual(2, check.CommunityMembershipRows.Count());
            Assert.AreEqual(2, check.CommunityCreationQuotaRows.Count());
            Assert.AreEqual(at, check.CommunityCreationQuotaRows.Single(row => row.ParticipantId == owner).OccurredAt);
        }
        using var later = AtlasTime.Use(new Clock(at.AddHours(24)));
        service.Create("Next day", "Description", owner, at.AddHours(24));
        Assert.AreEqual(0, repo.GetCreationAllowance(owner).Remaining);
    }

    [TestMethod]
    public void InvalidCreationAndRollbackLeaveNoConsumptionOrMembership()
    {
        using var database = SqlTestDatabase.Create(useCurrentModel: true);
        var owner = database.AddParticipant();
        var repo = new SqlCommunityRepository(database.Open);
        var service = Service(repo, database);
        Assert.Throws<ArgumentException>(() => service.Create("", "Description", owner, AtlasTime.UtcNow));
        Assert.ThrowsExactly<InvalidOperationException>(() => OperationBoundary.Execute(repo, () =>
        {
            service.Create("Rolled back", "Description", owner, AtlasTime.UtcNow);
            throw new InvalidOperationException("Injected workflow failure");
        }));
        using (var check = database.Open())
        {
            Assert.AreEqual(0, check.CommunityRows.Count());
            Assert.AreEqual(0, check.CommunityMembershipRows.Count());
            Assert.AreEqual(0, check.CommunityCreationQuotaRows.Count());
        }
        // Direct repository inserts enforce the same budget; supplied timestamps cannot bypass it.
        repo.Save(new Community("Direct", "Description", owner, AtlasTime.UtcNow.AddDays(-7)));
        Assert.ThrowsExactly<CommunityCreationQuotaExceededException>(() =>
            repo.Save(new Community("Direct blocked", "Description", owner, AtlasTime.UtcNow.AddDays(-7))));
    }

    [TestMethod]
    public async Task CompetingCreationsCannotBothTakeTheDailySlot()
    {
        using var database = SqlTestDatabase.Create(useCurrentModel: true);
        var owner = database.AddParticipant();
        using var start = new Barrier(2);
        async Task<bool> Attempt(string name) => await Task.Run(() =>
        {
            start.SignalAndWait();
            try
            {
                var repo = new SqlCommunityRepository(database.Open);
                Service(repo, database).Create(name, "Description", owner, AtlasTime.UtcNow);
                return true;
            }
            catch (CommunityCreationQuotaExceededException) { return false; }
        });
        var results = await Task.WhenAll(Attempt("First"), Attempt("Second"));
        Assert.AreEqual(1, results.Count(success => success));
        using var check = database.Open();
        Assert.AreEqual(1, check.CommunityRows.Count());
        Assert.AreEqual(1, check.CommunityMembershipRows.Count());
        Assert.AreEqual(1, check.CommunityCreationQuotaRows.Count());
    }

    [TestMethod]
    public void TotalOwnershipCapIncludesArchivedCommunities()
    {
        using var database = SqlTestDatabase.Create(useCurrentModel: true);
        var owner = database.AddParticipant();
        var other = database.AddParticipant();
        var repo = new SqlCommunityRepository(database.Open);
        var service = Service(repo, database);
        var start = DateTimeOffset.Parse("2026-10-01T12:00:00Z");
        for (var i = 0; i < SqlCommunityRepository.MaximumOwnedCommunities; i++)
        {
            using var clock = AtlasTime.Use(new Clock(start.AddDays(i)));
            var created = service.Create($"Owned {i}", "Description", owner, start.AddDays(i));
            if (i == 0) service.Archive(created, owner, start.AddDays(i));
        }
        using (var clock = AtlasTime.Use(new Clock(start.AddDays(6))))
        {
            Assert.ThrowsExactly<InvalidOperationException>(() =>
                service.Create("Sixth", "Description", owner, start.AddDays(6)));
            service.Create("Another participant", "Description", other, start.AddDays(6));
        }
        using var check = database.Open();
        Assert.AreEqual(SqlCommunityRepository.MaximumOwnedCommunities, check.CommunityRows.Count(row => row.OwnerParticipantId == owner));
    }

    private static CommunityService Service(SqlCommunityRepository repo, SqlTestDatabase database) =>
        new(repo, new SqlCommunityMembershipRepository(database.Open), new SqlCommunityNodeRepository(database.Open));
    private sealed class Clock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
