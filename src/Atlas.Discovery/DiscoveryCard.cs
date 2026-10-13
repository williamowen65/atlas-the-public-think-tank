namespace Atlas.Discovery;

/// <summary>Immutable public display data; never contains personalized votes or mutable aggregates.</summary>
public sealed record DiscoveryCard(Guid NodeId, string Title, string TypeName, string AuthorName,
    string Status, DateTimeOffset CreatedAt, string Description, string Breadcrumb,
    string SubNodes, string Reactions, string Communities, int CommentCount, bool IsHidden);
