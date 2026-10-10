using Atlas.ConsoleApp.Eventing;
using Atlas.ConsoleApp.Notifications;
using Atlas.Identity;
using Atlas.Comments.Comments;
using Atlas.ConsoleApp.Communities;
using Atlas.Communities.Communities;
using Atlas.Communities.Memberships;
using Atlas.Communities.Nodes;
using Atlas.ConsoleApp.Participants;
using Atlas.Content.Documents;
using Atlas.Discovery;
using Atlas.Moderation;
using Atlas.Notifications;
using Atlas.Contracts.Notifications.V1;
using Atlas.Graph.Nodes;
using Atlas.Graph.Nodes.NodeTypes;
using Atlas.Graph.Reactions;
using Atlas.Participants.Participants;
using Atlas.Voting;
using Atlas.Voting.Data;
using Atlas.Voting.Target;
using VotingParticipantId = Atlas.Voting.Votes.ParticipantId;

namespace Atlas.ConsoleApp;

/// <summary>Runs the console host and coordinates user-facing workflows across Atlas boundaries.</summary>
public sealed class ConsoleApplication
{
    private readonly INodeRepository _nodeRepository;
    private readonly INodeTypeRepository _nodeTypeRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly IParticipantRepository _participantRepository;
    private readonly IVoteRepository _voteRepository;
    private readonly CastVote _castVote;
    private readonly UndoVote _undoVote;
    private readonly IReactionDefinitionRepository _tagDefinitions;
    private readonly INodeReactionRepository _nodeTags;
    private readonly InMemoryEventPublisher _eventPublisher;
    private readonly ICommunityRepository _communities;
    private readonly ICommunityMembershipRepository _communityMemberships;
    private readonly ICommunityNodeRepository _communityNodes;
    private readonly CommunityService _communityService;
    private readonly ICommentRepository _comments;
    private readonly IDiscoveryService _discovery;
    private readonly ModerationService _moderation;
    private readonly IModeratorAuthorization _moderatorAuthorization;
    private readonly NotificationService _notificationService;
    private readonly CurrentUserNotifications _currentUserNotifications;
    private readonly INotificationRepository _notificationRepository;
    private readonly IAuthenticatedActor _authenticatedActor;
    private readonly Func<bool>? _validateSession;
    public bool ExitRequested { get; private set; }
    private readonly string _nodeCollectionKey;
    private readonly string _nodeTypeCollectionKey;
    private readonly string _documentCollectionKey;
    private readonly string _participantCollectionKey;
    private readonly string _voteCollectionKey;
    private readonly string _tagDefinitionCollectionKey;
    private readonly string _nodeTagCollectionKey;
    private readonly string _communityCollectionKey;
    private readonly string _communityMembershipCollectionKey;
    private readonly string _communityNodeCollectionKey;
    private readonly string _commentCollectionKey;

    /// <summary>Creates a validated console application instance.</summary>
    public ConsoleApplication(
        INodeRepository nodes,
        INodeTypeRepository nodeTypes,
        IDocumentRepository documents,
        IParticipantRepository participants,
        IVoteRepository votes,
        CastVote castVote,
        UndoVote undoVote,
        IReactionDefinitionRepository tagDefinitions,
        INodeReactionRepository nodeTags,
        InMemoryEventPublisher eventPublisher,
        string nodeCollectionKey,
        string nodeTypeCollectionKey,
        string documentCollectionKey,
        string participantCollectionKey,
        string voteCollectionKey,
        string tagDefinitionCollectionKey,
        string nodeTagCollectionKey,
        string communityCollectionKey,
        string communityMembershipCollectionKey,
        string communityNodeCollectionKey,
        ICommunityRepository communities,
        ICommunityMembershipRepository communityMemberships,
        ICommunityNodeRepository communityNodes,
        CommunityService communityService,
        ICommentRepository comments,
        string commentCollectionKey,
        IDiscoveryService discovery,
        ModerationService moderation,
        IModeratorAuthorization moderatorAuthorization,
        NotificationService notificationService,
        INotificationRepository notificationRepository,
        IAuthenticatedActor authenticatedActor,
        Func<bool>? validateSession = null)
    {
        _nodeRepository = nodes;
        _nodeTypeRepository = nodeTypes;
        _documentRepository = documents;
        _participantRepository = participants;
        _voteRepository = votes;
        _castVote = castVote;
        _undoVote = undoVote;
        _tagDefinitions = tagDefinitions;
        _nodeTags = nodeTags;
        _eventPublisher = eventPublisher;
        _nodeCollectionKey = nodeCollectionKey;
        _nodeTypeCollectionKey = nodeTypeCollectionKey;
        _documentCollectionKey = documentCollectionKey;
        _participantCollectionKey = participantCollectionKey;
        _voteCollectionKey = voteCollectionKey;
        _tagDefinitionCollectionKey = tagDefinitionCollectionKey;
        _nodeTagCollectionKey = nodeTagCollectionKey;
        _communityCollectionKey = communityCollectionKey;
        _communityMembershipCollectionKey = communityMembershipCollectionKey;
        _communityNodeCollectionKey = communityNodeCollectionKey;
        _communities = communities;
        _communityMemberships = communityMemberships;
        _communityNodes = communityNodes;
        _communityService = communityService;
        _comments = comments;
        _commentCollectionKey = commentCollectionKey;
        _discovery = discovery;
        _moderation = moderation;
        _moderatorAuthorization = moderatorAuthorization;
        _notificationService = notificationService;
        _currentUserNotifications = new CurrentUserNotifications(authenticatedActor, notificationService);
        _notificationRepository = notificationRepository;
        _authenticatedActor = authenticatedActor;
        _validateSession = validateSession;
    }

