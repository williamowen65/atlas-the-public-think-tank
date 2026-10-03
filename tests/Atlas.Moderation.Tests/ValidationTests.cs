namespace Atlas.Moderation.Tests;

[TestClass]
public sealed class ValidationTests
{
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
