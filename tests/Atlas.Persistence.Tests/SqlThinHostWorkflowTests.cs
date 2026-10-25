using Atlas.Application;
using Atlas.Application.Comments;
using Atlas.Application.Eventing;
using Atlas.Application.Voting;
using Atlas.Comments.Comments;
using Atlas.Contracts.Notifications.V1;
using Atlas.Graph.Nodes.NodeTypes;
using Atlas.Graph.Reactions;
using Atlas.Application.Storage;
using Atlas.Moderation;
using Atlas.Voting;
using Atlas.Voting.Eligibility;

namespace Atlas.Persistence.Tests;

[TestClass]
public sealed class SqlThinHostWorkflowTests
{
    [TestMethod]
    public void ReactionAndVoteShareRollbackBoundaryWithoutConsole()
    {
        using var database = SqlTestDatabase.Create();
        var actor = database.AddParticipant();
        var nodes = new SqlNodeRepository(database.Open);
        var documents = new SqlDocumentRepository(database.Open);
        var type = new NodeTypeCreationService(new SqlNodeTypeRepository(database.Open))
            .Create("Idea", "Ideas", actor.ToString(), DateTimeOffset.UtcNow);
        var node = new NodeCreationService(nodes, documents).Create("Node", "Text", type.Id, actor, []);
        var definitions = new SqlReactionDefinitionRepository(database.Open);
        definitions.Save(ReactionDefinition.Create("Helpful", "👍", "Helpful", actor, DateTimeOffset.UtcNow));
        var reactions = new SqlNodeReactionRepository(database.Open);
        var votes = new SqlVoteRepository(database.Open);
        var eligibility = new RepositoryVotingEligibility(nodes, new SqlParticipantRepository(database.Open), reactions);
        var workflow = new ReactionVotingService(definitions, reactions, new CastVote(votes, new VoteMutationPolicy(eligibility)), new SqlParticipantRepository(database.Open));
        Assert.Throws<ArgumentException>(() => workflow.Vote(node, "Helpful", actor, 999));
        using (var check = database.Open())
        {
            Assert.AreEqual(0, check.NodeReactionRows.Count());
            Assert.AreEqual(0, check.VoteRows.Count());
        }
        workflow.Vote(node, "Helpful", actor, 1);
        using (var check = database.Open())
        {
            Assert.AreEqual(1, check.NodeReactionRows.Count());
            Assert.AreEqual(1, check.VoteRows.Count());
        }
    }

    [TestMethod]
    public void CommentVotesPersistChangeUndoAndRejectRemovedAndArchivedTargets()
    {
        using var database = SqlTestDatabase.Create();
        var actor = database.AddParticipant();
        var nodes = new SqlNodeRepository(database.Open);
        var type = new NodeTypeCreationService(new SqlNodeTypeRepository(database.Open)).Create("Idea", "Ideas", actor.ToString(), DateTimeOffset.UtcNow);
        var node = new NodeCreationService(nodes, new SqlDocumentRepository(database.Open)).Create("Node", "Text", type.Id, actor, []);
        var comments = new SqlCommentRepository(database.Open);
        var comment = new Comment(CommentTarget.Node(node.Id.Value), null, actor, "Discuss", DateTimeOffset.UtcNow);
        comments.Save(comment);
        var votes = new SqlVoteRepository(database.Open);
        var eligibility = new RepositoryVotingEligibility(nodes, new SqlParticipantRepository(database.Open), new SqlNodeReactionRepository(database.Open), comments);
        var policy = new VoteMutationPolicy(eligibility);
        var cast = new CastVote(votes, policy);
        var undo = new UndoVote(votes, policy);
        var target = new Atlas.Voting.Target.CommentVoteTarget(comment.Id.Value);
        var voter = new Atlas.Voting.Votes.ParticipantId(actor);
        cast.Execute(target, voter, 1);
        cast.Execute(target, voter, -1);
        var summary = new GetCommentVoteSummary(votes).Execute(target, voter);
        Assert.AreEqual(-1, summary.Score);
        Assert.AreEqual(1, summary.TotalVotes);
        Assert.AreEqual(-1, summary.CurrentParticipantVote);
        Assert.IsTrue(undo.Execute(target, voter));
        Assert.IsFalse(undo.Execute(target, voter));
        Assert.AreEqual(0, new GetCommentVoteSummary(votes).Execute(target).Score);
        comment.RemoveByAuthor(actor, comment.UpdatedAt.AddSeconds(1)); comments.Save(comment);
        Assert.Throws<InvalidOperationException>(() => cast.Execute(target, voter, 1));
        var active = new Comment(CommentTarget.Node(node.Id.Value), null, actor, "Still here", DateTimeOffset.UtcNow);
        comments.Save(active);
        using (var db = database.Open()) { db.NodeRows.Single(n => n.Id == node.Id.Value).Status = "Archived"; db.SaveChanges(); }
        Assert.Throws<InvalidOperationException>(() => cast.Execute(new Atlas.Voting.Target.CommentVoteTarget(active.Id.Value), voter, 1));
        using var check = database.Open();
        Assert.AreEqual(0, check.VoteRows.Count());
    }

