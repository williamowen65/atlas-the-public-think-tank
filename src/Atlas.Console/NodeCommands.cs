using Atlas.ConsoleApp.Eventing;
using Atlas.ConsoleApp.Participants;
using Atlas.Content.Blocks;
using Atlas.Content.Documents;
using Atlas.Graph.Nodes;
using Atlas.Graph.Nodes.NodeTypes;
using Atlas.Graph.Tags;
using Atlas.Participants.Participants;
using Atlas.Voting;
using Atlas.Voting.Data;
using Atlas.Voting.Target;

namespace Atlas.ConsoleApp;

/// <summary>Coordinates interactive node commands without moving domain rules out of Graph.</summary>
public static class NodeCommands
{
    /// <summary>Runs the interactive node commands workflow.</summary>
    public static Participant Run(
        Node node,
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
        var viewingNode = true;

        while (viewingNode)
        {
            var actorParticipantId = currentParticipant.Id.Value;
            var isAuthor = actorParticipantId == node.AuthorId.Value;

            Console.Clear();

            var voteTarget =
                new NodeVoteTarget(node.Id.Value);

            var votingParticipantId =
                new Atlas.Voting.Votes.ParticipantId(
                    currentParticipant.Id.Value);

            var voteSummary =
                new GetVoteSummary(votes).Execute(
                    voteTarget,
                    votingParticipantId);

            var voteCount =
                voteSummary.VoteCount;

            var averageRating =
                voteSummary.AverageVote;

            var myVote =
                voteSummary.CurrentParticipantVote;

            var children = nodes
                .GetAll()
                .Where(candidate =>
                    candidate.ParentNodeIds.Contains(node.Id))
                .ToList();

            var childVoteSummaries =
                new Dictionary<NodeId, NodeVoteSummary>();

            foreach (var child in children)
            {
                var childVoteTarget =
                    new NodeVoteTarget(child.Id.Value);

                var childSummary =
                    new GetVoteSummary(votes).Execute(
                        childVoteTarget,
                        votingParticipantId);

                childVoteSummaries[child.Id] =
                    new NodeVoteSummary(
                        childSummary.VoteCount,
                        childSummary.AverageVote,
                        childSummary.CurrentParticipantVote);
            }

            NodeDisplay.WriteDetails(
                node,
                nodes,
                nodeTypes,
                documents,
                participants,
                nodeTags,
                tagDefinitions,
                votes,
                votingParticipantId,
                voteCount,
                averageRating,
                myVote,
                childVoteSummaries);

            Console.WriteLine();
            Console.WriteLine(
                $"Choose an action (as {currentParticipant.DisplayName}):");

            var authorOnlyStatus = isAuthor
                ? string.Empty
                : " [disabled — requires node author]";

            Console.WriteLine($"1. Rename{authorOnlyStatus}");
            Console.WriteLine($"2. Manage description blocks{authorOnlyStatus}");
            Console.WriteLine($"3. Change type{authorOnlyStatus}");
            Console.WriteLine($"4. Archive{authorOnlyStatus}");
            Console.WriteLine($"5. Restore{authorOnlyStatus}");
            Console.WriteLine(
                $"6. Change requested sub-node types{authorOnlyStatus}");

            Console.WriteLine("7. Select sub-node");
            Console.WriteLine("8. Add sub-node");
            Console.WriteLine($"9. Attach to parent{authorOnlyStatus}");
            Console.WriteLine($"10. Detach from parent{authorOnlyStatus}");
            Console.WriteLine("11. View author profile");
            Console.WriteLine("12. Manage tags");
            Console.WriteLine(
                myVote is null
                    ? "13. Vote on node"
                    : "13. Change your vote");
            Console.WriteLine("14. Undo your vote");
            Console.WriteLine("15. View votes");
            Console.WriteLine("16. Return to node browser");
            Console.WriteLine();

            Console.Write("Selection: ");

            try
            {
                switch (Console.ReadLine())
                {
                    case "1" or "2" or "3" or "4" or "5" or "6" or
                        "9" or "10" when !isAuthor:
                        ConsoleUi.Pause(
                            "This action is disabled because it requires " +
                            "the node author.");
                        break;

                    case "1":
                        Rename(node, nodes, actorParticipantId);
                        break;

                    case "2":
                        DescriptionBlockCommands.Run(
                            node,
                            documents,
                            actorParticipantId);
                        break;

                    case "3":
                        ChangeType(
                            node,
                            nodes,
                            nodeTypes,
                            actorParticipantId);
                        break;

                    case "4":
                        node.Archive(
                            actorParticipantId,
                            DateTimeOffset.UtcNow);
                        nodes.Save(node);
                        PublishDomainEvents(node, eventPublisher);
                        ConsoleUi.Pause("Node archived and saved.");
                        break;

                    case "5":
                        node.Restore(
                            actorParticipantId,
                            DateTimeOffset.UtcNow);
                        nodes.Save(node);
                        PublishDomainEvents(node, eventPublisher);
                        ConsoleUi.Pause("Node restored and saved.");
                        break;

                    case "6":
                        ChangeRequestedSubNodeTypes(
                            node,
                            nodes,
                            nodeTypes,
                            actorParticipantId);
                        break;

                    case "7":
                        node = SelectSubNode(
                                   node,
                                   nodes,
                                   nodeTypes,
                                   documents,
                                   participants,
                                   votes,
                                   nodeTags,
                                   tagDefinitions,
                                   currentParticipant)
                               ?? node;
                        break;

                    case "8":
                        AddSubNode(
                            node,
                            nodes,
                            nodeTypes,
                            documents,
                            currentParticipant,
                            eventPublisher);
                        break;

                    case "9":
                        AttachToParent(
                            node,
                            nodes,
                            actorParticipantId,
                            eventPublisher);
                        break;

                    case "10":
                        DetachFromParent(
                            node,
                            nodes,
                            actorParticipantId,
                            eventPublisher);
                        break;

                    case "11":
                        currentParticipant = ViewAuthorProfile(
                            node,
                            nodes,
                            nodeTypes,
                            documents,
                            participants,
                            currentParticipant);
                        break;

                    case "12":
                        TagCommands.Run(
                            node,
                            tagDefinitions,
                            nodeTags,
                            votes,
                            castVote,
                            currentParticipant);
                        break;

                    case "13":
                        VoteOnNode(
                            node,
                            currentParticipant,
                            castVote);
                        break;

                    case "14":
                        UndoVoteOnNode(
                            node,
                            currentParticipant,
                            undoVote);
                        break;

                    case "15":
                        currentParticipant = ViewNodeVotes(
                            node,
                            nodes,
                            nodeTypes,
                            documents,
                            participants,
                            votes,
                            currentParticipant);
                        break;

                    case "16":
                        viewingNode = false;
                        break;

                    default:
                        ConsoleUi.Pause("That is not a valid selection.");
                        break;
                }
            }
            catch (Exception exception) when (
                exception is ArgumentException or UnauthorizedAccessException)
            {
                ConsoleUi.Pause(
                    $"Unable to update node: {exception.Message}");
            }
            catch (InvalidOperationException exception)
            {
                ConsoleUi.Pause(
                    $"Unable to complete action: {exception.Message}");
            }
        }

        return currentParticipant;
    }




