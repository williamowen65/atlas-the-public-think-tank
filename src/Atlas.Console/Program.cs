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

var dataDirectory = Path.GetFullPath(
    Path.Combine(
        AppContext.BaseDirectory,
        "..",
        "..",
        "..",
        "..",
        "..",
        "data"));

var nodeDataFilePath = Path.Combine(
    dataDirectory,
    "nodes.json");

var nodeTypeDataFilePath = Path.Combine(
    dataDirectory,
    "node-types.json");

var documentDataFilePath = Path.Combine(
    dataDirectory,
    "documents.json");

var blockDataFilePath = Path.Combine(
    dataDirectory,
    "blocks.json");

var participantDataFilePath = Path.Combine(
    dataDirectory,
    "participants.json");

var voteDataFilePath = Path.Combine(
    dataDirectory,
    "votes.json");

var tagDefinitionDataFilePath = Path.Combine(
    dataDirectory,
    "reaction-definitions.json");

var nodeTagDataFilePath = Path.Combine(
    dataDirectory,
    "node-reactions.json");

var communityDataFilePath = Path.Combine(dataDirectory, "communities.json");
var communityMembershipDataFilePath = Path.Combine(dataDirectory, "community-memberships.json");
var communityNodeDataFilePath = Path.Combine(dataDirectory, "community-nodes.json");
var commentDataFilePath = Path.Combine(dataDirectory, "comments.json");
var moderationDataFilePath = Path.Combine(dataDirectory, "moderation-cases.json");

INodeTypeRepository nodeTypeRepository =
    new JsonNodeTypeRepository(nodeTypeDataFilePath);

SeedSystemNodeTypes(nodeTypeRepository);

IDocumentRepository documentRepository =
    new JsonDocumentRepository(
        documentDataFilePath,
        blockDataFilePath);

IParticipantRepository participantRepository =
    new JsonParticipantRepository(participantDataFilePath);

IVoteRepository voteRepository =
    new JsonVoteRepository(voteDataFilePath);

IReactionDefinitionRepository tagDefinitionRepository =
    new JsonReactionDefinitionRepository(tagDefinitionDataFilePath);

INodeReactionRepository nodeTagRepository =
    new JsonNodeReactionRepository(nodeTagDataFilePath);

ICommunityRepository communityRepository = new JsonCommunityRepository(communityDataFilePath);
ICommunityMembershipRepository communityMembershipRepository = new JsonCommunityMembershipRepository(communityMembershipDataFilePath);
ICommunityNodeRepository communityNodeRepository = new JsonCommunityNodeRepository(communityNodeDataFilePath);
var communityService = new CommunityService(communityRepository, communityMembershipRepository, communityNodeRepository);
ICommentRepository commentRepository = new JsonCommentRepository(commentDataFilePath);
IModerationCaseRepository moderationCases = new JsonModerationCaseRepository(moderationDataFilePath);
var moderatorAuthorization = new ConfiguredModeratorAuthorization(
    configuration["ATLAS_MODERATOR_PARTICIPANT_IDS"] is { } configuredIds
        ? configuredIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
        : configuration.GetSection("ATLAS_MODERATOR_PARTICIPANT_IDS")
            .GetChildren().Select(entry => entry.Value ?? string.Empty));
var moderation = new ModerationService(moderationCases, moderatorAuthorization);

var legacyParticipant =
    EnsureDefaultDemoParticipant(participantRepository);

INodeRepository nodeRepository =
    new JsonNodeRepository(
        nodeDataFilePath,
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
INotificationRepository notificationRepository = new JsonNotificationRepository(
    Path.Combine(dataDirectory, "notifications.json"), Path.Combine(dataDirectory, "notification-preferences.json"));
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
    nodeDataFilePath,
    nodeTypeDataFilePath,
    documentDataFilePath,
    participantDataFilePath,
    voteDataFilePath,
    tagDefinitionDataFilePath,
    nodeTagDataFilePath,
    communityDataFilePath,
    communityMembershipDataFilePath,
    communityNodeDataFilePath,
    communityRepository,
    communityMembershipRepository,
    communityNodeRepository,
    communityService,
    commentRepository,
    commentDataFilePath,
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
