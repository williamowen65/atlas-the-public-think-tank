namespace Atlas.Graph.NodeLifecycle;

public sealed record NodeCreated(
    Guid NodeId,
    string InitialDescription,
    DateTimeOffset OccurredAt);