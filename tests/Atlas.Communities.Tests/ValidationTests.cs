using Atlas.Communities.Communities;

namespace Atlas.Communities.Tests;

[TestClass]
public sealed class ValidationTests
{
    [TestMethod]
    public void DefaultsAndRegressingChangesAreRejectedWithoutMutation()
    {
        var now = DateTimeOffset.UtcNow;
        var actor = Guid.NewGuid();
        Assert.Throws<ArgumentException>(() => Community.Reconstitute(default, "Name", "", actor, CommunityStatus.Active, now, now));
        Assert.Throws<ArgumentException>(() => new Atlas.Communities.Memberships.CommunityMembership(default, actor, now));
        var community = new Community("Name", "Original", actor, now);
        Assert.Throws<ArgumentException>(() => community.Rename(actor, "Changed", now.AddMinutes(-1)));
        Assert.Throws<ArgumentException>(() => community.Archive(actor, now.AddMinutes(-1)));
        Assert.AreEqual("Name", community.Name);
        Assert.AreEqual(CommunityStatus.Active, community.Status);
        Assert.AreEqual(now, community.UpdatedAt);
        community.Archive(actor, now);
        community.Archive(actor, now.AddMinutes(-1)); // Intentional repeated-operation no-op.
    }

    [TestMethod]
    public void RejoinCannotEraseLaterLeaveHistory()
    {
        var now = DateTimeOffset.UtcNow;
        var item = new Atlas.Communities.Memberships.CommunityMembership(CommunityId.New(), Guid.NewGuid(), now);
        item.Leave(now.AddHours(1));
        Assert.Throws<ArgumentException>(() => item.Join(now.AddMinutes(1)));
        Assert.AreEqual(now, item.JoinedAt);
        Assert.AreEqual(now.AddHours(1), item.LeftAt);
        item.Join(now.AddHours(2));
        Assert.IsTrue(item.IsActive);
    }

    [TestMethod]
    public void ReconstitutionRejectsUndefinedStatus()
    {
        var now = DateTimeOffset.UtcNow;
        Assert.Throws<ArgumentOutOfRangeException>(() => Community.Reconstitute(
            CommunityId.New(), "Community", "", Guid.NewGuid(), (CommunityStatus)999, now, now));
    }
}
