using Atlas.Content.Blocks;
using Atlas.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
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
        // Create an isolated SQL database and valid author/type references.
        using var database = SqlTestDatabase.Create();
        var author = database.AddParticipant();
        var type = NodeTypeDefinition.CreateSystemDefined("Idea", "Ideas", Now);
        new SqlNodeTypeRepository(database.Open).Save(type);
        // Keep real SQL lookups and transactions, but make the final Node save throw.
        var nodes = new FailingNodes(new SqlNodeRepository(database.Open));
        var service = new NodeCreationService(nodes, new SqlDocumentRepository(database.Open));
        // Run block -> document -> Node creation; failure at the last step must escape to the caller.
        Assert.Throws<InjectedFailure>(() => service.Create("Title", "Description", type.Id, author, []));
        // Read through a fresh context after rollback; earlier block/document writes must also be gone.
        using var check = database.Open();
        Assert.AreEqual(0, check.NodeRows.Count());
        Assert.AreEqual(0, check.DocumentRows.Count());
        Assert.AreEqual(0, check.BlockRows.Count());
    }

    [TestMethod]
    public void FailedOwnerMembershipRollsBackCommunity()
    {
        // Use a real community repository so the first write reaches SQL.
        using var database = SqlTestDatabase.Create();
        var communities = new SqlCommunityRepository(database.Open);
        // Replace only owner-membership saving with a failure after community creation.
        var service = new CommunityService(communities, new FailingMemberships(), new SqlCommunityNodeRepository(database.Open));
        // The complete create operation must fail when its owner membership cannot be saved.
        Assert.Throws<InjectedFailure>(() => service.Create("Community", "Description", database.AddParticipant(), Now));
        // Verify that neither the community nor a partial owner membership was committed.
        Assert.IsEmpty(communities.GetAll());
        using var check = database.Open();
        Assert.AreEqual(0, check.CommunityMembershipRows.Count());
    }

    [TestMethod]
    public void FailedSecondModerationSaveRollsBackWholeGroup()
    {
        // Create two pending reports for the same node and an authorized moderator.
        using var database = SqlTestDatabase.Create();
        var moderator = database.AddParticipant(); var reporter = database.AddParticipant(); var node = database.AddNode();
        var cases = new SqlModerationCaseRepository(database.Open);
        var original = new ModerationService(cases, new Moderator(moderator));
        original.ReportNode(node, reporter, "Title", "spam", null, Now);
        original.ReportNode(node, reporter, "Title", "spam", null, Now);
        // Allow the first report update to reach SQL, then throw on the second save.
        var service = new ModerationService(new FailingCases(cases), new Moderator(moderator));
        // Attempt one decision covering both reports inside the operation boundary.
        Assert.Throws<InjectedFailure>(() => service.DecideNode(moderator, node, ModerationDecision.HideNode, "Reviewed", Now.AddMinutes(1)));
        // Reload persisted reports: both must remain pending, with no reviewer recorded.
        Assert.IsTrue(cases.GetAll().All(item => item.Status == ModerationStatus.Submitted && item.ReviewerId is null));
        Assert.HasCount(2, cases.GetAll());
    }

    [TestMethod]
    public void FailedSupersessionSaveRollsBackReplacement()
    {
        // Create a valid node and two available reaction definitions.
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
        // Persist the original active reaction before testing replacement.
        var first = new NodeReactionApplicationService(definitions, reactions).Apply(node, "Helpful", actor, true, Now);
        // Save the replacement normally, but fail the following old-reaction supersession save.
        var service = new NodeReactionApplicationService(definitions, new FailingReactions(reactions));
        // The replacement operation must propagate the injected failure.
        Assert.Throws<InjectedFailure>(() => service.Replace(node, first.Id, "Interesting", actor, true, false, Now.AddMinutes(1)));
        // Verify rollback removed the replacement and preserved the original state and audit history.
        Assert.HasCount(1, reactions.GetAll());
        Assert.AreEqual(NodeReactionLifecycleState.Active, reactions.GetById(first.Id)!.LifecycleState);
        Assert.HasCount(1, reactions.GetById(first.Id)!.AuditHistory);
    }

    [TestMethod]
    public async Task CompetingCommunityNamesHaveOneCompleteWinner()
    {
        // Give both competing creates the same valid owner and requested community name.
        using var database = SqlTestDatabase.Create();
        var owner = database.AddParticipant();
        // A closed test gate delays competitors until all tasks are launched; it is not a SQL lock.
        using var start = new ManualResetEventSlim();
        // Each task builds its own repositories/context; no EF context is shared between competitors.
        Task<bool> Create() => Task.Run(() =>
        {
            // Wait for the shared start signal; scheduling after release is intentionally unspecified.
            start.Wait();
            var service = new CommunityService(new SqlCommunityRepository(database.Open),
                new SqlCommunityMembershipRepository(database.Open), new SqlCommunityNodeRepository(database.Open));
            // Record success or a rejected/conflicting operation; unrelated exception types still fail the test.
            try { service.Create("Same name", "Description", owner, Now); return true; }
            catch (InvalidOperationException) { return false; }
        });
        var one = Create(); var two = Create(); start.Set();
        // Await both outcomes before querying; do not assume which task wins.
        var results = await Task.WhenAll(one, two);
        // Require one complete winner: one community and its owner membership, with no partial loser.
        Assert.AreEqual(1, results.Count(result => result));
        using var check = database.Open();
        Assert.AreEqual(1, check.CommunityRows.Count());
        Assert.AreEqual(1, check.CommunityMembershipRows.Count());
    }

    [TestMethod]
    public async Task CompetingModeratorsCannotSplitGroupDecisions()
    {
        // Persist three pending reports that must be decided as one group.
        using var database = SqlTestDatabase.Create();
        var actor = database.AddParticipant(); var reporter = database.AddParticipant(); var node = database.AddNode();
        var cases = new SqlModerationCaseRepository(database.Open);
        var service = new ModerationService(cases, new Moderator(actor));
        for (var i = 0; i < 3; i++) service.ReportNode(node, reporter, "Title", "spam", null, Now);
        // A closed test gate delays competitors until all tasks are launched; it is not a SQL lock.
        using var start = new ManualResetEventSlim();
        // Run opposing decisions in separate execution flows and SQL operation contexts.
        Task<bool> Decide(ModerationDecision decision) => Task.Run(() =>
        {
            // Wait for the shared start signal; scheduling after release is intentionally unspecified.
            start.Wait();
            var operation = new ModerationService(new SqlModerationCaseRepository(database.Open), new Moderator(actor));
            // Treat a rejected/conflicting decision as the loser rather than choosing a winner in advance.
            try { operation.DecideNode(actor, node, decision, decision.ToString(), Now.AddMinutes(1)); return true; }
            catch (InvalidOperationException) { return false; }
        });
        var one = Decide(ModerationDecision.HideNode); var two = Decide(ModerationDecision.Dismiss); start.Set();
        // Wait for both tasks and require only one successful group decision.
        Assert.AreEqual(1, (await Task.WhenAll(one, two)).Count(result => result));
        // Reload all reports; every report must have the same completed status and decision reason.
        var reports = cases.GetAll();
        Assert.HasCount(3, reports);
        Assert.AreEqual(1, reports.Select(item => item.Status).Distinct().Count());
        Assert.AreEqual(1, reports.Select(item => item.DecisionReason).Distinct().Count());
        Assert.IsTrue(reports.All(item => item.Status != ModerationStatus.Submitted));
    }

    [TestMethod]
    public void MissingWrongKindInactiveAndHiddenReferencesAreRejected()
    {
        // Start with an existing participant and node so each rejection isolates one reference rule.
        using var database = SqlTestDatabase.Create();
        var actor = database.AddParticipant(); var node = database.AddNode();
        var votes = new SqlVoteRepository(database.Open);
        // A nonexistent Node ID must be rejected before a vote can be persisted.
        Assert.Throws<InvalidOperationException>(() => votes.Save(new Vote(new NodeVoteTarget(Guid.NewGuid()), new VotingParticipantId(actor), 1)));
        // An existing Node ID cannot satisfy a reference that specifically requires a reaction.
        Assert.Throws<InvalidOperationException>(() => votes.Save(new Vote(new NodeReactionVoteTarget(node), new VotingParticipantId(actor), 1)));
        // Deactivate the participant directly in SQL, then verify voting rejects that inactive actor.
        using (var db = database.Open()) { db.ParticipantRows.Single(row => row.Id == actor).IsActive = false; db.SaveChanges(); }
        Assert.Throws<InvalidOperationException>(() => votes.Save(new Vote(new NodeVoteTarget(node), new VotingParticipantId(actor), 1)));
        // Hide the node through the real moderation workflow, using active participants.
        var moderator = database.AddParticipant(); var reporter = database.AddParticipant();
        var moderation = new ModerationService(new SqlModerationCaseRepository(database.Open), new Moderator(moderator));
        moderation.ReportNode(node, reporter, "Title", "spam", null, Now);
        moderation.DecideNode(moderator, node, ModerationDecision.HideNode, "Reviewed", Now.AddMinutes(1));
        // The hidden node still exists, but it is no longer an available voting target.
        Assert.Throws<InvalidOperationException>(() => votes.Save(new Vote(new NodeVoteTarget(node), new VotingParticipantId(reporter), 1)));
        // Confirm none of the rejected proposals left a vote behind.
        using var check = database.Open(); Assert.AreEqual(0, check.VoteRows.Count());
    }

    [TestMethod]
    public void SqlUniqueConflictHasApplicationErrorAndRollsBack()
    {
        // Persist the first vote for one participant/target pair.
        using var database = SqlTestDatabase.Create();
        var actor = database.AddParticipant(); var node = database.AddNode();
        var repository = new SqlVoteRepository(database.Open);
        repository.Save(new Vote(new NodeVoteTarget(node), new VotingParticipantId(actor), 1));
        // A second distinct vote for that pair hits the real unique constraint, translated to an application conflict.
        Assert.Throws<OperationConflictException>(() => repository.Save(new Vote(new NodeVoteTarget(node), new VotingParticipantId(actor), 2)));
        // Verify the original vote remains and the rejected insert added no second record.
        using var check = database.Open(); Assert.AreEqual(1, check.VoteRows.Count());
    }

    [TestMethod]
    public async Task CompetingOppositeParentLinksCannotCreateCycle()
    {
        // Create two unrelated nodes under a fixed clock so later relationship timestamps are valid.
        using var database = SqlTestDatabase.Create();
        using var clock = AtlasTime.Use(new FixedClock());
        var actor = database.AddParticipant();
        var type = NodeTypeDefinition.CreateSystemDefined("Idea", "Ideas", Now);
        new SqlNodeTypeRepository(database.Open).Save(type);
        var nodes = new SqlNodeRepository(database.Open);
        var creation = new NodeCreationService(nodes, new SqlDocumentRepository(database.Open));
        var a = creation.Create("A", "Description", type.Id, actor, []);
        var b = creation.Create("B", "Description", type.Id, actor, []);
        // A closed test gate delays competitors until all tasks are launched; it is not a SQL lock.
        using var start = new ManualResetEventSlim();
        // Give each competing attachment its own repository and operation context.
        Task<bool> Attach(Node child, NodeId parent) => Task.Run(() =>
        {
            // Wait for the shared start signal; scheduling after release is intentionally unspecified.
            start.Wait();
            var repository = new SqlNodeRepository(database.Open);
            // The topology boundary takes the graph lock before ancestry checks; one proposal must be rejected.
            try { new NodeRelationshipService(repository).Attach(child, parent, actor, Now.AddMinutes(1)); return true; }
            catch (InvalidOperationException) { return false; }
        });
        // Propose A under B and B under A, then release both tasks to compete.
        var one = Attach(a, b.Id); var two = Attach(b, a.Id); start.Set();
        Assert.AreEqual(1, (await Task.WhenAll(one, two)).Count(result => result));
        // Reload database state and verify exactly one edge exists, never the two-edge cycle.
        var loadedA = nodes.GetById(a.Id)!; var loadedB = nodes.GetById(b.Id)!;
        Assert.AreEqual(1, loadedA.ParentNodeIds.Count + loadedB.ParentNodeIds.Count);
        Assert.IsFalse(loadedA.ParentNodeIds.Contains(b.Id) && loadedB.ParentNodeIds.Contains(a.Id));
    }

    [TestMethod]
    public void NodeReferenceChecksRejectMissingDocumentTypeAndInactiveAuthor()
    {
        // Seed valid references and fix time so reference failures are not timestamp failures.
        using var database = SqlTestDatabase.Create();
        using var clock = AtlasTime.Use(new FixedClock());
        var actor = database.AddParticipant();
        var type = NodeTypeDefinition.CreateSystemDefined("Idea", "Ideas", Now);
        new SqlNodeTypeRepository(database.Open).Save(type);
        var nodes = new SqlNodeRepository(database.Open);
        var creation = new NodeCreationService(nodes, new SqlDocumentRepository(database.Open));
        // Reject node creation with a type ID that has no backing row.
        Assert.Throws<InvalidOperationException>(() => creation.Create("Title", "Text", NodeTypeId.New(), actor, []));
        // Also protect direct repository saves: a made-up document ID must not be accepted.
        Assert.Throws<InvalidOperationException>(() => nodes.Save(new Node(new NodeTitle("Title"), new NodeDescriptionId(Guid.NewGuid()), type.Id, new NodeAuthorId(actor), Now)));
        // Deactivate the existing author, then reject a new node attributed to that author.
        using (var db = database.Open()) { db.ParticipantRows.Single(row => row.Id == actor).IsActive = false; db.SaveChanges(); }
        Assert.Throws<InvalidOperationException>(() => creation.Create("Title", "Text", type.Id, actor, []));
        // Check that all rejected operations left no Node, block or document records.
        using var check = database.Open();
        Assert.AreEqual(0, check.NodeRows.Count()); Assert.AreEqual(0, check.BlockRows.Count()); Assert.AreEqual(0, check.DocumentRows.Count());
    }

    [TestMethod]
    public void CommunityAndCommentReferencePoliciesRejectInvalidProposals()
    {
        // Prepare active participants, two distinct targets and the community coordination service.
        using var database = SqlTestDatabase.Create();
        var actor = database.AddParticipant(); var firstNode = database.AddNode(); var otherNode = database.AddNode();
        var communities = new SqlCommunityRepository(database.Open);
        var associations = new SqlCommunityNodeRepository(database.Open);
        var membership = new SqlCommunityMembershipRepository(database.Open);
        var service = new CommunityService(communities, membership, associations);
        // Reject a missing owner, then create a valid community with its owner membership.
        Assert.Throws<InvalidOperationException>(() => service.Create("Invalid owner", "", Guid.NewGuid(), Now));
        var community = service.Create("Community", "", actor, Now);
        // Reject a community association to a nonexistent node.
        Assert.Throws<InvalidOperationException>(() => service.AssociateNode(community, Guid.NewGuid(), actor, Now));
        var comments = new SqlCommentRepository(database.Open);
        // Save one valid parent comment against the first target.
        var parent = new Atlas.Comments.Comments.Comment(new("Node", firstNode), null, actor, "Parent", Now);
        comments.Save(parent);
        // A reply must share its parent target; unsupported target kinds and earlier reply timestamps also fail.
        Assert.Throws<ArgumentException>(() => comments.Save(new(new("Node", otherNode), parent.Id, actor, "Foreign target", Now.AddMinutes(1))));
        Assert.Throws<ArgumentException>(() => comments.Save(new(new("Unknown", firstNode), null, actor, "Wrong kind", Now)));
        Assert.Throws<ArgumentException>(() => comments.Save(new(new("Node", firstNode), parent.Id, actor, "Earlier reply", Now.AddMinutes(-1))));
        // Deactivate the actor and reject both joining and associating a node.
        using (var db = database.Open()) { db.ParticipantRows.Single(row => row.Id == actor).IsActive = false; db.SaveChanges(); }
        Assert.Throws<InvalidOperationException>(() => service.Join(community, actor, Now));
        Assert.Throws<InvalidOperationException>(() => service.AssociateNode(community, firstNode, actor, Now));
        // Keep only the valid parent comment and owner membership; rejected proposals must leave no association.
        using var check = database.Open();
        Assert.AreEqual(1, check.CommentRows.Count()); Assert.AreEqual(0, check.CommunityNodeRows.Count());
        Assert.AreEqual(1, check.CommunityMembershipRows.Count());
    }

    [TestMethod]
    public void FailedGroupedReviewAndRestorationPreservePersistedHistory()
    {
        // Create two reports and successfully hide their shared node.
        using var database = SqlTestDatabase.Create();
        var actor = database.AddParticipant(); var reporter = database.AddParticipant(); var node = database.AddNode();
        var cases = new SqlModerationCaseRepository(database.Open);
        var service = new ModerationService(cases, new Moderator(actor));
        for (var index = 0; index < 2; index++) service.ReportNode(node, reporter, "Title", "spam", null, Now);
        service.DecideNode(actor, node, ModerationDecision.HideNode, "Reviewed", Now.AddMinutes(1));
        // Fail the second grouped review-request save so the first update must roll back.
        var failingReview = new ModerationService(new FailingCases(cases), new Moderator(actor));
        Assert.Throws<InjectedFailure>(() => failingReview.RequestNodeReview(node, reporter, reporter, Now.AddMinutes(2), Now.AddMinutes(3)));
        // Both reports must retain the hidden state without a partially recorded review request.
        Assert.IsTrue(cases.GetAll().All(item => item.ReviewRequestedAt is null && item.IsHidden));
        // Retry through the healthy repository and persist the review request for the whole group.
        service.RequestNodeReview(node, reporter, reporter, Now.AddMinutes(2), Now.AddMinutes(3));
        // Fail the second restoration save; neither report may retain a partial restoration history.
        var failingRestore = new ModerationService(new FailingCases(cases), new Moderator(actor));
        Assert.Throws<InjectedFailure>(() => failingRestore.RestoreNode(actor, node, "Restored", Now.AddMinutes(4)));
        Assert.IsTrue(cases.GetAll().All(item => item.IsHidden && item.VisibilityRestoredAt is null && item.RestoredBy is null));
        // Finally perform a successful restoration and verify both reports record the same restoration time.
        service.RestoreNode(actor, node, "Restored", Now.AddMinutes(4));
        Assert.IsTrue(cases.GetAll().All(item => !item.IsHidden && item.VisibilityRestoredAt == Now.AddMinutes(4)));
    }

    [TestMethod]
    public async Task CompetingNewVotesReturnOneConflictAndLeaveOneRecord()
    {
        // Use the same active participant and target for two different proposed ratings.
        using var database = SqlTestDatabase.Create();
        var actor = database.AddParticipant(); var node = database.AddNode();
        // A closed test gate delays competitors until all tasks are launched; it is not a SQL lock.
        using var start = new ManualResetEventSlim();
        // Each task creates its own repository/context and attempts a new vote insert.
        Task<bool> Save(int value) => Task.Run(() =>
        {
            // Wait for the shared start signal; scheduling after release is intentionally unspecified.
            start.Wait();
            try { new SqlVoteRepository(database.Open).Save(new Vote(new NodeVoteTarget(node), new VotingParticipantId(actor), value)); return true; }
            // Only the expected translated persistence conflict counts as the losing outcome.
            catch (OperationConflictException) { return false; }
        });
        // Launch both proposals and open the start gate; either rating may win.
        var one = Save(1); var two = Save(2); start.Set();
        Assert.AreEqual(1, (await Task.WhenAll(one, two)).Count(result => result));
        // After both finish, verify SQL contains exactly one vote for the pair.
        using var check = database.Open(); Assert.AreEqual(1, check.VoteRows.Count());
    }

    [TestMethod]
    public async Task CompetingDirectNodeSavesCannotCreateTransitiveCycle()
    {
        // Create three independent nodes under a fixed clock.
        using var database = SqlTestDatabase.Create();
        using var clock = AtlasTime.Use(new FixedClock());
        var actor = database.AddParticipant();
        var type = NodeTypeDefinition.CreateSystemDefined("Idea", "Ideas", Now);
        new SqlNodeTypeRepository(database.Open).Save(type);
        var nodes = new SqlNodeRepository(database.Open);
        var creation = new NodeCreationService(nodes, new SqlDocumentRepository(database.Open));
        var a = creation.Create("A", "Description", type.Id, actor, []);
        var b = creation.Create("B", "Description", type.Id, actor, []);
        var c = creation.Create("C", "Description", type.Id, actor, []);
        // Build three locally valid proposals; together A -> B -> C -> A would form a cycle.
        a.AttachToParent(b.Id, actor, Now.AddMinutes(1));
        b.AttachToParent(c.Id, actor, Now.AddMinutes(1));
        c.AttachToParent(a.Id, actor, Now.AddMinutes(1));
        // A closed test gate delays competitors until all tasks are launched; it is not a SQL lock.
        using var start = new ManualResetEventSlim();
        // Bypass the relationship service to test that full repository saves also coordinate ancestry checks.
        Task<bool> Save(Node value) => Task.Run(() =>
        {
            // Wait for the shared start signal; scheduling after release is intentionally unspecified.
            start.Wait();
            try { new SqlNodeRepository(database.Open).Save(value); return true; }
            catch (InvalidOperationException) { return false; }
        });
        // Launch all three proposals without sharing EF contexts, then release the start gate.
        var operations = new[] { Save(a), Save(b), Save(c) };
        start.Set();
        // Any two edges are valid; the final cycle-closing proposal must fail regardless of scheduling.
        Assert.AreEqual(2, (await Task.WhenAll(operations)).Count(result => result));
        // Reload every node and independently walk ancestry to verify the committed graph is acyclic.
        var loaded = new[] { nodes.GetById(a.Id)!, nodes.GetById(b.Id)!, nodes.GetById(c.Id)! };
        Assert.AreEqual(2, loaded.Sum(value => value.ParentNodeIds.Count));
        foreach (var value in loaded)
            GraphRelationshipValidation.EnsureAcyclic(value.Id, value.ParentNodeIds, id => nodes.GetById(id)?.ParentNodeIds);
    }

    [TestMethod]
    public async Task MetadataContentAndVoteWritesCompleteWhileGraphLockIsHeld()
    {
        // Seed a voting target and a complete editable node before taking the test lock.
        using var database = SqlTestDatabase.Create();
        var actor = database.AddParticipant();
        var node = database.AddNode();
        using var clock = AtlasTime.Use(new FixedClock());
        var type = NodeTypeDefinition.CreateSystemDefined("Idea", "Ideas", Now);
        new SqlNodeTypeRepository(database.Open).Save(type);
        var nodes = new SqlNodeRepository(database.Open);
        var editable = new NodeCreationService(nodes, new SqlDocumentRepository(database.Open))
            .Create("Original", "Description", type.Id, actor, []);
        // The acquired signal confirms lock ownership; the release signal controls how long the holder keeps it.
        using var acquired = new ManualResetEventSlim();
        using var release = new ManualResetEventSlim();
        // Start separate execution flows outside any ambient operation context.
        var holder = Task.Run(() => new SqlNodeRepository(database.Open).ExecuteTopology(() =>
        {
            // ExecuteTopology acquired the SQL application lock before entering this callback; tell the test it is held.
            acquired.Set();
            // Keep the transaction open until released, with a timeout so a failing test cannot wait indefinitely.
            if (!release.Wait(TimeSpan.FromSeconds(30))) throw new TimeoutException("Test did not release graph lock.");
            return true;
        }));
        Task? writes = null;
        try
        {
            // Do not begin independent writes until the holder has confirmed acquisition.
            Assert.IsTrue(acquired.Wait(TimeSpan.FromSeconds(10)), "Graph boundary did not start.");
            // Use a separate execution flow created outside the holder callback so it cannot inherit that ambient context.
            writes = Task.Run(() =>
            {
                // Rename through the metadata path, which preserves parents and requests no topology lock.
                editable.Rename(new NodeTitle("Renamed without graph lock"), actor, Now.AddMinutes(1));
                new SqlNodeRepository(database.Open).SaveMetadata(editable);
                // Content and vote writes also use ordinary transactions without requesting the topology lock.
                new SqlDocumentRepository(database.Open).SaveBlock(new MarkdownTextBlock("Independent content", Now));
                new SqlVoteRepository(database.Open).Save(new Vote(new NodeVoteTarget(node), new VotingParticipantId(actor), 3));
            });
            // The writes must finish BEFORE releasing the graph lock, not just eventually succeed.
            await writes.WaitAsync(TimeSpan.FromSeconds(10));
            // Verify persisted results while the holder is still waiting: seed block plus new block, one vote and renamed title.
            using var check = database.Open();
            Assert.AreEqual(2, check.BlockRows.Count());
            Assert.AreEqual(1, check.VoteRows.Count());
            Assert.AreEqual("Renamed without graph lock", nodes.GetById(editable.Id)!.Title.Value);
        }
        finally
        {
            // Always release the holder, even after a failed assertion, and await outstanding tasks before deleting the database.
            release.Set();
            await holder;
            if (writes is not null) await writes;
        }
    }

    [TestMethod]
    public void NestedNodeBoundaryAcquiresGraphLockAndOuterRollbackReleasesIt()
    {
        // Use a Content-rooted operation to test acquiring topology coordination later inside the same transaction.
        using var database = SqlTestDatabase.Create();
        AtlasDataContext? outer = null;
        AtlasDataContext Open()
        {
            var candidate = database.Open();
            // Retain the first context reference so assertions can inspect the actual outer transaction.
            outer ??= candidate;
            return candidate;
        }
        var content = new SqlDocumentRepository(Open);
        // Start the outer Content transaction; its ordinary boundary requests no graph lock.
        Assert.Throws<InjectedFailure>(() => content.Execute<bool>(() =>
        {
            // APPLOCK_MODE reports whether this transaction currently owns the named resource.
            Assert.AreEqual("NoLock", outer!.Database.SqlQueryRaw<string>(
                "SELECT APPLOCK_MODE('public', 'AtlasGraphTopology', 'Transaction') AS [Value]").Single());
            // The nested topology boundary joins the existing context/transaction and acquires its graph lock.
            new SqlNodeRepository(database.Open).ExecuteTopology(() =>
            {
                Assert.AreEqual("Exclusive", outer.Database.SqlQueryRaw<string>(
                    "SELECT APPLOCK_MODE('public', 'AtlasGraphTopology', 'Transaction') AS [Value]").Single());
                return true;
            });
            // Write a real block after the nested callback returns; it still belongs to the outer transaction.
            content.SaveBlock(new MarkdownTextBlock("Must roll back", Now));
            // A nested return must neither commit nor release the transaction-owned graph lock.
            Assert.AreEqual("Exclusive", outer.Database.SqlQueryRaw<string>(
                "SELECT APPLOCK_MODE('public', 'AtlasGraphTopology', 'Transaction') AS [Value]").Single());
            // Abort the outer operation so both its SQL writes and its transaction-owned lock are released.
            throw new InjectedFailure();
        }));
        // A fresh context must see no block; APPLOCK_TEST = 1 means another owner could now acquire the lock.
        using var check = database.Open();
        Assert.AreEqual(0, check.BlockRows.Count());
        Assert.AreEqual(1, check.Database.SqlQueryRaw<int>(
            "SELECT APPLOCK_TEST('public', 'AtlasGraphTopology', 'Exclusive', 'Session') AS [Value]").Single());
    }

    [TestMethod]
    public void MetadataSavePreservesParentsAddedSinceNodeWasLoaded()
    {
        // Create an initially parentless child and a possible parent using valid references and fixed time.
        using var database = SqlTestDatabase.Create();
        using var clock = AtlasTime.Use(new FixedClock());
        var actor = database.AddParticipant();
        var type = NodeTypeDefinition.CreateSystemDefined("Idea", "Ideas", Now);
        new SqlNodeTypeRepository(database.Open).Save(type);
        var nodes = new SqlNodeRepository(database.Open);
        var creation = new NodeCreationService(nodes, new SqlDocumentRepository(database.Open));
        var child = creation.Create("Child", "Description", type.Id, actor, []);
        var parent = creation.Create("Parent", "Description", type.Id, actor, []);
        // Keep an older child snapshot whose parent collection is still empty.
        var stale = nodes.GetById(child.Id)!;
        // Persist a new parent using the topology workflow after taking that snapshot.
        new NodeRelationshipService(nodes).Attach(child, parent.Id, actor, Now.AddMinutes(1));
        // Rename the older snapshot, then save metadata without replacing stored parent rows.
        stale.Rename(new NodeTitle("Renamed"), actor, Now.AddMinutes(2));
        nodes.SaveMetadata(stale);
        // Reload and verify both changes survived: the new title and the parent absent from the stale snapshot.
        var saved = nodes.GetById(child.Id)!;
        Assert.AreEqual("Renamed", saved.Title.Value);
        CollectionAssert.AreEqual(new[] { parent.Id }, saved.ParentNodeIds.ToArray());
    }

    [TestMethod]
    public void SqlDocumentInsertFailureDuringNodeCreationRollsBackEarlierBlockSave()
    {
        // Seed valid author/type references and fixed time so document persistence is the only intended failure.
        using var database = SqlTestDatabase.Create();
        using var clock = AtlasTime.Use(new FixedClock());
        var author = database.AddParticipant();
        var type = NodeTypeDefinition.CreateSystemDefined("Idea", "Ideas", Now);
        new SqlNodeTypeRepository(database.Open).Save(type);
        using (var setup = database.Open())
        {
            // Test-only constraint in this disposable database: reject real document INSERTs.
            // No repository save is replaced and no application exception is injected.
            setup.Database.ExecuteSqlRaw("ALTER TABLE [DocumentRows] ADD CONSTRAINT [CK_Test_RejectDocumentInsert] CHECK ([Id] = '00000000-0000-0000-0000-000000000000')");
        }
        // Record evidence that the block INSERT succeeded before the document INSERT was rejected.
        var blockWasSavedBeforeFailure = false;
        AtlasDataContext Open()
        {
            var context = database.Open();
            // Observe the successful SQL write inside the transaction before document saving starts.
            context.SavedChanges += (_, _) =>
                blockWasSavedBeforeFailure |= context.BlockRows.Any() && !context.DocumentRows.Any();
            return context;
        }
        // Use real Node and Document repositories with the observing factory; their production save behavior is unchanged.
        var service = new NodeCreationService(new SqlNodeRepository(Open), new SqlDocumentRepository(Open));
        // Run the complete creation workflow and require the translated persistence error to reach the caller.
        var failure = Assert.Throws<OperationConflictException>(() =>
            service.Create("Title", "Description", type.Id, author, []));
        Assert.IsTrue(blockWasSavedBeforeFailure, "The real block save must succeed before document insertion fails.");
        // Unwrap the application/EF exceptions to verify the original SQL error came from the test constraint.
        Exception? cause = failure;
        while (cause is not null && cause is not SqlException) cause = cause.InnerException;
        Assert.IsInstanceOfType<SqlException>(cause);
        var sqlFailure = (SqlException)cause!;
        Assert.AreEqual(547, sqlFailure.Number);
        StringAssert.Contains(sqlFailure.Message, "CK_Test_RejectDocumentInsert");
        // Read committed state through a fresh context: rollback must remove the earlier block and leave no related rows.
        using var check = database.Open();
        Assert.AreEqual(0, check.BlockRows.Count());
        Assert.AreEqual(0, check.DocumentRows.Count());
        Assert.AreEqual(0, check.Set<DocumentBlockRow>().Count());
        Assert.AreEqual(0, check.NodeRows.Count());
    }

    // Stable UTC time keeps concurrency tests focused on storage rules rather than wall-clock changes.
    private sealed class FixedClock : TimeProvider { public override DateTimeOffset GetUtcNow() => Now; }

    // A distinct exception identifies deliberate test failures, separate from real SQL conflicts.
    private sealed class InjectedFailure : Exception;
    // Grant moderator authorization only to the seeded actor used by each scenario.
    private sealed class Moderator(Guid id) : IModeratorAuthorization { public bool IsAtlasModerator(Guid actor) => actor == id; }
    // Reject owner-membership saving after the real community write has occurred.
    private sealed class FailingMemberships : ICommunityMembershipRepository
    {
        public IReadOnlyCollection<CommunityMembership> GetByCommunity(CommunityId id) => [];
        public IReadOnlyCollection<CommunityMembership> GetByParticipant(Guid id) => [];
        public CommunityMembership? Get(CommunityId id, Guid participant) => null;
        public void Save(CommunityMembership membership) => throw new InjectedFailure();
    }
    // Forward transaction and reference capabilities to SQL; replace only the final Node save.
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
    // Forward SQL behavior, persist the first report update, then abort the second group save.
    private sealed class FailingCases(SqlModerationCaseRepository inner) : IModerationCaseRepository, IOperationBoundary, IReferenceLookup
    {
        private int _saves;
        public T Execute<T>(Func<T> operation) => inner.Execute(operation);
        public bool IsAvailable(string kind, Guid id, bool active) => inner.IsAvailable(kind, id, active);
        public ModerationCase? GetById(Guid id) => inner.GetById(id);
        public IReadOnlyCollection<ModerationCase> GetAll() => inner.GetAll();
        public void Save(ModerationCase item) { if (++_saves == 2) throw new InjectedFailure(); inner.Save(item); }
    }
    // Persist the new reaction first, then abort supersession of the old association.
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
