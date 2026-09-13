using Atlas.ConsoleApp.Eventing;
using Atlas.ConsoleApp.Participants;
using Atlas.Content.Documents;
using Atlas.Graph.Nodes;
using Atlas.Graph.Nodes.NodeTypes;
using Atlas.Participants.Participants;
using Atlas.Voting;
using Atlas.Voting.Data;
using Atlas.Voting.Target;

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
    private readonly InMemoryEventPublisher _eventPublisher;
    private Participant _currentParticipant;
    private readonly string _nodeDataFilePath;
    private readonly string _nodeTypeDataFilePath;
    private readonly string _documentDataFilePath;
    private readonly string _participantDataFilePath;
    private readonly string _voteDataFilePath;

    /// <summary>Creates a validated console application instance.</summary>
    public ConsoleApplication(
        INodeRepository nodes,
        INodeTypeRepository nodeTypes,
        IDocumentRepository documents,
        IParticipantRepository participants,
        IVoteRepository votes,
        CastVote castVote,
        InMemoryEventPublisher eventPublisher,
        string nodeDataFilePath,
        string nodeTypeDataFilePath,
        string documentDataFilePath,
        string participantDataFilePath,
        string voteDataFilePath,
        Participant initialParticipant)
    {
        _nodeRepository = nodes;
        _nodeTypeRepository = nodeTypes;
        _documentRepository = documents;
        _participantRepository = participants;
        _voteRepository = votes;
        _castVote = castVote;
        _eventPublisher = eventPublisher;
        _currentParticipant = initialParticipant;
        _nodeDataFilePath = nodeDataFilePath;
        _nodeTypeDataFilePath = nodeTypeDataFilePath;
        _documentDataFilePath = documentDataFilePath;
        _participantDataFilePath = participantDataFilePath;
        _voteDataFilePath = voteDataFilePath;
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
                    ListNodeTypes();
                    break;

                case "7":
                    ShowDataFiles();
                    break;

                case "8":
                    ListContentDocuments();
                    break;

                case "9":
                    running = false;
                    break;

                default:
                    ConsoleUi.Pause(
                        "Please select an option from 1 through 9.");
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
        Console.WriteLine("5. Browse nodes");
        Console.WriteLine("6. List node types");
        Console.WriteLine("7. Show data files");
        Console.WriteLine("8. List Content documents");
        Console.WriteLine("9. Exit");
        Console.WriteLine();
    }


    /// <summary>Selects participant for the current console workflow.</summary>
    private void SelectParticipant()
    {
        Console.Clear();
        Console.WriteLine("SELECT PARTICIPANT");
        Console.WriteLine("------------------");

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

            var participants = _participantRepository
                .GetAll()
                .OrderBy(participant => participant.DisplayName)
                .ToList();

            if (participants.Count == 0)
            {
                ConsoleUi.Pause("No participants have been created.");
                return;
            }

            var nodes = _nodeRepository.GetAll();

            ParticipantDisplay.WriteTableHeader();

            for (var index = 0; index < participants.Count; index++)
            {
                ParticipantDisplay.WriteTableRow(
                    participants[index],
                    nodes,
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

        NodeCreationWorkflow.Create(
            _nodeRepository,
            _nodeTypeRepository,
            _documentRepository,
            _currentParticipant,
            _eventPublisher);
    }

    /// <summary>Displays nodes in the console workflow.</summary>
    private void BrowseNodes()
    {
        var browsing = true;

        while (browsing)
        {
            Console.Clear();
            Console.WriteLine("BROWSE NODES");
            Console.WriteLine("------------");

            var nodes = _nodeRepository.GetAll().ToList();

            if (nodes.Count == 0)
            {
                ConsoleUi.Pause("No nodes have been created.");
                return;
            }

            NodeDisplay.WriteTableHeader();

            for (var index = 0; index < nodes.Count; index++)
            {

                Node node = nodes[index];

                var voteTarget = new NodeVoteTarget(node.Id.Value);
                var votes = _voteRepository.GetTargetVotes(voteTarget);

                var voteCount = votes.Count;
                var averageRating = votes.Count == 0
                    ? (double?)null
                    : votes.Average(vote => vote.Value.Value);

                NodeDisplay.WriteTableRow(
                    node,
                    _nodeRepository,
                    _nodeTypeRepository,
                    _documentRepository,
                    _participantRepository,
                    index + 1,
                    voteCount,
                    averageRating);
            }

            Console.WriteLine();
            Console.WriteLine("Enter a node number to open it.");
            Console.WriteLine("Enter 0 to return to the main menu.");
            Console.WriteLine();
            Console.Write("Selection: ");

            if (!int.TryParse(Console.ReadLine(), out var selection))
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
                _eventPublisher,
                _currentParticipant);
        }
    }

    /// <summary>Displays node types in the console workflow.</summary>
    private void ListNodeTypes()
    {
        Console.Clear();
        Console.WriteLine("NODE TYPES");
        Console.WriteLine("----------");

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
            Console.WriteLine($"Content:     {document.Content}");
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
    }

    /// <summary>Displays data file in the console workflow.</summary>
    private static void ShowDataFile(
        string heading,
        string filePath)
    {
        Console.Clear();
        Console.WriteLine(heading);
        Console.WriteLine(new string('-', heading.Length));
        Console.WriteLine(filePath);
        Console.WriteLine();

        Console.WriteLine(
            File.Exists(filePath)
                ? File.ReadAllText(filePath)
                : "The data file has not been created yet.");

        ConsoleUi.Pause();
    }
}
