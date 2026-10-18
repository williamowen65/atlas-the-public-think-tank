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

    private sealed class Moderator(Guid id) : IModeratorAuthorization
    {
        public bool IsAtlasModerator(Guid participantId) => participantId == id;
    }
}
