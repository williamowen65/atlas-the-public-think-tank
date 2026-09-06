namespace Atlas.Graph;

public abstract record NodeLifecycleEvent(
    Guid NodeId,
    Guid DescriptionId,
    Guid AuthorId,
    DateTimeOffset OccurredAt);

public sealed record NodeCreated(
    Guid NodeId,
    Guid DescriptionId,
    Guid AuthorId,
    DateTimeOffset OccurredAt)
    : NodeLifecycleEvent(
        NodeId,
        DescriptionId,
        AuthorId,
        OccurredAt);

public sealed record NodeArchived(
    Guid NodeId,
    Guid DescriptionId,
    Guid AuthorId,
    DateTimeOffset OccurredAt)
    : NodeLifecycleEvent(
        NodeId,
        DescriptionId,
        AuthorId,
        OccurredAt);

public sealed record NodeRestored(
    Guid NodeId,
    Guid DescriptionId,
    Guid AuthorId,
    DateTimeOffset OccurredAt)
    : NodeLifecycleEvent(
        NodeId,
        DescriptionId,
        AuthorId,
        OccurredAt);
