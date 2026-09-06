using Atlas.Graph.Nodes;
using Atlas.Graph.Nodes.NodeTypes;
using Atlas.Participants.Participants;

namespace Atlas.ConsoleApp.Participants;

public static class ParticipantDisplay
{
    private const int NameWidth = 24;
    private const int BioWidth = 42;
    private const int StatusWidth = 8;
    private const int NodesMinimumWidth = 36;

    public static void WriteTableHeader()
    {
        Console.WriteLine(
            $"{"#",3}  " +
            $"{"Participant",-NameWidth}  " +
            $"{"Bio",-BioWidth}  " +
            $"{"Status",-StatusWidth}  " +
            "Nodes");

        Console.WriteLine(
            new string(
                '-',
                3 + 2 +
                NameWidth + 2 +
                BioWidth + 2 +
                StatusWidth + 2 +
                NodesMinimumWidth));
    }

    public static void WriteTableRow(
        Participant participant,
        IReadOnlyCollection<Node> nodes,
        INodeTypeRepository nodeTypes,
        int number)
    {
        var status = participant.IsActive
            ? "Active"
            : "Inactive";

        var nodeSummary = ResolveNodeSummary(
            participant,
            nodes,
            nodeTypes);

        Console.WriteLine(
            $"{number,3}  " +
            $"{Truncate(participant.DisplayName, NameWidth),-NameWidth}  " +
            $"{Truncate(participant.Bio, BioWidth),-BioWidth}  " +
            $"{status,-StatusWidth}  " +
            nodeSummary);
    }

    public static void WriteProfile(
        Participant participant,
        IReadOnlyCollection<Node> nodes,
        Participant currentParticipant)
    {
        var authoredNodeCount = nodes.Count(
            node => node.AuthorId.Value == participant.Id.Value);

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
        Console.WriteLine($"Authored nodes: {authoredNodeCount}");
        Console.WriteLine(
            $"Viewing as:   {currentParticipant.DisplayName}");
    }

    private static string ResolveNodeSummary(
        Participant participant,
        IReadOnlyCollection<Node> nodes,
        INodeTypeRepository nodeTypes)
    {
        var groups = nodes
            .Where(node =>
                node.AuthorId.Value == participant.Id.Value)
            .GroupBy(node => node.TypeId)
            .Select(group =>
            {
                var nodeType = nodeTypes.GetById(group.Key);

                return new
                {
                    Name = nodeType?.Name ?? "Unknown",
                    Count = group.Count(),
                    AutoPluralize =
                        nodeType?.AutoPluralize ?? true
                };
            })
            .OrderBy(group => group.Name)
            .ToList();

        if (groups.Count == 0)
        {
            return "None";
        }

        return string.Join(
            " · ",
            groups.Select(group =>
                NodeDisplay.FormatTypeCount(
                    group.Name,
                    group.Count,
                    group.AutoPluralize)));
    }

    private static string Truncate(string value, int maximumLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "-";
        }

        return value.Length <= maximumLength
            ? value
            : value[..(maximumLength - 3)] + "...";
    }
}
