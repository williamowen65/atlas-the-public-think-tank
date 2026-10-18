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
        INodeTypeRepository nodeTypes
    )
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
                    index + 1
                );
            }

            Console.WriteLine();
            Console.Write("Participant number to view (0 returns): ");
            if (!int.TryParse(
                Console.ReadLine(),
                out var selection
            ) ||
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
        ModerationService moderation
    )
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
                Page: page
            );
            var resultPage = discovery.DiscoverPage(query);
            // Keep result numbers distinct across pages, and map selections back to this page.
            var firstNumber = DiscoveryPaging.FirstNumber(resultPage);
            var results = resultPage.Items.ToList();
            var hasCards = results.All(result => result.Card is not null);
            var available = hasCards ? new List<Node>() : results
                .Select(result => nodes.GetById(new NodeId(result.NodeId))).OfType<Node>().ToList();
            var displayedCount = hasCards ? results.Count : available.Count;

            if (!Console.IsOutputRedirected) Console.Clear();
            Console.WriteLine("DISCOVERY");
            Console.WriteLine("---------");
            Console.WriteLine("Viewing as: Anonymous");
            Console.WriteLine($"Text: {searchText ?? "(all)"}");
            Console.WriteLine($"Community: {communities.GetById(communityId is null ? default : new CommunityId(communityId.Value))?.Name ?? (communityId is null ? "(all)" : "(unknown)")}");
            Console.WriteLine($"Node type: {(nodeTypeId is null ? "(all)" : nodeTypes.GetById(new NodeTypeId(nodeTypeId.Value))?.Name ?? "(unknown)")}");
            Console.WriteLine($"Author: {(authorId is null ? "(all)" : participants.GetById(new ParticipantId(authorId.Value))?.DisplayName ?? "(unknown)")}");
            Console.WriteLine($"Status: {(isArchived is null ? "All" : isArchived.Value ? "Archived" : "Active")}");
            Console.WriteLine();

            for (var index = 0; index < displayedCount; index++)
            {
                var result = results[index];
                if (hasCards)
                {
                    NodeDisplay.WriteDiscoveryCard(result, firstNumber + index);
                    continue;
                }
                NodeDisplay.WriteTableRow(
                    available[index],
                    nodes,
                    nodeTypes,
                    documents,
                    participants,
                    firstNumber + index,
                    result.VoteCount,
                    result.AverageVote,
                    null,
                    nodeTags,
                    tagDefinitions,
                    votes,
                    communities,
                    communityNodes,
                    comments,
                    includeCreatedDate: true,
                    moderation: moderation
                );
            }
            if (displayedCount == 0) Console.WriteLine("No nodes match the current filters.");

            Console.WriteLine();
            DiscoveryPaging.WriteSummary(
                resultPage,
                displayedCount
            );
            Console.WriteLine("Node number to view; S text; C community; T node type; A author; U status; X clear; 0 return.");
            DiscoveryPaging.WriteNavigation(resultPage);
            Console.Write("Selection: ");
            var input = Console.ReadLine()?.Trim();
            if (input == "0") return;
            if (DiscoveryPaging.TryNavigate(
                input,
                resultPage,
                ref page
            )) continue;
            if (string.Equals(
                input,
                "s",
                StringComparison.OrdinalIgnoreCase
            ))
            {
                Console.Write("Search text (blank clears): ");
                var value = Console.ReadLine();
                searchText = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
                page = 1; continue;
            }
            if (string.Equals(
                input,
                "c",
                StringComparison.OrdinalIgnoreCase
            ))
            {
                communityId = SelectId(
                    communities.GetAll().OrderBy(x => x.Name).Select(x => (x.Id.Value, x.Name)).ToList(),
                    "community"
                );
                page = 1; continue;
            }
            if (string.Equals(
                input,
                "t",
                StringComparison.OrdinalIgnoreCase
            ))
            {
                nodeTypeId = SelectId(
                    nodeTypes.GetAll().Where(x => !x.IsArchived).OrderBy(x => x.Name).Select(x => (x.Id.Value, x.Name)).ToList(),
                    "node type"
                );
                page = 1; continue;
            }
            if (string.Equals(
                input,
                "a",
                StringComparison.OrdinalIgnoreCase
            ))
            {
                authorId = SelectId(
                    participants.GetAll().OrderBy(x => x.DisplayName).Select(x => (x.Id.Value, x.DisplayName)).ToList(),
                    "author"
                );
                page = 1; continue;
            }
            if (string.Equals(
                input,
                "u",
                StringComparison.OrdinalIgnoreCase
            ))
            {
                Console.Write("Status: 1 Active, 2 Archived, 3 All: ");
                isArchived = Console.ReadLine() switch { "1" => false, "2" => true, "3" => null, _ => isArchived };
                page = 1; continue;
            }
            if (string.Equals(
                input,
                "x",
                StringComparison.OrdinalIgnoreCase
            ))
            {
                searchText = null; communityId = null; nodeTypeId = null; authorId = null; isArchived = false; page = 1; continue;
            }
            if (!int.TryParse(
                input,
                out var selection
            ) || selection < firstNumber || selection >= firstNumber + displayedCount)
            {
                ConsoleUi.Pause("That is not a valid selection."); continue;
            }

            var node = hasCards ? nodes.GetById(new NodeId(results[selection - firstNumber].NodeId)) : available[selection - firstNumber];
            if (node is null) continue;
            ViewNode(node, nodes, nodeTypes, documents, participants, nodeTags,
                tagDefinitions, votes, communities, communityNodes, comments, moderation);
        }
    }

    /// <summary>Public node actions never receive an actor or mutation services.</summary>
    public static void ViewNode(
        Node node,
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
        while (true)
        {
            if (!Console.IsOutputRedirected) Console.Clear();
            var summary = new Atlas.Voting.GetVoteSummary(votes)
                .Execute(new Atlas.Voting.Target.NodeVoteTarget(node.Id.Value));
            NodeDisplay.WriteDetails(node, nodes, nodeTypes, documents, participants,
                nodeTags, tagDefinitions, votes, communities, communityNodes, comments,
                null, summary.VoteCount, summary.AverageVote, null, moderation: moderation);
            Console.WriteLine();
            Console.WriteLine("Actions (as Anonymous)");
            Console.WriteLine("  Explore:");
            Console.WriteLine("    7 Select sub-node · 11 Author profile · 15 View votes");
            Console.WriteLine("    17 View communities · 19 View comments");
            Console.WriteLine(node.ParentNodeIds.Count == 0
                ? "    24 Browse ancestors [disabled — root node]"
                : "    24 Browse ancestors");
            Console.WriteLine("  Contribute [disabled — sign in as an active participant]");
            Console.WriteLine("    20 Return to previous page");
            Console.Write("Selection: ");
            var input = Console.ReadLine()?.Trim();
            // Blank/EOF also returns, supporting the former pause-only public view.
            if (input is "20" or "0" || string.IsNullOrWhiteSpace(input)) return;
            switch (input)
            {
                case "7":
                    var children = nodes.GetChildren(node.Id).OrderBy(child => child.CreatedAt).ToArray();
                    var selected = SelectId(children.Select(child =>
                        (child.Id.Value, NodeDisplay.PublicTitle(child, moderation))).ToArray(), "sub-node");
                    if (selected is { } childId)
                        node = children.Single(child => child.Id.Value == childId);
                    break;
                case "24":
                    node = NodeCommands.SelectAncestor(node, nodes, nodeTypes, moderation) ?? node;
                    break;
                case "11":
                    var author = participants.GetById(new ParticipantId(node.AuthorId.Value));
                    Console.WriteLine("AUTHOR PROFILE");
                    Console.WriteLine(author is null ? "Author profile not found."
                        : $"{author.DisplayName}\n{author.Bio}\nStatus: {(author.IsActive ? "Active" : "Inactive")}");
                    ConsoleUi.Pause();
                    break;
                case "15":
                    Console.WriteLine("NODE VOTES");
                    var records = votes.GetTargetVotes(new Atlas.Voting.Target.NodeVoteTarget(node.Id.Value));
                    foreach (var vote in records)
                    {
                        var voter = participants.GetById(new ParticipantId(vote.ParticipantId.Id));
                        Console.WriteLine($"{voter?.DisplayName ?? "Unknown participant"}: {vote.Value.Value}");
                    }
                    if (records.Count == 0) Console.WriteLine("No participants have voted on this node.");
                    ConsoleUi.Pause();
                    break;
                case "17":
                    Console.WriteLine("NODE COMMUNITIES");
                    var links = communityNodes.GetByNode(node.Id.Value);
                    foreach (var link in links)
                    {
                        var community = communities.GetById(link.CommunityId);
                        if (community is not null) Console.WriteLine($"{community.Name}: {community.Description}");
                    }
                    if (links.Count == 0) Console.WriteLine("No communities linked.");
                    ConsoleUi.Pause();
                    break;
                case "19":
                    Console.WriteLine("COMMENTS (read only)");
                    // Do not bypass moderation by exposing related discussion text here.
                    if (moderation.Visibility(node.Id.Value).IsHidden)
                    {
                        ConsoleUi.Pause("Comments are unavailable while this node is hidden.");
                        break;
                    }
                    var discussion = comments.GetByTarget(CommentTarget.Node(node.Id.Value));
                    foreach (var comment in discussion.OrderBy(comment => comment.CreatedAt))
                    {
                        var name = participants.GetById(new ParticipantId(comment.AuthorParticipantId))?.DisplayName ?? "Unknown participant";
                        Console.WriteLine($"{name}: {(comment.IsRemoved ? "[removed]" : comment.Body)}");
                    }
                    if (discussion.Count == 0) Console.WriteLine("No comments yet.");
                    ConsoleUi.Pause();
                    break;
                case "1" or "2" or "3" or "4" or "5" or "6" or "8" or "9" or "10" or "12" or "13" or "14" or "16" or "18" or "21" or "22" or "23":
                    ConsoleUi.Pause("Sign in as an active participant to contribute. Anonymous exploration is read only.");
                    break;
                default:
                    ConsoleUi.Pause("That is not a valid exploration action.");
                    break;
            }
        }
    }

    private static Guid? SelectId(
        IReadOnlyList<(Guid Id, string Name)> values,
        string label
    )
    {
        Console.WriteLine($"Select {label} (0 clears):");
        for (var index = 0; index < values.Count; index++)
            Console.WriteLine($"{index + 1}. {values[index].Name}");
        Console.Write("Selection: ");
        if (!int.TryParse(
            Console.ReadLine(),
            out var selection
        ) || selection == 0) return null;
        return selection >= 1 && selection <= values.Count ? values[selection - 1].Id : null;
    }

    public static void BrowseCommunities(
        ICommunityRepository communities,
        ICommunityNodeRepository communityNodes
    )
    {
        if (!Console.IsOutputRedirected) Console.Clear();
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
        if (!Console.IsOutputRedirected) Console.Clear();
        Console.WriteLine("NODE TYPES");
        Console.WriteLine("----------");
        Console.WriteLine("Viewing as: Anonymous");
        Console.WriteLine();

        foreach (var nodeType in nodeTypes.GetAll().Where(type => !type.IsArchived).OrderBy(type => type.Name))
            Console.WriteLine($"- {nodeType.Name}: {nodeType.Description}");

        ConsoleUi.Pause();
    }
}
