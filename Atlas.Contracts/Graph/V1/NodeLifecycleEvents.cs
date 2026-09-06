namespace Atlas.Contracts.Graph.V1;

public abstract record NodeLifecycleEventV1(
    Guid NodeId,
    Guid DescriptionId,
    Guid AuthorId,
    DateTimeOffset OccurredAt);

public sealed record NodeCreatedV1(
    Guid NodeId,
    Guid DescriptionId,
    Guid AuthorId,
    DateTimeOffset OccurredAt)
    : NodeLifecycleEventV1(
        NodeId,
        DescriptionId,
        AuthorId,
        OccurredAt);

public sealed record NodeArchivedV1(
    Guid NodeId,
    Guid DescriptionId,
    Guid AuthorId,
    DateTimeOffset OccurredAt)
    : NodeLifecycleEventV1(
        NodeId,
        DescriptionId,
        AuthorId,
        OccurredAt);

public sealed record NodeRestoredV1(
    Guid NodeId,
    Guid DescriptionId,
    Guid AuthorId,
    DateTimeOffset OccurredAt)
    : NodeLifecycleEventV1(
        NodeId,
        DescriptionId,
        AuthorId,
        OccurredAt);
