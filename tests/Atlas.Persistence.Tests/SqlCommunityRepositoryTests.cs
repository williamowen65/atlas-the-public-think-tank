using Atlas.Communities.Communities;
using Atlas.Communities.Memberships;
using Atlas.Communities.Nodes;
using Atlas.ConsoleApp.Storage;

namespace Atlas.Persistence.Tests;
[TestClass]
public sealed class SqlCommunityRepositoryTests
{
    [TestMethod]
    public void Community_rows_survive_new_repository_instances()
    {
        using var database = SqlTestDatabase.Create();

            var communities = new SqlCommunityRepository(database.Open);
            var memberships = new SqlCommunityMembershipRepository(database.Open);
            var nodes = new SqlCommunityNodeRepository(database.Open);
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
}
