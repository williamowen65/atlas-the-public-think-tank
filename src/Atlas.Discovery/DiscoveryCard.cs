namespace Atlas.Discovery;

/// <summary>Immutable public display data; never contains personalized votes or mutable aggregates.</summary>
public sealed record DiscoveryCard(Guid NodeId, string Title, string TypeName, string AuthorName,
    string Status, DateTimeOffset CreatedAt, string Description, string Breadcrumb,
    string SubNodes, string Reactions, string Communities, int CommentCount, bool IsHidden)
{
    public IReadOnlyList<DiscoveryPreviewBlock> PreviewBlocks { get; init; } = [];

    public Guid? AuthorId { get; init; }

    public IReadOnlyList<DiscoveryChildType> ChildTypes { get; init; } = Array.Empty<DiscoveryChildType>();


    /// <summary>Structured first segment and continuation, for future scroll/feed clients.</summary>
    public BreadcrumbPage? BreadcrumbSegment { get; init; }
}

/// <summary>Stable child-type navigation identity and existing count-aware display label.</summary>
public sealed record DiscoveryChildType(Guid TypeId, string Label, int Count);

/// <summary>Display-only content included with a Discovery card.</summary>
public sealed record DiscoveryPreviewBlock(Guid Id, string Kind, DiscoveryPreviewContent Content);

public sealed record DiscoveryPreviewContent(
    string? Markdown, string? Composition, string? Url, string? AltText,
    string? Caption, string? Layout, string? Title, string? Description,
    Guid? PollId, Guid? ChartId);
