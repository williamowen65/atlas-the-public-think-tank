using Atlas.ConsoleApp.Eventing;
using NodeCreationCoordinator = Atlas.Application.NodeCreationService;
using Atlas.Content.Blocks;
using Atlas.Content.Documents;
using Atlas.Graph.Nodes;
using Atlas.Graph.Nodes.NodeTypes;
using Atlas.Participants.Participants;

namespace Atlas.ConsoleApp;

/// <summary>Coordinates document-first creation across Content, Graph, persistence, and event publication.</summary>
public static class NodeCreationWorkflow
{
    /// <summary>Coordinates creation and persistence for this workflow.</summary>
    public static Node? Create(
        INodeRepository nodes,
        INodeTypeRepository nodeTypes,
        IDocumentRepository documents,
        Guid authorParticipantId,
        InMemoryEventPublisher eventPublisher,
        NodeTypeDefinition? preselectedType = null,
        Node? parent = null
    )
    {
        if (preselectedType is not null)
        {
            Console.WriteLine($"Type:   {preselectedType.Name}");
        }

        if (parent is not null)
        {
            Console.WriteLine($"Parent: {parent.Title}");
        }

        if (preselectedType is not null || parent is not null)
        {
            Console.WriteLine();
        }

        // This display is advisory. The atomic save rechecks after acquiring the graph lock.
        if (nodes is INodeCreationQuota quota)
        {
            var allowance = quota.GetCreationAllowance(authorParticipantId);
            Console.WriteLine($"Node allowance: {allowance.Remaining} of {allowance.Limit} remaining per rolling {allowance.Window.TotalHours:g} hours.");
            if (allowance.NextSlotAt is { } nextSlot)
                Console.WriteLine($"Next slot available: {nextSlot:yyyy-MM-dd HH:mm:ss} UTC (slots return individually).");
            if (allowance.Remaining == 0)
            {
                ConsoleUi.Pause(new NodeCreationQuotaExceededException(allowance).Message);
                return null;
            }
        }

        Console.Write("Title: ");
        var title = Console.ReadLine();

        Console.Write("Description (optional): ");
        var description = Console.ReadLine();

        var nodeType = preselectedType
            ?? ConsoleUi.ReadNodeType(
                nodeTypes,
                authorParticipantId.ToString()
            );

        if (nodeType is null)
        {
            return null;
        }

        var requestedSubNodeTypes =
            ConsoleUi.ReadRequestedSubNodeTypes(
                nodeTypes,
                authorParticipantId.ToString()
            );

        if (requestedSubNodeTypes is null)
        {
            return null;
        }

        try
        {
            var node = new NodeCreationCoordinator(
                nodes,
                documents
            ).Create(
                title ?? string.Empty,
                description ?? string.Empty,
                nodeType.Id,
                authorParticipantId,
                requestedSubNodeTypes.Select(type => type.Id),
                parent?.Id
            );

            Console.WriteLine(
                $"[ATLAS.GRAPH] Saved node {node.Id} with " +
                $"description reference {node.DescriptionId}.");

            if (nodes is INodeCreationQuota updatedQuota)
            {
                var allowance = updatedQuota.GetCreationAllowance(authorParticipantId);
                Console.WriteLine($"Remaining Node allowance: {allowance.Remaining}/{allowance.Limit}.");
                if (allowance.NextSlotAt is { } nextSlot)
                    Console.WriteLine($"Next slot available: {nextSlot:yyyy-MM-dd HH:mm:ss} UTC.");
            }

            foreach (var domainEvent in node.DomainEvents)
            {
                eventPublisher.Publish(domainEvent);
            }

            node.ClearDomainEvents();

            var relationship = parent is null
                ? string.Empty
                : $" under {parent.Title}";

            ConsoleUi.Pause(
                $"Node created as {nodeType.Name}{relationship}: " +
                $"{node.Title}");

            return node;
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
        {
            ConsoleUi.Pause(
                $"Unable to create node: {exception.Message}");
            return null;
        }
    }
}
