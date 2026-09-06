using Atlas.Content.Documents;
using Atlas.Graph.Nodes;
using Atlas.Graph.Nodes.NodeTypes;
using Atlas.Participants.Participants;

namespace Atlas.ConsoleApp;

public static class NodeDisplay
{
    private const int TitleWidth = 28;
    private const int TypeWidth = 14;
    private const int AuthorWidth = 20;
    private const int DescriptionWidth = 32;
    private const int SubNodesWidth = 36;

    public static void WriteTableHeader()
    {
        Console.WriteLine(
            $"{"#",3}  " +
            $"{"Title",-TitleWidth}  " +
            $"{"Type",-TypeWidth}  " +
            $"{"Authored By",-AuthorWidth}  " +
            $"{"Description",-DescriptionWidth}  " +
            $"{"Sub-nodes",-SubNodesWidth}  " +
            $"{"Votes",5}  " +
            $"{"Avg",5}  " +
            "Status");

        Console.WriteLine(
            new string(
                '-',
                3 + 2 +
                TitleWidth + 2 +
                TypeWidth + 2 +
                AuthorWidth + 2 +
                DescriptionWidth + 2 +
                SubNodesWidth + 2 +
                5 + 2 +
                5 + 2 +
                10));
    }

    public static void WriteTableRow(
        Node node,
        INodeTypeRepository nodeTypes,
        IDocumentRepository documents,
        IParticipantRepository participants,
        int number,
        int? voteCount = null,
        double? averageVote = null)
    {
        var typeName = ResolveTypeName(node, nodeTypes);
        var description = ResolveDescription(node, documents);
        var authorName = ResolveAuthorName(node, participants);
        var subNodeSummary =
            ResolveRequestedSubNodeSummary(node, nodeTypes);

        Console.WriteLine(
            $"{number,3}  " +
            $"{Truncate(node.Title.Value, TitleWidth),-TitleWidth}  " +
            $"{Truncate(typeName, TypeWidth),-TypeWidth}  " +
            $"{Truncate(authorName, AuthorWidth),-AuthorWidth}  " +
            $"{Truncate(description, DescriptionWidth),-DescriptionWidth}  " +
            $"{Truncate(subNodeSummary, SubNodesWidth),-SubNodesWidth}  " +
            $"{FormatVoteCount(voteCount),5}  " +
            $"{FormatAverageVote(averageVote),5}  " +
            node.Status);
    }

    public static void WriteDetails(
        Node node,
        INodeTypeRepository nodeTypes,
        IDocumentRepository documents,
        IParticipantRepository participants,
        int? voteCount = null,
        double? averageVote = null)
    {
        var description = ResolveDescription(node, documents);
        var authorName = ResolveAuthorName(node, participants);

        Console.WriteLine("ATLAS NODE");
        Console.WriteLine("----------");
        Console.WriteLine($"ID:             {node.Id}");
        Console.WriteLine($"Title:          {node.Title}");
        Console.WriteLine(
            $"Type:           {ResolveTypeName(node, nodeTypes)}");
        Console.WriteLine($"Type ID:        {node.TypeId}");
        Console.WriteLine($"Description ID: {node.DescriptionId}");
        Console.WriteLine($"Author ID:      {node.AuthorId}");
        Console.WriteLine($"Authored By:    {authorName}");
        Console.WriteLine($"Status:         {node.Status}");
        Console.WriteLine($"Votes:          {FormatVoteCount(voteCount)}");
        Console.WriteLine($"Average:        {FormatAverageVote(averageVote)}");
        Console.WriteLine($"Created:        {node.CreatedAt.LocalDateTime}");
        Console.WriteLine($"Updated:        {node.UpdatedAt.LocalDateTime}");
        Console.WriteLine();
        Console.WriteLine("Description");
        Console.WriteLine("-----------");
        Console.WriteLine(description);

        WriteRequestedSubNodeTables(node, nodeTypes);
    }

    private static void WriteRequestedSubNodeTables(
        Node node,
        INodeTypeRepository nodeTypes)
    {
        foreach (var request in node.RequestedSubNodeTypes)
        {
            var typeName = nodeTypes.GetById(request.TypeId)?.Name
                ?? $"Unknown ({request.TypeId})";
            var heading = $"{typeName.ToUpperInvariant()} SUB-NODES (0)";

            Console.WriteLine();
            Console.WriteLine(heading);
            Console.WriteLine(new string('-', heading.Length));
            WriteTableHeader();
            Console.WriteLine(
                $"No {typeName} sub-nodes have been added yet.");
        }

        if (node.RequestedSubNodeTypes.Count == 0)
        {
            Console.WriteLine();
            Console.WriteLine("SUB-NODES");
            Console.WriteLine("---------");
            Console.WriteLine(
                "This node does not currently request any sub-node types.");
        }
    }

    private static string ResolveRequestedSubNodeSummary(
        Node node,
        INodeTypeRepository nodeTypes)
    {
        if (node.RequestedSubNodeTypes.Count == 0)
        {
            return "None";
        }

        return string.Join(
            " · ",
            node.RequestedSubNodeTypes.Select(request =>
            {
                var name = nodeTypes.GetById(request.TypeId)?.Name
                    ?? "Unknown";

                return $"{name} 0";
            }));
    }

    private static string ResolveDescription(
        Node node,
        IDocumentRepository documents)
    {
        var document = documents.GetById(
            new DocumentId(node.DescriptionId.Value));

        if (document is null)
        {
            return "Description document not found.";
        }

        return string.IsNullOrWhiteSpace(document.Content)
            ? "No description has been provided."
            : document.Content;
    }

    private static string ResolveAuthorName(
        Node node,
        IParticipantRepository participants)
    {
        return participants.GetById(
                   new ParticipantId(node.AuthorId.Value))
               ?.DisplayName
            ?? $"Unknown ({node.AuthorId})";
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
