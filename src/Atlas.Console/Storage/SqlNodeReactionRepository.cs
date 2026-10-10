using Atlas.Persistence;
using Atlas.Graph.Nodes;
using Atlas.Graph.Reactions;

namespace Atlas.ConsoleApp.Storage;

/// <summary>Maps domain objects directly to EF Core rows in SQL Server.</summary>
public sealed class SqlNodeReactionRepository(Func<AtlasDataContext>? contextFactory = null) : SqlRepository(contextFactory), INodeReactionRepository
{
    public IReadOnlyCollection<NodeReaction> GetAll() => ReadRows<NodeReactionRow>().Select(ToDomain).ToList();
    public NodeReaction? GetById(NodeReactionId id) => FindRow<NodeReactionRow>(id.Value) is { } row ? ToDomain(row) : null;
    public IReadOnlyCollection<NodeReaction> GetActiveForNode(NodeId nodeId) =>
        QueryRows<NodeReactionRow>(row => row.NodeId == nodeId.Value && row.LifecycleState == "Active").Select(ToDomain).ToList();
    public NodeReaction? GetActive(NodeId nodeId, ReactionDefinitionId definitionId) =>
        QueryRows<NodeReactionRow>(row => row.NodeId == nodeId.Value && row.ReactionDefinitionId == definitionId.Value && row.LifecycleState == "Active")
            .Select(ToDomain).SingleOrDefault();
    public void Save(NodeReaction reaction) => Execute(() =>
    {
        ArgumentNullException.ThrowIfNull(reaction);
        var isNew = FindRow<NodeReactionRow>(reaction.Id.Value) is null;
        ReferenceValidation.Require(this, "Participant", reaction.AppliedByParticipantId, isNew);
        ReferenceValidation.Require(this, "Node", reaction.NodeId.Value, isNew);
        var definition = FindRow<ReactionDefinitionRow>(reaction.ReactionDefinitionId.Value);
        if (definition is null || isNew && definition.IsSuppressed) throw new ArgumentException("Reaction definition is missing or suppressed.");
        if (!reaction.IsRemoved && QueryRows<NodeReactionRow>(row => row.Id != reaction.Id.Value && row.NodeId == reaction.NodeId.Value &&
            row.ReactionDefinitionId == reaction.ReactionDefinitionId.Value && row.LifecycleState == "Active").Any())
            throw new InvalidOperationException("That reaction is already applied to this node.");
        SaveRow(ToStorage(reaction));
        return true;
    });

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
                        : null)));

    private static NodeReactionLifecycleState ParseLifecycle(NodeReactionRow stored) =>
        Enum.Parse<NodeReactionLifecycleState>(stored.LifecycleState);
}
