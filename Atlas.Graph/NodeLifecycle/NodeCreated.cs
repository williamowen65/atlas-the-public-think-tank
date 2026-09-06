namespace Atlas.Graph.NodeLifecycle;

public sealed record NodeCreated(
    Guid NodeId,
    Guid DescriptionId,
    DateTimeOffset OccurredAt);