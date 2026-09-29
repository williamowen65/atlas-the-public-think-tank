using Atlas.Moderation;
using Atlas.ConsoleApp.Storage;
namespace Atlas.Persistence.Tests;
[TestClass]
public sealed class SqlModerationRepositoryTests
{
    private readonly Guid _moderator = Guid.NewGuid();
    private readonly Guid _reporter = Guid.NewGuid();
    private readonly Guid _node = Guid.NewGuid();
    [TestMethod]
    public void Final_decision_survives_new_repository_instance()
    {
        using var database = SqlTestDatabase.Create();

            var service = new ModerationService(new SqlModerationCaseRepository(database.Open), new Moderator(_moderator));
            var reported = service.ReportNode(_node, _reporter, "A Node", "spam", "evidence", DateTimeOffset.UtcNow);
            service.DecideNode(_moderator, _node, ModerationDecision.HideNode,
                "Reviewed", DateTimeOffset.UtcNow, PublicModerationReason.Harassment);
            var loaded = new SqlModerationCaseRepository(database.Open).GetById(reported.Id);
            Assert.IsNotNull(loaded);
            Assert.AreEqual(ModerationStatus.Actioned, loaded.Status);
            Assert.AreEqual("Reviewed", loaded.DecisionReason);
            Assert.AreEqual(_moderator, loaded.ReviewerId);
            Assert.AreEqual(PublicModerationReason.Harassment, loaded.PublicReason);
            }
    private sealed class Moderator(Guid id) : IModeratorAuthorization
    { public bool IsAtlasModerator(Guid participantId) => participantId == id; }
}
