namespace Atlas.Contracts.Graph.V1;

/// <summary>Provides shared version-one metadata for Graph lifecycle integration events.</summary>
public abstract record NodeLifecycleEventV1(
    Guid NodeId,
    Guid DescriptionId,
    Guid AuthorId,
    DateTimeOffset OccurredAt);

/// <summary>Reports that a Graph node was created and identifies its external references.</summary>
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

/// <summary>Reports that a Graph node entered the archived lifecycle state.</summary>
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

/// <summary>Reports that a Graph node returned to the active lifecycle state.</summary>
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

/// <summary>Reports that a parent relationship was attached to a Graph node.</summary>
public sealed record NodeParentAttachedV1(
    Guid NodeId,
    Guid ParentNodeId,
    Guid DescriptionId,
    Guid AuthorId,
    DateTimeOffset OccurredAt)
    : NodeLifecycleEventV1(
        NodeId,
        DescriptionId,
        AuthorId,
        OccurredAt);

/// <summary>Reports that a parent relationship was detached from a Graph node.</summary>
public sealed record NodeParentDetachedV1(
    Guid NodeId,
    Guid ParentNodeId,
    Guid DescriptionId,
    Guid AuthorId,
    DateTimeOffset OccurredAt)
    : NodeLifecycleEventV1(
        NodeId,
        DescriptionId,
        AuthorId,
        OccurredAt);
