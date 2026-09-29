using Atlas.ConsoleApp;
using Atlas.Comments.Comments;
using Atlas.Communities.Communities;
using Atlas.Communities.Memberships;
using Atlas.Communities.Nodes;
using Atlas.ConsoleApp.Content;
using Atlas.ConsoleApp.Eventing;
using Atlas.ConsoleApp.Storage;
using Atlas.ConsoleApp.Voting;
using Atlas.ConsoleApp.Discovery;
using Atlas.Content.Documents;
using Atlas.Discovery;
using Atlas.Moderation;
using Atlas.Notifications;
using Atlas.Contracts.Notifications.V1;
using Atlas.ConsoleApp.Moderation;
using Atlas.Contracts.Graph.V1;
using Atlas.Graph.Nodes;
using Atlas.Graph.Nodes.NodeTypes;
using Atlas.Graph.Reactions;
using Atlas.Participants.Participants;
using Atlas.Voting;
using Atlas.Voting.Data;
using Atlas.Voting.Eligibility;
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
    .AddUserSecrets<Program>(optional: true)
    .AddEnvironmentVariables()
    .Build();

var connectionString = configuration["ATLAS_SQL_CONNECTION_STRING"];
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("Set ATLAS_SQL_CONNECTION_STRING to a SQL Server connection string.");

SqlStorage.Configure(connectionString);
if (args.Contains("--seed-demo", StringComparer.OrdinalIgnoreCase))
{
    SqlStorage.SeedDemoData();
    System.Console.WriteLine("Demo data seeded in SQL Server. Existing IDs were preserved.");
    return;
}

var nodeCollectionKey = "nodes";

var nodeTypeCollectionKey = "node-types";

var documentCollectionKey = "documents";

var blockCollectionKey = "blocks";

var participantCollectionKey = "participants";

var voteCollectionKey = "votes";

var tagDefinitionCollectionKey = "reaction-definitions";

var nodeTagCollectionKey = "node-reactions";

var communityCollectionKey = "communities";
var communityMembershipCollectionKey = "community-memberships";
var communityNodeCollectionKey = "community-nodes";
var commentCollectionKey = "comments";
var moderationCollectionKey = "moderation-cases";

INodeTypeRepository nodeTypeRepository =
    new SerializedNodeTypeRepository(nodeTypeCollectionKey);

SeedSystemNodeTypes(nodeTypeRepository);

IDocumentRepository documentRepository =
    new SerializedDocumentRepository(
        documentCollectionKey,
        blockCollectionKey);

IParticipantRepository participantRepository =
    new SerializedParticipantRepository(participantCollectionKey);

IVoteRepository voteRepository =
    new SerializedVoteRepository(voteCollectionKey);

IReactionDefinitionRepository tagDefinitionRepository =
    new SerializedReactionDefinitionRepository(tagDefinitionCollectionKey);

INodeReactionRepository nodeTagRepository =
    new SerializedNodeReactionRepository(nodeTagCollectionKey);

ICommunityRepository communityRepository = new SerializedCommunityRepository(communityCollectionKey);
ICommunityMembershipRepository communityMembershipRepository = new SerializedCommunityMembershipRepository(communityMembershipCollectionKey);
ICommunityNodeRepository communityNodeRepository = new SerializedCommunityNodeRepository(communityNodeCollectionKey);
var communityService = new CommunityService(communityRepository, communityMembershipRepository, communityNodeRepository);
ICommentRepository commentRepository = new SerializedCommentRepository(commentCollectionKey);
IModerationCaseRepository moderationCases = new SerializedModerationCaseRepository(moderationCollectionKey);
var moderatorAuthorization = new ConfiguredModeratorAuthorization(
    configuration["ATLAS_MODERATOR_PARTICIPANT_IDS"] is { } configuredIds
        ? configuredIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
        : configuration.GetSection("ATLAS_MODERATOR_PARTICIPANT_IDS")
            .GetChildren().Select(entry => entry.Value ?? string.Empty));
var moderation = new ModerationService(moderationCases, moderatorAuthorization);

var legacyParticipant =
    EnsureDefaultDemoParticipant(participantRepository);

