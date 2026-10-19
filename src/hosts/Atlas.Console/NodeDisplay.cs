using Atlas.Application.Queries;
using Atlas.Contracts.Observability;
using Atlas.Content.Blocks;
using Atlas.Comments.Comments;
using Atlas.Communities.Communities;
using Atlas.Communities.Nodes;
using Atlas.ConsoleApp.Communities;
using Atlas.Content.Documents;
using Atlas.Graph.Nodes;
using Atlas.Graph.Nodes.NodeTypes;
using Atlas.Graph.Reactions;
using Atlas.Moderation;
using Atlas.Participants.Participants;
using Atlas.Voting.Data;
using VotingParticipantId = Atlas.Voting.Votes.ParticipantId;

namespace Atlas.ConsoleApp;

/// <summary>Formats Graph nodes and their referenced data for the console host.</summary>
public static class NodeDisplay
{
    public static string PublicTitle(
        Node node,
        ModerationService moderation
    )
    {
        var visibility = moderation.Visibility(node.Id.Value);
        return visibility.IsHidden ? visibility.Notice : node.Title.Value;
    }
    private const int TitleWidth = 40;
    private const int TypeWidth = 14;
    private const int AuthorWidth = 20;
    private const int DescriptionWidth = 40;
    private const int StatusWidth = 10;
    private const int CreatedWidth = 10;
    private const int TagsWidth = 40;
    private const int CommunitiesWidth = 24;
    private const int CommentsWidth = 8;
    private const int SubNodesMinimumWidth = 52;

    private const int VoteCountWidth = 5;
    private const int AverageVoteWidth = 5;
    private const int CurrentVoteWidth = 7;

    /// <summary>Writes table header to the console display.</summary>
    public static void WriteTableHeader(bool includeCreatedDate = false)
    {
        // Selection numbers belong to the cards; a wide table header is unnecessary.
    }