    /// <summary>Adds sub node during the current workflow.</summary>
    private static void AddSubNode(
        Node parent,
        INodeRepository nodes,
        INodeTypeRepository nodeTypes,
        IDocumentRepository documents,
        Participant author,
        InMemoryEventPublisher eventPublisher)
    {
        var requestedTypes = parent.RequestedSubNodeTypes
            .Select(request => nodeTypes.GetById(request.TypeId))
            .Where(type => type is not null && !type.IsArchived)
            .Cast<NodeTypeDefinition>()
            .OrderBy(type => type.Name)
            .ToList();

        var requestedTypeIds = requestedTypes
            .Select(type => type.Id)
            .ToHashSet();

        var otherKnownTypes = nodeTypes
            .GetAll()
            .Where(type =>
                !type.IsArchived &&
                !requestedTypeIds.Contains(type.Id))
            .OrderBy(type => type.Name)
            .ToList();

        var existingChildren = nodes
            .GetAll()
            .Where(candidate =>
                candidate.ParentNodeIds.Contains(parent.Id))
            .ToList();

        Console.WriteLine();
        Console.WriteLine("Requested sub-node types:");

        for (var index = 0; index < requestedTypes.Count; index++)
        {
            var type = requestedTypes[index];
            var count = existingChildren.Count(
                child => child.TypeId == type.Id);

            Console.WriteLine(
                $"{index + 1}. {type.Name} ({count})");
        }

        if (requestedTypes.Count == 0)
        {
            Console.WriteLine("(none)");
        }

        var knownTypeSelection = requestedTypes.Count + 1;
        var createTypeSelection = requestedTypes.Count + 2;

        Console.WriteLine(
            $"{knownTypeSelection}. Use another known node type");
        Console.WriteLine(
            $"{createTypeSelection}. Create a custom sub-node type");
        Console.WriteLine("0. Cancel");
        Console.Write("Type: ");

        if (!int.TryParse(Console.ReadLine(), out var selection) ||
            selection < 0 ||
            selection > createTypeSelection)
        {
            ConsoleUi.Pause("That is not a valid type selection.");
            return;
        }

        if (selection == 0)
        {
            return;
        }

        NodeTypeDefinition? selectedType;

        if (selection == knownTypeSelection)
        {
            selectedType = SelectOtherKnownType(
                otherKnownTypes,
                existingChildren);

            if (selectedType is null)
            {
                return;
            }
        }
        else if (selection == createTypeSelection)
        {
            selectedType = ConsoleUi.CreateCustomNodeType(
                nodeTypes,
                author.Id.Value.ToString());

            if (selectedType is null)
            {
                return;
            }

            Console.WriteLine();
            Console.WriteLine(
                $"{selectedType.Name} is now globally available.");
        }
        else
        {
            selectedType = requestedTypes[selection - 1];
        }

        Console.Clear();
        Console.WriteLine(
            $"ADD {selectedType.Name.ToUpperInvariant()} SUB-NODE");
        Console.WriteLine(
            new string(
                '-',
                $"ADD {selectedType.Name} SUB-NODE".Length));

        NodeCreationWorkflow.Create(
            nodes,
            nodeTypes,
            documents,
            author,
            eventPublisher,
            selectedType,
            parent);
    }

