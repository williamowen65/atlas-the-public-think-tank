using Atlas.Graph.Nodes;
using Atlas.Participants.Participants;

namespace Atlas.ConsoleApp.Participants;

public static class ParticipantDisplay
{
    private const int NameWidth = 24;
    private const int BioWidth = 42;

    public static void WriteTableHeader()
    {
        Console.WriteLine(
            $"{"#",3}  " +
            $"{"Participant",-NameWidth}  " +
            $"{"Bio",-BioWidth}  " +
            $"{"Nodes",5}  " +
            "Status");

        Console.WriteLine(new string('-', 91));
    }

    public static void WriteTableRow(
        Participant participant,
        IReadOnlyCollection<Node> nodes,
        int number)
    {
        var nodeCount = nodes.Count(
            node => node.AuthorId.Value == participant.Id.Value);

        var status = participant.IsActive
            ? "Active"
            : "Inactive";

        Console.WriteLine(
            $"{number,3}  " +
            $"{Truncate(participant.DisplayName, NameWidth),-NameWidth}  " +
            $"{Truncate(participant.Bio, BioWidth),-BioWidth}  " +
            $"{nodeCount,5}  " +
            $"{status}");
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
