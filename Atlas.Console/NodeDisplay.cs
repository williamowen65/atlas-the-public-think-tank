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
    private const int StatusWidth = 10;
    private const int SubNodesMinimumWidth = 36;

    public static void WriteTableHeader()
    {
        Console.WriteLine(
            $"{"#",3}  " +
            $"{"Title",-TitleWidth}  " +
            $"{"Type",-TypeWidth}  " +
            $"{"Authored By",-AuthorWidth}  " +
            $"{"Description",-DescriptionWidth}  " +
            $"{"Votes",5}  " +
            $"{"Avg",5}  " +
            $"{"Status",-StatusWidth}  " +
            "Sub-nodes");

        Console.WriteLine(
            new string(
                '-',
                3 + 2 +
                TitleWidth + 2 +
                TypeWidth + 2 +
                AuthorWidth + 2 +
                DescriptionWidth + 2 +
                5 + 2 +
                5 + 2 +
                StatusWidth + 2 +
                SubNodesMinimumWidth));
    }

    public static void WriteTableRow(
        Node node,
        INodeRepository nodes,
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
            ResolveSubNodeSummary(node, nodes, nodeTypes);

        Console.WriteLine(
            $"{number,3}  " +
            $"{Truncate(node.Title.Value, TitleWidth),-TitleWidth}  " +
            $"{Truncate(typeName, TypeWidth),-TypeWidth}  " +
            $"{Truncate(authorName, AuthorWidth),-AuthorWidth}  " +
            $"{Truncate(description, DescriptionWidth),-DescriptionWidth}  " +
            $"{FormatVoteCount(voteCount),5}  " +
            $"{FormatAverageVote(averageVote),5}  " +
            $"{node.Status,-StatusWidth}  " +
            subNodeSummary);
    }

    public static void WriteDetails(
        Node node,
        INodeRepository nodes,
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
        Console.WriteLine(
            $"Parents:        {ResolveParentSummary(node, nodes)}");
        Console.WriteLine($"Status:         {node.Status}");
        Console.WriteLine($"Votes:          {FormatVoteCount(voteCount)}");
        Console.WriteLine($"Average:        {FormatAverageVote(averageVote)}");
        Console.WriteLine($"Created:        {node.CreatedAt.LocalDateTime}");
        Console.WriteLine($"Updated:        {node.UpdatedAt.LocalDateTime}");
        Console.WriteLine();
        Console.WriteLine("Description");
        Console.WriteLine("-----------");
        Console.WriteLine(description);

        WriteSubNodeTables(
            node,
            nodes,
            nodeTypes,
            documents,
            participants);
    }

    private static void WriteSubNodeTables(
        Node node,
        INodeRepository nodes,
        INodeTypeRepository nodeTypes,
        IDocumentRepository documents,
        IParticipantRepository participants)
    {
        var children = FindChildren(node, nodes);
        var typeIds = node.RequestedSubNodeTypes
            .Select(request => request.TypeId)
            .Concat(children.Select(child => child.TypeId))
            .Distinct()
            .OrderBy(typeId =>
                nodeTypes.GetById(typeId)?.Name ?? string.Empty)
            .ToList();

        foreach (var typeId in typeIds)
        {
            var typeName = nodeTypes.GetById(typeId)?.Name
                ?? $"Unknown ({typeId})";
            var matchingChildren = children
                .Where(child => child.TypeId == typeId)
                .ToList();
            var heading = FormatTypeCount(
                    typeName,
                    matchingChildren.Count)
                .ToUpperInvariant();

            Console.WriteLine();
            Console.WriteLine(heading);
            Console.WriteLine(new string('-', heading.Length));
            WriteTableHeader();

            if (matchingChildren.Count == 0)
            {
                Console.WriteLine(
                    $"No {PluralizeTypeName(typeName)} have been added yet.");
                continue;
            }

            for (var index = 0;
                 index < matchingChildren.Count;
                 index++)
            {
                WriteTableRow(
                    matchingChildren[index],
                    nodes,
                    nodeTypes,
                    documents,
                    participants,
                    index + 1);
            }
        }

        if (typeIds.Count == 0)
        {
            Console.WriteLine();
            Console.WriteLine("SUB-NODES");
            Console.WriteLine("---------");
            Console.WriteLine(
                "This node does not request or contain any sub-nodes.");
        }
    }

    private static string ResolveSubNodeSummary(
        Node node,
        INodeRepository nodes,
        INodeTypeRepository nodeTypes)
    {
        var children = FindChildren(node, nodes);
        var typeIds = node.RequestedSubNodeTypes
            .Select(request => request.TypeId)
            .Concat(children.Select(child => child.TypeId))
            .Distinct()
            .OrderBy(typeId =>
                nodeTypes.GetById(typeId)?.Name ?? string.Empty)
            .ToList();

        if (typeIds.Count == 0)
        {
            return "None";
        }

        return string.Join(
            " · ",
            typeIds.Select(typeId =>
            {
                var name = nodeTypes.GetById(typeId)?.Name
                    ?? "Unknown";
                var count = children.Count(
                    child => child.TypeId == typeId);

                return FormatTypeCount(name, count);
            }));
    }

    private static List<Node> FindChildren(
        Node node,
        INodeRepository nodes)
    {
        return nodes
            .GetAll()
            .Where(candidate =>
                candidate.ParentNodeIds.Contains(node.Id))
            .OrderBy(candidate => candidate.Title.Value)
            .ToList();
    }


    private static string ResolveParentSummary(
        Node node,
        INodeRepository nodes)
    {
        if (node.ParentNodeIds.Count == 0)
        {
            return "Root node";
        }

        return string.Join(
            " · ",
            node.ParentNodeIds.Select(parentId =>
                nodes.GetById(parentId)?.Title.Value
                ?? $"Unknown ({parentId})"));
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


    public static string FormatTypeCount(
        string singularTypeName,
        int count)
    {
        var displayedName = count == 1
            ? singularTypeName
            : PluralizeTypeName(singularTypeName);

        return $"{count} {displayedName}";
    }

    private static string PluralizeTypeName(string singularTypeName)
    {
        if (singularTypeName.EndsWith(
                "Evidence",
                StringComparison.OrdinalIgnoreCase))
        {
            return singularTypeName;
        }

        var finalWordStart =
            singularTypeName.LastIndexOf(' ') + 1;
        var prefix = singularTypeName[..finalWordStart];
        var finalWord = singularTypeName[finalWordStart..];

        if (finalWord.EndsWith(
                "y",
                StringComparison.OrdinalIgnoreCase) &&
            finalWord.Length > 1 &&
            !"aeiou".Contains(
                char.ToLowerInvariant(finalWord[^2])))
        {
            return prefix + finalWord[..^1] + "ies";
        }

        if (finalWord.EndsWith(
                "s",
                StringComparison.OrdinalIgnoreCase) ||
            finalWord.EndsWith(
                "x",
                StringComparison.OrdinalIgnoreCase) ||
            finalWord.EndsWith(
                "z",
                StringComparison.OrdinalIgnoreCase) ||
            finalWord.EndsWith(
                "ch",
                StringComparison.OrdinalIgnoreCase) ||
            finalWord.EndsWith(
                "sh",
                StringComparison.OrdinalIgnoreCase))
        {
            return prefix + finalWord + "es";
        }

        return prefix + finalWord + "s";
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
