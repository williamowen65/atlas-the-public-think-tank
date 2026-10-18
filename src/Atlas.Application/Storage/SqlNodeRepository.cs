using Atlas.Persistence;
using Atlas.Graph.Nodes;
using Atlas.Graph.Nodes.NodeTypes;

namespace Atlas.Application.Storage;

/// <summary>Maps domain objects directly to EF Core rows in SQL Server.</summary>
public sealed class SqlNodeRepository(Func<AtlasDataContext>? contextFactory = null, NodeCreationQuotaPolicy? quotaPolicy = null) : SqlRepository(contextFactory), INodeRepository, INodeTopologyBoundary, INodeCreationQuota
{
    private readonly NodeCreationQuotaPolicy _quotaPolicy = quotaPolicy ?? NodeCreationQuotaPolicy.Default;

    // Infrastructure-only scope: the trusted host must authorize admin callers first.
    private readonly AsyncLocal<int> _trustedCreationDepth = new();

    /// <summary>Run trusted seed/import/admin work without checking or consuming user slots.</summary>
    /// <remarks>Use this repository throughout the callback. Normal validation and topology
    /// transactions remain active. Never expose this capability as a client-supplied flag.</remarks>
    public T ExecuteTrustedCreation<T>(Func<T> operation) => ExecuteTopology(() =>
    {
        ArgumentNullException.ThrowIfNull(operation);
        _trustedCreationDepth.Value++;
        try { return operation(); }
        finally { _trustedCreationDepth.Value--; }
    });

    /// <summary>Advisory allowance; another committed creation can change it immediately.</summary>
    public NodeCreationAllowance GetCreationAllowance(Guid participantId)
    {
        if (participantId == Guid.Empty) throw new ArgumentException("A participant is required.", nameof(participantId));
        var now = AtlasTime.UtcNow;
        // Limit the SQL result to the newest policy.Limit rows, using the participant/time index.
        var history = QueryRows<NodeCreationQuotaRow>(
            row => row.ParticipantId == participantId && row.CreatedAt > now - _quotaPolicy.Window,
            query => query.OrderByDescending(row => row.CreatedAt).Take(_quotaPolicy.Limit));
        return _quotaPolicy.Evaluate(history.Select(row => row.CreatedAt), now);
    }

    public void EnsureCreationAllowed(Guid participantId)
    {
        if (_trustedCreationDepth.Value > 0) return;
        var allowance = GetCreationAllowance(participantId);
        if (allowance.Remaining == 0) throw new NodeCreationQuotaExceededException(allowance);
    }

    /// <summary>Coordinates creation and parent changes before graph checks and saves.</summary>
    /// <remarks>Metadata edits use the ordinary boundary and never request this named lock.</remarks>
    public T ExecuteTopology<T>(Func<T> operation) => ExecuteWithLock(
        operation,
        "AtlasGraphTopology"
    );


    // Read saved Nodes with their relationships. A missing ID returns null.
    public Node? GetById(NodeId id) => FindRow<NodeRow>(id.Value) is { } row ? ToDomain(row) : null;

    // These queries currently load and reconstitute all Nodes, then filter in memory.
    private IReadOnlyCollection<Node> All() => ReadRows<NodeRow>().Select(ToDomain).ToList();
    public IReadOnlyCollection<Node> GetChildren(NodeId parentId) => All().Where(node => node.ParentNodeIds.Contains(parentId)).ToList();
    public IReadOnlyCollection<Node> GetByAuthor(NodeAuthorId authorId) => QueryRows<NodeRow>(row => row.AuthorId == authorId.Value).Select(ToDomain).ToList();
    public IReadOnlyCollection<Node> GetParentCandidates(
        NodeId childId,
        IReadOnlyCollection<NodeId> existingParentIds
    ) =>
        All().Where(node => node.Id != childId && !existingParentIds.Contains(node.Id)).ToList();
    /// <summary>Persists a complete node, including its parents, under graph coordination.</summary>
    public void Save(Node node) => ExecuteTopology(() => { SaveCore(
        node,
        false
    ); return true; });

    /// <summary>Updates an existing node's metadata without acquiring the graph lock or rewriting parents.</summary>
    public void SaveMetadata(Node node) => Execute(() => { SaveCore(
        node,
        true
    ); return true; });