    /// <summary>Runs the interactive console application workflow.</summary>
    public void Run()
    {
        var running = true;

        while (running)
        {
            if (_validateSession is not null && !_validateSession())
            {
                ConsoleUi.Pause("Your session has ended. Please sign in again.");
                return;
            }
            Console.Clear();
            WriteMainMenu();

            Console.Write("Selection: ");

            switch (Console.ReadLine())
            {
                case "1":
                    return;

                case "2":
                    ViewParticipantProfile(new ParticipantId(_authenticatedActor.ParticipantId));
                    break;

                case "3":
                    BrowseParticipants();
                    break;

                case "4":
                    CreateNode();
                    break;

                case "5":
                    BrowseNodes();
                    break;

                case "6":
                    CreateCommunity();
                    break;

                case "7":
                    BrowseCommunities();
                    break;

                case "8":
                    ListNodeTypes();
                    break;

                case "9":
                    ShowSqlCollections();
                    break;

                case "10":
                    ListContentDocuments();
                    break;

                case "11":
                    BrowseNotifications();
                    break;

                case "12":
                    if (_moderatorAuthorization.IsAtlasModerator(_authenticatedActor.ParticipantId))
                        ReviewModerationQueue();
                    else
                        ConsoleUi.Pause("Review Node reports is available to Atlas moderators only.");
                    break;

                case "13":
                    ExitRequested = true;
                    running = false;
                    break;

                default:
                    ConsoleUi.Pause(
                        "Please select a listed option.");
                    break;
            }
        }
    }

    /// <summary>Writes main menu to the console display.</summary>
    private void WriteMainMenu()
    {
        Console.WriteLine("ATLAS");
        Console.WriteLine("-----");
        Console.WriteLine(
            $"Current participant: {AuthenticatedParticipantDisplayName()}");
        Console.WriteLine();
        Console.WriteLine("1. Sign out");
        Console.WriteLine("2. My profile");
        Console.WriteLine("3. Browse participants");
        Console.WriteLine("4. Create node");
        Console.WriteLine("5. Discover nodes");
        Console.WriteLine("6. Create community");
        Console.WriteLine("7. Browse communities");
        Console.WriteLine("8. List node types");
        Console.WriteLine("9. Show SQL data");
        Console.WriteLine("10. List Content documents");
        Console.WriteLine("11. Notifications and preferences");
        if (_moderatorAuthorization.IsAtlasModerator(_authenticatedActor.ParticipantId))
            Console.WriteLine("12. Review Node reports");
        else
            Console.WriteLine("12. Review Node reports [disabled — Atlas moderators only]");
        Console.WriteLine("13. Exit");
        Console.WriteLine();
    }


    /// <summary>Displays participants in the console workflow.</summary>
    private void BrowseParticipants()
    {
        var browsing = true;

        while (browsing)
        {
            Console.Clear();
            Console.WriteLine("BROWSE PARTICIPANTS");
            Console.WriteLine("-------------------");
            WriteActingAs();

            var participants = _participantRepository
                .GetAll()
                .OrderBy(participant => participant.DisplayName)
                .ToList();

            if (participants.Count == 0)
            {
                ConsoleUi.Pause("No participants have been created.");
                return;
            }

            ParticipantDisplay.WriteTableHeader();

            for (var index = 0; index < participants.Count; index++)
            {
                ParticipantDisplay.WriteTableRow(
                    participants[index],
                    _nodeRepository.GetByAuthor(
                        new NodeAuthorId(participants[index].Id.Value)),
                    _nodeTypeRepository,
                    index + 1);
            }

            Console.WriteLine();
            Console.WriteLine("Enter a participant number to view their profile.");
            Console.WriteLine("Enter 0 to return to the main menu.");
            Console.WriteLine();
            Console.Write("Selection: ");

            if (!int.TryParse(Console.ReadLine(), out var selection) ||
                selection < 0 ||
                selection > participants.Count)
            {
                ConsoleUi.Pause("That is not a valid selection.");
                continue;
            }

            if (selection == 0)
            {
                browsing = false;
                continue;
            }

            ViewParticipantProfile(
                participants[selection - 1].Id);
        }
    }