    /// <summary>Writes table row to the console display.</summary>
    public static void WriteTableRow(
        Node node,
        INodeRepository nodes,
        INodeTypeRepository nodeTypes,
        IDocumentRepository documents,
        IParticipantRepository participants,
        int number,
        int? voteCount = null,
        double? averageVote = null,
        int? currentParticipantVote = null,
        INodeReactionRepository? nodeTags = null,
        IReactionDefinitionRepository? tagDefinitions = null,
        IVoteRepository? votes = null,
        ICommunityRepository? communities = null,
        ICommunityNodeRepository? communityNodes = null,
        ICommentRepository? comments = null,
        bool includeCreatedDate = false,
        ModerationService? moderation = null
    )
    {
        var visibility = moderation?.Visibility(node.Id.Value) ?? ModerationVisibility.Visible;
        var typeName = ResolveTypeName(
            node,
            nodeTypes
        );
        var description = visibility.IsHidden ? visibility.Notice
            : ResolveFeedDescription(
                node,
                documents
            );
        var authorName = ResolveAuthorName(
            node,
            participants
        );
        var subNodeSummary =
            ResolveSubNodeSummary(
                node,
                nodes,
                nodeTypes
            );

        Console.WriteLine();
        WriteWrapped(
            $"{number}. {typeName} · ",
            visibility.IsHidden ? visibility.Notice : node.Title.Value
        );
        Console.WriteLine();
        WriteBreadcrumb(ResolveBreadcrumb(
            node,
            nodes,
            nodeTypes,
            moderation
        ));
        Console.WriteLine($"   By {authorName} · {node.Status}" +
            (includeCreatedDate ? $" · Created {node.CreatedAt:yyyy-MM-dd}" : string.Empty));
        Console.WriteLine();
        WriteWrapped(
            "   ",
            description
        );
        Console.WriteLine();
        Console.WriteLine($"   Importance: {FormatVoteCount(voteCount)} votes · " +
            $"avg {FormatAverageVote(averageVote)} · " +
            $"my rating {FormatCurrentParticipantVote(currentParticipantVote)}");
        WriteWrapped(
            "   Sub-nodes: ",
            subNodeSummary
        );
        WriteWrapped(
            "   Reactions: ",
            ResolveTags(
                node,
                nodeTags,
                tagDefinitions,
                votes
            )
        );
        WriteWrapped(
            "   Communities: ",
            ResolveCommunities(
                node,
                communities,
                communityNodes
            )
        );
        Console.WriteLine($"   Comments: {ResolveCommentCount(
            node,
            comments
        )}");
    }

    /// <summary>Renders a batched immutable Discovery card without repository fan-out.</summary>
    public static void WriteDiscoveryCard(Atlas.Discovery.RankedDiscoveryItem item, int number, int? currentVote = null)
    {
        var card = item.Card ?? throw new ArgumentException("Discovery card is required.");
        Console.WriteLine();
        WriteWrapped($"{number}. {card.TypeName} · ", card.Title);
        Console.WriteLine();
        WriteBreadcrumb(card.Breadcrumb);
        Console.WriteLine($"   By {card.AuthorName} · {card.Status} · Created {card.CreatedAt:yyyy-MM-dd}");
        Console.WriteLine();
        WriteWrapped("   ", card.Description);
        Console.WriteLine();
        Console.WriteLine($"   Importance: {FormatVoteCount(item.VoteCount)} votes · avg {FormatAverageVote(item.AverageVote)} · my rating {FormatCurrentParticipantVote(currentVote)}");
        WriteWrapped("   Sub-nodes: ", card.SubNodes);
        WriteWrapped("   Reactions: ", card.Reactions);
        WriteWrapped("   Communities: ", card.Communities);
        Console.WriteLine($"   Comments: {card.CommentCount}");
    }

    /// <summary>Writes details to the console display.</summary>
    public static void WriteDetails(
        Node node,
        INodeRepository nodes,
        INodeTypeRepository nodeTypes,
        IDocumentRepository documents,
        IParticipantRepository participants,
        INodeReactionRepository nodeTags,
        IReactionDefinitionRepository tagDefinitions,
        IVoteRepository votes,
        ICommunityRepository? communities = null,
        ICommunityNodeRepository? communityNodes = null,
        ICommentRepository? comments = null,
        VotingParticipantId? votingParticipantId = null,
        int? voteCount = null,
        double? averageVote = null,
        int? currentParticipantVote = null,
        IReadOnlyDictionary<NodeId, NodeVoteSummary>? childVoteSummaries = null,
        ModerationService? moderation = null,
        bool showOriginalContent = false
    )
    {
        // Covers both signed-in and anonymous detail rendering. Keep input prompts and
        // mutations outside this privacy-isolated measurement; no IDs enter its envelope.
        Telemetry.ObserveNodeView(() => WriteDetailsCore(
            node, nodes, nodeTypes, documents, participants, nodeTags, tagDefinitions, votes, communities, communityNodes, comments, votingParticipantId, voteCount, averageVote, currentParticipantVote, childVoteSummaries, moderation, showOriginalContent));
    }

    private static void WriteDetailsCore(
        Node node,
        INodeRepository nodes,
        INodeTypeRepository nodeTypes,
        IDocumentRepository documents,
        IParticipantRepository participants,
        INodeReactionRepository nodeTags,
        IReactionDefinitionRepository tagDefinitions,
        IVoteRepository votes,
        ICommunityRepository? communities = null,
        ICommunityNodeRepository? communityNodes = null,
        ICommentRepository? comments = null,
        VotingParticipantId? votingParticipantId = null,
        int? voteCount = null,
        double? averageVote = null,
        int? currentParticipantVote = null,
        IReadOnlyDictionary<NodeId, NodeVoteSummary>? childVoteSummaries = null,
        ModerationService? moderation = null,
        bool showOriginalContent = false
    )
    {
        var visibility = moderation?.Visibility(node.Id.Value) ?? ModerationVisibility.Visible;
        var hidden = visibility.IsHidden && !showOriginalContent;
        var description = hidden ? visibility.Notice : showOriginalContent
            ? ResolveDescription(
                node,
                documents,
                includeBlockDetails: true
            )
            : ResolveFeedDescription(
                node,
                documents
            );
        var authorName = ResolveAuthorName(
            node,
            participants
        );

        Console.WriteLine();
        Console.WriteLine("ATLAS NODE");
        Console.WriteLine();
        WriteWrapped(
            "  ",
            $"{ResolveTypeName(
                node,
                nodeTypes
            )} · {(hidden ? visibility.Notice : node.Title.Value)}"
        );
        Console.WriteLine();
        WriteBreadcrumb(
            ResolveBreadcrumb(
                node,
                nodes,
                nodeTypes,
                moderation
            ),
            indent: 2
        );
        Console.WriteLine($"  By {authorName} · {node.Status} · Created {node.CreatedAt:yyyy-MM-dd} · Updated {node.UpdatedAt:yyyy-MM-dd}");
        Console.WriteLine();
        WriteWrapped(
            "  ",
            description
        );
        Console.WriteLine();
        Console.WriteLine($"  Importance: {FormatVoteCount(voteCount)} votes · avg {FormatAverageVote(averageVote)} · my rating {FormatCurrentParticipantVote(currentParticipantVote)}");
        Console.WriteLine($"  Communities: {ResolveCommunities(
            node,
            communities,
            communityNodes
        )} · Comments: {ResolveCommentCount(
            node,
            comments
        )}");
        Console.WriteLine($"  Node ID: {node.Id}");

        ReactionDisplay.WriteDetails(
            node,
            nodeTags,
            tagDefinitions,
            votes,
            votingParticipantId
        );

        Console.WriteLine();
        Console.WriteLine("SUB-NODES");

        WriteSubNodeTables(
            node,
            nodes,
            nodeTypes,
            documents,
            participants,
            nodeTags,
            tagDefinitions,
            votes,
            comments,
            childVoteSummaries,
            moderation
        );
    }

    private static string ResolveCommentCount(
        Node node,
        ICommentRepository? comments
    ) =>
        comments is null ? "—" : comments.CountByTarget(CommentTarget.Node(node.Id.Value)).ToString();

    private static string ResolveCommunities(
        Node node,
        ICommunityRepository? communities,
        ICommunityNodeRepository? communityNodes
    )
    {
        if (communities is null || communityNodes is null) return "None";
        var names = CommunityDisplay.NamesForNode(
            node.Id.Value,
            communityNodes,
            communities
        );
        return string.IsNullOrWhiteSpace(names) ? "None" : names;
    }

    /// <summary>Writes sub node tables to the console display.</summary>
    private static void WriteSubNodeTables(
        Node node,
        INodeRepository nodes,
        INodeTypeRepository nodeTypes,
        IDocumentRepository documents,
        IParticipantRepository participants,
        INodeReactionRepository nodeTags,
        IReactionDefinitionRepository tagDefinitions,
        IVoteRepository votes,
        ICommentRepository? comments,
        IReadOnlyDictionary<NodeId, NodeVoteSummary>? childVoteSummaries,
        ModerationService? moderation
    )
    {
        var children = FindChildren(
            node,
            nodes
        );
        var typeIds = node.RequestedSubNodeTypes
            .Select(request => request.TypeId)
            .Concat(children.Select(child => child.TypeId))
            .Distinct()
            .OrderBy(typeId =>
                nodeTypes.GetById(typeId)?.Name ?? string.Empty)
            .ToList();

        foreach (var typeId in typeIds)
        {
            var nodeType = nodeTypes.GetById(typeId);
            var typeName = nodeType?.Name
                ?? $"Unknown ({typeId})";
            var autoPluralize =
                nodeType?.AutoPluralize ?? true;
            var matchingChildren = children
                .Where(child => child.TypeId == typeId)
                .ToList();
            var heading = FormatTypeCount(
                typeName,
                matchingChildren.Count,
                autoPluralize
            )
                .ToUpperInvariant();

            Console.WriteLine($"  {heading}");

            if (matchingChildren.Count == 0)
            {
                Console.WriteLine(
                    $"    No {Atlas.Discovery.DiscoveryDisplayText.PluralizeTypeName(
                        typeName,
                        autoPluralize
                    )} " +
                    "have been added yet.");
                continue;
            }

            foreach (var child in matchingChildren)
            {
                NodeVoteSummary? voteSummary = null;

                if (childVoteSummaries is not null)
                {
                    childVoteSummaries.TryGetValue(
                        child.Id,
                        out voteSummary
                    );
                }

                var childVisibility = moderation?.Visibility(child.Id.Value) ?? ModerationVisibility.Visible;
                WriteWrapped(
                    "    > ",
                    childVisibility.IsHidden ? childVisibility.Notice : child.Title.Value
                );
                Console.WriteLine($"       {FormatVoteCount(voteSummary?.VoteCount)} votes · avg {FormatAverageVote(voteSummary?.AverageVote)}");
            }
        }

        if (typeIds.Count == 0)
        {
            Console.WriteLine(
                "  This node does not request or contain any sub-nodes.");
        }
    }

    /// <summary>Resolves sub node summary for the current console view.</summary>
    private static string ResolveSubNodeSummary(
        Node node,
        INodeRepository nodes,
        INodeTypeRepository nodeTypes
    )
    {
        var children = FindChildren(
            node,
            nodes
        );
        var typeIds = node.RequestedSubNodeTypes
            .Select(request => request.TypeId)
            .Concat(children.Select(child => child.TypeId))
            .Distinct()
            .OrderBy(typeId =>
                nodeTypes.GetById(typeId)?.Name ?? string.Empty)
            .ToList();

        if (typeIds.Count == 0)
        {
            return "None";
        }

        return string.Join(
            " · ",
            typeIds.Select(typeId =>
            {
                var nodeType = nodeTypes.GetById(typeId);
                var name = nodeType?.Name
                    ?? "Unknown";
                var count = children.Count(
                    child => child.TypeId == typeId);

                return FormatTypeCount(
                    name,
                    count,
                    nodeType?.AutoPluralize ?? true
                );
            })
        );
    }

    /// <summary>Finds nodes that directly reference the selected node as a parent.</summary>
    private static List<Node> FindChildren(
        Node node,
        INodeRepository nodes
    )
    {
        return nodes
            .GetChildren(node.Id)
            .OrderBy(candidate => candidate.Title.Value)
            .ToList();
    }


    /// <summary>Resolves parent summary for the current console view.</summary>
    private static string ResolveParentSummary(
        Node node,
        INodeRepository nodes,
        ModerationService? moderation
    )
    {
        if (node.ParentNodeIds.Count == 0)
        {
            return "Root node";
        }

        return string.Join(
            " · ",
            node.ParentNodeIds.Select(parentId =>
                (moderation?.Visibility(parentId.Value).IsHidden == true
                    ? moderation.Visibility(parentId.Value).Notice
                    : nodes.GetById(parentId)?.Title.Value)
                ?? $"Unknown ({parentId})")
        );
    }

    private static string ResolveBreadcrumb(
        Node node,
        INodeRepository nodes,
        INodeTypeRepository nodeTypes,
        ModerationService? moderation
    )
    {
        if (node.ParentNodeIds.Count == 0)
            return "Root node";

        string Segment(Node item)
        {
            var visibility = moderation?.Visibility(item.Id.Value) ?? ModerationVisibility.Visible;
            return $"{ResolveTypeName(
                item,
                nodeTypes
            )} · " +
                (visibility.IsHidden ? visibility.Notice : item.Title.Value);
        }

        // Cards and opened-node views both show only the first three ancestors.
        // Use Explore parents to continue or choose an alternative route.
        var segment = AncestorNavigation.ReadSegment(node, nodes, new HashSet<NodeId>());
        return string.Join(" → ", segment.Items.Select(Segment)) +
            (segment.CycleDetected ? " → [Circular parent link]" : segment.HasMore ? " → …" : "") +
            (segment.HasAlternativeRoutes ? " [other parent routes available]" : "");
    }

    private static void WriteBreadcrumb(
        string breadcrumb,
        int indent = 3
    )
    {
        var pathPrefix = new string(
            ' ',
            indent
        ) + "Path: ";
        var continuation = new string(
            ' ',
            pathPrefix.Length
        );
        var paths = breadcrumb.Split(
            " | ",
            StringSplitOptions.None
        );
        for (var pathIndex = 0; pathIndex < paths.Length; pathIndex++)
        {
            if (pathIndex > 0)
                Console.WriteLine(new string(
                    ' ',
                    indent + 2
                ) + "and");
            var segments = paths[pathIndex].Split(
                " → ",
                StringSplitOptions.None
            );
            for (var depth = 0; depth < segments.Length; depth++)
                WriteWrapped(
                    pathIndex == 0 && depth == 0 ? pathPrefix : continuation,
                    segments[depth]
                );
        }
    }

    /// <summary>Resolves description for the current console view.</summary>
    private static string ResolveDescription(
        Node node,
        IDocumentRepository documents,
        bool includeBlockDetails
    )
    {
        var document = documents.GetById(
            new DocumentId(node.DescriptionId.Value));

        if (document is null)
        {
            return "Description document not found.";
        }

        var blocks = documents.GetBlocks(document);

        if (blocks.Count == 0)
        {
            return "No description has been provided.";
        }

        if (!includeBlockDetails)
        {
            return string.Join(
                " | ",
                blocks.Select((
                    block,
                    index
                ) =>
                    $"{index + 1}:{block.Kind}")
            );
        }

        return string.Join(
            Environment.NewLine + Environment.NewLine,
            blocks.Select((
                block,
                index
            ) =>
                $"BLOCK {index + 1} · {block.Kind.ToUpperInvariant()} · {block.Id}" +
                Environment.NewLine +
                (block switch
            {
                MarkdownTextBlock text => text.Markdown,
                ImageBlock image => $"URL: {image.Url}\nAlt: {image.AltText}\nCaption: {image.Caption}",
                VideoBlock video => string.IsNullOrWhiteSpace(video.Caption)
                    ? $"URL: {video.Url}"
                    : $"URL: {video.Url}\nCaption: {video.Caption}",
                LinkPreviewBlock link => $"{link.Title}\n{link.Url}\n{link.Description}",
                PollReferenceBlock poll => $"Poll ID: {poll.PollId}",
                ChartReferenceBlock chart => string.IsNullOrWhiteSpace(chart.Title)
                    ? $"Chart ID: {chart.ChartId}"
                    : $"Chart ID: {chart.ChartId}\nTitle: {chart.Title}",
                _ => $"[{block.Kind}]"
            }))
        );
    }

    private static string ResolveFeedDescription(
        Node node,
        IDocumentRepository documents
    )
    {
        var document = documents.GetById(new DocumentId(node.DescriptionId.Value));
        if (document is null)
            return "Description document not found.";

        var blocks = documents.GetBlocks(document);
        if (blocks.Count == 0)
            return "No description has been provided.";

        return string.Join(
            "\n\n",
            blocks.Select(block => block switch
        {
            MarkdownTextBlock text => text.Markdown,
            ImageBlock image => $"Image: {image.AltText} ({image.Url})",
            VideoBlock video => $"Video: {video.Caption} ({video.Url})",
            LinkPreviewBlock link => $"Link: {link.Title} ({link.Url}) — {link.Description}",
            PollReferenceBlock => "Poll",
            ChartReferenceBlock chart => $"Chart: {chart.Title}",
            _ => block.Kind
        })
        );
    }

    private static void WriteWrapped(
        string prefix,
        string value
    )
    {
        var continuation = new string(
            ' ',
            prefix.Length
        );
        var width = Math.Max(
            20,
            (Console.IsOutputRedirected ? 100 : Console.WindowWidth) - prefix.Length - 2
        );
        foreach (var paragraph in value.Replace(
            "\r\n",
            "\n"
        ).Split('\n'))
        {
            var line = prefix;
            foreach (var word in paragraph.Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries
            ))
            {
                if (line.Length > prefix.Length && line.Length + word.Length + 1 > prefix.Length + width)
                {
                    Console.WriteLine(line);
                    line = continuation;
                }
                line += (line.Length > prefix.Length ? " " : string.Empty) + word;
            }
            Console.WriteLine(line);
            prefix = continuation;
        }
    }

    /// <summary>Resolves author name for the current console view.</summary>
    private static string ResolveAuthorName(
        Node node,
        IParticipantRepository participants
    )
    {
        return participants.GetById(
                   new ParticipantId(node.AuthorId.Value))
               ?.DisplayName
            ?? $"Unknown ({node.AuthorId})";
    }

    /// <summary>Resolves type name for the current console view.</summary>
    private static string ResolveTypeName(
        Node node,
        INodeTypeRepository nodeTypes
    )
    {
        return nodeTypes.GetById(node.TypeId)?.Name
            ?? $"Unknown ({node.TypeId})";
    }

    /// <summary>Resolves a compact tag summary when tag repositories are available.</summary>
    private static string ResolveTags(
        Node node,
        INodeReactionRepository? nodeTags,
        IReactionDefinitionRepository? tagDefinitions,
        IVoteRepository? votes
    )
    {
        return nodeTags is null || tagDefinitions is null || votes is null
            ? "—"
            : ReactionDisplay.FormatCompact(
                node,
                nodeTags,
                tagDefinitions,
                votes
            );
    }


    /// <summary>Formats type count for display.</summary>
    public static string FormatTypeCount(string name, int count, bool autoPluralize) =>
        Atlas.Discovery.DiscoveryDisplayText.FormatTypeCount(name, count, autoPluralize);

    /// <summary>Shortens text to the requested display width.</summary>
    private static string Truncate(
        string value,
        int maximumLength
    )
    {
        if (value.Length <= maximumLength)
        {
            return value;
        }

        return maximumLength <= 3
            ? value[..maximumLength]
            : $"{value[..(maximumLength - 3)]}...";
    }

    /// <summary>Formats vote count for display.</summary>
    private static string FormatVoteCount(int? voteCount)
    {
        return voteCount?.ToString() ?? "—";
    }

    /// <summary>Formats average vote for display.</summary>
    private static string FormatAverageVote(double? averageVote)
    {
        return averageVote?.ToString("0.00") ?? "—";
    }

    private static string FormatCurrentParticipantVote(
    int? currentParticipantVote)
    {
        return currentParticipantVote?.ToString() ?? "—";
    }

    private static string Center(
        string value,
        int width
    )
    {
        if (value.Length >= width)
        {
            return value;
        }

        var totalPadding = width - value.Length;
        var leftPadding = totalPadding / 2;
        var rightPadding = totalPadding - leftPadding;

        return new string(
            ' ',
            leftPadding
        ) +
               value +
               new string(
                   ' ',
                   rightPadding
               );
    }

    /// <summary>Writes the voter-list table heading.</summary>
    public static void WriteVoterListHeader()
    {
        Console.WriteLine(
            $"{"#",3}  " +
            $"{Center(
                "Vote",
                6
            )}  " +
            "Participant");

        Console.WriteLine(
            new string(
                '-',
                3 + 2 +
                6 + 2 +
                30
            ));
    }

    /// <summary>Writes one voter-list table row.</summary>
    public static void WriteVoterListRow(
        int number,
        string participantName,
        int rating
    )
    {
        Console.WriteLine(
            $"{number,3}  " +
            $"{Center(
                rating.ToString(),
                6
            )}  " +
            participantName);
    }
}