INodeRepository nodeRepository =
    new SerializedNodeRepository(
        nodeCollectionKey,
        nodeTypeRepository,
        documentRepository,
        new NodeAuthorId(legacyParticipant.Id.Value));

var votingEligibility =
    new RepositoryVotingEligibility(
        nodeRepository,
        participantRepository,
        nodeTagRepository);

var voteMutationPolicy =
    new VoteMutationPolicy(votingEligibility);

var castVote =
    new CastVote(
        voteRepository,
        voteMutationPolicy);

var undoVote =
    new UndoVote(
        voteRepository,
        voteMutationPolicy);

IDiscoveryCandidateSource discoverySource = new RepositoryDiscoveryCandidateSource(
    (IDiscoveryNodeReader)nodeRepository,
    documentRepository,
    voteRepository,
    communityNodeRepository,
    nodeTagRepository,
    moderationCases);
IDiscoveryService discovery = new DiscoveryService(discoverySource);

var eventPublisher = new InMemoryEventPublisher();
INotificationRepository notificationRepository = new SerializedNotificationRepository(
    "notifications", "notification-preferences");
var notificationService = new NotificationService(notificationRepository,
    [new SimulatedDelivery(DeliveryChannel.Email), new SimulatedDelivery(DeliveryChannel.Push)]);
eventPublisher.Subscribe<NotificationRequestedV1>(request => notificationService.Handle(request));

var contentSubscriber =
    new ObserveNodeLifecycleInContent(documentRepository);

eventPublisher.Subscribe<NodeCreatedV1>(
    contentSubscriber.Handle);

eventPublisher.Subscribe<NodeArchivedV1>(
    contentSubscriber.Handle);

var application = new ConsoleApplication(
    nodeRepository,
    nodeTypeRepository,
    documentRepository,
    participantRepository,
    voteRepository,
    castVote,
    undoVote,
    tagDefinitionRepository,
    nodeTagRepository,
    eventPublisher,
    nodeCollectionKey,
    nodeTypeCollectionKey,
    documentCollectionKey,
    participantCollectionKey,
    voteCollectionKey,
    tagDefinitionCollectionKey,
    nodeTagCollectionKey,
    communityCollectionKey,
    communityMembershipCollectionKey,
    communityNodeCollectionKey,
    communityRepository,
    communityMembershipRepository,
    communityNodeRepository,
    communityService,
    commentRepository,
    commentCollectionKey,
    discovery,
    legacyParticipant,
    moderation,
    moderatorAuthorization,
    notificationService,
    notificationRepository);

application.Run();

static void SeedSystemNodeTypes(
    INodeTypeRepository nodeTypes)
{
    var existingTypes = nodeTypes.GetAll();
    var createdAt = DateTimeOffset.UtcNow;

    var systemTypes = new[]
    {
        ("Issue", "A problem or concern to investigate.", true),
        ("Question", "A question that invites answers.", true),
        ("Idea", "A proposed concept or possibility.", true),
        ("Solution", "A proposed response to a problem.", true),
        ("Evidence", "Information supporting or challenging a claim.", false),
        ("Relationship", "A connection involving multiple nodes.", true),
        ("Location", "A place associated with another node.", true)
    };

    foreach (var (name, description, autoPluralize) in systemTypes)
    {
        var existingType = existingTypes.SingleOrDefault(type =>
            string.Equals(
                type.Name,
                name,
                StringComparison.OrdinalIgnoreCase));

        if (existingType is not null)
        {
            existingType.ChangeAutoPluralize(
                autoPluralize,
                actorId: "system",
                actorIsModerator: true,
                changedAt: createdAt);

            nodeTypes.Save(existingType);
            continue;
        }

        nodeTypes.Save(
            NodeTypeDefinition.CreateSystemDefined(
                name,
                description,
                createdAt,
                autoPluralize));
    }
}

static Participant EnsureDefaultDemoParticipant(
    IParticipantRepository participants)
{
    var existing = participants
        .GetAll()
        .FirstOrDefault(participant =>
            string.Equals(
                participant.DisplayName,
                "Demo User 01",
                StringComparison.OrdinalIgnoreCase));

    if (existing is not null)
    {
        return existing;
    }

    var participant = new Participant(
        "Demo User 01",
        DateTimeOffset.UtcNow);

    participants.Save(participant);
    return participant;
}
