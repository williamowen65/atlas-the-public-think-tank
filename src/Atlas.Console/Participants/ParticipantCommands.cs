using Atlas.Content.Documents;
using Atlas.Discovery;
using Atlas.Graph.Nodes;
using Atlas.Identity;
using Atlas.Graph.Nodes.NodeTypes;
using Atlas.Participants.Participants;
using Atlas.Participants.Profiles;

namespace Atlas.ConsoleApp.Participants;

/// <summary>Coordinates participant profile and authored-node interactions in the console host.</summary>
public static class ParticipantCommands
{
    /// <summary>Runs the interactive participant commands workflow.</summary>
    public static Participant Run(
        ParticipantId participantId,
        IParticipantRepository participants,
        INodeRepository nodes,
        INodeTypeRepository nodeTypes,
        IDocumentRepository documents,
        Participant currentParticipant,
        IUserContext userContext,
        Atlas.Moderation.ModerationService moderation,
        IDiscoveryService? discovery = null,
        Func<Node, Participant, Participant>? openNode = null,
        Action<Node, int, RankedDiscoveryItem?, Participant>? writeNodeRow = null,
        Func<DiscoveryQuery, string, DiscoveryQuery>? changeFilter = null)
    {
        var viewing = true;

        while (viewing)
        {
            var participant = participants.GetById(participantId);

            if (participant is null)
            {
                ConsoleUi.Pause("That participant no longer exists.");
                return currentParticipant;
            }

            var authoredNodes = nodes
                .GetByAuthor(new NodeAuthorId(participant.Id.Value))
                .ToList();

            Console.Clear();
            ParticipantDisplay.WriteProfile(
                participant,
                authoredNodes,
                currentParticipant);
            Console.WriteLine();
            Console.WriteLine("1. Edit profile");
            Console.WriteLine("2. View authored nodes");
            Console.WriteLine("4. Return");
            Console.WriteLine();
            Console.Write("Selection: ");

            switch (Console.ReadLine())
            {
                case "1":
                    currentParticipant = EditProfile(
                        participant,
                        participants,
                        currentParticipant,
                        userContext.ParticipantId);
                    break;

                case "2":
                    currentParticipant = ViewAuthoredNodes(
                        participant,
                        authoredNodes,
                        nodes,
                        nodeTypes,
                        documents,
                        participants,
                        moderation,
                        currentParticipant,
                        discovery,
                        openNode,
                        writeNodeRow,
                        changeFilter);
                    break;

                case "4":
                    viewing = false;
                    break;

                default:
                    ConsoleUi.Pause(
                        "Please select a listed option.");
                    break;
            }
        }

        return currentParticipant;
    }

    /// <summary>Edits profile through the authorized workflow.</summary>
    private static Participant EditProfile(
        Participant participant,
        IParticipantRepository participants,
        Participant currentParticipant,
        Guid actorParticipantId)
    {
        Console.Clear();
        Console.WriteLine("EDIT PARTICIPANT PROFILE");
        Console.WriteLine("------------------------");
        Console.WriteLine(
            $"Editing {participant.DisplayName} " +
            $"as {currentParticipant.DisplayName}.");
        Console.WriteLine();
        Console.Write(
            $"Display name ({participant.DisplayName}): ");
        var displayName = Console.ReadLine();

        Console.WriteLine(
            "Bio (blank keeps the current bio; /clear removes it):");
        Console.Write("> ");
        var bio = Console.ReadLine();

        var requestedDisplayName =
            string.IsNullOrWhiteSpace(displayName)
                ? participant.DisplayName
                : displayName;

        var requestedBio = bio switch
        {
            null or "" => participant.Bio,
            "/clear" => string.Empty,
            _ => bio
        };

        try
        {
            var workflow =
                new UpdateParticipantProfile(participants);

            var updatedParticipant = workflow.Execute(
                new ParticipantId(actorParticipantId),
                participant.Id,
                requestedDisplayName,
                requestedBio,
                DateTimeOffset.UtcNow);

            ConsoleUi.Pause("Profile updated.");

            return currentParticipant.Id == updatedParticipant.Id
                ? updatedParticipant
                : currentParticipant;
        }
        catch (UnauthorizedAccessException exception)
        {
            ConsoleUi.Pause($"Permission denied: {exception.Message}");
        }
        catch (ArgumentException exception)
        {
            ConsoleUi.Pause($"Unable to update profile: {exception.Message}");
        }
        catch (InvalidOperationException exception)
        {
            ConsoleUi.Pause($"Unable to update profile: {exception.Message}");
        }

        return currentParticipant;
    }

