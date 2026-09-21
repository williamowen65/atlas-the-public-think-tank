using Atlas.Communities.Nodes;
using Atlas.Content.Blocks;
using Atlas.Content.Documents;
using Atlas.Discovery;
using Atlas.Graph.Nodes;
using Atlas.Graph.Reactions;
using Atlas.Voting;
using Atlas.Voting.Data;
using Atlas.Voting.Target;

namespace Atlas.ConsoleApp.Discovery;

/// <summary>
/// Console composition adapter. It is the only place where the current JSON
/// repositories are projected into Discovery's cross-boundary read model.
/// </summary>
internal sealed class RepositoryDiscoveryCandidateSource : IDiscoveryCandidateSource
{
    private readonly IDiscoveryNodeReader _nodes;
    private readonly IDocumentRepository _documents;
    private readonly IVoteRepository _votes;
    private readonly ICommunityNodeRepository _communityNodes;
    private readonly INodeReactionRepository _nodeReactions;

    public RepositoryDiscoveryCandidateSource(
        IDiscoveryNodeReader nodes,
        IDocumentRepository documents,
        IVoteRepository votes,
        ICommunityNodeRepository communityNodes,
        INodeReactionRepository nodeReactions)
    {
        _nodes = nodes;
        _documents = documents;
        _votes = votes;
        _communityNodes = communityNodes;
        _nodeReactions = nodeReactions;
    }

    public IReadOnlyCollection<DiscoveryCandidate> GetCandidates() =>
        _nodes.ReadNodesForDiscovery().Select(node =>
        {
            var summary = new GetVoteSummary(_votes).Execute(new NodeVoteTarget(node.Id.Value));
            var document = _documents.GetById(new DocumentId(node.DescriptionId.Value));
            var content = document is null
                ? string.Empty
                : string.Join(' ', _documents.GetBlocks(document).Select(SearchableText));
            var communityIds = _communityNodes.GetByNode(node.Id.Value)
                .Select(association => association.CommunityId.Value)
                .ToList();
            var reactionIds = _nodeReactions.GetActiveForNode(node.Id)
                .Select(reaction => reaction.ReactionDefinitionId.Value)
                .ToList();

            return new DiscoveryCandidate(
                node.Id.Value,
                node.Title.Value,
                content,
                node.Status == NodeStatus.Archived,
                node.CreatedAt,
                node.UpdatedAt,
                summary.VoteCount,
                summary.AverageVote,
                communityIds,
                reactionIds);
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