    [TestMethod]
    public void CommentRecipientsAndModerationNotificationsAreReusableApplicationBehavior()
    {
        using var database = SqlTestDatabase.Create();
        var author = database.AddParticipant(); var commenter = database.AddParticipant(); var moderator = database.AddParticipant();
        var nodes = new SqlNodeRepository(database.Open); var documents = new SqlDocumentRepository(database.Open);
        var type = new NodeTypeCreationService(new SqlNodeTypeRepository(database.Open))
            .Create("Idea", "Ideas", author.ToString(), DateTimeOffset.UtcNow);
        var node = new NodeCreationService(nodes, documents).Create("Node", "Text", type.Id, author, []);
        var moderation = new ModerationService(new SqlModerationCaseRepository(database.Open), new Moderator(moderator));
        var publisher = new InMemoryEventPublisher(); var received = new List<NotificationRequestedV1>();
        publisher.Subscribe<NotificationRequestedV1>(received.Add);
        var comments = new CommentWorkflowService(new SqlCommentRepository(database.Open), nodes, moderation, publisher);
        var root = comments.AddTopLevel(CommentTarget.Node(node.Id.Value), commenter, "Comment", DateTimeOffset.UtcNow);
        comments.Reply(root.Id, author, "Reply", DateTimeOffset.UtcNow);
        Assert.AreEqual(author, received[0].RecipientParticipantId);
        Assert.AreEqual(commenter, received[1].RecipientParticipantId);
        Assert.AreEqual("CommentReplied", received[1].Kind);
        moderation.ReportNode(node.Id.Value, commenter, node.Title.Value, "Spam", null, DateTimeOffset.UtcNow);
        new ModerationWorkflowService(nodes, documents, moderation, publisher)
            .Decide(node.Id.Value, moderator, ModerationDecision.HideNode, "Reviewed", PublicModerationReason.Spam);
        Assert.IsTrue(moderation.Visibility(node.Id.Value).IsHidden);
        Assert.IsTrue(received.Any(item => item.Kind == "ReportDecided" && item.RecipientParticipantId == commenter));
        Assert.IsTrue(received.Any(item => item.Kind == "NodeHidden" && item.RecipientParticipantId == author));
    }

    [TestMethod]
    public void CommunityMutationServicesEnforceOwnerAndPersistLifecycle()
    {
        using var database = SqlTestDatabase.Create();
        var owner = database.AddParticipant(); var other = database.AddParticipant();
        var repository = new SqlCommunityRepository(database.Open);
        var service = new Atlas.Communities.Communities.CommunityService(repository,
            new SqlCommunityMembershipRepository(database.Open), new SqlCommunityNodeRepository(database.Open));
        var community = service.Create("Local group", "Original", owner, DateTimeOffset.UtcNow);
        var denied = Assert.Throws<InvalidOperationException>(() => service.ChangeDescription(community, other, "Forbidden", DateTimeOffset.UtcNow));
        Assert.AreEqual("Only the community owner may change this community.", denied.Message);
        Assert.AreEqual("Original", repository.GetById(community.Id)!.Description);
        service.ChangeDescription(community, owner, "Updated", DateTimeOffset.UtcNow);
        service.Archive(community, owner, DateTimeOffset.UtcNow);
        Assert.AreEqual(Atlas.Communities.Communities.CommunityStatus.Archived, repository.GetById(community.Id)!.Status);
        service.Restore(community, owner, DateTimeOffset.UtcNow);
        Assert.AreEqual(Atlas.Communities.Communities.CommunityStatus.Active, repository.GetById(community.Id)!.Status);
        Assert.AreEqual("Updated", repository.GetById(community.Id)!.Description);
    }

