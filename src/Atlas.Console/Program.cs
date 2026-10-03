using Atlas.Identity;
using Atlas.ConsoleApp.Identity;
using Microsoft.Extensions.DependencyInjection;
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
using var identityServices = new ServiceCollection().AddAtlasIdentity(connectionString)
    .BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
var accountScopes = identityServices.GetRequiredService<IServiceScopeFactory>();

if (args.Contains("--seed-demo", StringComparer.OrdinalIgnoreCase))
{
    await SqlStorage.SeedDemoDataAsync(accountScopes);
    System.Console.WriteLine("Demo accounts and data seeded in SQL Server through Identity registration.");
    System.Console.WriteLine("Demo sign-in: demo01@example.test through demo05@example.test");
    System.Console.WriteLine("Demo password: Atlas_Demo_Only_94!Password");
    return;
}

var nodeCollectionKey = "nodes";

var nodeTypeCollectionKey = "node-types";

var documentCollectionKey = "documents";


var participantCollectionKey = "participants";

var voteCollectionKey = "votes";

var tagDefinitionCollectionKey = "reaction-definitions";

var nodeTagCollectionKey = "node-reactions";

var communityCollectionKey = "communities";
var communityMembershipCollectionKey = "community-memberships";
var communityNodeCollectionKey = "community-nodes";
var commentCollectionKey = "comments";

INodeTypeRepository nodeTypeRepository =
    new SqlNodeTypeRepository();

SeedSystemNodeTypes(nodeTypeRepository);

IDocumentRepository documentRepository =
    new SqlDocumentRepository();

IParticipantRepository participantRepository =
    new SqlParticipantRepository();

IVoteRepository voteRepository =
    new SqlVoteRepository();

IReactionDefinitionRepository tagDefinitionRepository =
    new SqlReactionDefinitionRepository();

INodeReactionRepository nodeTagRepository =
    new SqlNodeReactionRepository();

ICommunityRepository communityRepository = new SqlCommunityRepository();
ICommunityMembershipRepository communityMembershipRepository = new SqlCommunityMembershipRepository();
ICommunityNodeRepository communityNodeRepository = new SqlCommunityNodeRepository();
var communityService = new CommunityService(communityRepository, communityMembershipRepository, communityNodeRepository);
ICommentRepository commentRepository = new SqlCommentRepository();
IModerationCaseRepository moderationCases = new SqlModerationCaseRepository();
if (args.Contains("--confirm-email", StringComparer.OrdinalIgnoreCase))
{
    await ConsoleAccountCommands.ConfirmEmailAsync(accountScopes);
    return;
}
var session = new ConsoleIdentitySession(accountScopes);
IModeratorAuthorization moderatorAuthorization = session;
var moderation = new ModerationService(moderationCases, moderatorAuthorization);

INodeRepository nodeRepository = new SqlNodeRepository();

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
INotificationRepository notificationRepository = new SqlNotificationRepository();
var notificationService = new NotificationService(notificationRepository,
    [new SimulatedDelivery(DeliveryChannel.Email), new SimulatedDelivery(DeliveryChannel.Push)]);
eventPublisher.Subscribe<NotificationRequestedV1>(request => notificationService.Handle(request));

var contentSubscriber =
    new ObserveNodeLifecycleInContent(documentRepository);

eventPublisher.Subscribe<NodeCreatedV1>(
    contentSubscriber.Handle);

eventPublisher.Subscribe<NodeArchivedV1>(
    contentSubscriber.Handle);

while (true)
{
    System.Console.Clear();
    System.Console.WriteLine("ATLAS");
    System.Console.WriteLine("-----");
    System.Console.WriteLine("Viewing as: Anonymous");
    System.Console.WriteLine();
    System.Console.WriteLine("1. Sign in");
    System.Console.WriteLine("2. Register");
    System.Console.WriteLine("3. Browse participants");
    System.Console.WriteLine("4. Discover nodes");
    System.Console.WriteLine("5. Browse communities");
    System.Console.WriteLine("6. List node types");
    System.Console.WriteLine("7. My profile [disabled — sign in required]");
    System.Console.WriteLine("8. Create node [disabled — sign in required]");
    System.Console.WriteLine("9. Create community [disabled — sign in required]");
    System.Console.WriteLine("10. Notifications and preferences [disabled — sign in required]");
    System.Console.WriteLine("11. Developer SQL/content diagnostics [disabled — sign in required]");
    System.Console.WriteLine("0. Exit");
    System.Console.WriteLine();
    System.Console.Write("Selection: ");

    var selection = System.Console.ReadLine();
    if (selection is null or "0") break;

    if (selection == "2")
    {
        await ConsoleAccountCommands.RegisterAsync(accountScopes);
        continue;
    }

    if (selection == "3")
    {
        AnonymousBrowseCommands.BrowseParticipants(
            participantRepository,
            nodeRepository,
            nodeTypeRepository);
        continue;
    }

    if (selection == "4")
    {
        AnonymousBrowseCommands.DiscoverNodes(
            discovery,
            nodeRepository,
            nodeTypeRepository,
            documentRepository,
            participantRepository,
            nodeTagRepository,
            tagDefinitionRepository,
            voteRepository,
            communityRepository,
            communityNodeRepository,
            commentRepository,
            moderation);
        continue;
    }

    if (selection == "5")
    {
        AnonymousBrowseCommands.BrowseCommunities(
            communityRepository,
            communityNodeRepository);
        continue;
    }

    if (selection == "6")
    {
        AnonymousBrowseCommands.ListNodeTypes(nodeTypeRepository);
        continue;
    }

    if (selection is "7" or "8" or "9" or "10" or "11")
    {
        ConsoleUi.Pause("Sign in is required for this action.");
        continue;
    }

    if (selection != "1")
    {
        ConsoleUi.Pause("Please select a listed option.");
        continue;
    }

    if (!await ConsoleAccountCommands.SignInAsync(accountScopes, session))
        continue;

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
        moderation,
        moderatorAuthorization,
        notificationService,
        notificationRepository,
        session,
        () => session.RefreshAsync().GetAwaiter().GetResult());

    application.Run();
    session.SignOut();
    if (application.ExitRequested) break;
}

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
