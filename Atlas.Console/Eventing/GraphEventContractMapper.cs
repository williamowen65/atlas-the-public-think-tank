using Atlas.Contracts.Graph.V1;
using Atlas.Graph;

namespace Atlas.ConsoleApp.Eventing;

public static class GraphEventContractMapper
{
    public static NodeLifecycleEventV1 ToIntegrationContract(
        NodeLifecycleEventV1 domainEvent)
    {
        return domainEvent switch
        {
            NodeCreatedV1 message => new NodeCreatedV1(
                message.NodeId,
                message.DescriptionId,
                message.AuthorId,
                message.OccurredAt),

            NodeArchivedV1 message => new NodeArchivedV1(
                message.NodeId,
                message.DescriptionId,
                message.AuthorId,
                message.OccurredAt),

            NodeRestoredV1 message => new NodeRestoredV1(
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