    /// <summary>Validates references and persists the selected metadata or complete-node operation.</summary>
    private void SaveCore(
        Node node,
        bool metadataOnly
    )
    {
        ArgumentNullException.ThrowIfNull(node);

        // Load the stored version to distinguish creation from updates; metadata cannot create a Node.
        var previous = FindRow<NodeRow>(node.Id.Value);
        if (metadataOnly && previous is null) throw new InvalidOperationException("Metadata edits require an existing node.");

        // Require existing references throughout. Creation and newly selected types additionally require availability.
        // A full save already holds AtlasGraphTopology. Check and consume in that same
        // transaction so overlapping requests cannot both take the final slot.
        if (previous is null) EnsureCreationAllowed(node.AuthorId.Value);
        ReferenceValidation.Require(
            this,
            "Participant",
            node.AuthorId.Value,
            previous is null
        );
        ReferenceValidation.Require(
            this,
            "Document",
            node.DescriptionId.Value,
            false
        );
        ReferenceValidation.Require(
            this,
            "NodeType",
            node.TypeId.Value,
            previous is null || previous.TypeId != node.TypeId.Value
        );
        foreach (var request in node.RequestedSubNodeTypes)
            ReferenceValidation.Require(
                this,
                "NodeType",
                request.TypeId.Value,
                previous is null || !(previous.RequestedSubNodeTypeIds ?? []).Contains(request.TypeId.Value)
            );

        // Only full saves validate and replace topology. Traverse stored ancestry to reject indirect cycles,
        // then require newly attached parents to be available.
        if (!metadataOnly)
        {
            GraphRelationshipValidation.EnsureAcyclic(
                node.Id,
                node.ParentNodeIds,
                parentId =>
                FindRow<NodeRow>(parentId.Value) is { } parent ? (parent.ParentNodeIds ?? []).Select(id => new NodeId(id)).ToList() : null
            );
            foreach (var parent in node.ParentNodeIds)
                ReferenceValidation.Require(
                    this,
                    "Node",
                    parent.Value,
                    previous is null || !(previous.ParentNodeIds ?? []).Contains(parent.Value)
                );
        }

        // Metadata saves preserve the stored Parents collection, so stale metadata cannot overwrite newer links.
        SaveRow(
            ToStorage(node),
            metadataOnly ? [nameof(NodeRow.Parents)] : []
        );
        if (previous is null && _trustedCreationDepth.Value == 0)
        {
            // Use the server clock, not the Node's supplied timestamp. A failed later
            // write rolls back both this ledger entry and the complete creation workflow.
            SaveRow(new NodeCreationQuotaRow
            {
                NodeId = node.Id.Value,
                ParticipantId = node.AuthorId.Value,
                CreatedAt = AtlasTime.UtcNow
            });
        }
    }


    // Convert value-object IDs and relationship lists into the data-only SQL representation.
    private static NodeRow ToStorage(Node node)
    {
        return new NodeRow
        {
            Id = node.Id.Value,
            Title = node.Title.Value,
            DescriptionId = node.DescriptionId.Value,
            TypeId = node.TypeId.Value,
            AuthorId = node.AuthorId.Value,
            RequestedSubNodeTypeIds = node.RequestedSubNodeTypes
                .Select(request => request.TypeId.Value)
                .ToList(),
            ParentNodeIds = node.ParentNodeIds
                .Select(parentId => parentId.Value)
                .ToList(),
            Status = node.Status.ToString(),
            CreatedAt = node.CreatedAt,
            UpdatedAt = node.UpdatedAt
        };
    }

    /// <summary>Reconstitutes a domain object from its data-only persistence representation.</summary>
    private Node ToDomain(NodeRow storedNode)
    {

        // Parse lifecycle state and reject missing required references before rebuilding the domain object.
        var status = Enum.Parse<NodeStatus>(
            storedNode.Status,
            ignoreCase: true
        );

        var descriptionId = storedNode.DescriptionId
            ?? throw new InvalidDataException(
                $"Node {storedNode.Id} has no description ID.");

        var authorId = storedNode.AuthorId
            ?? throw new InvalidDataException(
                $"Node {storedNode.Id} has no author ID.");


        // Restore saved identity, relationships and timestamps; reconstitution applies domain validation.
        return Node.Reconstitute(
            new NodeId(storedNode.Id),
            new NodeTitle(storedNode.Title),
            new NodeDescriptionId(descriptionId),
            new NodeTypeId(storedNode.TypeId ?? throw new InvalidDataException("Node has no type ID.")),
            new NodeAuthorId(authorId),
            status,
            (storedNode.RequestedSubNodeTypeIds ?? [])
                .Select(id => new NodeTypeId(id)),
            (storedNode.ParentNodeIds ?? [])
                .Select(id => new NodeId(id)),
            storedNode.CreatedAt,
            storedNode.UpdatedAt
        );
    }

}
