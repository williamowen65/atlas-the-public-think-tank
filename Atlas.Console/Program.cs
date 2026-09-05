using Atlas.Graph;
using Atlas.Graph.Nodes;

//One caveat: don’t put business logic into Program.cs. It should merely construct objects, call their public operations, and show the results. Its job is to demonstrate the architecture—not become another version of Atlas.

// The console app is worthwhile for this particular rewrite, even though production Atlas will ultimately be driven by the web/API host.

var node = new Node(
    new NodeTitle("How can coastal communities adapt?"),
    NodeType.Question,
    DateTimeOffset.UtcNow);

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
    Console.WriteLine("5. Exit");
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
                break;

            case "2":
                ChangeNodeType(node);
                break;

            case "3":
                node.Archive(DateTimeOffset.UtcNow);
                Console.WriteLine("Node archived.");
                Pause();
                break;

            case "4":
                node.Restore(DateTimeOffset.UtcNow);
                Console.WriteLine("Node restored.");
                Pause();
                break;

            case "5":
                running = false;
                break;

            default:
                Console.WriteLine("Please select an option from 1 through 5.");
                Pause();
                break;
        }
    }
    catch (ArgumentException exception)
    {
        Console.WriteLine($"Unable to update node: {exception.Message}");
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

    Console.WriteLine("Node renamed.");
    Pause();
}

static void ChangeNodeType(Node node)
{
    Console.WriteLine("Available node types:");

    foreach (var nodeType in Enum.GetValues<NodeType>())
    {
        Console.WriteLine($"- {nodeType}");
    }

    Console.WriteLine();
    Console.Write("New type: ");

    var input = Console.ReadLine();

    if (!Enum.TryParse<NodeType>(
            input,
            ignoreCase: true,
            out var newType))
    {
        Console.WriteLine("That is not a recognized node type.");
        Pause();
        return;
    }

    node.ChangeType(newType, DateTimeOffset.UtcNow);

    Console.WriteLine($"Node type changed to {newType}.");
    Pause();
}

static void DisplayNode(Node node)
{
    Console.WriteLine("ATLAS NODE");
    Console.WriteLine("----------");
    Console.WriteLine($"ID:      {node.Id}");
    Console.WriteLine($"Title:   {node.Title}");
    Console.WriteLine($"Type:    {node.Type}");
    Console.WriteLine($"Status:  {node.Status}");
    Console.WriteLine($"Created: {node.CreatedAt.LocalDateTime}");
    Console.WriteLine($"Updated: {node.UpdatedAt.LocalDateTime}");
}

static void Pause()
{
    Console.WriteLine();
    Console.WriteLine("Press Enter to continue...");
    Console.ReadLine();
}