namespace Atlas.Moderation.Tests;

[TestClass]
public sealed class ValidationTests
{
    [TestMethod]
    public void RestoredFieldsRequireCompleteConsistentHistory()
    {
        var now = DateTimeOffset.UtcNow;
        var actor = Guid.NewGuid();
        Assert.Throws<ArgumentException>(() => ModerationCase.Reconstitute(Guid.NewGuid(), Guid.NewGuid(), actor,
            "spam", null, "Title", now, ModerationStatus.Actioned, actor, "Reviewed", now,
            restoredBy: actor, restorationReason: "Restored"));
        Assert.Throws<ArgumentException>(() => ModerationCase.Reconstitute(Guid.NewGuid(), Guid.NewGuid(), actor,
            "spam", null, "Title", now, ModerationStatus.Dismissed, actor, "Reviewed", now,
            publicReason: PublicModerationReason.Spam));
        Assert.Throws<ArgumentException>(() => ModerationCase.Reconstitute(Guid.NewGuid(), Guid.NewGuid(), actor,
            "spam", null, "Title", now, ModerationStatus.Actioned, actor, "Reviewed", now,
            reviewRequestedAt: now.AddMinutes(1), visibilityRestoredAt: now.AddMinutes(2),
            restoredBy: actor, restorationReason: new string('x', 2001)));
    }

    [TestMethod]
    public void DecidedCaseRequiresDecisionTimeAndBoundedRationale()
    {
        var created = DateTimeOffset.UtcNow;
        foreach (var status in new[] { ModerationStatus.Dismissed, ModerationStatus.Actioned })
        {
            Assert.Throws<ArgumentException>(() => ModerationCase.Reconstitute(Guid.NewGuid(),
                Guid.NewGuid(), Guid.NewGuid(), "spam", null, "Title", created,
                status, Guid.NewGuid(), "Reviewed", null));
            Assert.Throws<ArgumentException>(() => ModerationCase.Reconstitute(Guid.NewGuid(),
                Guid.NewGuid(), Guid.NewGuid(), "spam", null, "Title", created,
                status, Guid.NewGuid(), new string('x', 2001), created.AddMinutes(1)));
            var valid = ModerationCase.Reconstitute(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                "spam", null, "Title", created, status, Guid.NewGuid(), "Reviewed", created.AddMinutes(1));
            Assert.AreEqual(status, valid.Status);
        }
    }
}
