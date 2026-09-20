using Atlas.Communities.Communities;
using Atlas.Communities.Memberships;
using Atlas.Communities.Nodes;

namespace Atlas.Communities.Tests;

[TestClass]
public sealed class CommunityTests
{
    [TestMethod]
    public void Create_AssignsOwnerAndActiveStatus()
    {
        var owner = Guid.NewGuid();
        var created = new DateTimeOffset(2026, 9, 20, 12, 0, 0, TimeSpan.Zero);
        var community = new Community("Puget Sound", "Regional ideas", owner, created);
        Assert.AreEqual(owner, community.OwnerParticipantId);
        Assert.AreEqual(CommunityStatus.Active, community.Status);
        Assert.AreEqual(created, community.CreatedAt);
    }

    [TestMethod]
    public void OwnerCanEditAndArchiveCommunity()
    {
        var owner = Guid.NewGuid();
        var created = DateTimeOffset.UtcNow;
        var community = new Community("Nature", "Original", owner, created);
        community.Rename(owner, "Nature and Wildlife", created.AddMinutes(1));
        community.ChangeDescription(owner, "Updated", created.AddMinutes(2));
        community.Archive(owner, created.AddMinutes(3));
        Assert.AreEqual("Nature and Wildlife", community.Name);
        Assert.AreEqual("Updated", community.Description);
        Assert.AreEqual(CommunityStatus.Archived, community.Status);
    }

    [TestMethod]
    public void NonOwnerCannotEditCommunity()
    {
        var community = new Community("Teachers", "Education", Guid.NewGuid(), DateTimeOffset.UtcNow);
        Assert.ThrowsException<InvalidOperationException>(() => community.Rename(Guid.NewGuid(), "Other", DateTimeOffset.UtcNow));
    }

    [TestMethod]
    public void CreateServiceRejectsDuplicateNamesAndJoinsOwner()
    {
        var communities = new MemoryCommunities();
        var memberships = new MemoryMemberships();
        var service = new CommunityService(communities, memberships, new MemoryNodes());
        var owner = Guid.NewGuid();
        var created = service.Create("Climate Adaptation", "", owner, DateTimeOffset.UtcNow);
        Assert.IsTrue(memberships.Get(created.Id, owner)!.IsActive);
        Assert.ThrowsException<InvalidOperationException>(() => service.Create(" climate adaptation ", "", Guid.NewGuid(), DateTimeOffset.UtcNow));
    }

    [TestMethod]
    public void NodeCanBelongToMultipleCommunitiesWithoutDuplication()
    {
        var communities = new MemoryCommunities();
        var nodes = new MemoryNodes();
        var service = new CommunityService(communities, new MemoryMemberships(), nodes);
        var participant = Guid.NewGuid();
        var nodeId = Guid.NewGuid();
        var first = service.Create("Future Sound", "", participant, DateTimeOffset.UtcNow);
        var second = service.Create("Climate Adaptation", "", participant, DateTimeOffset.UtcNow);
        service.AssociateNode(first, nodeId, participant, DateTimeOffset.UtcNow);
        service.AssociateNode(second, nodeId, participant, DateTimeOffset.UtcNow);
        service.AssociateNode(first, nodeId, participant, DateTimeOffset.UtcNow);
        Assert.AreEqual(2, nodes.GetByNode(nodeId).Count);
    }

    [TestMethod]
    public void ParticipantCanLeaveAndRejoinButOwnerCannotLeave()
    {
        var communities = new MemoryCommunities();
        var memberships = new MemoryMemberships();
        var service = new CommunityService(communities, memberships, new MemoryNodes());
        var owner = Guid.NewGuid();
        var member = Guid.NewGuid();
        var community = service.Create("Civic Life", "", owner, DateTimeOffset.UtcNow);
        service.Join(community, member, DateTimeOffset.UtcNow);
        service.Leave(community, member, DateTimeOffset.UtcNow.AddMinutes(1));
        Assert.IsFalse(memberships.Get(community.Id, member)!.IsActive);
        service.Join(community, member, DateTimeOffset.UtcNow.AddMinutes(2));
        Assert.IsTrue(memberships.Get(community.Id, member)!.IsActive);
        Assert.ThrowsException<InvalidOperationException>(() => service.Leave(community, owner, DateTimeOffset.UtcNow));
    }

    private sealed class MemoryCommunities : ICommunityRepository
    {
        private readonly List<Community> _items = [];
        public IReadOnlyCollection<Community> GetAll() => _items;
        public Community? GetById(CommunityId id) => _items.SingleOrDefault(x => x.Id == id);
        public void Save(Community community) { _items.RemoveAll(x => x.Id == community.Id); _items.Add(community); }
    }

    private sealed class MemoryMemberships : ICommunityMembershipRepository
    {
        private readonly List<CommunityMembership> _items = [];
        public IReadOnlyCollection<CommunityMembership> GetByCommunity(CommunityId id) => _items.Where(x => x.CommunityId == id).ToList();
        public IReadOnlyCollection<CommunityMembership> GetByParticipant(Guid id) => _items.Where(x => x.ParticipantId == id).ToList();
        public CommunityMembership? Get(CommunityId id, Guid participantId) => _items.SingleOrDefault(x => x.CommunityId == id && x.ParticipantId == participantId);
        public void Save(CommunityMembership item) { _items.RemoveAll(x => x.CommunityId == item.CommunityId && x.ParticipantId == item.ParticipantId); _items.Add(item); }
    }

    private sealed class MemoryNodes : ICommunityNodeRepository
    {
        private readonly List<CommunityNodeAssociation> _items = [];
        public IReadOnlyCollection<CommunityNodeAssociation> GetByCommunity(CommunityId id) => _items.Where(x => x.CommunityId == id).ToList();
        public IReadOnlyCollection<CommunityNodeAssociation> GetByNode(Guid id) => _items.Where(x => x.NodeId == id).ToList();
        public void Add(CommunityNodeAssociation item) { if (!_items.Any(x => x.CommunityId == item.CommunityId && x.NodeId == item.NodeId)) _items.Add(item); }
        public void Remove(CommunityId id, Guid nodeId) => _items.RemoveAll(x => x.CommunityId == id && x.NodeId == nodeId);
    }
}
