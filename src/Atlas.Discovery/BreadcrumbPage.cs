namespace Atlas.Discovery;

/// <summary>A bounded segment moving from an immediate parent toward a root, along one route.</summary>
public sealed record BreadcrumbPage(
    IReadOnlyList<BreadcrumbItem> Items,
    Guid? ContinuationNodeId,
    bool HasAlternativeRoutes,
    bool CycleDetected)
{
    public const int PageSize = 3;

    /// <summary>Labels are already redacted for moderation; do not look up hidden titles again.</summary>
    public string DisplayText => Items.Count == 0
        ? (CycleDetected ? "[Circular parent link]" : "Root node")
        : string.Join(" → ", Items.Select(item => item.Label)) +
            (CycleDetected ? " → [Circular parent link]" : ContinuationNodeId is not null ? " → …" : "") +
            (HasAlternativeRoutes ? " [other parent routes available]" : "");
}

/// <summary>One public ancestor label, not a domain aggregate or a full ancestor graph.</summary>
public sealed record BreadcrumbItem(Guid NodeId, string Label);
