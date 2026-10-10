using Atlas.Communities.Communities;
using Atlas.Communities.Memberships;
using Atlas.ConsoleApp;
using Atlas.ConsoleApp.Storage;
using Atlas.Contracts.Operations;
using Atlas.Graph.Nodes;
using Atlas.Graph.Nodes.NodeTypes;
using Atlas.Graph.Reactions;
using Atlas.Moderation;
using Atlas.Voting;
using Atlas.Voting.Target;
using VotingParticipantId = Atlas.Voting.Votes.ParticipantId;

namespace Atlas.Persistence.Tests;

[TestClass]
public sealed class SqlOperationValidationTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 10, 12, 0, 0, TimeSpan.Zero);

    [TestMethod]
    public void FailedNodeSaveRollsBackBlocksAndDocument()
    {
        using var database = SqlTestDatabase.Create();
        var author = database.AddParticipant();
        var type = NodeTypeDefinition.CreateSystemDefined("Idea", "Ideas", Now);
        new SqlNodeTypeRepository(database.Open).Save(type);
        var nodes = new FailingNodes(new SqlNodeRepository(database.Open));
        var service = new NodeCreationService(nodes, new SqlDocumentRepository(database.Open));
        Assert.Throws<InjectedFailure>(() => service.Create("Title", "Description", type.Id, author, []));
        using var check = database.Open();
        Assert.AreEqual(0, check.NodeRows.Count());
        Assert.AreEqual(0, check.DocumentRows.Count());
        Assert.AreEqual(0, check.BlockRows.Count());
    }

    [TestMethod]
    public void FailedOwnerMembershipRollsBackCommunity()
    {
        using var database = SqlTestDatabase.Create();
        var communities = new SqlCommunityRepository(database.Open);
        var service = new CommunityService(communities, new FailingMemberships(), new SqlCommunityNodeRepository(database.Open));
        Assert.Throws<InjectedFailure>(() => service.Create("Community", "Description", database.AddParticipant(), Now));
        Assert.AreEqual(0, communities.GetAll().Count);
        using var check = database.Open();
        Assert.AreEqual(0, check.CommunityMembershipRows.Count());
    }

    [TestMethod]
    public void FailedSecondModerationSaveRollsBackWholeGroup()
    {
        using var database = SqlTestDatabase.Create();
        var moderator = database.AddParticipant(); var reporter = database.AddParticipant(); var node = database.AddNode();
        var cases = new SqlModerationCaseRepository(database.Open);
        var original = new ModerationService(cases, new Moderator(moderator));
        original.ReportNode(node, reporter, "Title", "spam", null, Now);
        original.ReportNode(node, reporter, "Title", "spam", null, Now);
        var service = new ModerationService(new FailingCases(cases), new Moderator(moderator));
        Assert.Throws<InjectedFailure>(() => service.DecideNode(moderator, node, ModerationDecision.HideNode, "Reviewed", Now.AddMinutes(1)));
        Assert.IsTrue(cases.GetAll().All(item => item.Status == ModerationStatus.Submitted && item.ReviewerId is null));
        Assert.AreEqual(2, cases.GetAll().Count);
    }

    [TestMethod]
    public void FailedSupersessionSaveRollsBackReplacement()
    {
        using var database = SqlTestDatabase.Create();
        var actor = database.AddParticipant();
        var type = NodeTypeDefinition.CreateSystemDefined("Idea", "Ideas", Now);
        new SqlNodeTypeRepository(database.Open).Save(type);
        var nodes = new SqlNodeRepository(database.Open);
        var node = new NodeCreationService(nodes, new SqlDocumentRepository(database.Open)).Create("Title", "Description", type.Id, actor, []);
        var definitions = new SqlReactionDefinitionRepository(database.Open);
        definitions.Save(ReactionDefinition.Create("Helpful", "👍", "Helpful", actor, Now));
        definitions.Save(ReactionDefinition.Create("Interesting", "💡", "Interesting", actor, Now));
        var reactions = new SqlNodeReactionRepository(database.Open);
        var first = new NodeReactionApplicationService(definitions, reactions).Apply(node, "Helpful", actor, true, Now);
        var service = new NodeReactionApplicationService(definitions, new FailingReactions(reactions));
        Assert.Throws<InjectedFailure>(() => service.Replace(node, first.Id, "Interesting", actor, true, false, Now.AddMinutes(1)));
        Assert.AreEqual(1, reactions.GetAll().Count);
        Assert.AreEqual(NodeReactionLifecycleState.Active, reactions.GetById(first.Id)!.LifecycleState);
        Assert.AreEqual(1, reactions.GetById(first.Id)!.AuditHistory.Count);
    }

    [TestMethod]
    public async Task CompetingCommunityNamesHaveOneCompleteWinner()
    {
        using var database = SqlTestDatabase.Create();
        var owner = database.AddParticipant();
        using var start = new ManualResetEventSlim();
        Task<bool> Create() => Task.Run(() =>
        {
            start.Wait();
            var service = new CommunityService(new SqlCommunityRepository(database.Open),
                new SqlCommunityMembershipRepository(database.Open), new SqlCommunityNodeRepository(database.Open));
            try { service.Create("Same name", "Description", owner, Now); return true; }
            catch (InvalidOperationException) { return false; }
        });
        var one = Create(); var two = Create(); start.Set();
        var results = await Task.WhenAll(one, two);
        Assert.AreEqual(1, results.Count(result => result));
        using var check = database.Open();
        Assert.AreEqual(1, check.CommunityRows.Count());
        Assert.AreEqual(1, check.CommunityMembershipRows.Count());
    }

    [TestMethod]
    public async Task CompetingModeratorsCannotSplitGroupDecisions()
    {
        using var database = SqlTestDatabase.Create();
        var actor = database.AddParticipant(); var reporter = database.AddParticipant(); var node = database.AddNode();
        var cases = new SqlModerationCaseRepository(database.Open);
        var service = new ModerationService(cases, new Moderator(actor));
        for (var i = 0; i < 3; i++) service.ReportNode(node, reporter, "Title", "spam", null, Now);
        using var start = new ManualResetEventSlim();
        Task<bool> Decide(ModerationDecision decision) => Task.Run(() =>
        {
            start.Wait();
            var operation = new ModerationService(new SqlModerationCaseRepository(database.Open), new Moderator(actor));
            try { operation.DecideNode(actor, node, decision, decision.ToString(), Now.AddMinutes(1)); return true; }
            catch (InvalidOperationException) { return false; }
        });
        var one = Decide(ModerationDecision.HideNode); var two = Decide(ModerationDecision.Dismiss); start.Set();
        Assert.AreEqual(1, (await Task.WhenAll(one, two)).Count(result => result));
        var reports = cases.GetAll();
        Assert.AreEqual(3, reports.Count);
        Assert.AreEqual(1, reports.Select(item => item.Status).Distinct().Count());
        Assert.AreEqual(1, reports.Select(item => item.DecisionReason).Distinct().Count());
        Assert.IsTrue(reports.All(item => item.Status != ModerationStatus.Submitted));
    }

    [TestMethod]
    public void MissingWrongKindInactiveAndHiddenReferencesAreRejected()
    {
        using var database = SqlTestDatabase.Create();
        var actor = database.AddParticipant(); var node = database.AddNode();
        var votes = new SqlVoteRepository(database.Open);
        Assert.Throws<InvalidOperationException>(() => votes.Save(new Vote(new NodeVoteTarget(Guid.NewGuid()), new VotingParticipantId(actor), 1)));
        Assert.Throws<InvalidOperationException>(() => votes.Save(new Vote(new NodeReactionVoteTarget(node), new VotingParticipantId(actor), 1)));
        using (var db = database.Open()) { db.ParticipantRows.Single(row => row.Id == actor).IsActive = false; db.SaveChanges(); }
        Assert.Throws<InvalidOperationException>(() => votes.Save(new Vote(new NodeVoteTarget(node), new VotingParticipantId(actor), 1)));
        var moderator = database.AddParticipant(); var reporter = database.AddParticipant();
        var moderation = new ModerationService(new SqlModerationCaseRepository(database.Open), new Moderator(moderator));
        moderation.ReportNode(node, reporter, "Title", "spam", null, Now);
        moderation.DecideNode(moderator, node, ModerationDecision.HideNode, "Reviewed", Now.AddMinutes(1));
        Assert.Throws<InvalidOperationException>(() => votes.Save(new Vote(new NodeVoteTarget(node), new VotingParticipantId(reporter), 1)));
        using var check = database.Open(); Assert.AreEqual(0, check.VoteRows.Count());
    }

    [TestMethod]
    public void SqlUniqueConflictHasApplicationErrorAndRollsBack()
    {
        using var database = SqlTestDatabase.Create();
        var actor = database.AddParticipant(); var node = database.AddNode();
        var repository = new SqlVoteRepository(database.Open);
        repository.Save(new Vote(new NodeVoteTarget(node), new VotingParticipantId(actor), 1));
        Assert.Throws<OperationConflictException>(() => repository.Save(new Vote(new NodeVoteTarget(node), new VotingParticipantId(actor), 2)));
        using var check = database.Open(); Assert.AreEqual(1, check.VoteRows.Count());
    }

    [TestMethod]
    public async Task CompetingOppositeParentLinksCannotCreateCycle()
    {
        using var database = SqlTestDatabase.Create();
        using var clock = AtlasTime.Use(new FixedClock());
        var actor = database.AddParticipant();
        var type = NodeTypeDefinition.CreateSystemDefined("Idea", "Ideas", Now);
        new SqlNodeTypeRepository(database.Open).Save(type);
        var nodes = new SqlNodeRepository(database.Open);
        var creation = new NodeCreationService(nodes, new SqlDocumentRepository(database.Open));
        var a = creation.Create("A", "Description", type.Id, actor, []);
        var b = creation.Create("B", "Description", type.Id, actor, []);
        using var start = new ManualResetEventSlim();
        Task<bool> Attach(Node child, NodeId parent) => Task.Run(() =>
        {
            start.Wait();
            var repository = new SqlNodeRepository(database.Open);
            try { new NodeRelationshipService(repository).Attach(child, parent, actor, Now.AddMinutes(1)); return true; }
            catch (InvalidOperationException) { return false; }
        });
        var one = Attach(a, b.Id); var two = Attach(b, a.Id); start.Set();
        Assert.AreEqual(1, (await Task.WhenAll(one, two)).Count(result => result));
        var loadedA = nodes.GetById(a.Id)!; var loadedB = nodes.GetById(b.Id)!;
        Assert.AreEqual(1, loadedA.ParentNodeIds.Count + loadedB.ParentNodeIds.Count);
        Assert.IsFalse(loadedA.ParentNodeIds.Contains(b.Id) && loadedB.ParentNodeIds.Contains(a.Id));
    }

    [TestMethod]
    public void NodeReferenceChecksRejectMissingDocumentTypeAndInactiveAuthor()
    {
        using var database = SqlTestDatabase.Create();
        using var clock = AtlasTime.Use(new FixedClock());
        var actor = database.AddParticipant();
        var type = NodeTypeDefinition.CreateSystemDefined("Idea", "Ideas", Now);
        new SqlNodeTypeRepository(database.Open).Save(type);
        var nodes = new SqlNodeRepository(database.Open);
        var creation = new NodeCreationService(nodes, new SqlDocumentRepository(database.Open));
        Assert.Throws<InvalidOperationException>(() => creation.Create("Title", "Text", NodeTypeId.New(), actor, []));
        Assert.Throws<InvalidOperationException>(() => nodes.Save(new Node(new NodeTitle("Title"), new NodeDescriptionId(Guid.NewGuid()), type.Id, new NodeAuthorId(actor), Now)));
        using (var db = database.Open()) { db.ParticipantRows.Single(row => row.Id == actor).IsActive = false; db.SaveChanges(); }
        Assert.Throws<InvalidOperationException>(() => creation.Create("Title", "Text", type.Id, actor, []));
        using var check = database.Open();
        Assert.AreEqual(0, check.NodeRows.Count()); Assert.AreEqual(0, check.BlockRows.Count()); Assert.AreEqual(0, check.DocumentRows.Count());
    }

    [TestMethod]
    public void CommunityAndCommentReferencePoliciesRejectInvalidProposals()
    {
        using var database = SqlTestDatabase.Create();
        var actor = database.AddParticipant(); var firstNode = database.AddNode(); var otherNode = database.AddNode();
        var communities = new SqlCommunityRepository(database.Open);
        var associations = new SqlCommunityNodeRepository(database.Open);
        var membership = new SqlCommunityMembershipRepository(database.Open);
        var service = new CommunityService(communities, membership, associations);
        Assert.Throws<InvalidOperationException>(() => service.Create("Invalid owner", "", Guid.NewGuid(), Now));
        var community = service.Create("Community", "", actor, Now);
        Assert.Throws<InvalidOperationException>(() => service.AssociateNode(community, Guid.NewGuid(), actor, Now));
        var comments = new SqlCommentRepository(database.Open);
        var parent = new Atlas.Comments.Comments.Comment(new("Node", firstNode), null, actor, "Parent", Now);
        comments.Save(parent);
        Assert.Throws<ArgumentException>(() => comments.Save(new(new("Node", otherNode), parent.Id, actor, "Foreign target", Now.AddMinutes(1))));
        Assert.Throws<ArgumentException>(() => comments.Save(new(new("Unknown", firstNode), null, actor, "Wrong kind", Now)));
        Assert.Throws<ArgumentException>(() => comments.Save(new(new("Node", firstNode), parent.Id, actor, "Earlier reply", Now.AddMinutes(-1))));
        using (var db = database.Open()) { db.ParticipantRows.Single(row => row.Id == actor).IsActive = false; db.SaveChanges(); }
        Assert.Throws<InvalidOperationException>(() => service.Join(community, actor, Now));
        Assert.Throws<InvalidOperationException>(() => service.AssociateNode(community, firstNode, actor, Now));
        using var check = database.Open();
        Assert.AreEqual(1, check.CommentRows.Count()); Assert.AreEqual(0, check.CommunityNodeRows.Count());
        Assert.AreEqual(1, check.CommunityMembershipRows.Count());
    }

    [TestMethod]
    public void FailedGroupedReviewAndRestorationPreservePersistedHistory()
    {
        using var database = SqlTestDatabase.Create();
        var actor = database.AddParticipant(); var reporter = database.AddParticipant(); var node = database.AddNode();
        var cases = new SqlModerationCaseRepository(database.Open);
        var service = new ModerationService(cases, new Moderator(actor));
        for (var index = 0; index < 2; index++) service.ReportNode(node, reporter, "Title", "spam", null, Now);
        service.DecideNode(actor, node, ModerationDecision.HideNode, "Reviewed", Now.AddMinutes(1));
        var failingReview = new ModerationService(new FailingCases(cases), new Moderator(actor));
        Assert.Throws<InjectedFailure>(() => failingReview.RequestNodeReview(node, reporter, reporter, Now.AddMinutes(2), Now.AddMinutes(3)));
        Assert.IsTrue(cases.GetAll().All(item => item.ReviewRequestedAt is null && item.IsHidden));
        service.RequestNodeReview(node, reporter, reporter, Now.AddMinutes(2), Now.AddMinutes(3));
        var failingRestore = new ModerationService(new FailingCases(cases), new Moderator(actor));
        Assert.Throws<InjectedFailure>(() => failingRestore.RestoreNode(actor, node, "Restored", Now.AddMinutes(4)));
        Assert.IsTrue(cases.GetAll().All(item => item.IsHidden && item.VisibilityRestoredAt is null && item.RestoredBy is null));
        service.RestoreNode(actor, node, "Restored", Now.AddMinutes(4));
        Assert.IsTrue(cases.GetAll().All(item => !item.IsHidden && item.VisibilityRestoredAt == Now.AddMinutes(4)));
    }

    [TestMethod]
    public async Task CompetingNewVotesReturnOneConflictAndLeaveOneRecord()
    {
        using var database = SqlTestDatabase.Create();
        var actor = database.AddParticipant(); var node = database.AddNode();
        using var start = new ManualResetEventSlim();
        Task<bool> Save(int value) => Task.Run(() =>
        {
            start.Wait();
            try { new SqlVoteRepository(database.Open).Save(new Vote(new NodeVoteTarget(node), new VotingParticipantId(actor), value)); return true; }
            catch (OperationConflictException) { return false; }
        });
        var one = Save(1); var two = Save(2); start.Set();
        Assert.AreEqual(1, (await Task.WhenAll(one, two)).Count(result => result));
        using var check = database.Open(); Assert.AreEqual(1, check.VoteRows.Count());
    }

    private sealed class FixedClock : TimeProvider { public override DateTimeOffset GetUtcNow() => Now; }

    private sealed class InjectedFailure : Exception;
    private sealed class Moderator(Guid id) : IModeratorAuthorization { public bool IsAtlasModerator(Guid actor) => actor == id; }
    private sealed class FailingMemberships : ICommunityMembershipRepository
    {
        public IReadOnlyCollection<CommunityMembership> GetByCommunity(CommunityId id) => [];
        public IReadOnlyCollection<CommunityMembership> GetByParticipant(Guid id) => [];
        public CommunityMembership? Get(CommunityId id, Guid participant) => null;
        public void Save(CommunityMembership membership) => throw new InjectedFailure();
    }
    private sealed class FailingNodes(SqlNodeRepository inner) : INodeRepository, IOperationBoundary, IReferenceLookup
    {
        public T Execute<T>(Func<T> operation) => inner.Execute(operation);
        public bool IsAvailable(string kind, Guid id, bool active) => inner.IsAvailable(kind, id, active);
        public Node? GetById(NodeId id) => inner.GetById(id);
        public IReadOnlyCollection<Node> GetChildren(NodeId id) => inner.GetChildren(id);
        public IReadOnlyCollection<Node> GetByAuthor(NodeAuthorId id) => inner.GetByAuthor(id);
        public IReadOnlyCollection<Node> GetParentCandidates(NodeId id, IReadOnlyCollection<NodeId> parents) => inner.GetParentCandidates(id, parents);
        public void Save(Node node) => throw new InjectedFailure();
    }
    private sealed class FailingCases(SqlModerationCaseRepository inner) : IModerationCaseRepository, IOperationBoundary, IReferenceLookup
    {
        private int _saves;
        public T Execute<T>(Func<T> operation) => inner.Execute(operation);
        public bool IsAvailable(string kind, Guid id, bool active) => inner.IsAvailable(kind, id, active);
        public ModerationCase? GetById(Guid id) => inner.GetById(id);
        public IReadOnlyCollection<ModerationCase> GetAll() => inner.GetAll();
        public void Save(ModerationCase item) { if (++_saves == 2) throw new InjectedFailure(); inner.Save(item); }
    }
    private sealed class FailingReactions(SqlNodeReactionRepository inner) : INodeReactionRepository, IOperationBoundary, IReferenceLookup
    {
        private int _saves;
        public T Execute<T>(Func<T> operation) => inner.Execute(operation);
        public bool IsAvailable(string kind, Guid id, bool active) => inner.IsAvailable(kind, id, active);
        public IReadOnlyCollection<NodeReaction> GetAll() => inner.GetAll();
        public NodeReaction? GetById(NodeReactionId id) => inner.GetById(id);
        public IReadOnlyCollection<NodeReaction> GetActiveForNode(NodeId id) => inner.GetActiveForNode(id);
        public NodeReaction? GetActive(NodeId node, ReactionDefinitionId definition) => inner.GetActive(node, definition);
        public void Save(NodeReaction item) { if (++_saves == 2) throw new InjectedFailure(); inner.Save(item); }
    }
}
