using Atlas.Contracts.Identity;
using System.Reflection;
using Atlas.Comments.Comments;
using Atlas.Communities.Communities;
using Atlas.Communities.Nodes;
using Atlas.Communities.Memberships;
using Atlas.Content.Documents;
using Atlas.Discovery;
using Atlas.Graph.Nodes;
using Atlas.Graph.Nodes.NodeTypes;
using Atlas.Graph.Reactions;
using Atlas.Moderation;
using Atlas.Identity;
using Atlas.Participants.Participants;
using Atlas.Voting.Data;

namespace Atlas.ConsoleApp.Tests;

[TestClass]
[DoNotParallelize] // These tests temporarily replace the process-wide console input/output.
public sealed class AnonymousDiscoveryTests
{
    [TestMethod]
    public void Anonymous_node_actions_allow_exploration_and_reject_contribution_commands()
    {
        var (pages, output) = Browse(1, "1\n13\n\n19\n0\n15\n\n17\n\n11\n\n24\n\n20\n0\n");
        StringAssert.Contains(output, "Actions (as Anonymous)");
        StringAssert.Contains(output, "  Explore:");
        StringAssert.Contains(output, "Contribute [disabled");
        StringAssert.Contains(output, "Sign in as an active participant to contribute");
        StringAssert.Contains(output, "COMMENTS (read only)");
        StringAssert.Contains(output, "NODE VOTES");
        StringAssert.Contains(output, "NODE COMMUNITIES");
        StringAssert.Contains(output, "AUTHOR PROFILE");
        StringAssert.Contains(output, "This is a root node with no ancestors");
        Assert.IsGreaterThanOrEqualTo(2, pages.Count, "Returning from node exploration must restore Discovery.");
    }

    [TestMethod]
    public void Anonymous_node_actions_navigate_children_and_parents_without_writes()
    {
        var now = DateTimeOffset.UtcNow;
        var type = NodeTypeDefinition.CreateSystemDefined("Idea", "Ideas", now);
        var author = new NodeAuthorId(Guid.NewGuid());
        var root = new Node(new NodeTitle("Public root"), new NodeDescriptionId(Guid.NewGuid()), type.Id, author, now);
        var child = Node.Reconstitute(new NodeId(Guid.NewGuid()), new NodeTitle("Public child"),
            new NodeDescriptionId(Guid.NewGuid()), type.Id, author, NodeStatus.Active, [], [root.Id], now, now);
        var nodes = Stub<INodeRepository>((method, args) => method.Name switch
        {
            "GetById" => (NodeId)args![0]! == root.Id ? root : child,
            "GetChildren" => (NodeId)args![0]! == root.Id ? new[] { child } : Array.Empty<Node>(),
            "Save" or "SaveMetadata" => throw new AssertFailedException("Anonymous navigation attempted a write."),
            _ => EmptyValue(method.ReturnType)
        });
        var types = Stub<INodeTypeRepository>((method, _) => method.Name == "GetById" ? type : EmptyValue(method.ReturnType));
        var originalInput = System.Console.In;
        var originalOutput = System.Console.Out;
        using var reader = new StringReader("7\n1\n24\n1\n20\n");
        using var writer = new StringWriter();
        try
        {
            System.Console.SetIn(reader);
            System.Console.SetOut(writer);
            AnonymousBrowseCommands.ViewNode(root, nodes, types, Stub<IDocumentRepository>(),
                Stub<IParticipantRepository>(), Stub<INodeReactionRepository>(), Stub<IReactionDefinitionRepository>(),
                Stub<IVoteRepository>(), Stub<ICommunityRepository>(), Stub<ICommunityNodeRepository>(),
                Stub<ICommentRepository>(), new ModerationService(Stub<IModerationCaseRepository>(), Stub<IModeratorAuthorization>()));
        }
        finally
        {
            System.Console.SetIn(originalInput);
            System.Console.SetOut(originalOutput);
        }
        var output = writer.ToString();
        StringAssert.Contains(output, "Public child");
        StringAssert.Contains(output, "ANCESTORS (nearest first, up to three)");
        Assert.AreEqual(3, output.Split("Actions (as Anonymous)").Length - 1);
    }

