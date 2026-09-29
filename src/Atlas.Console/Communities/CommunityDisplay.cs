using Atlas.Communities.Communities;
using Atlas.Communities.Memberships;
using Atlas.Communities.Nodes;
using Atlas.Graph.Nodes;
using Atlas.Participants.Participants;

namespace Atlas.ConsoleApp.Communities;

public static class CommunityDisplay
{
    public static void WriteTableHeader()
    {
        Console.WriteLine($"{"#",3}  {"Name",-28}  {"Owner",-20}  {"Members",7}  {"Nodes",5}  {"Status",-10}  Description");
        Console.WriteLine(new string('-', 108));
    }

    public static void WriteTableRow(
        Community community,
        IParticipantRepository participants,
        ICommunityMembershipRepository memberships,
        ICommunityNodeRepository communityNodes,
        int number)
    {
        var owner = participants.GetById(new ParticipantId(community.OwnerParticipantId))?.DisplayName ?? "Unknown";
        var memberCount = memberships.GetByCommunity(community.Id).Count(x => x.IsActive);
        var nodeCount = communityNodes.GetByCommunity(community.Id).Count;
        Console.WriteLine($"{number,3}  {Trim(community.Name, 28),-28}  {Trim(owner, 20),-20}  {memberCount,7}  {nodeCount,5}  {community.Status,-10}  {Trim(community.Description, 34)}");
    }

    public static void WriteDetails(
        Community community,
        IParticipantRepository participants,
        ICommunityMembershipRepository memberships,
        ICommunityNodeRepository communityNodes)
    {
        var owner = participants.GetById(new ParticipantId(community.OwnerParticipantId))?.DisplayName ?? "Unknown";
        Console.WriteLine("ATLAS COMMUNITY");
        Console.WriteLine("---------------");
        Console.WriteLine($"Name:        {community.Name}");
        Console.WriteLine($"Owner:       {owner}");
        Console.WriteLine($"Status:      {community.Status}");
        Console.WriteLine($"Members:     {memberships.GetByCommunity(community.Id).Count(x => x.IsActive)}");
        Console.WriteLine($"Nodes:       {communityNodes.GetByCommunity(community.Id).Count}");
        Console.WriteLine($"Description: {community.Description}");
        Console.WriteLine($"ID:          {community.Id}");
    }

    public static string NamesForNode(Guid nodeId, ICommunityNodeRepository associations, ICommunityRepository communities)
    {
        var names = associations.GetByNode(nodeId)
            .Select(x => communities.GetById(x.CommunityId)?.Name)
            .Where(x => !string.IsNullOrWhiteSpace(x));
        return string.Join(", ", names!);
    }

    private static string Trim(string value, int length) => value.Length <= length ? value : value[..(length - 1)] + "…";
}