    /// <summary>Displays participant profile in the console workflow.</summary>
    private void ViewParticipantProfile(
        ParticipantId participantId)
    {
        ParticipantCommands.Run(
            participantId,
            _participantRepository,
            _nodeRepository,
            _nodeTypeRepository,
            _documentRepository,
            _authenticatedActor,
            _moderation,
            _discovery,
            node => NodeCommands.Run(node, _nodeRepository, _nodeTypeRepository,
                _documentRepository, _participantRepository, _voteRepository, _castVote,
                _undoVote, _tagDefinitions, _nodeTags, _eventPublisher, _authenticatedActor,
                _communities, _communityMemberships, _communityNodes, _communityService,
                _comments, _moderation),
            (node, number, result) => NodeDisplay.WriteTableRow(node,
                _nodeRepository, _nodeTypeRepository, _documentRepository,
                _participantRepository, number, result?.VoteCount, result?.AverageVote,
                _voteRepository.GetByParticipantAndTarget(
                    new VotingParticipantId(_authenticatedActor.ParticipantId),
                    new NodeVoteTarget(node.Id.Value))?.Value.Value,
                _nodeTags, _tagDefinitions, _voteRepository, _communities, _communityNodes,
                _comments, includeCreatedDate: true, moderation: _moderation),
            ChangeAuthoredNodeFilter);
    }

    private DiscoveryQuery ChangeAuthoredNodeFilter(DiscoveryQuery query, string action)
    {
        switch (action)
        {
            case "S":
                Console.Write("Search text (blank clears): ");
                return query with { SearchText = NullIfWhiteSpace(Console.ReadLine()) };
            case "C": return query with { CommunityId = SelectDiscoveryCommunity() };
            case "R": return query with { ReactionDefinitionIds = SelectDiscoveryReactions() };
            case "V":
                var (minimumCount, maximumCount) = ReadIntegerRange("vote count", 0);
                var (minimumAverage, maximumAverage) = ReadDoubleRange("average vote", 0, 10);
                return query with { MinimumVoteCount = minimumCount, MaximumVoteCount = maximumCount,
                    MinimumAverageVote = minimumAverage, MaximumAverageVote = maximumAverage };
            case "D":
                var (from, through) = ReadDateRange();
                return query with { CreatedFrom = from, CreatedThrough = through };
            case "X": return new DiscoveryQuery(IncludeArchived: true);
            default: return query;
        }
    }

    /// <summary>Creates node during the current workflow.</summary>
    private void CreateNode()
    {
        Console.Clear();
        Console.WriteLine("CREATE NODE");
        Console.WriteLine("-----------");
        WriteActingAs();

        NodeCreationWorkflow.Create(
            _nodeRepository,
            _nodeTypeRepository,
            _documentRepository,
            _authenticatedActor.ParticipantId,
            _eventPublisher);
    }