    [TestMethod]
    public void UpperAndLowerCaseCommandsNavigateAnonymousDiscoveryPages()
    {
        var (pages, output) = Browse(
            6,
            "N\nP\nn\np\n0\n"
        );
        CollectionAssert.AreEqual(
            new[] {
                1,
                2,
                1,
                2,
                1
            },
            pages.ToArray()
        );
        StringAssert.Contains(
            output,
            "Page 2 of 2; showing 3 of 6 matching nodes."
        );
        StringAssert.Contains(
            output,
            "Node 6"
        );
    }

    [TestMethod]
    public void AnonymousPagesUseDistinctNumbersAndOpenTheCorrespondingNode()
    {
        // Advance to the final partial page, then open its globally numbered result.
        var (pages, output) = Browse(
            7,
            "N\nN\n7\n\nP\n0\n"
        );
        CollectionAssert.AreEqual(
            new[] {
                1,
                2,
                3,
                3,
                2
            },
            pages.ToArray()
        );
        StringAssert.Contains(
            output,
            "1. Idea · Node 1"
        );
        StringAssert.Contains(
            output,
            "4. Idea · Node 4"
        );
        StringAssert.Contains(
            output,
            "7. Idea · Node 7"
        );
        StringAssert.Contains(
            output,
            "Page 3 of 3; showing 1 of 7 matching nodes."
        );
        var details = output[(output.IndexOf(
            "ATLAS NODE",
            StringComparison.Ordinal
        ))..];
        StringAssert.Contains(
            details,
            "Idea · Node 7"
        );
        Assert.IsFalse(output.Contains(
            "That is not a valid selection.",
            StringComparison.Ordinal
        ));
    }

    [TestMethod]
    public void UnavailablePagesExplainWhyAnonymousResultsDoNotChange()
    {
        // Extra blank lines acknowledge the explicit boundary messages before the next command.
        var (pages, output) = Browse(
            3,
            "N\n\nP\n\n0\n"
        );
        CollectionAssert.AreEqual(
            new[] {
                1,
                1,
                1
            },
            pages.ToArray()
        );
        StringAssert.Contains(
            output,
            "Page 1 of 1; showing 3 of 3 matching nodes."
        );
        StringAssert.Contains(
            output,
            "There is no next page for the current filters."
        );
        StringAssert.Contains(
            output,
            "You are already on the first page."
        );
        Assert.IsFalse(output.Contains(
            "N: next page.",
            StringComparison.Ordinal
        ));
        Assert.IsFalse(output.Contains(
            "P: previous page.",
            StringComparison.Ordinal
        ));
    }

