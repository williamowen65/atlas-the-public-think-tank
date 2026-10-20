using Atlas.Content.Blocks;
using Atlas.Content.Documents;
using Atlas.Contracts.Operations;
using Atlas.Graph.Nodes;
using Atlas.Graph.Nodes.NodeTypes;
using Creation = Atlas.Application.NodeCreationService;

[assembly: Parallelize(Scope = ExecutionScope.MethodLevel)]

namespace Atlas.Application.Tests;

[TestClass]
public sealed class WorkflowTests
{
    private static readonly DateTimeOffset At = DateTimeOffset.UtcNow.AddMinutes(-1);

    [TestMethod]
    public void NodeAndDescriptionCanBeCreatedWithoutReferencingConsole()
    {
        var store = new Store();
        var node = Create(store);
        var document = ((IDocumentRepository)store).GetById(new DocumentId(node.DescriptionId.Value))!;
        var block = store.GetBlockById(document.BlockIds.Single())!;
        Assert.AreEqual(node.CreatedAt, document.CreatedAt);
        Assert.AreEqual(node.CreatedAt, block.CreatedAt);
        Assert.AreEqual(store.Actor, node.AuthorId.Value);
        Assert.HasCount(1, store.Nodes);
    }

    [TestMethod]
    public void InvalidNodeProposalWritesNoContent()
    {
        var store = new Store();
        Assert.Throws<ArgumentException>(() => Create(store, " "));
        Assert.IsEmpty(store.Nodes); Assert.IsEmpty(store.Documents); Assert.IsEmpty(store.Blocks);
    }

    [TestMethod]
    public void UnavailableParentAndTypeAreRejectedBeforeWrites()
    {
        var store = new Store();
        Assert.Throws<InvalidOperationException>(() => new Creation(store, store).Create("Node", "Text", store.Type.Id, store.Actor, [], NodeId.New()));
        Assert.Throws<InvalidOperationException>(() => new Creation(store, store).Create("Node", "Text", NodeTypeId.New(), store.Actor, []));
        Assert.IsEmpty(store.Documents); Assert.IsEmpty(store.Blocks); Assert.IsEmpty(store.Nodes);
    }

    [TestMethod]
    public void FinalNodeFailureRollsBackEarlierContentThroughSuppliedBoundary()
    {
        var store = new Store { FailNodeSave = true };
        Assert.Throws<InvalidOperationException>(() => Create(store));
        Assert.AreEqual(1, store.Rollbacks);
        Assert.IsEmpty(store.Documents); Assert.IsEmpty(store.Blocks); Assert.IsEmpty(store.Nodes);
    }

    [TestMethod]
    public void DescriptionOwnershipIsEnforcedByReusableCoordinator()
    {
        var store = new Store(); var node = Create(store);
        var stranger = Guid.NewGuid(); store.Participants.Add(stranger);
        var service = new NodeDescriptionService(store, store);
        Assert.Throws<UnauthorizedAccessException>(() => service.Add(node.Id, stranger, new MarkdownInput("Other")));
        Assert.HasCount(1, store.Blocks);
    }

    [TestMethod]
    public void UnavailableActorCannotEditEvenWithMatchingId()
    {
        var store = new Store(); var node = Create(store); store.Participants.Clear();
        Assert.Throws<InvalidOperationException>(() => new NodeDescriptionService(store, store).Add(node.Id, store.Actor, new MarkdownInput("Other")));
        Assert.HasCount(1, store.Blocks);
    }

    [TestMethod]
    public void ContentCompositionEditsPreserveStableIdsAndRetainRemovedPayload()
    {
        var store = new Store(); var node = Create(store);
        var service = new NodeDescriptionService(store, store);
        var block = service.Add(node.Id, store.Actor, new MarkdownInput("Added"), 0);
        var updated = service.Update(node.Id, store.Actor, block.Id, new MarkdownInput("Updated"));
        Assert.AreEqual(block.Id, updated.Id);
        Assert.AreEqual("Updated", ((MarkdownTextBlock)updated).Markdown);
        service.Move(node.Id, store.Actor, block.Id, 1);
        Assert.AreEqual(block.Id, store.Documents.Values.Single().BlockIds.Last());
        service.Remove(node.Id, store.Actor, block.Id);
        Assert.HasCount(1, store.Documents.Values.Single().BlockIds);
        Assert.IsNotNull(store.GetBlockById(block.Id));
    }