    /// <summary>Displays Nodes exclusively through ranked Discovery results.</summary>
    private void BrowseNodes()
    {
        var browsing = true;
        var visiblePages = 1;
        string? searchText = null;
        Guid? communityId = null;
        Guid? nodeTypeId = null;
        Guid? authorId = null;
        bool? isArchived = false;
        IReadOnlyCollection<Guid> reactionIds = Array.Empty<Guid>();
        int? minimumVoteCount = null;
        int? maximumVoteCount = null;
        double? minimumAverageVote = null;
        double? maximumAverageVote = null;
        DateOnly? createdFrom = null;
        DateOnly? createdThrough = null;

        while (browsing)
        {
            Console.Clear();
            Console.WriteLine("DISCOVERY");
            Console.WriteLine("---------");
            WriteActingAs();
            Console.WriteLine($"Search: {searchText ?? "(all)"}");
            Console.WriteLine($"Community: {ResolveCommunityFilterName(communityId)}");
            Console.WriteLine($"Node type: {(nodeTypeId is null ? "(all)" : _nodeTypeRepository.GetById(new NodeTypeId(nodeTypeId.Value))?.Name ?? "(unknown)")}");
            Console.WriteLine($"Author: {(authorId is null ? "(all)" : _participantRepository.GetById(new ParticipantId(authorId.Value))?.DisplayName ?? "(unknown)")}");
            Console.WriteLine($"Status: {(isArchived is null ? "All" : isArchived.Value ? "Archived" : "Active")}");
            Console.WriteLine($"Reactions: {ResolveReactionFilterNames(reactionIds)}");
            Console.WriteLine($"Vote count: {FormatRange(minimumVoteCount, maximumVoteCount)}");
            Console.WriteLine($"Average vote: {FormatRange(minimumAverageVote, maximumAverageVote)}");
            Console.WriteLine($"Created: {FormatDateRange(createdFrom, createdThrough)}");
            Console.WriteLine();

            var query = new DiscoveryQuery(
                SearchText: searchText,
                CommunityId: communityId,
                NodeTypeId: nodeTypeId,
                AuthorParticipantId: authorId,
                IsArchived: isArchived,
                ReactionDefinitionIds: reactionIds,
                minimumVoteCount,
                maximumVoteCount,
                minimumAverageVote,
                maximumAverageVote,
                createdFrom,
                createdThrough);
            var pages = Enumerable.Range(1, visiblePages)
                .Select(page => _discovery.DiscoverPage(query with { Page = page }))
                .ToList();
            var results = pages.SelectMany(page => page.Items).ToList();
            var hasMore = pages[^1].HasNextPage;
            var nodes = results
                .Select(result => _nodeRepository.GetById(new NodeId(result.NodeId)))
                .Where(node => node is not null)
                .Cast<Node>()
                .ToList();

            if (nodes.Count == 0)
            {
                Console.WriteLine("No nodes match the current Discovery filters.");
            }
            else
            {
                NodeDisplay.WriteTableHeader(includeCreatedDate: true);
                var votingParticipantId = new VotingParticipantId(_authenticatedActor.ParticipantId);
                for (var index = 0; index < nodes.Count; index++)
                {
                    var result = results[index];
                    var myVote = _voteRepository.GetByParticipantAndTarget(
                        votingParticipantId,
                        new NodeVoteTarget(result.NodeId))?.Value.Value;
                    NodeDisplay.WriteTableRow(
                        nodes[index], _nodeRepository, _nodeTypeRepository,
                        _documentRepository, _participantRepository, index + 1,
                        result.VoteCount, result.AverageVote, myVote,
                        _nodeTags, _tagDefinitions, _voteRepository,
                        _communities, _communityNodes, _comments,
                        includeCreatedDate: true, moderation: _moderation);
                }
            }

            Console.WriteLine();
            Console.WriteLine($"Showing {nodes.Count} of {pages[^1].TotalCount} matching nodes.");
            Console.WriteLine("Enter a node number to open it, P<number> to report that Node, S text, C community, T node type, A author,");
            Console.WriteLine("U status, R reactions, V vote ranges, D created date, X clear, or 0 to return.");
            if (hasMore) Console.WriteLine("N to load the next page.");
            Console.WriteLine();
            Console.Write("Selection: ");
            var input = Console.ReadLine()?.Trim();
            if (string.Equals(input, "n", StringComparison.OrdinalIgnoreCase) && hasMore)
            {
                visiblePages++;
                continue;
            }
            if (input?.StartsWith("p", StringComparison.OrdinalIgnoreCase) == true &&
                int.TryParse(input[1..], out var reportNumber))
            {
                if (reportNumber < 1 || reportNumber > nodes.Count)
                    ConsoleUi.Pause("That node does not exist.");
                else
                    NodeCommands.ReportNode(nodes[reportNumber - 1], _authenticatedActor.ParticipantId, _moderation);
                continue;
            }
            if (string.Equals(input, "s", StringComparison.OrdinalIgnoreCase))
            {
                visiblePages = 1;
                Console.Write("Search text (blank clears): ");
                searchText = NullIfWhiteSpace(Console.ReadLine());
                continue;
            }
            if (string.Equals(input, "c", StringComparison.OrdinalIgnoreCase))
            {
                visiblePages = 1;
                communityId = SelectDiscoveryCommunity();
                continue;
            }
            if (string.Equals(input, "t", StringComparison.OrdinalIgnoreCase))
            {
                visiblePages = 1;
                var types = _nodeTypeRepository.GetAll().Where(type => !type.IsArchived).OrderBy(type => type.Name).ToList();
                for (var index = 0; index < types.Count; index++) Console.WriteLine($"{index + 1}. {types[index].Name}");
                Console.Write("Node type (0 clears): ");
                nodeTypeId = int.TryParse(Console.ReadLine(), out var selected) && selected >= 1 && selected <= types.Count
                    ? types[selected - 1].Id.Value : null;
                continue;
            }
            if (string.Equals(input, "a", StringComparison.OrdinalIgnoreCase))
            {
                visiblePages = 1;
                var authors = _participantRepository.GetAll().OrderBy(participant => participant.DisplayName).ToList();
                for (var index = 0; index < authors.Count; index++) Console.WriteLine($"{index + 1}. {authors[index].DisplayName}");
                Console.Write("Author (0 clears): ");
                authorId = int.TryParse(Console.ReadLine(), out var selected) && selected >= 1 && selected <= authors.Count
                    ? authors[selected - 1].Id.Value : null;
                continue;
            }
            if (string.Equals(input, "u", StringComparison.OrdinalIgnoreCase))
            {
                visiblePages = 1;
                Console.Write("Status: 1 Active, 2 Archived, 3 All: ");
                isArchived = Console.ReadLine() switch { "1" => false, "2" => true, "3" => null, _ => isArchived };
                continue;
            }
            if (string.Equals(input, "r", StringComparison.OrdinalIgnoreCase))
            {
                visiblePages = 1;
                reactionIds = SelectDiscoveryReactions();
                continue;
            }
            if (string.Equals(input, "v", StringComparison.OrdinalIgnoreCase))
            {
                visiblePages = 1;
                (minimumVoteCount, maximumVoteCount) = ReadIntegerRange("vote count", minimum: 0);
                (minimumAverageVote, maximumAverageVote) = ReadDoubleRange("average vote", 0, 10);
                continue;
            }
            if (string.Equals(input, "d", StringComparison.OrdinalIgnoreCase))
            {
                visiblePages = 1;
                (createdFrom, createdThrough) = ReadDateRange();
                continue;
            }
            if (string.Equals(input, "x", StringComparison.OrdinalIgnoreCase))
            {
                visiblePages = 1;
                searchText = null;
                communityId = null;
                nodeTypeId = null;
                authorId = null;
                isArchived = false;
                reactionIds = Array.Empty<Guid>();
                minimumVoteCount = null;
                maximumVoteCount = null;
                minimumAverageVote = null;
                maximumAverageVote = null;
                createdFrom = null;
                createdThrough = null;
                continue;
            }
            if (!int.TryParse(input, out var selection))
            {
                ConsoleUi.Pause("That is not a valid selection.");
                continue;
            }

            if (selection == 0)
            {
                browsing = false;
                continue;
            }

            if (selection < 1 || selection > nodes.Count)
            {
                ConsoleUi.Pause("That node does not exist.");
                continue;
            }

            NodeCommands.Run(
                nodes[selection - 1],
                _nodeRepository,
                _nodeTypeRepository,
                _documentRepository,
                _participantRepository,
                _voteRepository,
                _castVote,
                _undoVote,
                _tagDefinitions,
                _nodeTags,
                _eventPublisher,
                _authenticatedActor,
                _communities,
                _communityMemberships,
                _communityNodes,
                _communityService,
                _comments,
                _moderation);
        }
    }