    /// <summary>Selects other known type for the current console workflow.</summary>
    private static NodeTypeDefinition? SelectOtherKnownType(
        IReadOnlyList<NodeTypeDefinition> knownTypes,
        IReadOnlyCollection<Node> existingChildren)
    {
        if (knownTypes.Count == 0)
        {
            ConsoleUi.Pause(
                "There are no other known node types available.");
            return null;
        }

        Console.WriteLine();
        Console.WriteLine("Other known node types:");

        for (var index = 0; index < knownTypes.Count; index++)
        {
            var type = knownTypes[index];
            var count = existingChildren.Count(
                child => child.TypeId == type.Id);

            Console.WriteLine(
                $"{index + 1}. {type.Name} ({count})");
        }

        Console.WriteLine("0. Cancel");
        Console.Write("Type: ");

        if (!int.TryParse(Console.ReadLine(), out var selection) ||
            selection < 0 ||
            selection > knownTypes.Count)
        {
            ConsoleUi.Pause("That is not a valid type selection.");
            return null;
        }

        return selection == 0
            ? null
            : knownTypes[selection - 1];
    }

    /// <summary>Selects sub node for the current console workflow.</summary>
    private static Node? SelectSubNode(
        Node parent,
        INodeRepository nodes,
        INodeTypeRepository nodeTypes,
        IDocumentRepository documents,
        IParticipantRepository participants,
        IVoteRepository votes,
        INodeTagRepository nodeTags,
        ITagDefinitionRepository tagDefinitions,
        Participant currentParticipant)
    {
        var childGroups = nodes
            .GetAll()
            .Where(candidate =>
                candidate.ParentNodeIds.Contains(parent.Id))
            .GroupBy(child => child.TypeId)
            .Select(group => new
            {
                TypeId = group.Key,
                TypeName = nodeTypes.GetById(group.Key)?.Name
                    ?? $"Unknown ({group.Key})",
                AutoPluralize =
                    nodeTypes.GetById(group.Key)?.AutoPluralize
                    ?? true,
                Children = group
                    .OrderBy(child => child.Title.Value)
                    .ToList()
            })
            .OrderBy(group => group.TypeName)
            .ToList();

        if (childGroups.Count == 0)
        {
            ConsoleUi.Pause("This node has no sub-nodes to select.");
            return null;
        }

        Console.WriteLine();
        Console.WriteLine("Select a sub-node type:");

        for (var index = 0; index < childGroups.Count; index++)
        {
            Console.WriteLine(
                $"{index + 1}. {childGroups[index].TypeName} " +
                $"({childGroups[index].Children.Count})");
        }

        Console.WriteLine("0. Cancel");
        Console.Write("Type: ");

        if (!int.TryParse(Console.ReadLine(), out var typeSelection) ||
            typeSelection < 0 ||
            typeSelection > childGroups.Count)
        {
            ConsoleUi.Pause("That is not a valid type selection.");
            return null;
        }

        if (typeSelection == 0)
        {
            return null;
        }

        var selectedGroup = childGroups[typeSelection - 1];

        Console.WriteLine();
        Console.WriteLine(
            $"{selectedGroup.TypeName.ToUpperInvariant()} SUB-NODES");
        Console.WriteLine();
        NodeDisplay.WriteTableHeader();

        var votingParticipantId =  new Atlas.Voting.Votes.ParticipantId(currentParticipant.Id.Value);

        for (var index = 0;
             index < selectedGroup.Children.Count;
             index++)
        {
            var child = selectedGroup.Children[index];

            var voteTarget =
                new NodeVoteTarget(child.Id.Value);

            var voteSummary =
                new GetVoteSummary(votes).Execute(
                    voteTarget,
                    votingParticipantId);

            var voteCount =
                voteSummary.VoteCount;

            var averageRating =
                voteSummary.AverageVote;

            var myVote =
                voteSummary.CurrentParticipantVote;

            NodeDisplay.WriteTableRow(
                child,
                nodes,
                nodeTypes,
                documents,
                participants,
                index + 1,
                voteCount,
                averageRating,
                myVote,
                nodeTags,
                tagDefinitions,
                votes);
        }

        Console.WriteLine();
        Console.WriteLine("0. Cancel");
        Console.Write("Sub-node: ");

        if (!int.TryParse(Console.ReadLine(), out var nodeSelection) ||
            nodeSelection < 0 ||
            nodeSelection > selectedGroup.Children.Count)
        {
            ConsoleUi.Pause("That is not a valid sub-node selection.");
            return null;
        }

        return nodeSelection == 0
            ? null
            : selectedGroup.Children[nodeSelection - 1];
    }

