using Atlas.Communities.Communities;
using Atlas.Communities.Memberships;
using Atlas.Communities.Nodes;
using Atlas.ConsoleApp.Eventing;
using Atlas.Content.Documents;
using Atlas.Graph.Nodes;
using Atlas.Graph.Nodes.NodeTypes;
using Atlas.Graph.Tags;
using Atlas.Participants.Participants;
using Atlas.Voting;
using Atlas.Voting.Data;

namespace Atlas.ConsoleApp.Communities;

public static class CommunityCommands
{
    public static Participant Run(
        Community community,
        ICommunityRepository communities,
        ICommunityMembershipRepository memberships,
        ICommunityNodeRepository communityNodes,
        CommunityService service,
        INodeRepository nodes,
        INodeTypeRepository nodeTypes,
        IDocumentRepository documents,
        IParticipantRepository participants,
        IVoteRepository votes,
        CastVote castVote,
        UndoVote undoVote,
        ITagDefinitionRepository tagDefinitions,
        INodeTagRepository nodeTags,
        InMemoryEventPublisher eventPublisher,
        Participant currentParticipant)
    {
        while (true)
        {
            Console.Clear();
            CommunityDisplay.WriteDetails(community, participants, memberships, communityNodes);
            var associatedNodes = communityNodes.GetByCommunity(community.Id)
                .Select(x => nodes.GetById(new NodeId(x.NodeId)))
                .Where(x => x is not null)
                .Cast<Node>()
                .OrderBy(x => x.Title.Value)
                .ToList();

            Console.WriteLine();
            Console.WriteLine("COMMUNITY CONTENT");
            Console.WriteLine("-----------------");
            if (associatedNodes.Count == 0) Console.WriteLine("No nodes are associated with this community.");
            else
            {
                for (var index = 0; index < associatedNodes.Count; index++)
                    Console.WriteLine($"{index + 1}. {associatedNodes[index].Title}");
            }

            var membership = memberships.Get(community.Id, currentParticipant.Id.Value);
            var isOwner = community.OwnerParticipantId == currentParticipant.Id.Value;
            Console.WriteLine();
            Console.WriteLine("1. Select community node");
            Console.WriteLine(membership?.IsActive == true ? "2. Leave community" : "2. Join community");
            Console.WriteLine($"3. Rename{(isOwner ? "" : " [disabled — requires owner]")}");
            Console.WriteLine($"4. Edit description{(isOwner ? "" : " [disabled — requires owner]")}");
            Console.WriteLine($"5. Archive{(isOwner ? "" : " [disabled — requires owner]")}");
            Console.WriteLine($"6. Restore{(isOwner ? "" : " [disabled — requires owner]")}");
            Console.WriteLine("7. Return");
            Console.Write("Selection: ");

            try
            {
                switch (Console.ReadLine())
                {
                    case "1":
                        Console.Write("Node number (0 cancels): ");
                        if (int.TryParse(Console.ReadLine(), out var choice) && choice > 0 && choice <= associatedNodes.Count)
                        {
                            currentParticipant = NodeCommands.Run(associatedNodes[choice - 1], nodes, nodeTypes, documents, participants, votes, castVote, undoVote, tagDefinitions, nodeTags, eventPublisher, currentParticipant, communities, memberships, communityNodes, service);
                        }
                        break;
                    case "2" when membership?.IsActive == true:
                        service.Leave(community, currentParticipant.Id.Value, DateTimeOffset.UtcNow);
                        ConsoleUi.Pause("You left the community.");
                        break;
                    case "2":
                        service.Join(community, currentParticipant.Id.Value, DateTimeOffset.UtcNow);
                        ConsoleUi.Pause("You joined the community.");
                        break;
                    case "3" when isOwner:
                        Console.Write("New name: ");
                        service.Rename(community, currentParticipant.Id.Value, Console.ReadLine() ?? string.Empty, DateTimeOffset.UtcNow);
                        break;
                    case "4" when isOwner:
                        Console.Write("New description: ");
                        community.ChangeDescription(currentParticipant.Id.Value, Console.ReadLine() ?? string.Empty, DateTimeOffset.UtcNow);
                        communities.Save(community);
                        break;
                    case "5" when isOwner:
                        community.Archive(currentParticipant.Id.Value, DateTimeOffset.UtcNow);
                        communities.Save(community);
                        break;
                    case "6" when isOwner:
                        community.Restore(currentParticipant.Id.Value, DateTimeOffset.UtcNow);
                        communities.Save(community);
                        break;
                    case "3" or "4" or "5" or "6":
                        ConsoleUi.Pause("This action requires the community owner.");
                        break;
                    case "7": return currentParticipant;
                    default: ConsoleUi.Pause("That is not a valid selection."); break;
                }
            }
            catch (ArgumentException exception) { ConsoleUi.Pause(exception.Message); }
            catch (InvalidOperationException exception) { ConsoleUi.Pause(exception.Message); }
        }
    }
}
