using Atlas.Graph.Nodes;

namespace Atlas.ConsoleApp;

public sealed class ConsoleApplication
{
    private readonly INodeRepository _nodes;
    private readonly string _dataFilePath;

    public ConsoleApplication(
        INodeRepository nodes,
        string dataFilePath)
    {
        _nodes = nodes;
        _dataFilePath = dataFilePath;
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
                    CreateNode();
                    break;

                case "2":
                    ListNodes();
                    break;

                case "3":
                    BrowseNodes();
                    break;

                case "4":
                    ShowDataFile();
                    break;

                case "5":
                    running = false;
                    break;

                default:
                    ConsoleUi.Pause(
                        "Please select an option from 1 through 5.");
                    break;
            }
        }
    }

    private static void WriteMainMenu()
    {
        Console.WriteLine("ATLAS");
        Console.WriteLine("-----");
        Console.WriteLine("1. Create node");
        Console.WriteLine("2. List nodes");
        Console.WriteLine("3. Browse nodes");
        Console.WriteLine("4. Show data file");
        Console.WriteLine("5. Exit");
        Console.WriteLine();
    }

    private void CreateNode()
    {
        Console.Clear();
        Console.WriteLine("CREATE NODE");
        Console.WriteLine("-----------");

        Console.Write("Title: ");
        var title = Console.ReadLine();

        var nodeType = ConsoleUi.ReadNodeType();

        if (nodeType is null)
        {
            return;
        }

        try
        {
            var node = new Node(
                new NodeTitle(title ?? string.Empty),
                nodeType.Value,
                DateTimeOffset.UtcNow);

            _nodes.Save(node);

            ConsoleUi.Pause($"Node created: {node.Title}");
        }
        catch (ArgumentException exception)
        {
            ConsoleUi.Pause(
                $"Unable to create node: {exception.Message}");
        }
    }

    private void ListNodes()
    {
        Console.Clear();
        Console.WriteLine("ATLAS NODES");
        Console.WriteLine("-----------");

        var nodes = _nodes.GetAll().ToList();

        if (nodes.Count == 0)
        {
            ConsoleUi.Pause("No nodes have been created.");
            return;
        }

        for (var index = 0; index < nodes.Count; index++)
        {
            NodeDisplay.WriteSummary(nodes[index], index + 1);
        }

        ConsoleUi.Pause();
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

            for (var index = 0; index < nodes.Count; index++)
            {
                NodeDisplay.WriteSummary(nodes[index], index + 1);
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

            NodeCommands.Run(nodes[selection - 1], _nodes);
        }
    }

    private void ShowDataFile()
    {
        Console.Clear();
        Console.WriteLine("NODE DATA");
        Console.WriteLine("---------");
        Console.WriteLine(_dataFilePath);
        Console.WriteLine();

        if (File.Exists(_dataFilePath))
        {
            Console.WriteLine(File.ReadAllText(_dataFilePath));
        }
        else
        {
            Console.WriteLine(
                "The data file has not been created yet.");
        }

        ConsoleUi.Pause();
    }
}
