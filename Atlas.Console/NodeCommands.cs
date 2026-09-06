using Atlas.ConsoleApp.Eventing;
using Atlas.Content.Documents;
using Atlas.Graph.Nodes;
using Atlas.Graph.Nodes.NodeTypes;
using Atlas.Participants.Participants;

namespace Atlas.ConsoleApp;

public static class NodeCommands
{
    public static void Run(
        Node node,
        INodeRepository nodes,
        INodeTypeRepository nodeTypes,
        IDocumentRepository documents,
        IParticipantRepository participants,
        InMemoryEventPublisher eventPublisher,
        Participant currentParticipant)
    {
        var viewingNode = true;

        while (viewingNode)
        {
            Console.Clear();
            NodeDisplay.WriteDetails(
                node,
                nodes,
                nodeTypes,
                documents,
                participants);

            Console.WriteLine();
            Console.WriteLine("Choose an action:");
            Console.WriteLine("1. Rename");
            Console.WriteLine("2. Change description");
            Console.WriteLine("3. Change type");
            Console.WriteLine("4. Archive");
            Console.WriteLine("5. Restore");
            Console.WriteLine("6. Change requested sub-node types");
            Console.WriteLine("7. Select sub-node");
            Console.WriteLine("8. Add sub-node");
            Console.WriteLine("9. Attach to parent");
            Console.WriteLine("10. Detach from parent");
            Console.WriteLine("11. Return to node browser");
            Console.WriteLine();

            Console.Write("Selection: ");

            try
            {
                switch (Console.ReadLine())
                {
                    case "1":
                        Rename(node, nodes);
                        break;

                    case "2":
                        ChangeDescription(node, nodes, documents);
                        break;

                    case "3":
                        ChangeType(
                            node,
                            nodes,
                            nodeTypes,
                            currentParticipant.Id.Value.ToString());
                        break;

                    case "4":
                        node.Archive(DateTimeOffset.UtcNow);
                        nodes.Save(node);
                        PublishDomainEvents(node, eventPublisher);
                        ConsoleUi.Pause("Node archived and saved.");
                        break;

                    case "5":
                        node.Restore(DateTimeOffset.UtcNow);
                        nodes.Save(node);
                        PublishDomainEvents(node, eventPublisher);
                        ConsoleUi.Pause("Node restored and saved.");
                        break;

                    case "6":
                        ChangeRequestedSubNodeTypes(
                            node,
                            nodes,
                            nodeTypes,
                            currentParticipant.Id.Value.ToString());
                        break;

                    case "7":
                        node = SelectSubNode(
                                   node,
                                   nodes,
                                   nodeTypes,
                                   documents,
                                   participants)
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
                            eventPublisher);
                        break;

                    case "10":
                        DetachFromParent(
                            node,
                            nodes,
                            eventPublisher);
                        break;

                    case "11":
                        viewingNode = false;
                        break;

                    default:
                        ConsoleUi.Pause("That is not a valid selection.");
                        break;
                }
            }
            catch (ArgumentException exception)
            {
                ConsoleUi.Pause(
                    $"Unable to update node: {exception.Message}");
            }
        }
    }




    private static void AddSubNode(
        Node parent,
        INodeRepository nodes,
        INodeTypeRepository nodeTypes,
        IDocumentRepository documents,
        Participant author,
        InMemoryEventPublisher eventPublisher)
    {
        var availableTypes = parent.RequestedSubNodeTypes
            .Select(request => nodeTypes.GetById(request.TypeId))
            .Where(type => type is not null && !type.IsArchived)
            .Cast<NodeTypeDefinition>()
            .OrderBy(type => type.Name)
            .ToList();

        var existingChildren = nodes
            .GetAll()
            .Where(candidate =>
                candidate.ParentNodeIds.Contains(parent.Id))
            .ToList();

        Console.WriteLine();
        Console.WriteLine("Select the type of sub-node to add:");

        for (var index = 0; index < availableTypes.Count; index++)
        {
            var type = availableTypes[index];
            var count = existingChildren.Count(
                child => child.TypeId == type.Id);

            Console.WriteLine(
                $"{index + 1}. {type.Name} ({count})");
        }

        var createTypeSelection = availableTypes.Count + 1;

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

        if (selection == createTypeSelection)
        {
            selectedType = ConsoleUi.CreateCustomNodeType(
                nodeTypes,
                author.Id.Value.ToString());

            if (selectedType is null)
            {
                return;
            }

            parent.RequestSubNodeType(
                selectedType.Id,
                DateTimeOffset.UtcNow);

            nodes.Save(parent);

            Console.WriteLine();
            Console.WriteLine(
                $"{selectedType.Name} is now globally available " +
                $"and requested by {parent.Title}.");
        }
        else
        {
            selectedType = availableTypes[selection - 1];
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

    private static Node? SelectSubNode(
        Node parent,
        INodeRepository nodes,
        INodeTypeRepository nodeTypes,
        IDocumentRepository documents,
        IParticipantRepository participants)
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
                $"{index + 1}. " +
                NodeDisplay.FormatTypeCount(
                    childGroups[index].TypeName,
                    childGroups[index].Children.Count,
                    childGroups[index].AutoPluralize));
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

        for (var index = 0;
             index < selectedGroup.Children.Count;
             index++)
        {
            NodeDisplay.WriteTableRow(
                selectedGroup.Children[index],
                nodes,
                nodeTypes,
                documents,
                participants,
                index + 1);
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



    private static void AttachToParent(
        Node node,
        INodeRepository nodes,
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
            DateTimeOffset.UtcNow);

        nodes.Save(node);
        PublishDomainEvents(node, eventPublisher);

        ConsoleUi.Pause(
            $"Attached {node.Title} to parent {parent.Title}.");
    }

    private static void DetachFromParent(
        Node node,
        INodeRepository nodes,
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
            DateTimeOffset.UtcNow);

        nodes.Save(node);
        PublishDomainEvents(node, eventPublisher);

        ConsoleUi.Pause(
            $"Detached {node.Title} from parent {parent.Title}.");
    }

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

    private static void ChangeRequestedSubNodeTypes(
        Node node,
        INodeRepository nodes,
        INodeTypeRepository nodeTypes,
        string ownerId)
    {
        var selectedTypes =
            ConsoleUi.ReadRequestedSubNodeTypes(
                nodeTypes,
                ownerId);

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
                    changedAt);
            }
        }

        foreach (var selectedTypeId in selectedTypeIds)
        {
            node.RequestSubNodeType(
                selectedTypeId,
                changedAt);
        }

        nodes.Save(node);
        ConsoleUi.Pause(
            "Requested sub-node types updated and saved.");
    }

    private static void Rename(Node node, INodeRepository nodes)
    {
        Console.Write("New title: ");
        var title = Console.ReadLine();

        node.Rename(
            new NodeTitle(title ?? string.Empty),
            DateTimeOffset.UtcNow);

        nodes.Save(node);
        ConsoleUi.Pause("Node renamed and saved.");
    }

    private static void ChangeDescription(
        Node node,
        INodeRepository nodes,
        IDocumentRepository documents)
    {
        var currentDocument = documents.GetById(
            new DocumentId(node.DescriptionId.Value));

        Console.WriteLine("Current description:");
        Console.WriteLine(
            string.IsNullOrWhiteSpace(currentDocument?.Content)
                ? "(none)"
                : currentDocument.Content);
        Console.WriteLine();
        Console.Write("New description (blank clears it): ");
        var description = Console.ReadLine();
        var changedAt = DateTimeOffset.UtcNow;

        var replacement = new Document(
            description ?? string.Empty,
            changedAt);

        documents.Save(replacement);

        node.ReplaceDescriptionReference(
            new NodeDescriptionId(replacement.Id.Value),
            changedAt);

        nodes.Save(node);

        ConsoleUi.Pause(
            $"Description replaced with document {replacement.Id}.");
    }

    private static void ChangeType(
        Node node,
        INodeRepository nodes,
        INodeTypeRepository nodeTypes,
        string actorId)
    {
        var nodeType = ConsoleUi.ReadNodeType(
            nodeTypes,
            actorId);

        if (nodeType is null)
        {
            return;
        }

        node.ChangeType(
            nodeType.Id,
            DateTimeOffset.UtcNow);

        nodes.Save(node);

        ConsoleUi.Pause(
            $"Node type changed to {nodeType.Name} and saved.");
    }
}
