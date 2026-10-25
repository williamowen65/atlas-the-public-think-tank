namespace Atlas.Discovery;

/// <summary>Global search is separate from the node-only discovery feed.</summary>
public interface IPublicSearch
{
    PublicSearchPage Search(DiscoveryQuery query, string scope = "all");
    IReadOnlyList<CommentPathItem> CommentPath(Guid nodeId, Guid commentId);
}

public sealed record PublicSearchPage(IReadOnlyList<PublicSearchItem> Items, int TotalCount, int Page, int PageSize)
{
    public bool HasNextPage => (long)Page * PageSize < TotalCount;
}
public sealed record PublicSearchItem(Guid NodeId, int VoteCount, double? AverageVote, DiscoveryCard? Card, CommentSearchHit? Comment);
public sealed record CommentSearchHit(Guid Id, string NodeTitle, Guid AuthorId, string AuthorName, DateTimeOffset CreatedAt, string Excerpt, int Score = 0);
public sealed record CommentPathItem(Guid Id, string TargetKind, Guid TargetId, Guid? ParentCommentId, Guid AuthorId, string Body, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt, bool IsRemoved);


/// <summary>Opt-in evidence for developer tooling; the public search endpoint still returns PublicSearchPage.</summary>
public sealed record ExplainedSearchPage(PublicSearchPage Page, IReadOnlyList<SearchRankEvidence> Evidence, int? BestTextRank = null, double? MinimumTextRank = null);
public sealed record SearchRankEvidence(Guid Id, Guid NodeId, string Kind, int TextRank, double Support,
    double Multiplier, double FinalScore, DateTimeOffset TieDate, bool PhraseMatch = false, double TextSignal = 0);
