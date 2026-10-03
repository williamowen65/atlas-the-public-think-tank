using Atlas.Communities.Communities;
using Atlas.Comments.Comments;
using Atlas.Communities.Memberships;
using Atlas.Communities.Nodes;
using Atlas.ConsoleApp.Eventing;
using Atlas.Content.Documents;
using Atlas.Graph.Nodes;
using Atlas.Identity;
using Atlas.Graph.Nodes.NodeTypes;
using Atlas.Graph.Reactions;
using Atlas.Moderation;
using Atlas.Participants.Participants;
using Atlas.Voting;
using Atlas.Voting.Data;

namespace Atlas.ConsoleApp.Communities;

public static class CommunityCommands
{
    public static void Run(
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
        IReactionDefinitionRepository tagDefinitions,
        INodeReactionRepository nodeTags,
        InMemoryEventPublisher eventPublisher,
        IUserContext userContext,
        ICommentRepository comments,
        ModerationService moderation)
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
                    Console.WriteLine($"{index + 1}. {NodeDisplay.PublicTitle(associatedNodes[index], moderation)}");
            }

            var membership = memberships.Get(community.Id, userContext.ParticipantId);
            var isOwner = community.OwnerParticipantId == userContext.ParticipantId;
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
                            NodeCommands.Run(associatedNodes[choice - 1], nodes, nodeTypes, documents, participants, votes, castVote, undoVote, tagDefinitions, nodeTags, eventPublisher, userContext, communities, memberships, communityNodes, service, comments, moderation);
                        }
                        break;
                    case "2" when membership?.IsActive == true:
                        service.Leave(community, userContext.ParticipantId, DateTimeOffset.UtcNow);
                        ConsoleUi.Pause("You left the community.");
                        break;
                    case "2":
                        service.Join(community, userContext.ParticipantId, DateTimeOffset.UtcNow);
                        ConsoleUi.Pause("You joined the community.");
                        break;
                    case "3" when isOwner:
                        Console.Write("New name: ");
                        service.Rename(community, userContext.ParticipantId, Console.ReadLine() ?? string.Empty, DateTimeOffset.UtcNow);
                        break;
                    case "4" when isOwner:
                        Console.Write("New description: ");
                        community.ChangeDescription(userContext.ParticipantId, Console.ReadLine() ?? string.Empty, DateTimeOffset.UtcNow);
                        communities.Save(community);
                        break;
                    case "5" when isOwner:
                        community.Archive(userContext.ParticipantId, DateTimeOffset.UtcNow);
                        communities.Save(community);
                        break;
                    case "6" when isOwner:
                        community.Restore(userContext.ParticipantId, DateTimeOffset.UtcNow);
                        communities.Save(community);
                        break;
                    case "3" or "4" or "5" or "6":
                        ConsoleUi.Pause("This action requires the community owner.");
                        break;
                    case "7": return;
                    default: ConsoleUi.Pause("That is not a valid selection."); break;
                }
            }
            catch (ArgumentException exception) { ConsoleUi.Pause(exception.Message); }
            catch (InvalidOperationException exception) { ConsoleUi.Pause(exception.Message); }
        }
    }
}