    private void BrowseNotifications()
    {
        var offset = 0;
        const int pageSize = 10;
        while (true)
        {
            Console.Clear();
            WriteActingAs();
            Console.WriteLine("NOTIFICATIONS");
            var page = _currentUserNotifications.Page(offset, pageSize);
            for (var i = 0; i < page.Count; i++)
            {
                var item = page[i];
                Console.WriteLine($"{i + 1}. {(item.ReadAt is null ? "[new]" : "[read]")} {item.Kind} " +
                    $"({item.SubjectKind} {item.SubjectId}) — {item.CreatedAt:g}");
                foreach (var attempt in item.DeliveryAttempts)
                    Console.WriteLine($"   {attempt.Channel}: {attempt.Status}" +
                        (attempt.Error is null ? string.Empty : $" ({attempt.Error})"));
            }
            if (page.Count == 0) Console.WriteLine("No notifications on this page.");
            Console.Write("Number = mark read, D<number> = dismiss, N = next, P = previous, S = settings, 0 = return: ");
            var input = Console.ReadLine()?.Trim().ToUpperInvariant();
            if (input == "0") return;
            if (input == "N" && page.Count == pageSize) { offset += pageSize; continue; }
            if (input == "P") { offset = Math.Max(0, offset - pageSize); continue; }
            if (input == "S") { ConfigureNotifications(); continue; }
            var dismiss = input?.StartsWith('D') == true;
            var number = dismiss ? input![1..] : input;
            if (!int.TryParse(number, out var selection) || selection < 1 || selection > page.Count) continue;
            if (dismiss) _currentUserNotifications.Dismiss(page[selection - 1].Id);
            else _currentUserNotifications.MarkRead(page[selection - 1].Id);
        }
    }

    private void ConfigureNotifications()
    {
        var settings = _currentUserNotifications.Preferences();
        var choices = new (string Label, Func<bool> Get, Action<bool> Set)[]
        {
            ("Discussion in app", () => settings.DiscussionInApp, value => settings.DiscussionInApp = value),
            ("Moderation in app", () => settings.ModerationInApp, value => settings.ModerationInApp = value),
            ("Discussion email simulation", () => settings.DiscussionEmail, value => settings.DiscussionEmail = value),
            ("Moderation email simulation", () => settings.ModerationEmail, value => settings.ModerationEmail = value),
            ("Discussion push simulation", () => settings.DiscussionPush, value => settings.DiscussionPush = value),
            ("Moderation push simulation", () => settings.ModerationPush, value => settings.ModerationPush = value)
        };
        while (true)
        {
            Console.Clear();
            WriteActingAs();
            for (var i = 0; i < choices.Length; i++)
                Console.WriteLine($"{i + 1}. [{(choices[i].Get() ? 'x' : ' ')}] {choices[i].Label}");
            Console.Write("Toggle number (0 returns): ");
            if (!int.TryParse(Console.ReadLine(), out var selection) || selection == 0) return;
            if (selection < 1 || selection > choices.Length) continue;
            var choice = choices[selection - 1];
            choice.Set(!choice.Get());
            _currentUserNotifications.SavePreferences(settings);
        }
    }

