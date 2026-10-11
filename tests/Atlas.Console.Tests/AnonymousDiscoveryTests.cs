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

    private static (IReadOnlyList<int> Pages, string Output) Browse(
        int count,
        string input,
        bool authenticated = false
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
        var candidates = nodes.Select(n => new DiscoveryCandidate(
            n.Id.Value,
            n.Title.Value,
            "",
            type.Id.Value,
            author,
            false,
            now,
            now,
            0,
            null,
            [],
            []
        )).ToList();
        var discovery = new RecordingDiscovery(new DiscoveryService(new CandidateSource(candidates)));
        var nodeRepository = Stub<INodeRepository>((
            method,
            args
        ) => method.Name == "GetById"
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
            // Exercise the actual anonymous input/render loop with real Discovery filtering and slicing.
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

    private sealed class CandidateSource(IReadOnlyCollection<DiscoveryCandidate> candidates) : IDiscoveryCandidateSource
    {
        public IReadOnlyCollection<DiscoveryCandidate> GetCandidates() => candidates;
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
