using Atlas.Contracts.Graph.V1;
using Atlas.Graph;

namespace Atlas.ConsoleApp.Eventing;

public static class GraphEventContractMapper
{
    public static NodeLifecycleEventV1 ToIntegrationContract(
        NodeLifecycleEvent domainEvent)
    {
        return domainEvent switch
        {
            NodeCreated message => new NodeCreatedV1(
                message.NodeId,
                message.DescriptionId,
                message.AuthorId,
                message.OccurredAt),

            NodeArchived message => new NodeArchivedV1(
                message.NodeId,
                message.DescriptionId,
                message.AuthorId,
                message.OccurredAt),

            NodeRestored message => new NodeRestoredV1(
                message.NodeId,
                message.DescriptionId,
                message.AuthorId,
                message.OccurredAt),

            _ => throw new NotSupportedException(
                $"No integration contract mapping exists for " +
                $"{domainEvent.GetType().Name}.")
        };
    }
}
