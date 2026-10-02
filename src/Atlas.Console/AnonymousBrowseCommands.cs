using Atlas.Comments.Comments;
using Atlas.Communities.Communities;
using Atlas.Communities.Nodes;
using Atlas.Content.Documents;
using Atlas.Discovery;
using Atlas.Graph.Nodes;
using Atlas.Graph.Nodes.NodeTypes;
using Atlas.Graph.Reactions;
using Atlas.Moderation;
using Atlas.ConsoleApp.Participants;
using Atlas.Participants.Participants;
using Atlas.Voting.Data;

namespace Atlas.ConsoleApp;

/// <summary>
/// Public, read-only Console navigation available without an authenticated Atlas participant.
/// Actor-required operations deliberately remain in the authenticated ConsoleApplication.
/// </summary>
public static class AnonymousBrowseCommands
{
    public static void BrowseParticipants(
        IParticipantRepository participants,
        INodeRepository nodes,
        INodeTypeRepository nodeTypes)
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("BROWSE PARTICIPANTS");
            Console.WriteLine("-------------------");
            Console.WriteLine("Viewing as: Anonymous");
            Console.WriteLine();

            var available = participants.GetAll()
                .OrderBy(participant => participant.DisplayName)
                .ToList();

            if (available.Count == 0)
            {
                ConsoleUi.Pause("No participants have been created.");
                return;
            }

            ParticipantDisplay.WriteTableHeader();
            for (var index = 0; index < available.Count; index++)
            {
                ParticipantDisplay.WriteTableRow(
                    available[index],
                    nodes.GetByAuthor(new NodeAuthorId(available[index].Id.Value)),
                    nodeTypes,
                    index + 1);
            }

            Console.WriteLine();
            Console.Write("Participant number to view (0 returns): ");
            if (!int.TryParse(Console.ReadLine(), out var selection) ||
                selection < 0 ||
                selection > available.Count)
            {
                ConsoleUi.Pause("That is not a valid selection.");
                continue;
            }

            if (selection == 0) return;