    private void ReviewModerationQueue()
    {
        try
        {
            var queue = _moderation.NodeQueue(_authenticatedActor.ParticipantId);
            Console.Clear();
            Console.WriteLine("NODE REPORTS");
            Console.WriteLine("------------");
            if (queue.Count == 0) { ConsoleUi.Pause("No reports awaiting review."); return; }
            for (var index = 0; index < queue.Count; index++)
                Console.WriteLine($"{index + 1}. {queue[index].ReportedTitle} — {queue[index].PendingReports.Count} pending report(s)" +
                    (queue[index].ReviewRequested ? ", author review requested" : string.Empty));
            Console.Write("Node number (0 returns): ");
            if (!int.TryParse(Console.ReadLine(), out var selection) || selection < 1 || selection > queue.Count)
                return;
            var group = queue[selection - 1];
            while (true)
            {
                Console.Clear();
                var reports = _moderation.NodeHistory(_authenticatedActor.ParticipantId, group.NodeId);
                Console.WriteLine($"Reported title: {group.ReportedTitle}");
                Console.WriteLine($"{reports.Count} total report(s), {reports.Count(report => report.Status == ModerationStatus.Submitted)} pending");
                for (var index = 0; index < reports.Count; index++)
                {
                    var report = reports[index];
                    Console.WriteLine($"\nReport {index + 1}: {report.Reason} [{report.Status}]");
                    Console.WriteLine($"Explanation: {report.Explanation}");
                    Console.WriteLine($"Reporter: {report.ReporterId}; submitted: {report.CreatedAt:u}");
                    if (report.Status != ModerationStatus.Submitted)
                        Console.WriteLine($"Decision: {report.DecisionReason}; reviewer: {report.ReviewerId}; decided: {report.DecidedAt:u}");
                    if (report.VisibilityRestoredAt is not null)
                        Console.WriteLine($"Restored: {report.RestorationReason}; moderator: {report.RestoredBy}; at: {report.VisibilityRestoredAt:u}");
                }
                var reviewRequestedAt = reports
                    .Where(report => report.IsHidden && report.ReviewRequestedAt is not null)
                    .Max(report => report.ReviewRequestedAt);
                if (reviewRequestedAt is not null)
                {
                    Console.WriteLine();
                    Console.WriteLine("AUTHOR REVIEW REQUEST");
                    Console.WriteLine($"The author edited this hidden Node and requested review for restoration at {reviewRequestedAt:u}.");
                    Console.WriteLine("Open the Node and select 22 to inspect the revised title and description before restoring it.");
                }
                var node = _nodeRepository.GetById(new NodeId(group.NodeId));
                Console.WriteLine($"Current Node: {(node is null ? "unavailable" : NodeDisplay.PublicTitle(node, _moderation))}");
                Console.Write("V = view Node, D = dismiss reports, H = hide Node, R = restore after review, other = cancel: ");
                var action = Console.ReadLine()?.Trim().ToUpperInvariant();
                if (action == "V")
                {
                    if (node is null) { ConsoleUi.Pause("Node is unavailable."); continue; }
                    NodeCommands.Run(
                        node, _nodeRepository, _nodeTypeRepository, _documentRepository,
                        _participantRepository, _voteRepository, _castVote, _undoVote,
                        _tagDefinitions, _nodeTags, _eventPublisher, _authenticatedActor,
                        _communities, _communityMemberships, _communityNodes,
                        _communityService, _comments, _moderation);
                    continue;
                }
                if (action == "R")
                {
                    Console.Write("Restoration rationale: ");
                    _moderation.RestoreNode(_authenticatedActor.ParticipantId, group.NodeId,
                        Console.ReadLine() ?? string.Empty, AtlasTime.UtcNow);
                    if (node is not null)
                        _eventPublisher.Publish(new NotificationRequestedV1(Guid.NewGuid(), node.AuthorId.Value,
                            _authenticatedActor.ParticipantId, "NodeRestored", "Node", node.Id.Value, AtlasTime.UtcNow));
                    ConsoleUi.Pause("Node visibility restored after author review.");
                    return;
                }
                if (action is not ("D" or "H")) return;
                if (group.PendingReports.Count == 0) { ConsoleUi.Pause("No pending reports to decide."); return; }
                if (action == "H" && node is null) { ConsoleUi.Pause("Node is unavailable; no action taken."); return; }
                var publicReason = PublicModerationReason.Other;
                if (action == "H")
                {
                    Console.WriteLine("Public reason: 1 Spam, 2 Harassment, 3 Unsafe content, 4 Off topic, 5 Other");
                    Console.Write("Reason number: ");
                    publicReason = Console.ReadLine()?.Trim() switch
                    {
                        "1" => PublicModerationReason.Spam,
                        "2" => PublicModerationReason.Harassment,
                        "3" => PublicModerationReason.UnsafeContent,
                        "4" => PublicModerationReason.OffTopic,
                        _ => PublicModerationReason.Other
                    };
                }
                Console.Write("Decision rationale: ");
                var rationale = Console.ReadLine() ?? string.Empty;
                var decision = action == "H" ? ModerationDecision.HideNode : ModerationDecision.Dismiss;
                var decided = _moderation.DecideNode(_authenticatedActor.ParticipantId, group.NodeId, decision,
                    rationale, AtlasTime.UtcNow, publicReason);
                foreach (var report in decided)
                    _eventPublisher.Publish(new NotificationRequestedV1(report.Id, report.ReporterId,
                        _authenticatedActor.ParticipantId, "ReportDecided", "Node", group.NodeId, AtlasTime.UtcNow));
                if (node is not null && action == "H")
                    _eventPublisher.Publish(new NotificationRequestedV1(Guid.NewGuid(), node.AuthorId.Value,
                        _authenticatedActor.ParticipantId, "NodeHidden", "Node", node.Id.Value, AtlasTime.UtcNow));
                ConsoleUi.Pause(action == "H"
                    ? $"Node hidden on public surfaces; {decided.Count} pending report(s) closed."
                    : $"{decided.Count} pending report(s) dismissed.");
                return;
            }
        }
        catch (Exception error) when (error is ArgumentException or UnauthorizedAccessException or InvalidOperationException)
        {
            ConsoleUi.Pause($"Unable to decide report: {error.Message}");
        }
    }

