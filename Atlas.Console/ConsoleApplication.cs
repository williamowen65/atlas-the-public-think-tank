using Atlas.ConsoleApp.Eventing;
using Atlas.Content.Documents;
using Atlas.Graph;
using Atlas.Graph.Nodes;
using Atlas.Graph.Nodes.NodeTypes;
using Atlas.Participants.Participants;

namespace Atlas.ConsoleApp;

public sealed class ConsoleApplication
{
    private readonly INodeRepository _nodes;
    private readonly INodeTypeRepository _nodeTypes;
    private readonly IDocumentRepository _documents;
    private readonly IParticipantRepository _participants;
    private readonly InMemoryEventPublisher _eventPublisher;
    private Participant _currentParticipant;
    private readonly string _nodeDataFilePath;
    private readonly string _nodeTypeDataFilePath;
    private readonly string _documentDataFilePath;
    private readonly string _participantDataFilePath;

    public ConsoleApplication(
        INodeRepository nodes,
        INodeTypeRepository nodeTypes,
        IDocumentRepository documents,
        IParticipantRepository participants,
        InMemoryEventPublisher eventPublisher,
        string nodeDataFilePath,
        string nodeTypeDataFilePath,
        string documentDataFilePath,
        string participantDataFilePath,
        Participant initialParticipant)
    {
        _nodes = nodes;
        _nodeTypes = nodeTypes;
        _documents = documents;
        _participants = participants;
        _eventPublisher = eventPublisher;
        _currentParticipant = initialParticipant;
        _nodeDataFilePath = nodeDataFilePath;
        _nodeTypeDataFilePath = nodeTypeDataFilePath;
        _documentDataFilePath = documentDataFilePath;
        _participantDataFilePath = participantDataFilePath;
    }

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
                    CreateNode();
                    break;

                case "4":
                    BrowseNodes();
                    break;

                case "5":
                    ListNodeTypes();
                    break;

                case "6":
                    ShowDataFiles();
                    break;

                case "7":
                    ListContentDocuments();
                    break;

                case "8":
                    running = false;
                    break;

                default:
                    ConsoleUi.Pause(
                        "Please select an option from 1 through 8.");
                    break;
            }
        }
    }

    private void WriteMainMenu()
    {
        Console.WriteLine("ATLAS");
        Console.WriteLine("-----");
        Console.WriteLine(
            $"Current participant: {_currentParticipant.DisplayName}");
        Console.WriteLine();
        Console.WriteLine("1. Select participant");
        Console.WriteLine("2. Create participant");
        Console.WriteLine("3. Create node");
        Console.WriteLine("4. Browse nodes");
        Console.WriteLine("5. List node types");
        Console.WriteLine("6. Show data files");
        Console.WriteLine("7. List Content documents");
        Console.WriteLine("8. Exit");
        Console.WriteLine();
    }


    private void SelectParticipant()
    {
        Console.Clear();
        Console.WriteLine("SELECT PARTICIPANT");
        Console.WriteLine("------------------");

        var participants = _participants
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

    private void CreateParticipant()
    {
        Console.Clear();
        Console.WriteLine("CREATE PARTICIPANT");
        Console.WriteLine("------------------");
        Console.Write("Display name: ");
        var displayName = Console.ReadLine();

        try
        {
            var participant = new Participant(
                displayName ?? string.Empty,
                DateTimeOffset.UtcNow);

            _participants.Save(participant);
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

    private void CreateNode()
    {
        Console.Clear();
        Console.WriteLine("CREATE NODE");
        Console.WriteLine("-----------");

        Console.Write("Title: ");
        var title = Console.ReadLine();

        Console.Write("Description (optional): ");
        var description = Console.ReadLine();

        var nodeType = ConsoleUi.ReadNodeType(
            _nodeTypes,
            _currentParticipant.Id.Value.ToString());

        if (nodeType is null)
        {
            return;
        }

        try
        {
            var now = DateTimeOffset.UtcNow;
            var nodeTitle = new NodeTitle(title ?? string.Empty);

            var document = new Document(
                description ?? string.Empty,
                now);

            _documents.Save(document);

            Console.WriteLine();
            Console.WriteLine(
                $"[ATLAS.CONTENT] Saved description document " +
                $"{document.Id}.");

            var node = new Node(
                nodeTitle,
                new NodeDescriptionId(document.Id.Value),
                nodeType.Id,
                new NodeAuthorId(_currentParticipant.Id.Value),
                now);

            _nodes.Save(node);

            Console.WriteLine(
                $"[ATLAS.GRAPH] Saved node {node.Id} with " +
                $"description reference {node.DescriptionId}.");

            foreach (var domainEvent in
                     node.DomainEvents.OfType<NodeLifecycleEvent>())
            {
                _eventPublisher.Publish(
                    GraphEventContractMapper.ToIntegrationContract(
                        domainEvent));
            }

            node.ClearDomainEvents();

            ConsoleUi.Pause(
                $"Node created as {nodeType.Name}: {node.Title}");
        }
        catch (ArgumentException exception)
        {
            ConsoleUi.Pause(
                $"Unable to create node: {exception.Message}");
        }
    }

    private void BrowseNodes()
    {
        var browsing = true;

        while (browsing)
        {
            Console.Clear();
            Console.WriteLine("BROWSE NODES");
            Console.WriteLine("------------");

            var nodes = _nodes.GetAll().ToList();

            if (nodes.Count == 0)
            {
                ConsoleUi.Pause("No nodes have been created.");
                return;
            }

            NodeDisplay.WriteTableHeader();

            for (var index = 0; index < nodes.Count; index++)
            {
                NodeDisplay.WriteTableRow(
                    nodes[index],
                    _nodeTypes,
                    _documents,
                    _participants,
                    index + 1);
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

            NodeCommands.Run(
                nodes[selection - 1],
                _nodes,
                _nodeTypes,
                _documents,
                _participants,
                _eventPublisher,
                _currentParticipant.Id.Value.ToString());
        }
    }

    private void ListNodeTypes()
    {
        Console.Clear();
        Console.WriteLine("NODE TYPES");
        Console.WriteLine("----------");

        var nodeTypes = _nodeTypes
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

    private void ListContentDocuments()
    {
        Console.Clear();
        Console.WriteLine("ATLAS.CONTENT DOCUMENTS");
        Console.WriteLine("-----------------------");

        var documents = _documents.GetAll();

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

    private void ShowDataFiles()
    {
        ShowDataFile("NODE DATA", _nodeDataFilePath);
        ShowDataFile("NODE TYPE DATA", _nodeTypeDataFilePath);
        ShowDataFile("CONTENT DOCUMENT DATA", _documentDataFilePath);
        ShowDataFile("PARTICIPANT DATA", _participantDataFilePath);
    }

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
