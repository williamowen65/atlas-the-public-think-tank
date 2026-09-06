using Atlas.ConsoleApp.Eventing;
using Atlas.ConsoleApp.Participants;
using Atlas.Content.Documents;
using Atlas.Graph.Nodes;
using Atlas.Graph.Nodes.NodeTypes;
using Atlas.Participants.Participants;
using Atlas.Participants.Profiles;

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
        Console.Write("Short bio (optional): ");
        var bio = Console.ReadLine();

        try
        {
            var participant = new Participant(
                displayName ?? string.Empty,
                bio ?? string.Empty,
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

    private void BrowseParticipants()
    {
        var browsing = true;

        while (browsing)
        {
            Console.Clear();
            Console.WriteLine("BROWSE PARTICIPANTS");
            Console.WriteLine("-------------------");

            var participants = _participants
                .GetAll()
                .OrderBy(participant => participant.DisplayName)
                .ToList();

            if (participants.Count == 0)
            {
                ConsoleUi.Pause("No participants have been created.");
                return;
            }

            var nodes = _nodes.GetAll();

            ParticipantDisplay.WriteTableHeader();

            for (var index = 0; index < participants.Count; index++)
            {
                ParticipantDisplay.WriteTableRow(
                    participants[index],
                    nodes,
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

    private void ViewParticipantProfile(
        ParticipantId participantId)
    {
        var viewing = true;

        while (viewing)
        {
            var participant = _participants.GetById(participantId);

            if (participant is null)
            {
                ConsoleUi.Pause("That participant no longer exists.");
                return;
            }

            var authoredNodes = _nodes
                .GetAll()
                .Where(node =>
                    node.AuthorId.Value == participant.Id.Value)
                .ToList();

            Console.Clear();
            Console.WriteLine("PARTICIPANT PROFILE");
            Console.WriteLine("-------------------");
            Console.WriteLine($"Display name: {participant.DisplayName}");
            Console.WriteLine(
                $"Bio:          " +
                $"{(string.IsNullOrWhiteSpace(participant.Bio) ? "-" : participant.Bio)}");
            Console.WriteLine(
                $"Joined:       {participant.CreatedAt.LocalDateTime}");
            Console.WriteLine(
                $"Status:       {(participant.IsActive ? "Active" : "Inactive")}");
            Console.WriteLine($"Authored nodes: {authoredNodes.Count}");
            Console.WriteLine(
                $"Viewing as:   {_currentParticipant.DisplayName}");
            Console.WriteLine();
            Console.WriteLine("1. Edit profile");
            Console.WriteLine("2. View authored nodes");
            Console.WriteLine("3. Select as current participant");
            Console.WriteLine("4. Return to participant browser");
            Console.WriteLine();
            Console.Write("Selection: ");

            switch (Console.ReadLine())
            {
                case "1":
                    EditParticipantProfile(participant);
                    break;

                case "2":
                    ViewAuthoredNodes(participant, authoredNodes);
                    break;

                case "3":
                    if (!participant.IsActive)
                    {
                        ConsoleUi.Pause(
                            "An inactive participant cannot be selected.");
                        break;
                    }

                    _currentParticipant = participant;
                    ConsoleUi.Pause(
                        $"Current participant: {participant.DisplayName}");
                    break;

                case "4":
                    viewing = false;
                    break;

                default:
                    ConsoleUi.Pause(
                        "Please select an option from 1 through 4.");
                    break;
            }
        }
    }

    private void EditParticipantProfile(Participant participant)
    {
        Console.Clear();
        Console.WriteLine("EDIT PARTICIPANT PROFILE");
        Console.WriteLine("------------------------");
        Console.WriteLine(
            $"Editing {participant.DisplayName} " +
            $"as {_currentParticipant.DisplayName}.");
        Console.WriteLine();
        Console.Write(
            $"Display name ({participant.DisplayName}): ");
        var displayName = Console.ReadLine();

        Console.WriteLine(
            "Bio (blank keeps the current bio; /clear removes it):");
        Console.Write("> ");
        var bio = Console.ReadLine();

        var requestedDisplayName =
            string.IsNullOrWhiteSpace(displayName)
                ? participant.DisplayName
                : displayName;

        var requestedBio = bio switch
        {
            null or "" => participant.Bio,
            "/clear" => string.Empty,
            _ => bio
        };

        try
        {
            var workflow =
                new UpdateParticipantProfile(_participants);

            var updatedParticipant = workflow.Execute(
                _currentParticipant.Id,
                participant.Id,
                requestedDisplayName,
                requestedBio,
                DateTimeOffset.UtcNow);

            if (_currentParticipant.Id == updatedParticipant.Id)
            {
                _currentParticipant = updatedParticipant;
            }

            ConsoleUi.Pause("Profile updated.");
        }
        catch (UnauthorizedAccessException exception)
        {
            ConsoleUi.Pause($"Permission denied: {exception.Message}");
        }
        catch (ArgumentException exception)
        {
            ConsoleUi.Pause($"Unable to update profile: {exception.Message}");
        }
        catch (InvalidOperationException exception)
        {
            ConsoleUi.Pause($"Unable to update profile: {exception.Message}");
        }
    }

    private void ViewAuthoredNodes(
        Participant participant,
        IReadOnlyCollection<Node> authoredNodes)
    {
        Console.Clear();
        Console.WriteLine(
            $"NODES AUTHORED BY {participant.DisplayName.ToUpperInvariant()}");
        Console.WriteLine(
            new string('-', 18 + participant.DisplayName.Length));

        if (authoredNodes.Count == 0)
        {
            Console.WriteLine("No authored nodes.");
            ConsoleUi.Pause();
            return;
        }

        NodeDisplay.WriteTableHeader();

        var index = 1;

        foreach (var node in authoredNodes)
        {
            NodeDisplay.WriteTableRow(
                node,
                _nodes,
                _nodeTypes,
                _documents,
                _participants,
                index);

            index++;
        }

        ConsoleUi.Pause();
    }

    private void CreateNode()
    {
        Console.Clear();
        Console.WriteLine("CREATE NODE");
        Console.WriteLine("-----------");

        NodeCreationWorkflow.Create(
            _nodes,
            _nodeTypes,
            _documents,
            _currentParticipant,
            _eventPublisher);
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
                    _nodes,
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
                _currentParticipant);
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
