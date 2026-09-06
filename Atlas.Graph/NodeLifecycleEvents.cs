namespace Atlas.Graph;

public abstract record NodeLifecycleEvent(
    Guid NodeId,
    Guid DescriptionId,
    DateTimeOffset OccurredAt);

public sealed record NodeCreated(
    Guid NodeId,
    Guid DescriptionId,
    DateTimeOffset OccurredAt)
    : NodeLifecycleEvent(
        NodeId,
        DescriptionId,
        OccurredAt);

public sealed record NodeArchived(
    Guid NodeId,
    Guid DescriptionId,
    DateTimeOffset OccurredAt)
    : NodeLifecycleEvent(
        NodeId,
        DescriptionId,
        OccurredAt);

public sealed record NodeRestored(
    Guid NodeId,
    Guid DescriptionId,
    DateTimeOffset OccurredAt)
    : NodeLifecycleEvent(
        NodeId,
        DescriptionId,
        OccurredAt);