    private Guid? SelectDiscoveryCommunity()
    {
        var communities = _communities.GetAll().OrderBy(community => community.Name).ToList();
        Console.WriteLine("0. All communities");
        for (var index = 0; index < communities.Count; index++)
            Console.WriteLine($"{index + 1}. {communities[index].Name}");
        Console.Write("Community: ");
        return int.TryParse(Console.ReadLine(), out var selection) &&
               selection > 0 && selection <= communities.Count
            ? communities[selection - 1].Id.Value
            : null;
    }

    private string ResolveCommunityFilterName(Guid? id) => id is null
        ? "(all)"
        : _communities.GetById(new CommunityId(id.Value))?.Name ?? "(unavailable)";

    private IReadOnlyCollection<Guid> SelectDiscoveryReactions()
    {
        var definitions = _tagDefinitions.GetAll()
            .Where(definition => !definition.IsSuppressed)
            .OrderBy(definition => definition.Text)
            .ToList();
        Console.WriteLine("Select one or more reactions separated by commas. Matching Nodes must contain all selected reactions.");
        Console.WriteLine("0. All reactions");
        for (var index = 0; index < definitions.Count; index++)
            Console.WriteLine($"{index + 1}. {definitions[index].Emoji} {definitions[index].Text}");
        Console.Write("Reactions: ");
        var selections = (Console.ReadLine() ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(value => int.TryParse(value, out var number) ? number : -1)
            .Where(number => number > 0 && number <= definitions.Count)
            .Distinct()
            .ToList();
        return selections.Select(number => definitions[number - 1].Id.Value).ToList();
    }

    private string ResolveReactionFilterNames(IReadOnlyCollection<Guid> ids)
    {
        if (ids.Count == 0) return "(all)";
        return string.Join(" + ", ids.Select(id =>
            _tagDefinitions.GetById(new ReactionDefinitionId(id))?.Text ?? "(unavailable)"));
    }

    private static (int? Minimum, int? Maximum) ReadIntegerRange(string label, int minimum)
    {
        Console.Write($"Minimum {label} (blank for none): ");
        int? min = int.TryParse(Console.ReadLine(), out var parsedMin) && parsedMin >= minimum ? parsedMin : null;
        Console.Write($"Maximum {label} (blank for none): ");
        int? max = int.TryParse(Console.ReadLine(), out var parsedMax) && parsedMax >= minimum ? parsedMax : null;
        if (min.HasValue && max.HasValue && min.Value > max.Value) (min, max) = (max, min);
        return (min, max);
    }

    private static (double? Minimum, double? Maximum) ReadDoubleRange(string label, double minimum, double maximum)
    {
        Console.Write($"Minimum {label} ({minimum}–{maximum}, blank for none): ");
        double? min = double.TryParse(Console.ReadLine(), out var parsedMin) && parsedMin >= minimum && parsedMin <= maximum ? parsedMin : null;
        Console.Write($"Maximum {label} ({minimum}–{maximum}, blank for none): ");
        double? max = double.TryParse(Console.ReadLine(), out var parsedMax) && parsedMax >= minimum && parsedMax <= maximum ? parsedMax : null;
        if (min.HasValue && max.HasValue && min.Value > max.Value) (min, max) = (max, min);
        return (min, max);
    }

    private static string FormatRange<T>(T? minimum, T? maximum) where T : struct =>
        minimum is null && maximum is null ? "(all)" : $"{minimum?.ToString() ?? "no minimum"} to {maximum?.ToString() ?? "no maximum"}";

    private static (DateOnly? From, DateOnly? Through) ReadDateRange()
    {
        Console.Write("Created from (YYYY-MM-DD, blank for none): ");
        DateOnly? from = DateOnly.TryParse(Console.ReadLine(), out var parsedFrom) ? parsedFrom : null;
        Console.Write("Created through (YYYY-MM-DD, blank for none): ");
        DateOnly? through = DateOnly.TryParse(Console.ReadLine(), out var parsedThrough) ? parsedThrough : null;
        if (from.HasValue && through.HasValue && from.Value > through.Value)
            (from, through) = (through, from);
        return (from, through);
    }

    private static string FormatDateRange(DateOnly? from, DateOnly? through) =>
        from is null && through is null
            ? "(all)"
            : $"{from?.ToString("yyyy-MM-dd") ?? "no start"} to {through?.ToString("yyyy-MM-dd") ?? "no end"}";

    private static string? NullIfWhiteSpace(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private void CreateCommunity()
    {
        Console.Clear();
        Console.WriteLine("CREATE COMMUNITY");
        Console.WriteLine("----------------");
        WriteActingAs();
        Console.Write("Name: ");
        var name = Console.ReadLine() ?? string.Empty;
        Console.Write("Description: ");
        var description = Console.ReadLine() ?? string.Empty;

        try
        {
            var community = _communityService.Create(name, description, _authenticatedActor.ParticipantId, AtlasTime.UtcNow);
            ConsoleUi.Pause($"Created {community.Name}. You are its owner and first member.");
        }
        catch (ArgumentException exception) { ConsoleUi.Pause(exception.Message); }
        catch (InvalidOperationException exception) { ConsoleUi.Pause(exception.Message); }
    }

    private void BrowseCommunities()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("BROWSE COMMUNITIES");
            Console.WriteLine("------------------");
            WriteActingAs();
            var communities = _communities.GetAll().OrderBy(x => x.Name).ToList();
            if (communities.Count == 0) { ConsoleUi.Pause("No communities have been created."); return; }
            CommunityDisplay.WriteTableHeader();
            for (var index = 0; index < communities.Count; index++)
                CommunityDisplay.WriteTableRow(communities[index], _participantRepository, _communityMemberships, _communityNodes, index + 1);
            Console.WriteLine();
            Console.Write("Community number (0 returns): ");
            if (!int.TryParse(Console.ReadLine(), out var selection) || selection < 0 || selection > communities.Count)
            { ConsoleUi.Pause("That is not a valid selection."); continue; }
            if (selection == 0) return;
            CommunityCommands.Run(
                communities[selection - 1], _communities, _communityMemberships, _communityNodes, _communityService,
                _nodeRepository, _nodeTypeRepository, _documentRepository, _participantRepository,
                _voteRepository, _castVote, _undoVote, _tagDefinitions, _nodeTags, _eventPublisher, _authenticatedActor, _comments, _moderation);
        }
    }

