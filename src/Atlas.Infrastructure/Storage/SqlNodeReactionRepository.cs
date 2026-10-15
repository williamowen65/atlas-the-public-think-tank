using Atlas.Persistence;
using Atlas.Graph.Nodes;
using Atlas.Graph.Reactions;

namespace Atlas.Infrastructure.Storage;

/// <summary>Maps domain objects directly to EF Core rows in SQL Server.</summary>
public sealed class SqlNodeReactionRepository(Func<AtlasDataContext>? contextFactory = null) : SqlRepository(contextFactory), INodeReactionRepository
{

    // Read reactions as domain objects; active queries filter by lifecycle state before mapping.
    public IReadOnlyCollection<NodeReaction> GetAll() => ReadRows<NodeReactionRow>().Select(ToDomain).ToList();
    public NodeReaction? GetById(NodeReactionId id) => FindRow<NodeReactionRow>(id.Value) is { } row ? ToDomain(row) : null;
    // Return all non-removed reaction associations for this Node, for display and selection.
    // Active refers to the association's lifecycle; this query does not check Node availability or definition suppression.
    public IReadOnlyCollection<NodeReaction> GetActiveForNode(NodeId nodeId) =>
        QueryRows<NodeReactionRow>(row => row.NodeId == nodeId.Value && row.LifecycleState == "Active").Select(ToDomain).ToList();
    // Find the current association for one Node/definition pair, used when applying or replacing a reaction.
    // Return null when none exists; SingleOrDefault throws if duplicate active associations are stored.
    public NodeReaction? GetActive(
        NodeId nodeId,
        ReactionDefinitionId definitionId
    ) =>
        QueryRows<NodeReactionRow>(row => row.NodeId == nodeId.Value && row.ReactionDefinitionId == definitionId.Value && row.LifecycleState == "Active")
            .Select(ToDomain).SingleOrDefault();
    public void Save(NodeReaction reaction) => Execute(() =>
    {
        ArgumentNullException.ThrowIfNull(reaction);

        // New reactions need an active participant and available Node; updates require those references to exist.
        var isNew = FindRow<NodeReactionRow>(reaction.Id.Value) is null;
        ReferenceValidation.Require(
            this,
            "Participant",
            reaction.AppliedByParticipantId,
            isNew
        );
        ReferenceValidation.Require(
            this,
            "Node",
            reaction.NodeId.Value,
            isNew
        );

        // The definition must exist. Suppression prevents a new application, but does not block saving an existing one.
        var definition = FindRow<ReactionDefinitionRow>(reaction.ReactionDefinitionId.Value);
        if (definition is null || isNew && definition.IsSuppressed) throw new ArgumentException("Reaction definition is missing or suppressed.");

        // Prevent a second active reaction with the same Node/definition pair, excluding this reaction itself.
        if (!reaction.IsRemoved && QueryRows<NodeReactionRow>(row => row.Id != reaction.Id.Value && row.NodeId == reaction.NodeId.Value &&
            row.ReactionDefinitionId == reaction.ReactionDefinitionId.Value && row.LifecycleState == "Active").Any())
            throw new InvalidOperationException("That reaction is already applied to this node.");

        // Save the reaction and its audit entries inside the current operation transaction.
        SaveRow(ToStorage(reaction));
        return true;
    });


    // Map current state and ordered audit history into the persistence representation.
    private static NodeReactionRow ToStorage(NodeReaction nodeTag) => new()
    {
        Id = nodeTag.Id.Value,
        NodeId = nodeTag.NodeId.Value,
        ReactionDefinitionId = nodeTag.ReactionDefinitionId.Value,
        AppliedByParticipantId = nodeTag.AppliedByParticipantId,
        LifecycleState = nodeTag.LifecycleState.ToString(),
        Disposition = nodeTag.Disposition.ToString(),
        CreatedAt = nodeTag.CreatedAt,
        RemovedAt = nodeTag.RemovedAt,
        AuditHistory = nodeTag.AuditHistory
            .Select(entry => new ReactionAuditRow
            {
                Action = entry.Action.ToString(),
                ActorParticipantId = entry.ActorParticipantId,
                OccurredAt = entry.OccurredAt,
                LifecycleState = entry.LifecycleState.ToString(),
                Disposition = entry.Disposition.ToString(),
                RelatedNodeReactionId = entry.RelatedNodeReactionId?.Value
            })
            .ToList()
    };


    // Rebuild domain state and replay the stored audit representation in Position order.
    // Reconstitution validates that the history and current state agree.
    private static NodeReaction ToDomain(NodeReactionRow stored) =>
        NodeReaction.Reconstitute(
            new NodeReactionId(stored.Id),
            new NodeId(stored.NodeId),
            new ReactionDefinitionId(stored.ReactionDefinitionId),
            stored.AppliedByParticipantId,
            ParseLifecycle(stored),
            Enum.Parse<NodeReactionDisposition>(stored.Disposition),
            stored.CreatedAt,
            stored.RemovedAt,
            stored.AuditHistory.OrderBy(entry => entry.Position).Select(entry =>
                new NodeReactionAuditEntry(
                    Enum.Parse<NodeReactionAuditAction>(entry.Action),
                    entry.ActorParticipantId,
                    entry.OccurredAt,
                    Enum.Parse<NodeReactionLifecycleState>(entry.LifecycleState),
                    Enum.Parse<NodeReactionDisposition>(entry.Disposition),
                    entry.RelatedNodeReactionId.HasValue
                        ? new NodeReactionId(entry.RelatedNodeReactionId.Value)
                        : null
                ))
        );


    // Parse the saved lifecycle name; an unrecognized value fails instead of silently defaulting.
    private static NodeReactionLifecycleState ParseLifecycle(NodeReactionRow stored) =>
        Enum.Parse<NodeReactionLifecycleState>(stored.LifecycleState);
}