    /// <summary>Displays author profile in the console workflow.</summary>
    private static Participant ViewAuthorProfile(
        Node node,
        INodeRepository nodes,
        INodeTypeRepository nodeTypes,
        IDocumentRepository documents,
        IParticipantRepository participants,
        Participant currentParticipant)
    {
        var authorId = new ParticipantId(node.AuthorId.Value);

        if (participants.GetById(authorId) is null)
        {
            ConsoleUi.Pause(
                "The participant profile for this author was not found.");
            return currentParticipant;
        }

        return ParticipantCommands.Run(
            authorId,
            participants,
            nodes,
            nodeTypes,
            documents,
            currentParticipant);
    }

    /// <summary>Publishes recorded domain events and clears them after dispatch.</summary>
    private static void PublishDomainEvents(
        Node node,
        InMemoryEventPublisher eventPublisher)
    {
        foreach (var domainEvent in node.DomainEvents)
        {
            eventPublisher.Publish(domainEvent);
        }

        node.ClearDomainEvents();
    }



    /// <summary>Attaches a parent relationship and records the corresponding integration event.</summary>
    private static void AttachToParent(
        Node node,
        INodeRepository nodes,
        Guid actorParticipantId,
        InMemoryEventPublisher eventPublisher)
    {
        var candidates = nodes
            .GetAll()
            .Where(candidate =>
                candidate.Id != node.Id &&
                !node.ParentNodeIds.Contains(candidate.Id))
            .OrderBy(candidate => candidate.Title.Value)
            .ToList();

        if (candidates.Count == 0)
        {
            ConsoleUi.Pause("No available parent nodes were found.");
            return;
        }

        Console.WriteLine();
        Console.WriteLine("Available parent nodes:");

        for (var index = 0; index < candidates.Count; index++)
        {
            Console.WriteLine(
                $"{index + 1}. {candidates[index].Title}");
        }

        Console.WriteLine("0. Cancel");
        Console.Write("Parent: ");

        if (!int.TryParse(Console.ReadLine(), out var selection) ||
            selection < 0 ||
            selection > candidates.Count)
        {
            ConsoleUi.Pause("That is not a valid parent selection.");
            return;
        }

        if (selection == 0)
        {
            return;
        }

        var parent = candidates[selection - 1];

        if (WouldCreateCycle(node.Id, parent, nodes))
        {
            ConsoleUi.Pause(
                "That link would create a circular parent chain.");
            return;
        }

        node.AttachToParent(
            parent.Id,
            actorParticipantId,
            DateTimeOffset.UtcNow);

        nodes.Save(node);
        PublishDomainEvents(node, eventPublisher);

        ConsoleUi.Pause(
            $"Attached {node.Title} to parent {parent.Title}.");
    }