    /// <summary>Displays node types in the console workflow.</summary>
    private void ListNodeTypes()
    {
        Console.Clear();
        Console.WriteLine("NODE TYPES");
        Console.WriteLine("----------");
        WriteActingAs();

        var nodeTypes = _nodeTypeRepository
            .GetAll()
            .OrderBy(type => type.Name)
            .ToList();

        foreach (var nodeType in nodeTypes)
        {
            var kind = nodeType.IsSystemDefined
                ? "system"
                : $"custom, owner: {nodeType.OwnerId}";

            var status = nodeType.IsArchived
                ? "archived"
                : "active";

            Console.WriteLine(
                $"- {nodeType.Name} ({kind}, {status})");

            if (!string.IsNullOrWhiteSpace(nodeType.Description))
            {
                Console.WriteLine($"  {nodeType.Description}");
            }

            Console.WriteLine($"  ID: {nodeType.Id}");
        }

        ConsoleUi.Pause();
    }

    /// <summary>Displays content documents in the console workflow.</summary>
    private void ListContentDocuments()
    {
        Console.Clear();
        Console.WriteLine("ATLAS.CONTENT DOCUMENTS");
        Console.WriteLine("-----------------------");
        WriteActingAs();

        var documents = _documentRepository.GetAll();

        if (documents.Count == 0)
        {
            Console.WriteLine(
                "No Content documents have been created.");
            Console.WriteLine();
            Console.WriteLine(
                "Create a node to create a Content document.");
            ConsoleUi.Pause();
            return;
        }

        foreach (var document in documents)
        {
            Console.WriteLine($"Document ID: {document.Id}");
            Console.WriteLine($"Created:     {document.CreatedAt.LocalDateTime}");
            Console.WriteLine($"Updated:     {document.UpdatedAt.LocalDateTime}");
            Console.WriteLine($"Blocks:      {document.BlockIds.Count}");
            Console.WriteLine();
        }

        ConsoleUi.Pause();
    }

    /// <summary>Displays SQL rowss in the console workflow.</summary>
    private void ShowSqlCollections()
    {
        ShowSqlCollection("NODE DATA", _nodeCollectionKey);
        ShowSqlCollection("NODE TYPE DATA", _nodeTypeCollectionKey);
        ShowSqlCollection("CONTENT DOCUMENT DATA", _documentCollectionKey);
        ShowSqlCollection("PARTICIPANT DATA", _participantCollectionKey);
        ShowSqlCollection("VOTE DATA", _voteCollectionKey);
        ShowSqlCollection("REACTION DEFINITION DATA", _tagDefinitionCollectionKey);
        ShowSqlCollection("NODE REACTION DATA", _nodeTagCollectionKey);
        ShowSqlCollection("COMMUNITY DATA", _communityCollectionKey);
        ShowSqlCollection("COMMUNITY MEMBERSHIP DATA", _communityMembershipCollectionKey);
        ShowSqlCollection("COMMUNITY NODE DATA", _communityNodeCollectionKey);
        ShowSqlCollection("COMMENT DATA", _commentCollectionKey);
    }

    /// <summary>Displays SQL rows in the console workflow.</summary>
    private void ShowSqlCollection(
        string heading,
        string collectionKey)
    {
        Console.Clear();
        Console.WriteLine(heading);
        Console.WriteLine(new string('-', heading.Length));
        WriteActingAs();
        Console.WriteLine(collectionKey);
        Console.WriteLine();

        Console.WriteLine(
            Storage.SqlStorage.DescribeCollection(collectionKey));

        ConsoleUi.Pause();
    }

    private string AuthenticatedParticipantDisplayName() =>
        _participantRepository
            .GetById(new ParticipantId(_authenticatedActor.ParticipantId))
            ?.DisplayName
        ?? "authenticated participant";

    /// <summary>Makes the authenticated identity visible on Console screens.</summary>
    private void WriteActingAs()
    {
        Console.WriteLine($"Acting as: {AuthenticatedParticipantDisplayName()}");
        Console.WriteLine();
    }
}
