using Atlas.Persistence;
using Atlas.Graph.Reactions;

namespace Atlas.Infrastructure.Storage;

/// <summary>Maps domain objects directly to EF Core rows in SQL Server.</summary>
public sealed class SqlReactionDefinitionRepository(Func<AtlasDataContext>? contextFactory = null) : SqlRepository(contextFactory), IReactionDefinitionRepository
{

    // Read definitions as domain objects; normalized-text lookup uses the stored comparison key.
    public IReadOnlyCollection<ReactionDefinition> GetAll() => ReadRows<ReactionDefinitionRow>().Select(ToDomain).ToList();
    public ReactionDefinition? GetById(ReactionDefinitionId id) => FindRow<ReactionDefinitionRow>(id.Value) is { } row ? ToDomain(row) : null;
    public ReactionDefinition? GetByNormalizedText(string normalizedText) =>
        QueryRows<ReactionDefinitionRow>(row => row.NormalizedText == normalizedText).Select(ToDomain).SingleOrDefault();
    public void Save(ReactionDefinition definition)
    {

        // Reject another definition with the same normalized text before writing this definition.
        if (QueryRows<ReactionDefinitionRow>(row => row.Id != definition.Id.Value && row.NormalizedText == definition.NormalizedText).Any())
            throw new InvalidOperationException($"The reaction '{definition.Text}' already exists.");

        // SaveRow inserts or updates the definition within its own or the caller's transaction.
        SaveRow(ToStorage(definition));
    }


    // Store display text alongside its normalized lookup key, ownership and suppression state.
    private static ReactionDefinitionRow ToStorage(ReactionDefinition definition) => new()
    {
        Id = definition.Id.Value,
        Text = definition.Text,
        Emoji = definition.Emoji,
        Description = definition.Description,
        NormalizedText = definition.NormalizedText,
        CreatedByParticipantId = definition.CreatedByParticipantId,
        IsSuppressed = definition.IsSuppressed,
        CreatedAt = definition.CreatedAt,
        UpdatedAt = definition.UpdatedAt
    };


    // Reconstitute and validate the saved definition, preserving identity and lifecycle timestamps.
    private static ReactionDefinition ToDomain(ReactionDefinitionRow stored) =>
        ReactionDefinition.Reconstitute(
            new ReactionDefinitionId(stored.Id),
            stored.Text,
            stored.Emoji,
            stored.Description,
            stored.NormalizedText,
            stored.CreatedByParticipantId,
            stored.IsSuppressed,
            stored.CreatedAt,
            stored.UpdatedAt
        );
}