    [TestMethod]
    public void InvalidPositionOrPayloadDoesNotPersistAnOrphan()
    {
        var store = new Store(); var node = Create(store);
        var service = new NodeDescriptionService(store, store);
        Assert.Throws<ArgumentOutOfRangeException>(() => service.Add(node.Id, store.Actor, new MarkdownInput("Other"), 99));
        Assert.Throws<ArgumentException>(() => service.Add(node.Id, store.Actor, new ImageInput("url", "", null)));
        Assert.HasCount(1, store.Blocks); Assert.HasCount(1, store.Documents.Values.Single().BlockIds);
    }

    [TestMethod]
    public void BlockOutsideDocumentAndMismatchedInputAreRejected()
    {
        var store = new Store(); var node = Create(store);
        var service = new NodeDescriptionService(store, store);
        var outside = new MarkdownTextBlock("outside", At); store.SaveBlock(outside);
        Assert.Throws<InvalidOperationException>(() => service.Update(node.Id, store.Actor, outside.Id, new MarkdownInput("changed")));
        var inside = store.Documents.Values.Single().BlockIds.Single();
        Assert.Throws<InvalidOperationException>(() => service.Update(node.Id, store.Actor, inside, new ImageInput("url", "alt", null)));
        Assert.AreEqual("outside", outside.Markdown);
        Assert.AreEqual("Text", ((MarkdownTextBlock)store.GetBlockById(inside)!).Markdown);
    }

    [TestMethod]
    public void DuplicateCustomTypeIsRejectedIgnoringCaseAndWhitespace()
    {
        var store = new Store(); var types = new NodeTypeCreationService(store);
        var created = types.Create("Proposal", "Description", store.Actor.ToString(), At);
        Assert.IsNotNull(created);
        Assert.Throws<InvalidOperationException>(() => types.Create(" proposal ", "Other", store.Actor.ToString(), At));
        Assert.HasCount(2, store.Types);
    }

    [TestMethod]
    public void AllBlockInputsCreateAndEditableKindsUpdateThroughContentService()
    {
        var store = new Store(); var node = Create(store); var service = new NodeDescriptionService(store, store);
        BlockInput[] inputs = [new MarkdownInput("m"), new ImageInput("image", "alt", null),
            new VideoInput("video", null), new LinkPreviewInput("https://example.com", "link", null),
            new PollInput(), new ChartInput("chart")];
        foreach (var input in inputs)
        {
            var block = service.Add(node.Id, store.Actor, input);
            if (input is PollInput)
                Assert.Throws<InvalidOperationException>(() => service.Update(node.Id, store.Actor, block.Id, input));
            else Assert.AreEqual(block.Id, service.Update(node.Id, store.Actor, block.Id, input).Id);
        }
        Assert.HasCount(7, store.Blocks);
    }

    [TestMethod]
    public void NodeWorkflowPublishesOnlyAfterBoundaryCompletion()
    {
        var store = new Store();
        var events = new Recorder(() => Assert.IsFalse(store.InTransaction));
        var node = new NodeWorkflowService(store, events).Create(store, "Node", "Text", store.Type.Id, store.Actor, []);
        Assert.HasCount(1, events.Messages);
        Assert.IsEmpty(node.DomainEvents);
        new NodeWorkflowService(store, events).Archive(node, store.Actor);
        Assert.AreEqual(NodeStatus.Archived, store.GetById(node.Id)!.Status);
        Assert.HasCount(2, events.Messages);
        Assert.IsEmpty(node.DomainEvents);
    }