    /// <summary>Displays authored nodes in the console workflow.</summary>
    private static Participant ViewAuthoredNodes(
        Participant participant,
        IReadOnlyCollection<Node> authoredNodes,
        INodeRepository nodes,
        INodeTypeRepository nodeTypes,
        IDocumentRepository documents,
        IParticipantRepository participants,
        Atlas.Moderation.ModerationService moderation,
        Participant currentParticipant,
        IDiscoveryService? discovery,
        Func<Node, Participant, Participant>? openNode,
        Action<Node, int, RankedDiscoveryItem?, Participant>? writeNodeRow,
        Func<DiscoveryQuery, string, DiscoveryQuery>? changeFilter)
    {
        var query = new DiscoveryQuery(IncludeArchived: true);
        var visiblePages = 1;
        while (true)
        {
            Console.Clear();
            Console.WriteLine($"NODES AUTHORED BY {participant.DisplayName.ToUpperInvariant()}");
            Console.WriteLine(new string('-', 18 + participant.DisplayName.Length));
            Console.WriteLine($"Search: {query.SearchText ?? "(all)"}; Community: {query.CommunityId?.ToString() ?? "(all)"}; Reactions: {query.ReactionDefinitionIds?.Count ?? 0}");
            Console.WriteLine($"Votes: {query.MinimumVoteCount?.ToString() ?? "any"}–{query.MaximumVoteCount?.ToString() ?? "any"}; Average: {query.MinimumAverageVote?.ToString() ?? "any"}–{query.MaximumAverageVote?.ToString() ?? "any"}; Created: {query.CreatedFrom?.ToString() ?? "any"}–{query.CreatedThrough?.ToString() ?? "any"}");
            Console.WriteLine();
            // Refresh after edits; the profile's initial count may be stale on return.
            var authored = nodes.GetByAuthor(new NodeAuthorId(participant.Id.Value)).ToList();
            var byId = authored.ToDictionary(node => node.Id.Value);
            var pages = discovery is null ? null : Enumerable.Range(1, visiblePages)
                .Select(page => discovery.DiscoverAuthoredPage(query with { Page = page }, byId.Keys.ToList()))
                .ToList();
            var ranked = pages?.SelectMany(page => page.Items).ToList();
            var ordered = ranked is null ? authored : ranked
                .Where(item => byId.ContainsKey(item.NodeId))
                .Select(item => byId[item.NodeId]).ToList();
            if (ordered.Count == 0) Console.WriteLine("No authored nodes.");
            else
            {
                NodeDisplay.WriteTableHeader(includeCreatedDate: true);
                for (var index = 0; index < ordered.Count; index++)
                    if (writeNodeRow is not null)
                        writeNodeRow(ordered[index], index + 1, ranked is null ? null : ranked[index], currentParticipant);
                    else NodeDisplay.WriteTableRow(ordered[index], nodes, nodeTypes, documents,
                        participants, index + 1,
                        ranked is null ? null : ranked[index].VoteCount,
                        ranked is null ? null : ranked[index].AverageVote,
                        includeCreatedDate: true, moderation: moderation);
            }
            Console.WriteLine();
            if (pages is not null) Console.WriteLine($"Showing {ordered.Count} of {pages[^1].TotalCount} matching nodes.");
            Console.WriteLine("Node number to open; S search, C community, R reactions, V vote ranges, D created date, X clear, 0 return.");
            if (pages is not null && pages[^1].HasNextPage) Console.WriteLine("N to load the next page.");
            Console.Write("Selection: ");
            var input = Console.ReadLine()?.Trim();
            if (input == "0") return currentParticipant;
            if (string.Equals(input, "n", StringComparison.OrdinalIgnoreCase) && pages is not null && pages[^1].HasNextPage)
            {
                visiblePages++;
                continue;
            }
            if (changeFilter is not null && input is not null &&
                "SCRVDX".Contains(input.ToUpperInvariant()) && input.Length == 1)
            {
                query = changeFilter(query, input.ToUpperInvariant()) with { Page = 1 };
                visiblePages = 1;
                continue;
            }
            if (!int.TryParse(input, out var selection) || selection < 1 || selection > ordered.Count)
            {
                ConsoleUi.Pause("That node does not exist.");
                continue;
            }
            if (openNode is null)
                ConsoleUi.Pause("Node navigation is unavailable in this view.");
            else currentParticipant = openNode(ordered[selection - 1], currentParticipant);
        }
    }
}
