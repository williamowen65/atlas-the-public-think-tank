using Atlas.Graph.Nodes;

namespace Atlas.ConsoleApp;

public static class NodeDisplay
{
    public static void WriteSummary(Node node, int? number = null)
    {
        var prefix = number is null ? "- " : $"{number}. ";

        Console.WriteLine(
            $"{prefix}{node.Title} [{node.Type}] ({node.Status})");
    }

    public static void WriteDetails(Node node)
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
}
