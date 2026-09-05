using Atlas.Graph.Nodes;
using Atlas.Graph.Nodes.NodeTypes;

namespace Atlas.ConsoleApp;

public static class NodeDisplay
{
    private const int TitleWidth = 28;
    private const int TypeWidth = 14;
    private const int DescriptionWidth = 32;

    public static void WriteTableHeader()
    {
        Console.WriteLine(
            $"{"#",3}  " +
            $"{"Title",-TitleWidth}  " +
            $"{"Type",-TypeWidth}  " +
            $"{"Description",-DescriptionWidth}  " +
            $"{"Votes",5}  " +
            $"{"Avg",5}  " +
            "Status");

        Console.WriteLine(
            new string(
                '-',
                3 + 2 +
                TitleWidth + 2 +
                TypeWidth + 2 +
                DescriptionWidth + 2 +
                5 + 2 +
                5 + 2 +
                10));
    }

    public static void WriteTableRow(
        Node node,
        INodeTypeRepository nodeTypes,
        int number,
        int? voteCount = null,
        double? averageVote = null)
    {
        var typeName = ResolveTypeName(node, nodeTypes);
        var description = string.IsNullOrWhiteSpace(node.Description.Value)
            ? "—"
            : node.Description.Value;

        Console.WriteLine(
            $"{number,3}  " +
            $"{Truncate(node.Title.Value, TitleWidth),-TitleWidth}  " +
            $"{Truncate(typeName, TypeWidth),-TypeWidth}  " +
            $"{Truncate(description, DescriptionWidth),-DescriptionWidth}  " +
            $"{FormatVoteCount(voteCount),5}  " +
            $"{FormatAverageVote(averageVote),5}  " +
            node.Status);
    }

    public static void WriteDetails(
        Node node,
        INodeTypeRepository nodeTypes,
        int? voteCount = null,
        double? averageVote = null)
    {
        Console.WriteLine("ATLAS NODE");
        Console.WriteLine("----------");
        Console.WriteLine($"ID:          {node.Id}");
        Console.WriteLine($"Title:       {node.Title}");
        Console.WriteLine(
            $"Type:        {ResolveTypeName(node, nodeTypes)}");
        Console.WriteLine($"Type ID:     {node.TypeId}");
        Console.WriteLine($"Status:      {node.Status}");
        Console.WriteLine($"Votes:       {FormatVoteCount(voteCount)}");
        Console.WriteLine($"Average:     {FormatAverageVote(averageVote)}");
        Console.WriteLine($"Created:     {node.CreatedAt.LocalDateTime}");
        Console.WriteLine($"Updated:     {node.UpdatedAt.LocalDateTime}");
        Console.WriteLine();
        Console.WriteLine("Description");
        Console.WriteLine("-----------");
        Console.WriteLine(
            string.IsNullOrWhiteSpace(node.Description.Value)
                ? "No description has been provided."
                : node.Description.Value);
    }

    private static string ResolveTypeName(
        Node node,
        INodeTypeRepository nodeTypes)
    {
        return nodeTypes.GetById(node.TypeId)?.Name
            ?? $"Unknown ({node.TypeId})";
    }

    private static string Truncate(string value, int maximumLength)
    {
        if (value.Length <= maximumLength)
        {
            return value;
        }

        return maximumLength <= 3
            ? value[..maximumLength]
            : $"{value[..(maximumLength - 3)]}...";
    }

    private static string FormatVoteCount(int? voteCount)
    {
        return voteCount?.ToString() ?? "—";
    }

    private static string FormatAverageVote(double? averageVote)
    {
        return averageVote?.ToString("0.0") ?? "—";
    }
}
