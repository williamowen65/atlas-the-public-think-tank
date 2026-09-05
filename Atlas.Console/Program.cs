using Atlas.ConsoleApp.Storage;
using Atlas.Graph.Nodes;


var dataFilePath = Path.GetFullPath(
    Path.Combine(
        AppContext.BaseDirectory,
        "..",
        "..",
        "..",
        "nodes.json"));

Console.WriteLine($"Working directory: {Directory.GetCurrentDirectory()}");
Console.WriteLine($"JSON path: {dataFilePath}");

INodeRepository nodeRepository =
    new JsonNodeRepository(dataFilePath);

var node = nodeRepository
    .GetAll()
    .FirstOrDefault();

if (node is null)
{
    node = new Node(
        new NodeTitle("How can coastal communities adapt?"),
        NodeType.Question,
        DateTimeOffset.UtcNow);

    nodeRepository.Save(node);

    Console.WriteLine("A new node was created.");
    Console.WriteLine($"Data saved to: {dataFilePath}");
    Pause();
}

var running = true;

while (running)
{
    Console.Clear();

    DisplayNode(node);

    Console.WriteLine();
    Console.WriteLine("Choose an action:");
    Console.WriteLine("1. Rename node");
    Console.WriteLine("2. Change node type");
    Console.WriteLine("3. Archive node");
    Console.WriteLine("4. Restore node");
    Console.WriteLine("5. Show data file location");
    Console.WriteLine("6. Exit");
    Console.WriteLine();

    Console.Write("Selection: ");
    var selection = Console.ReadLine();

    Console.WriteLine();

    try
    {
        switch (selection)
        {
            case "1":
                RenameNode(node);
                nodeRepository.Save(node);
                break;

            case "2":
                if (ChangeNodeType(node))
                {
                    nodeRepository.Save(node);
                }

                break;

            case "3":
                node.Archive(DateTimeOffset.UtcNow);
                nodeRepository.Save(node);

                Console.WriteLine("Node archived and saved.");
                Pause();
                break;

            case "4":
                node.Restore(DateTimeOffset.UtcNow);
                nodeRepository.Save(node);

                Console.WriteLine("Node restored and saved.");
                Pause();
                break;

            case "5":
                ShowDataFileLocation(dataFilePath);
                break;

            case "6":
                running = false;
                break;

            default:
                Console.WriteLine(
                    "Please select an option from 1 through 6.");

                Pause();
                break;
        }
    }
    catch (ArgumentException exception)
    {
        Console.WriteLine(
            $"Unable to update node: {exception.Message}");

        Pause();
    }
}

static void RenameNode(Node node)
{
    Console.Write("New title: ");
    var title = Console.ReadLine();

    node.Rename(
        new NodeTitle(title ?? string.Empty),
        DateTimeOffset.UtcNow);

    Console.WriteLine("Node renamed and saved.");
    Pause();
}

static bool ChangeNodeType(Node node)
{
    Console.WriteLine("Available node types:");

    foreach (var nodeType in Enum.GetValues<NodeType>())
    {
        Console.WriteLine($"- {nodeType}");
    }

    Console.WriteLine();
    Console.Write("New type: ");

    var input = Console.ReadLine();

    if (!Enum.TryParse(
            input,
            ignoreCase: true,
            out NodeType newType))
    {
        Console.WriteLine(
            "That is not a recognized node type.");

        Pause();
        return false;
    }

    node.ChangeType(
        newType,
        DateTimeOffset.UtcNow);

    Console.WriteLine(
        $"Node type changed to {newType} and saved.");

    Pause();
    return true;
}

static void DisplayNode(Node node)
{
    Console.WriteLine("ATLAS NODE");
    Console.WriteLine("----------");
    Console.WriteLine($"ID:      {node.Id}");
    Console.WriteLine($"Title:   {node.Title}");
    Console.WriteLine($"Type:    {node.Type}");
    Console.WriteLine($"Status:  {node.Status}");
    Console.WriteLine(
        $"Created: {node.CreatedAt.LocalDateTime}");
    Console.WriteLine(
        $"Updated: {node.UpdatedAt.LocalDateTime}");
}

static void ShowDataFileLocation(string dataFilePath)
{
    Console.WriteLine("Node data is stored at:");
    Console.WriteLine(dataFilePath);
    Console.WriteLine();

    if (File.Exists(dataFilePath))
    {
        Console.WriteLine("Current JSON:");
        Console.WriteLine("-------------");
        Console.WriteLine(File.ReadAllText(dataFilePath));
    }
    else
    {
        Console.WriteLine(
            "The data file has not been created yet.");
    }

    Pause();
}

static void Pause()
{
    Console.WriteLine();
    Console.WriteLine("Press Enter to continue...");
    Console.ReadLine();
}