using Atlas.ConsoleApp.Eventing;
using Atlas.Comments.Comments;
using Atlas.ConsoleApp.Communities;
using Atlas.Communities.Communities;
using Atlas.Communities.Memberships;
using Atlas.Communities.Nodes;
using Atlas.ConsoleApp.Participants;
using Atlas.Content.Documents;
using Atlas.Discovery;
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
    private Participant _currentParticipant;
    private readonly string _nodeDataFilePath;
    private readonly string _nodeTypeDataFilePath;
    private readonly string _documentDataFilePath;
    private readonly string _participantDataFilePath;
    private readonly string _voteDataFilePath;
    private readonly string _tagDefinitionDataFilePath;
    private readonly string _nodeTagDataFilePath;
    private readonly string _communityDataFilePath;
    private readonly string _communityMembershipDataFilePath;
    private readonly string _communityNodeDataFilePath;
    private readonly string _commentDataFilePath;

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
        string nodeDataFilePath,
        string nodeTypeDataFilePath,
        string documentDataFilePath,
        string participantDataFilePath,
        string voteDataFilePath,
        string tagDefinitionDataFilePath,
        string nodeTagDataFilePath,
        string communityDataFilePath,
        string communityMembershipDataFilePath,
        string communityNodeDataFilePath,
        ICommunityRepository communities,
        ICommunityMembershipRepository communityMemberships,
        ICommunityNodeRepository communityNodes,
        CommunityService communityService,
        ICommentRepository comments,
        string commentDataFilePath,
        IDiscoveryService discovery,
        Participant initialParticipant)
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
        _currentParticipant = initialParticipant;
        _nodeDataFilePath = nodeDataFilePath;
        _nodeTypeDataFilePath = nodeTypeDataFilePath;
        _documentDataFilePath = documentDataFilePath;
        _participantDataFilePath = participantDataFilePath;
        _voteDataFilePath = voteDataFilePath;
        _tagDefinitionDataFilePath = tagDefinitionDataFilePath;
        _nodeTagDataFilePath = nodeTagDataFilePath;
        _communityDataFilePath = communityDataFilePath;
        _communityMembershipDataFilePath = communityMembershipDataFilePath;
        _communityNodeDataFilePath = communityNodeDataFilePath;
        _communities = communities;
        _communityMemberships = communityMemberships;
        _communityNodes = communityNodes;
        _communityService = communityService;
        _comments = comments;
        _commentDataFilePath = commentDataFilePath;
        _discovery = discovery;
    }

    /// <summary>Runs the interactive console application workflow.</summary>
    public void Run()
    {
        var running = true;

        while (running)
        {
            Console.Clear();
            WriteMainMenu();

            Console.Write("Selection: ");

            switch (Console.ReadLine())
            {
                case "1":
                    SelectParticipant();
                    break;

                case "2":
                    CreateParticipant();
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
                    ShowDataFiles();
                    break;

                case "10":
                    ListContentDocuments();
                    break;

                case "11":
                    running = false;
                    break;

                default:
                    ConsoleUi.Pause(
                        "Please select an option from 1 through 11.");
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
            $"Current participant: {_currentParticipant.DisplayName}");
        Console.WriteLine();
        Console.WriteLine("1. Select participant");
        Console.WriteLine("2. Create participant");
        Console.WriteLine("3. Browse participants");
        Console.WriteLine("4. Create node");
        Console.WriteLine("5. Discover nodes");
        Console.WriteLine("6. Create community");
        Console.WriteLine("7. Browse communities");
        Console.WriteLine("8. List node types");
        Console.WriteLine("9. Show data files");
        Console.WriteLine("10. List Content documents");
        Console.WriteLine("11. Exit");
        Console.WriteLine();
    }


    /// <summary>Selects participant for the current console workflow.</summary>
    private void SelectParticipant()
    {
        Console.Clear();
        Console.WriteLine("SELECT PARTICIPANT");
        Console.WriteLine("------------------");
        WriteActingAs();

        var participants = _participantRepository
            .GetAll()
            .Where(participant => participant.IsActive)
            .OrderBy(participant => participant.DisplayName)
            .ToList();

        for (var index = 0; index < participants.Count; index++)
        {
            Console.WriteLine(
                $"{index + 1}. {participants[index].DisplayName}");
        }

        Console.WriteLine();
        Console.Write("Selection (0 cancels): ");

        if (!int.TryParse(Console.ReadLine(), out var selection) ||
            selection < 0 ||
            selection > participants.Count)
        {
            ConsoleUi.Pause("That is not a valid selection.");
            return;
        }

        if (selection == 0)
        {
            return;
        }

        _currentParticipant = participants[selection - 1];

        ConsoleUi.Pause(
            $"Current participant: {_currentParticipant.DisplayName}");
    }

    /// <summary>Creates participant during the current workflow.</summary>
    private void CreateParticipant()
    {
        Console.Clear();
        Console.WriteLine("CREATE PARTICIPANT");
        Console.WriteLine("------------------");
        WriteActingAs();
        Console.Write("Display name: ");
        var displayName = Console.ReadLine();
        Console.Write("Short bio (optional): ");
        var bio = Console.ReadLine();

        try
        {
            var participant = new Participant(
                displayName ?? string.Empty,
                bio ?? string.Empty,
                DateTimeOffset.UtcNow);

            _participantRepository.Save(participant);
            _currentParticipant = participant;

            ConsoleUi.Pause(
                $"Created and selected {participant.DisplayName}.");
        }
        catch (ArgumentException exception)
        {
            ConsoleUi.Pause(
                $"Unable to create participant: {exception.Message}");
        }
        catch (InvalidOperationException exception)
        {
            ConsoleUi.Pause(
                $"Unable to create participant: {exception.Message}");
        }
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
        _currentParticipant = ParticipantCommands.Run(
            participantId,
            _participantRepository,
            _nodeRepository,
            _nodeTypeRepository,
            _documentRepository,
            _currentParticipant);
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
            _currentParticipant,
            _eventPublisher);
    }

    /// <summary>Displays Nodes exclusively through ranked Discovery results.</summary>
    private void BrowseNodes()
    {
        var browsing = true;
        string? searchText = null;
        Guid? communityId = null;
        IReadOnlyCollection<Guid> reactionIds = Array.Empty<Guid>();
        int? minimumVoteCount = null;
        int? maximumVoteCount = null;
        double? minimumAverageVote = null;
        double? maximumAverageVote = null;

        while (browsing)
        {
            Console.Clear();
            Console.WriteLine("DISCOVERY");
            Console.WriteLine("---------");
            WriteActingAs();
            Console.WriteLine($"Search: {searchText ?? "(all)"}");
            Console.WriteLine($"Community: {ResolveCommunityFilterName(communityId)}");
            Console.WriteLine($"Reactions: {ResolveReactionFilterNames(reactionIds)}");
            Console.WriteLine($"Vote count: {FormatRange(minimumVoteCount, maximumVoteCount)}");
            Console.WriteLine($"Average vote: {FormatRange(minimumAverageVote, maximumAverageVote)}");
            Console.WriteLine();

            var results = _discovery.Discover(new DiscoveryQuery(
                searchText,
                communityId,
                reactionIds,
                minimumVoteCount,
                maximumVoteCount,
                minimumAverageVote,
                maximumAverageVote));
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
                NodeDisplay.WriteTableHeader();
                var votingParticipantId = new VotingParticipantId(_currentParticipant.Id.Value);
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
                        _communities, _communityNodes, _comments);
                }
            }

            Console.WriteLine();
            Console.WriteLine("Enter a node number to open it, S for text, C for community,");
            Console.WriteLine("R for reactions, V for vote ranges, X to clear filters, or 0 to return.");
            Console.WriteLine();
            Console.Write("Selection: ");
            var input = Console.ReadLine()?.Trim();
            if (string.Equals(input, "s", StringComparison.OrdinalIgnoreCase))
            {
                Console.Write("Search text (blank clears): ");
                searchText = NullIfWhiteSpace(Console.ReadLine());
                continue;
            }
            if (string.Equals(input, "c", StringComparison.OrdinalIgnoreCase))
            {
                communityId = SelectDiscoveryCommunity();
                continue;
            }
            if (string.Equals(input, "r", StringComparison.OrdinalIgnoreCase))
            {
                reactionIds = SelectDiscoveryReactions();
                continue;
            }
            if (string.Equals(input, "v", StringComparison.OrdinalIgnoreCase))
            {
                (minimumVoteCount, maximumVoteCount) = ReadIntegerRange("vote count", minimum: 0);
                (minimumAverageVote, maximumAverageVote) = ReadDoubleRange("average vote", 0, 10);
                continue;
            }
            if (string.Equals(input, "x", StringComparison.OrdinalIgnoreCase))
            {
                searchText = null;
                communityId = null;
                reactionIds = Array.Empty<Guid>();
                minimumVoteCount = null;
                maximumVoteCount = null;
                minimumAverageVote = null;
                maximumAverageVote = null;
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

            _currentParticipant = NodeCommands.Run(
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
                _currentParticipant,
                _communities,
                _communityMemberships,
                _communityNodes,
                _communityService,
                _comments);
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
        var min = int.TryParse(Console.ReadLine(), out var parsedMin) && parsedMin >= minimum ? parsedMin : null;
        Console.Write($"Maximum {label} (blank for none): ");
        var max = int.TryParse(Console.ReadLine(), out var parsedMax) && parsedMax >= minimum ? parsedMax : null;
        if (min.HasValue && max.HasValue && min.Value > max.Value) (min, max) = (max, min);
        return (min, max);
    }

    private static (double? Minimum, double? Maximum) ReadDoubleRange(string label, double minimum, double maximum)
    {
        Console.Write($"Minimum {label} ({minimum}–{maximum}, blank for none): ");
        var min = double.TryParse(Console.ReadLine(), out var parsedMin) && parsedMin >= minimum && parsedMin <= maximum ? parsedMin : null;
        Console.Write($"Maximum {label} ({minimum}–{maximum}, blank for none): ");
        var max = double.TryParse(Console.ReadLine(), out var parsedMax) && parsedMax >= minimum && parsedMax <= maximum ? parsedMax : null;
        if (min.HasValue && max.HasValue && min.Value > max.Value) (min, max) = (max, min);
        return (min, max);
    }

    private static string FormatRange<T>(T? minimum, T? maximum) where T : struct =>
        minimum is null && maximum is null ? "(all)" : $"{minimum?.ToString() ?? "no minimum"} to {maximum?.ToString() ?? "no maximum"}";

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
            var community = _communityService.Create(name, description, _currentParticipant.Id.Value, DateTimeOffset.UtcNow);
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
            _currentParticipant = CommunityCommands.Run(
                communities[selection - 1], _communities, _communityMemberships, _communityNodes, _communityService,
                _nodeRepository, _nodeTypeRepository, _documentRepository, _participantRepository,
                _voteRepository, _castVote, _undoVote, _tagDefinitions, _nodeTags, _eventPublisher, _currentParticipant, _comments);
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

    /// <summary>Displays data files in the console workflow.</summary>
    private void ShowDataFiles()
    {
        ShowDataFile("NODE DATA", _nodeDataFilePath);
        ShowDataFile("NODE TYPE DATA", _nodeTypeDataFilePath);
        ShowDataFile("CONTENT DOCUMENT DATA", _documentDataFilePath);
        ShowDataFile("PARTICIPANT DATA", _participantDataFilePath);
        ShowDataFile("VOTE DATA", _voteDataFilePath);
        ShowDataFile("REACTION DEFINITION DATA", _tagDefinitionDataFilePath);
        ShowDataFile("NODE REACTION DATA", _nodeTagDataFilePath);
        ShowDataFile("COMMUNITY DATA", _communityDataFilePath);
        ShowDataFile("COMMUNITY MEMBERSHIP DATA", _communityMembershipDataFilePath);
        ShowDataFile("COMMUNITY NODE DATA", _communityNodeDataFilePath);
        ShowDataFile("COMMENT DATA", _commentDataFilePath);
    }

    /// <summary>Displays data file in the console workflow.</summary>
    private void ShowDataFile(
        string heading,
        string filePath)
    {
        Console.Clear();
        Console.WriteLine(heading);
        Console.WriteLine(new string('-', heading.Length));
        WriteActingAs();
        Console.WriteLine(filePath);
        Console.WriteLine();

        Console.WriteLine(
            File.Exists(filePath)
                ? File.ReadAllText(filePath)
                : "The data file has not been created yet.");

        ConsoleUi.Pause();
    }

    /// <summary>Makes the simulated authenticated identity visible on Console screens.</summary>
    private void WriteActingAs()
    {
        Console.WriteLine($"Acting as: {_currentParticipant.DisplayName}");
        Console.WriteLine();
    }
}
