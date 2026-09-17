namespace Atlas.Graph.Tags;

/// <summary>Defines Graph persistence operations for reusable tag definitions.</summary>
public interface ITagDefinitionRepository
{
    /// <summary>Loads all reusable definitions.</summary>
    IReadOnlyCollection<TagDefinition> GetAll();

    /// <summary>Loads a definition by its Graph-owned identifier.</summary>
    TagDefinition? GetById(TagDefinitionId id);

    /// <summary>Loads the unique definition matching normalized text.</summary>
    TagDefinition? GetByNormalizedText(string normalizedText);

    /// <summary>Persists the current definition state.</summary>
    void Save(TagDefinition definition);
}
