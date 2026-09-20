using Atlas.Communities.Communities;
using Atlas.Communities.Memberships;
using Atlas.Communities.Nodes;
using Atlas.ConsoleApp.Storage;

namespace Atlas.Console.Tests;

[TestClass]
public sealed class CommunityRepositoryTests
{
    [TestMethod]
    public void CommunityRecordsRoundTripThroughSeparateJsonRepositories()
    {
        var directory = Path.Combine(Path.GetTempPath(), "AtlasCommunityTests", Guid.NewGuid().ToString());
        try
        {
            var communities = new JsonCommunityRepository(Path.Combine(directory, "communities.json"));
            var memberships = new JsonCommunityMembershipRepository(Path.Combine(directory, "memberships.json"));
            var nodes = new JsonCommunityNodeRepository(Path.Combine(directory, "nodes.json"));
            var owner = Guid.NewGuid();
            var nodeId = Guid.NewGuid();
            var created = new DateTimeOffset(2026, 9, 20, 12, 0, 0, TimeSpan.Zero);
            var community = new Community("Puget Sound", "Regional ideas", owner, created);

            communities.Save(community);
            memberships.Save(new CommunityMembership(community.Id, owner, created));
            nodes.Add(new CommunityNodeAssociation(community.Id, nodeId, owner, created));

            var reloaded = communities.GetById(community.Id);
            Assert.IsNotNull(reloaded);
            Assert.AreEqual("Puget Sound", reloaded.Name);
            Assert.IsTrue(memberships.Get(community.Id, owner)!.IsActive);
            Assert.AreEqual(nodeId, nodes.GetByCommunity(community.Id).Single().NodeId);
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        }
    }
}
