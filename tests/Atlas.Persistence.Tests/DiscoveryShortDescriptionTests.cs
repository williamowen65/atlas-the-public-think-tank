using Atlas.Application.Storage;
using Atlas.Discovery;
using Atlas.Persistence.Discovery;
using Atlas.Graph.Nodes;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Persistence.Tests;

[TestClass]
public sealed class DiscoveryShortDescriptionTests
{
    [TestMethod]
    public void ShortDescriptionRoundTripsSearchesAndInvalidatesCachedCards()
    {
        using var fixture = SqlTestDatabase.Create(useCurrentModel: true);
        var at = DateTimeOffset.UtcNow;
        var candidate = new DiscoveryCandidate(Guid.NewGuid(), "Node title", "Long document content", Guid.NewGuid(), Guid.NewGuid(), false, at, at, 0, null, [], []);
        DiscoverySqlFixture.Seed(fixture, candidate);
        var repository = new SqlNodeRepository(fixture.Open);
        var node = repository.GetById(new NodeId(candidate.NodeId))!;
        node.ChangeShortDescription("Distinct concise introduction", node.AuthorId.Value, at.AddMinutes(1));
        repository.SaveMetadata(node);
        Assert.AreEqual(node.ShortDescription, repository.GetById(node.Id)!.ShortDescription);
        using var cache = new DiscoveryCache();
        var query = new SqlDiscoveryReadQuery(fixture.OpenForDiscovery, cache);
        var card = query.DiscoverPage(new(SearchText: "Distinct concise")).Items.Single().Card!;
        Assert.AreEqual("Distinct concise introduction", card.ShortDescription);
        Assert.AreEqual("Long document content", card.Description);
        node.ChangeShortDescription("Updated introduction", node.AuthorId.Value, at.AddMinutes(2));
        repository.SaveMetadata(node);
        Assert.AreEqual("Updated introduction", query.DiscoverPage(new()).Items.Single().Card!.ShortDescription);
        using var db = fixture.Open();
        db.ModerationCaseRows.Add(new ModerationCaseRow { Id = Guid.NewGuid(), NodeId = candidate.NodeId, ReporterId = candidate.AuthorParticipantId, ReportedTitle = candidate.Title, Reason = "Spam", Status = 2, PublicReason = 1, CreatedAt = at });
        db.SaveChanges();
        var hidden = query.DiscoverAuthoredPage(new(), [candidate.NodeId]).Items.Single().Card!;
        Assert.IsTrue(hidden.IsHidden);
        Assert.AreNotEqual("Updated introduction", hidden.ShortDescription);
        StringAssert.Contains(hidden.ShortDescription, "Hidden by moderator");
    }
}
