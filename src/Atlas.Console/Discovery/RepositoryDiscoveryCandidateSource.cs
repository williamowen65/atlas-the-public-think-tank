using Atlas.Communities.Nodes;
using Atlas.Content.Blocks;
using Atlas.Content.Documents;
using Atlas.Discovery;
using Atlas.Graph.Nodes;
using Atlas.Voting;
using Atlas.Voting.Data;
using Atlas.Voting.Target;

namespace Atlas.ConsoleApp.Discovery;

/// <summary>
/// Console composition adapter. It is the only place where the current JSON
/// repositories are projected into Discovery's cross-boundary read model.
/// </summary>
public sealed class RepositoryDiscoveryCandidateSource : IDiscoveryCandidateSource
{
    private readonly INodeRepository _nodes;
    private readonly IDocumentRepository _documents;
    private readonly IVoteRepository _votes;
    private readonly ICommunityNodeRepository _communityNodes;

    public RepositoryDiscoveryCandidateSource(
        INodeRepository nodes,
        IDocumentRepository documents,
        IVoteRepository votes,
        ICommunityNodeRepository communityNodes)
    {
        _nodes = nodes;
        _documents = documents;
        _votes = votes;
        _communityNodes = communityNodes;
    }

    public IReadOnlyCollection<DiscoveryCandidate> GetCandidates() =>
        _nodes.GetAll().Select(node =>
        {
            var summary = new GetVoteSummary(_votes).Execute(new NodeVoteTarget(node.Id.Value));
            var document = _documents.GetById(new DocumentId(node.DescriptionId.Value));
            var content = document is null
                ? string.Empty
                : string.Join(' ', _documents.GetBlocks(document).Select(SearchableText));
            var communityIds = _communityNodes.GetByNode(node.Id.Value)
                .Select(association => association.CommunityId.Value)
                .ToList();

            return new DiscoveryCandidate(
                node.Id.Value,
                node.Title.Value,
                content,
                node.Status == NodeStatus.Archived,
                node.UpdatedAt,
                summary.VoteCount,
                summary.AverageVote,
                communityIds);
        }).ToList();

    private static string SearchableText(ContentBlock block) => block switch
    {
        MarkdownTextBlock markdown => markdown.Markdown,
        ImageBlock image => $"{image.AltText} {image.Caption}",
        VideoBlock video => $"{video.Caption} {video.Url}",
        LinkPreviewBlock link => $"{link.Title} {link.Description} {link.Url}",
        ChartReferenceBlock chart => chart.Title,
        _ => string.Empty
    };
}
