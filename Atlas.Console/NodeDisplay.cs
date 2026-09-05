using Atlas.Graph.Nodes;
using Atlas.Graph.Nodes.NodeTypes;

namespace Atlas.ConsoleApp;

public static class NodeDisplay
{
    public static void WriteSummary(
        Node node,
        INodeTypeRepository nodeTypes,
        int? number = null)
    {
        var prefix = number is null ? "- " : $"{number}. ";
        var typeName = ResolveTypeName(node, nodeTypes);

        Console.WriteLine(
            $"{prefix}{node.Title} [{typeName}] ({node.Status})");
    }

    public static void WriteDetails(
        Node node,
        INodeTypeRepository nodeTypes)
    {
        Console.WriteLine("ATLAS NODE");
        Console.WriteLine("----------");
        Console.WriteLine($"ID:      {node.Id}");
        Console.WriteLine($"Title:   {node.Title}");
        Console.WriteLine(
            $"Type:    {ResolveTypeName(node, nodeTypes)}");
        Console.WriteLine($"Type ID: {node.TypeId}");
        Console.WriteLine($"Status:  {node.Status}");
        Console.WriteLine($"Created: {node.CreatedAt.LocalDateTime}");
        Console.WriteLine($"Updated: {node.UpdatedAt.LocalDateTime}");
    }

    private static string ResolveTypeName(
        Node node,
        INodeTypeRepository nodeTypes)
    {
        return nodeTypes.GetById(node.TypeId)?.Name
            ?? $"Unknown ({node.TypeId})";
    }
}