    [TestMethod]
    public void AuthenticatedDiscoveryReplacesPagesAndSupportsPrevious()
    {
        var (pages, output) = Browse(
            7,
            "N\nN\nP\n0\n",
            authenticated: true
        );
        CollectionAssert.AreEqual(
            new[] {
                1,
                2,
                3,
                2
            },
            pages.ToArray()
        );
        var screens = output.Split(
            "DISCOVERY",
            StringSplitOptions.None
        );
        StringAssert.Contains(
            screens[2],
            "4. Idea · Node 4"
        );
        Assert.IsFalse(screens[2].Contains(
            "1. Idea · Node 1",
            StringComparison.Ordinal
        ));
        StringAssert.Contains(
            screens[3],
            "7. Idea · Node 7"
        );
        StringAssert.Contains(
            screens[4],
            "4. Idea · Node 4"
        );
        StringAssert.Contains(
            screens[4],
            "P: previous page."
        );
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void Batched_cards_render_without_loading_domain_nodes(bool authenticated)
    {
        var (pages, output) = Browse(7, "N\nN\nP\n0\n", authenticated, useCards: true);
        CollectionAssert.AreEqual(new[] { 1, 2, 3, 2 }, pages.ToArray());
        StringAssert.Contains(output, "7. Idea · Node 7");
        StringAssert.Contains(output, "Batched description");
    }

    private static (IReadOnlyList<int> Pages, string Output) Browse(
        int count,
        string input,
        bool authenticated = false,
        bool useCards = false
    )
    {
        var now = new DateTimeOffset(
            2026,
            10,
            11,
            12,
            0,
            0,
            TimeSpan.Zero
        );
        var author = Guid.NewGuid();
        var type = NodeTypeDefinition.CreateSystemDefined(
            "Idea",
            "Ideas",
            now
        );
        var nodes = Enumerable.Range(
            1,
            count
        ).Select(i => new Node(
            new NodeTitle($"Node {i}"),
            new NodeDescriptionId(Guid.NewGuid()),
            type.Id,
            new NodeAuthorId(author),
            now
        )).ToList();
        var discovery = new RecordingDiscovery(new DiscoveryService(new FixedQuery(nodes.Select((node, index) =>
            new RankedDiscoveryItem(node.Id.Value, index + 1, 5, 1, 5, useCards ? new DiscoveryCard(node.Id.Value, node.Title.Value, "Idea", "Author", "Active", now, "Batched description", "Root node", "None", "—", "None", 0, false) : null)).ToArray())));
        var nodeRepository = Stub<INodeRepository>((
            method,
            args
        ) => useCards ? throw new AssertFailedException("Batched card rendering must not query Node aggregates.") : method.Name == "GetById"
            ? nodes.SingleOrDefault(n => n.Id == (NodeId)args![0]!) : EmptyValue(method.ReturnType));
        var types = Stub<INodeTypeRepository>((
            method,
            _
        ) => method.Name == "GetById"
            ? type : EmptyValue(method.ReturnType));
        var originalInput = System.Console.In;
        var originalOutput = System.Console.Out;
        using var reader = new StringReader(input);
        using var writer = new StringWriter();
        try
        {
            System.Console.SetIn(reader);
            System.Console.SetOut(writer);
            // Exercise the actual input/render loop using fixed ranked pages from the read port.
            if (authenticated)
            {
                // Exercise the authenticated browse loop; contribution services are unused here.
                var app = new ConsoleApplication(
                    nodes: nodeRepository,
                    nodeTypes: types,
                    documents: Stub<IDocumentRepository>(),
                    participants: Stub<IParticipantRepository>(),
                    votes: Stub<IVoteRepository>(),
                    castVote: null!,
                    undoVote: null!,
                    tagDefinitions: Stub<IReactionDefinitionRepository>(),
                    nodeTags: Stub<INodeReactionRepository>(),
                    eventPublisher: null!,
                    nodeCollectionKey: "",
                    nodeTypeCollectionKey: "",
                    documentCollectionKey: "",
                    participantCollectionKey: "",
                    voteCollectionKey: "",
                    tagDefinitionCollectionKey: "",
                    nodeTagCollectionKey: "",
                    communityCollectionKey: "",
                    communityMembershipCollectionKey: "",
                    communityNodeCollectionKey: "",
                    communities: Stub<ICommunityRepository>(),
                    communityMemberships: Stub<ICommunityMembershipRepository>(),
                    communityNodes: Stub<ICommunityNodeRepository>(),
                    communityService: null!,
                    comments: Stub<ICommentRepository>(),
                    commentCollectionKey: "",
                    discovery: discovery,
                    moderation: new ModerationService(
                        Stub<IModerationCaseRepository>(),
                        Stub<IModeratorAuthorization>()
                    ),
                    moderatorAuthorization: Stub<IModeratorAuthorization>(),
                    notificationService: null!,
                    notificationRepository: null!,
                    authenticatedActor: Stub<IAuthenticatedActor>((
                        _,
                        _
                    ) => author)
                );
                typeof(ConsoleApplication).GetMethod(
                    "BrowseNodes",
                    BindingFlags.Instance | BindingFlags.NonPublic
                )!
                    .Invoke(
                        app,
                        null
                    );
            }
            else
            {
            AnonymousBrowseCommands.DiscoverNodes(
                discovery,
                nodeRepository,
                types,
                Stub<IDocumentRepository>(),
                Stub<IParticipantRepository>(),
                Stub<INodeReactionRepository>(),
                Stub<IReactionDefinitionRepository>(),
                Stub<IVoteRepository>(),
                Stub<ICommunityRepository>(),
                Stub<ICommunityNodeRepository>(),
                Stub<ICommentRepository>(),
                new ModerationService(
                    Stub<IModerationCaseRepository>(),
                    Stub<IModeratorAuthorization>()
                )
            );
            }
        }
        finally
        {
            System.Console.SetIn(originalInput);
            System.Console.SetOut(originalOutput);
        }
        return (discovery.Pages, writer.ToString());
    }

    // Read-only display collaborators have no related records; SQL is not needed to test navigation.
    private static T Stub<T>(Func<MethodInfo, object?[]?, object?>? handler = null) where T : class
    {
        var value = DispatchProxy.Create<T, ReadOnlyProxy>();
        ((ReadOnlyProxy)(object)value).Handler = handler ?? ((
            method,
            _
        ) => EmptyValue(method.ReturnType));
        return value;
    }

    private static object? EmptyValue(Type type)
    {
        if (type == typeof(Task<CommentPage>)) return Task.FromResult(new CommentPage([], null));
        if (type.IsGenericType && typeof(System.Collections.IEnumerable).IsAssignableFrom(type))
            return Array.CreateInstance(
                type.GetGenericArguments()[0],
                0
            );
        return type.IsValueType ? Activator.CreateInstance(type) : null;
    }

    public class ReadOnlyProxy : DispatchProxy
    {
        public Func<MethodInfo, object?[]?, object?> Handler { get; set; } = null!;
        protected override object? Invoke(
            MethodInfo? targetMethod,
            object?[]? args
        ) => Handler(
            targetMethod!,
            args
        );
    }

    private sealed class FixedQuery(IReadOnlyList<RankedDiscoveryItem> items) : IDiscoveryReadQuery
    {
        public DiscoveryPage DiscoverPage(DiscoveryQuery query) => new(
            items.Skip((query.Page - 1) * IDiscoveryService.PageSize).Take(IDiscoveryService.PageSize).ToArray(), items.Count, query.Page, IDiscoveryService.PageSize);
        public DiscoveryPage DiscoverAuthoredPage(DiscoveryQuery query, IReadOnlyCollection<Guid> ids) => DiscoverPage(query);
        public IReadOnlyList<RankedDiscoveryItem> Discover(DiscoveryQuery query) => items;
        public IReadOnlyList<RankedDiscoveryItem> DiscoverAuthored(DiscoveryQuery query, IReadOnlyCollection<Guid> ids) => items;
    }

    private sealed class RecordingDiscovery(IDiscoveryService inner) : IDiscoveryService
    {
        public List<int> Pages { get; } = [];
        public DiscoveryPage DiscoverPage(DiscoveryQuery query)
        {
            Pages.Add(query.Page);
            return inner.DiscoverPage(query);
        }
        public DiscoveryPage DiscoverAuthoredPage(
            DiscoveryQuery query,
            IReadOnlyCollection<Guid> ids
        ) => inner.DiscoverAuthoredPage(
            query,
            ids
        );
        public IReadOnlyList<RankedDiscoveryItem> Discover(DiscoveryQuery query) => inner.Discover(query);
        public IReadOnlyList<RankedDiscoveryItem> DiscoverAuthored(
            DiscoveryQuery query,
            IReadOnlyCollection<Guid> ids
        ) => inner.DiscoverAuthored(
            query,
            ids
        );
    }
}