    /// <summary>Detaches a parent relationship and records the corresponding integration event.</summary>
    private static void DetachFromParent(
        Node node,
        INodeRepository nodes,
        Guid actorParticipantId,
        InMemoryEventPublisher eventPublisher)
    {
        var parents = node.ParentNodeIds
            .Select(parentId => nodes.GetById(parentId))
            .Where(parent => parent is not null)
            .Cast<Node>()
            .OrderBy(parent => parent.Title.Value)
            .ToList();

        if (parents.Count == 0)
        {
            ConsoleUi.Pause("This node has no parents to detach.");
            return;
        }

        Console.WriteLine();
        Console.WriteLine("Current parents:");

        for (var index = 0; index < parents.Count; index++)
        {
            Console.WriteLine(
                $"{index + 1}. {parents[index].Title}");
        }

        Console.WriteLine("0. Cancel");
        Console.Write("Parent to detach: ");

        if (!int.TryParse(Console.ReadLine(), out var selection) ||
            selection < 0 ||
            selection > parents.Count)
        {
            ConsoleUi.Pause("That is not a valid parent selection.");
            return;
        }

        if (selection == 0)
        {
            return;
        }

        var parent = parents[selection - 1];

        node.DetachFromParent(
            parent.Id,
            actorParticipantId,
            DateTimeOffset.UtcNow);

        nodes.Save(node);
        PublishDomainEvents(node, eventPublisher);

        ConsoleUi.Pause(
            $"Detached {node.Title} from parent {parent.Title}.");
    }

    /// <summary>Checks the current graph before a host-coordinated parent attachment.</summary>
    private static bool WouldCreateCycle(
        NodeId childNodeId,
        Node proposedParent,
        INodeRepository nodes)
    {
        var pending = new Stack<NodeId>(
            proposedParent.ParentNodeIds);
        var visited = new HashSet<NodeId>();

        while (pending.Count > 0)
        {
            var currentId = pending.Pop();

            if (currentId == childNodeId)
            {
                return true;
            }

            if (!visited.Add(currentId))
            {
                continue;
            }

            var current = nodes.GetById(currentId);

            if (current is null)
            {
                continue;
            }

            foreach (var parentId in current.ParentNodeIds)
            {
                pending.Push(parentId);
            }
        }

        return false;
    }

    /// <summary>Changes requested sub node types during the current workflow.</summary>
    private static void ChangeRequestedSubNodeTypes(
        Node node,
        INodeRepository nodes,
        INodeTypeRepository nodeTypes,
        Guid actorParticipantId)
    {
        var selectedTypes =
            ConsoleUi.ReadRequestedSubNodeTypes(
                nodeTypes,
                actorParticipantId.ToString());

        if (selectedTypes.Count == 0)
        {
            return;
        }

        var selectedTypeIds = selectedTypes
            .Select(type => type.Id)
            .ToHashSet();
        var changedAt = DateTimeOffset.UtcNow;

        foreach (var existingRequest in
                 node.RequestedSubNodeTypes.ToList())
        {
            if (!selectedTypeIds.Contains(existingRequest.TypeId))
            {
                node.StopRequestingSubNodeType(
                    existingRequest.TypeId,
                    actorParticipantId,
                    changedAt);
            }
        }

        foreach (var selectedTypeId in selectedTypeIds)
        {
            node.RequestSubNodeType(
                selectedTypeId,
                actorParticipantId,
                changedAt);
        }

        nodes.Save(node);
        ConsoleUi.Pause(
            "Requested sub-node types updated and saved.");
    }

    /// <summary>Changes the validated name and advances the modification timestamp when the value differs.</summary>
    private static void Rename(
        Node node,
        INodeRepository nodes,
        Guid actorParticipantId)
    {
        Console.Write("New title: ");
        var title = Console.ReadLine();

        node.Rename(
            new NodeTitle(title ?? string.Empty),
            actorParticipantId,
            DateTimeOffset.UtcNow);

        nodes.Save(node);
        ConsoleUi.Pause("Node renamed and saved.");
    }

    /// <summary>Changes the node type and advances the modification timestamp when the value differs.</summary>
    private static void ChangeType(
        Node node,
        INodeRepository nodes,
        INodeTypeRepository nodeTypes,
        Guid actorParticipantId)
    {
        var nodeType = ConsoleUi.ReadNodeType(
            nodeTypes,
            actorParticipantId.ToString());

        if (nodeType is null)
        {
            return;
        }

        node.ChangeType(
            nodeType.Id,
            actorParticipantId,
            DateTimeOffset.UtcNow);

        nodes.Save(node);

        ConsoleUi.Pause(
            $"Node type changed to {nodeType.Name} and saved.");
    }

