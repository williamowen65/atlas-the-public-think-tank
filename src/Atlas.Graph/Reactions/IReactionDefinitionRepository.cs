namespace Atlas.Graph.Reactions;

/// <summary>Defines Graph persistence operations for reusable reaction definitions.</summary>
public interface IReactionDefinitionRepository
{
    /// <summary>Loads all reusable definitions.</summary>
    IReadOnlyCollection<ReactionDefinition> GetAll();

    /// <summary>Loads a definition by its Graph-owned identifier.</summary>
    ReactionDefinition? GetById(ReactionDefinitionId id);

    /// <summary>Loads the unique definition matching normalized text.</summary>
    ReactionDefinition? GetByNormalizedText(string normalizedText);

    /// <summary>Persists the current definition state.</summary>
    void Save(ReactionDefinition definition);
}