    [TestMethod]
    public void FailedCommitDoesNotDispatchButSubscriberFailureDoesNotUndoCommit()
    {
        var failing = new Store { FailNodeSave = true };
        var events = new Recorder(() => { });
        Assert.Throws<InvalidOperationException>(() => new NodeWorkflowService(failing, events).Create(failing, "Node", "Text", failing.Type.Id, failing.Actor, []));
        Assert.IsEmpty(events.Messages);
        var store = new Store();
        var brokenSubscriber = new Recorder(() => throw new InvalidOperationException("subscriber"));
        Assert.Throws<InvalidOperationException>(() => new NodeWorkflowService(store, brokenSubscriber).Create(store, "Node", "Text", store.Type.Id, store.Actor, []));
        Assert.HasCount(1, store.Nodes);
        Assert.IsNotEmpty(store.Nodes.Values.Single().DomainEvents);
    }

    /// <summary>A valid participant still cannot mutate a node owned by somebody else.</summary>
    [TestMethod]
    [DataRow("rename")]
    [DataRow("archive")]
    [DataRow("restore")]
    [DataRow("attach")]
    [DataRow("detach")]
    [DataRow("type")]
    [DataRow("request-type")]
    [DataRow("withdraw-type")]
    public void RejectedNodeMutationLeavesStateAndEventsUnchanged(string command)
    {
        var store = new Store();
        var node = Create(store);
        node.ClearDomainEvents();
        var outsider = Guid.NewGuid();
        store.Participants.Add(outsider);
        var before = (node.Title.Value, node.Status, node.UpdatedAt);
        var parents = node.ParentNodeIds.ToArray();
        var published = new Recorder(() => { });
        var workflow = new NodeWorkflowService(store, published);

        Assert.ThrowsExactly<UnauthorizedAccessException>(() =>
        {
            switch (command)
            {
                case "rename": workflow.Rename(node, outsider, "Forbidden"); break;
                case "archive": workflow.Archive(node, outsider); break;
                case "restore": workflow.Restore(node, outsider); break;
                case "attach": workflow.Attach(node, NodeId.New(), outsider); break;
                case "detach": workflow.Detach(node, NodeId.New(), outsider); break;
                case "type": workflow.ChangeType(node, store.Type.Id, outsider); break;
                case "request-type": workflow.RequestType(node, store.Type.Id, outsider, true); break;
                case "withdraw-type": workflow.RequestType(node, store.Type.Id, outsider, false); break;
            }
        });

        // Check both the caller's aggregate and observable workflow effects.
        Assert.AreEqual(before, (node.Title.Value, node.Status, node.UpdatedAt));
        CollectionAssert.AreEqual(parents, node.ParentNodeIds.ToArray());
        Assert.IsEmpty(node.DomainEvents);
        Assert.IsEmpty(published.Messages);
        Assert.HasCount(1, store.Nodes);
    }

    /// <summary>Every description entry point enforces ownership before touching payload or composition.</summary>
    [TestMethod]
    [DataRow("add")]
    [DataRow("update")]
    [DataRow("move")]
    [DataRow("remove")]
    public void RejectedDescriptionMutationPreservesPayloadCompositionAndTimestamp(string command)
    {
        var store = new Store();
        var node = Create(store);
        var document = store.Documents.Values.Single();
        var block = (MarkdownTextBlock)store.GetBlockById(document.BlockIds.Single())!;
        var before = (block.Markdown, block.UpdatedAt, document.UpdatedAt);
        var outsider = Guid.NewGuid();
        store.Participants.Add(outsider);
        var service = new NodeDescriptionService(store, store);

        Assert.ThrowsExactly<UnauthorizedAccessException>(() =>
        {
            switch (command)
            {
                case "add": service.Add(node.Id, outsider, new MarkdownInput("Forbidden")); break;
                case "update": service.Update(node.Id, outsider, block.Id, new MarkdownInput("Forbidden")); break;
                case "move": service.Move(node.Id, outsider, block.Id, 0); break;
                case "remove": service.Remove(node.Id, outsider, block.Id); break;
            }
        });
        Assert.AreEqual(before, (block.Markdown, block.UpdatedAt, document.UpdatedAt));
        CollectionAssert.AreEqual(new[] { block.Id }, document.BlockIds.ToArray());
        Assert.HasCount(1, store.Blocks);
    }