    [TestMethod]
    public void ApplicationStorageDoesNotReferenceConsole()
    {
        var references = typeof(SqlNodeRepository).Assembly.GetReferencedAssemblies().Select(a => a.Name).ToList();
        Assert.DoesNotContain("Atlas.Console", references);
        Assert.DoesNotContain("System.Console", references);
    }

    /// <summary>Rejected workflow calls create neither business side effects nor committed success audit/outbox rows.</summary>
    [TestMethod]
    public void RejectedCommentsAndModerationLeaveSqlStateAndNotificationsUnchanged()
    {
        using var database = SqlTestDatabase.Create();
        var author = database.AddParticipant();
        var outsider = database.AddParticipant();
        var moderator = database.AddParticipant();
        var nodes = new SqlNodeRepository(database.Open);
        var documents = new SqlDocumentRepository(database.Open);
        var type = new NodeTypeCreationService(new SqlNodeTypeRepository(database.Open))
            .Create("Idea", "Ideas", author.ToString(), DateTimeOffset.UtcNow);
        var node = new NodeCreationService(nodes, documents).Create("Node", "Text", type.Id, author, []);
        var cases = new SqlModerationCaseRepository(database.Open);
        var moderation = new ModerationService(cases, new Moderator(moderator));
        var report = moderation.ReportNode(node.Id.Value, outsider, node.Title.Value, "Spam", null, DateTimeOffset.UtcNow);
        var publisher = new InMemoryEventPublisher();
        var requests = new List<NotificationRequestedV1>();
        publisher.Subscribe<NotificationRequestedV1>(requests.Add);
        var comments = new CommentWorkflowService(new SqlCommentRepository(database.Open), nodes, moderation, publisher);
        var comment = comments.AddTopLevel(CommentTarget.Node(node.Id.Value), author, "Original", DateTimeOffset.UtcNow);
        requests.Clear();
        using var before = database.Open();
        var audits = before.AuditRecords.Count();
        var outbox = before.AuditOutbox.Count();

        Assert.ThrowsExactly<UnauthorizedAccessException>(() => comments.Edit(comment.Id, outsider, "Forbidden", DateTimeOffset.UtcNow));
        Assert.ThrowsExactly<InvalidOperationException>(() => comments.Reply(CommentId.New(), outsider, "Orphan", DateTimeOffset.UtcNow));
        Assert.ThrowsExactly<UnauthorizedAccessException>(() =>
            new ModerationWorkflowService(nodes, documents, moderation, publisher)
                .Decide(node.Id.Value, outsider, ModerationDecision.HideNode, "Forbidden", PublicModerationReason.Spam));

        // Fresh context checks committed SQL state, independent of caller-held objects.
        using var after = database.Open();
        Assert.AreEqual("Original", after.CommentRows.Single().Body);
        Assert.AreEqual(1, after.CommentRows.Count());
        Assert.AreEqual(ModerationStatus.Submitted, cases.GetById(report.Id)!.Status);
        Assert.IsFalse(moderation.Visibility(node.Id.Value).IsHidden);
        Assert.IsEmpty(requests);
        Assert.AreEqual(audits, after.AuditRecords.Count());
        Assert.AreEqual(outbox, after.AuditOutbox.Count());
    }

    private sealed class Moderator(Guid id) : IModeratorAuthorization
    {
        public bool IsAtlasModerator(Guid participantId) => participantId == id;
    }
}