            var participant = available[selection - 1];
            var authored = nodes.GetByAuthor(new NodeAuthorId(participant.Id.Value));
            Console.Clear();
            Console.WriteLine("PARTICIPANT PROFILE");
            Console.WriteLine("-------------------");
            Console.WriteLine($"Display name: {participant.DisplayName}");
            Console.WriteLine($"Bio:          {(string.IsNullOrWhiteSpace(participant.Bio) ? "-" : participant.Bio)}");
            Console.WriteLine($"Joined:       {participant.CreatedAt.LocalDateTime}");
            Console.WriteLine($"Status:       {(participant.IsActive ? "Active" : "Inactive")}");
            Console.WriteLine($"Authored nodes: {authored.Count}");
            Console.WriteLine("Viewing as:   Anonymous");
            ConsoleUi.Pause();
        }
    }

    public static void DiscoverNodes(
        IDiscoveryService discovery,
        INodeRepository nodes,
        INodeTypeRepository nodeTypes,
        IDocumentRepository documents,
        IParticipantRepository participants,
        INodeReactionRepository nodeTags,
        IReactionDefinitionRepository tagDefinitions,
        IVoteRepository votes,
        ICommunityRepository communities,
        ICommunityNodeRepository communityNodes,
        ICommentRepository comments,
        ModerationService moderation)
    {
        var page = 1;
        string? searchText = null;
        Guid? communityId = null;
        Guid? nodeTypeId = null;
        Guid? authorId = null;
        bool? isArchived = false;

        while (true)
        {
            var query = new DiscoveryQuery(
                SearchText: searchText,
                CommunityId: communityId,
                NodeTypeId: nodeTypeId,
                AuthorParticipantId: authorId,
                IsArchived: isArchived,
                Page: page);
            var resultPage = discovery.DiscoverPage(query);
            var results = resultPage.Items.ToList();
            var available = results
                .Select(result => nodes.GetById(new NodeId(result.NodeId)))
                .Where(node => node is not null)
                .Cast<Node>()
                .ToList();

            Console.Clear();
            Console.WriteLine("DISCOVERY");
            Console.WriteLine("---------");
            Console.WriteLine("Viewing as: Anonymous");
            Console.WriteLine($"Text: {searchText ?? "(all)"}");
            Console.WriteLine($"Community: {communities.GetById(communityId is null ? default : new CommunityId(communityId.Value))?.Name ?? (communityId is null ? "(all)" : "(unknown)")}");
            Console.WriteLine($"Node type: {nodeTypeId is null ? "(all)" : nodeTypes.GetById(new NodeTypeId(nodeTypeId.Value))?.Name ?? "(unknown)"}");
            Console.WriteLine($"Author: {authorId is null ? "(all)" : participants.GetById(new ParticipantId(authorId.Value))?.DisplayName ?? "(unknown)"}");
            Console.WriteLine($"Status: {(isArchived is null ? "All" : isArchived.Value ? "Archived" : "Active")}");
            Console.WriteLine();

            for (var index = 0; index < available.Count; index++)
            {
                var result = results[index];
                NodeDisplay.WriteTableRow(
                    available[index], nodes, nodeTypes, documents, participants, index + 1,
                    result.VoteCount, result.AverageVote, null, nodeTags, tagDefinitions, votes,
                    communities, communityNodes, comments, includeCreatedDate: true, moderation: moderation);
            }
            if (available.Count == 0) Console.WriteLine("No nodes match the current filters.");

            Console.WriteLine();
            Console.WriteLine("Node number to view; S text; C community; T node type; A author; U status; X clear; N/P pages; 0 return.");
            Console.Write("Selection: ");
            var input = Console.ReadLine()?.Trim();
            if (input == "0") return;
            if (string.Equals(input, "n", StringComparison.OrdinalIgnoreCase) && resultPage.HasNextPage) { page++; continue; }
            if (string.Equals(input, "p", StringComparison.OrdinalIgnoreCase) && page > 1) { page--; continue; }
            if (string.Equals(input, "s", StringComparison.OrdinalIgnoreCase))
            {
                Console.Write("Search text (blank clears): ");
                var value = Console.ReadLine();
                searchText = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
                page = 1; continue;
            }
            if (string.Equals(input, "c", StringComparison.OrdinalIgnoreCase))
            {
                communityId = SelectId(communities.GetAll().OrderBy(x => x.Name).Select(x => (x.Id.Value, x.Name)).ToList(), "community");
                page = 1; continue;
            }
            if (string.Equals(input, "t", StringComparison.OrdinalIgnoreCase))
            {
                nodeTypeId = SelectId(nodeTypes.GetAll().Where(x => !x.IsArchived).OrderBy(x => x.Name).Select(x => (x.Id.Value, x.Name)).ToList(), "node type");
                page = 1; continue;
            }
            if (string.Equals(input, "a", StringComparison.OrdinalIgnoreCase))
            {
                authorId = SelectId(participants.GetAll().OrderBy(x => x.DisplayName).Select(x => (x.Id.Value, x.DisplayName)).ToList(), "author");
                page = 1; continue;
            }
            if (string.Equals(input, "u", StringComparison.OrdinalIgnoreCase))
            {
                Console.Write("Status: 1 Active, 2 Archived, 3 All: ");
                isArchived = Console.ReadLine() switch { "1" => false, "2" => true, "3" => null, _ => isArchived };
                page = 1; continue;
            }
            if (string.Equals(input, "x", StringComparison.OrdinalIgnoreCase))
            {
                searchText = null; communityId = null; nodeTypeId = null; authorId = null; isArchived = false; page = 1; continue;
            }
            if (!int.TryParse(input, out var selection) || selection < 1 || selection > available.Count)
            {
                ConsoleUi.Pause("That is not a valid selection."); continue;
            }

            var node = available[selection - 1];
            var ranked = results[selection - 1];
            Console.Clear();
            NodeDisplay.WriteDetails(node, nodes, nodeTypes, documents, participants, nodeTags,
                tagDefinitions, votes, communities, communityNodes, comments, null,
                ranked.VoteCount, ranked.AverageVote, null, moderation: moderation);
            Console.WriteLine();
            Console.WriteLine("Read-only public view. Sign in to contribute or modify Atlas content.");
            ConsoleUi.Pause();
        }
    }

    private static Guid? SelectId(IReadOnlyList<(Guid Id, string Name)> values, string label)
    {
        Console.WriteLine($"Select {label} (0 clears):");
        for (var index = 0; index < values.Count; index++)
            Console.WriteLine($"{index + 1}. {values[index].Name}");
        Console.Write("Selection: ");
        if (!int.TryParse(Console.ReadLine(), out var selection) || selection == 0) return null;
        return selection >= 1 && selection <= values.Count ? values[selection - 1].Id : null;
    }

    public static void BrowseCommunities(
        ICommunityRepository communities,
        ICommunityNodeRepository communityNodes)
    {
        Console.Clear();
        Console.WriteLine("BROWSE COMMUNITIES");
        Console.WriteLine("------------------");
        Console.WriteLine("Viewing as: Anonymous");
        Console.WriteLine();

        var available = communities.GetAll().OrderBy(community => community.Name).ToList();
        if (available.Count == 0)
        {
            ConsoleUi.Pause("No communities have been created.");
            return;
        }

        foreach (var community in available)
        {
            var nodeCount = communityNodes.GetByCommunity(community.Id).Count;
            Console.WriteLine($"- {community.Name}");
            Console.WriteLine($"  {community.Description}");
            Console.WriteLine($"  Nodes: {nodeCount}");
            Console.WriteLine();
        }

        ConsoleUi.Pause();
    }

    public static void ListNodeTypes(INodeTypeRepository nodeTypes)
    {
        Console.Clear();
        Console.WriteLine("NODE TYPES");
        Console.WriteLine("----------");
        Console.WriteLine("Viewing as: Anonymous");
        Console.WriteLine();

        foreach (var nodeType in nodeTypes.GetAll().Where(type => !type.IsArchived).OrderBy(type => type.Name))
            Console.WriteLine($"- {nodeType.Name}: {nodeType.Description}");

        ConsoleUi.Pause();
    }
}