    /// <summary>Reads and casts the current participant's rating for a node.</summary>
    private static void VoteOnNode(
        Node node,
        Participant currentParticipant,
        CastVote castVote)
    {
        Console.Write("Rating from 0 through 10: ");

        if (!int.TryParse(Console.ReadLine(), out var rating) ||
            rating < 0 ||
            rating > 10)
        {
            ConsoleUi.Pause(
                "Please enter a whole number from 0 through 10.");
            return;
        }

        var voteTarget = new NodeVoteTarget(node.Id.Value);

        var votingParticipantId =
            new Atlas.Voting.Votes.ParticipantId(
                currentParticipant.Id.Value);

        castVote.Execute(
            voteTarget,
            votingParticipantId,
            rating);

        ConsoleUi.Pause("Vote saved.");
    }

    /// <summary>
    /// Requests removal of the current participant's vote.
    /// Voting enforces participant eligibility and target availability.
    /// </summary>
    private static void UndoVoteOnNode(
        Node node,
        Participant currentParticipant,
        UndoVote undoVote)
    {
        var voteTarget =
            new NodeVoteTarget(node.Id.Value);

        var votingParticipantId =
            new Atlas.Voting.Votes.ParticipantId(
                currentParticipant.Id.Value);

        var removed = undoVote.Execute(
            voteTarget,
            votingParticipantId);

        ConsoleUi.Pause(
            removed
                ? "Your vote was removed."
                : "You do not have a current vote on this node.");
    }

    /// <summary>
    /// Displays the participants who voted on a node and allows
    /// navigation to a selected participant profile.
    /// </summary>
    private static Participant ViewNodeVotes(
        Node node,
        INodeRepository nodes,
        INodeTypeRepository nodeTypes,
        IDocumentRepository documents,
        IParticipantRepository participants,
        IVoteRepository votes,
        Participant currentParticipant)
    {

        // Fetch Votes
        var voteTarget = new NodeVoteTarget(node.Id.Value);

        var nodeVotes = votes
            .GetTargetVotes(voteTarget)
            .OrderByDescending(vote => vote.Value.Value)
            .ToList();

        if (nodeVotes.Count == 0)
        {
            ConsoleUi.Pause(
                "No participants have voted on this node.");
            return currentParticipant;
        }

        // Match votes to participants
        var voterRows = nodeVotes
            .Select(vote =>
            {
                var participantId =
                    new ParticipantId(
                        vote.ParticipantId.Id);

                var participant =
                    participants.GetById(participantId);

                return new
                {
                    Vote = vote,
                    Participant = participant
                };
            })
            .ToList();

        
        // Add voter list heading
        Console.Clear();
        Console.WriteLine($"VOTES FOR: {node.Title}");
        Console.WriteLine();

        NodeDisplay.WriteVoterListHeader();

        // Display each voter
        for (var index = 0;
            index < voterRows.Count;
            index++)
        {
            var row = voterRows[index];

            var participantName =
                row.Participant?.DisplayName
                ?? $"Unknown ({row.Vote.ParticipantId.Id})";

            NodeDisplay.WriteVoterListRow(
                index + 1,
                participantName,
                row.Vote.Value.Value);
        }

        // Prompt for a voter selection
        Console.WriteLine();
        Console.WriteLine(
            "Select a voter to view their participant profile.");
        Console.WriteLine(
            "Enter 0 to return to the node.");
        Console.WriteLine();
        Console.Write("Selection: ");

        // Validate Input
        if (!int.TryParse(Console.ReadLine(), out var selection) ||
            selection < 0 ||
            selection > voterRows.Count)
        {
            ConsoleUi.Pause(
                "That is not a valid voter selection.");

            return currentParticipant;
        }

        if (selection == 0)
        {
            return currentParticipant;
        }

        // Get the selected voter
        var selectedRow = voterRows[selection - 1];
        if (selectedRow.Participant is null)
        {
            ConsoleUi.Pause(
                "That participant profile could not be found.");

            return currentParticipant;
        }

        // Open the participant profile
        return ParticipantCommands.Run(
            selectedRow.Participant.Id,
            participants,
            nodes,
            nodeTypes,
            documents,
            currentParticipant);
        }

}