    private sealed class Recorder(Action onPublish) : Atlas.Contracts.Events.IEventPublisher
    {
        public List<object> Messages { get; } = [];
        public void Publish(object message) { onPublish(); Messages.Add(message); }
    }

    private static Node Create(Store store, string title = "Node") =>
        new Creation(store, store).Create(title, "Text", store.Type.Id, store.Actor, []);

    // A shared transactional test adapter: production atomicity is separately exercised
    // by SqlOperationValidationTests using real SQL repositories and injected failures.
    private sealed class Store : INodeRepository, IDocumentRepository, INodeTypeRepository, IReferenceLookup, IOperationBoundary
    {
        public Guid Actor { get; } = Guid.NewGuid();
        public HashSet<Guid> Participants { get; } = [];
        public NodeTypeDefinition Type { get; } = NodeTypeDefinition.CreateSystemDefined("Idea", "Ideas", At);
        public Dictionary<NodeId, Node> Nodes { get; private set; } = [];
        public Dictionary<DocumentId, Document> Documents { get; private set; } = [];
        public Dictionary<BlockId, ContentBlock> Blocks { get; private set; } = [];
        public List<NodeTypeDefinition> Types { get; } = [];
        public bool FailNodeSave { get; init; }
        public int Rollbacks { get; private set; }
        private int _depth;
        public bool InTransaction => _depth > 0;
        public Store() { Participants.Add(Actor); Types.Add(Type); }
        public bool IsAvailable(string kind, Guid id, bool active) => kind switch
        {
            "Participant" => Participants.Contains(id), "NodeType" => Types.Any(x => x.Id.Value == id),
            "Document" => Documents.Keys.Any(x => x.Value == id), "Node" => Nodes.Keys.Any(x => x.Value == id), _ => false
        };
        public T Execute<T>(Func<T> operation)
        {
            if (_depth++ > 0) { try { return operation(); } finally { _depth--; } }
            var nodes = new Dictionary<NodeId, Node>(Nodes);
            var docs = new Dictionary<DocumentId, Document>(Documents);
            var blocks = new Dictionary<BlockId, ContentBlock>(Blocks);
            try { return operation(); }
            catch { Nodes = nodes; Documents = docs; Blocks = blocks; Rollbacks++; throw; }
            finally { _depth--; }
        }
        public Node? GetById(NodeId id) => Nodes.GetValueOrDefault(id);
        public IReadOnlyCollection<Node> GetChildren(NodeId id) => Nodes.Values.Where(n => n.ParentNodeIds.Contains(id)).ToList();
        public IReadOnlyCollection<Node> GetByAuthor(NodeAuthorId id) => Nodes.Values.Where(n => n.AuthorId == id).ToList();
        public IReadOnlyCollection<Node> GetParentCandidates(NodeId id, IReadOnlyCollection<NodeId> parents) => Nodes.Values.ToList();
        public void Save(Node node) { if (FailNodeSave) throw new InvalidOperationException("Injected node failure"); Nodes[node.Id] = node; }
        IReadOnlyCollection<Document> IDocumentRepository.GetAll() => Documents.Values.ToList();
        Document? IDocumentRepository.GetById(DocumentId id) => Documents.TryGetValue(id, out var d)
            ? Document.Reconstitute(d.Id, d.BlockIds, d.CreatedAt, d.UpdatedAt) : null;
        public void Save(Document document) => Documents[document.Id] = document;
        public ContentBlock? GetBlockById(BlockId id) => Blocks.GetValueOrDefault(id);
        public IReadOnlyCollection<ContentBlock> GetBlocks(Document document) => document.BlockIds.Select(id => Blocks[id]).ToList();
        public void SaveBlock(ContentBlock block) => Blocks[block.Id] = block;
        IReadOnlyCollection<NodeTypeDefinition> INodeTypeRepository.GetAll() => Types;
        public NodeTypeDefinition? GetById(NodeTypeId id) => Types.FirstOrDefault(x => x.Id == id);
        public void Save(NodeTypeDefinition type) => Types.Add(type);
    }
}
