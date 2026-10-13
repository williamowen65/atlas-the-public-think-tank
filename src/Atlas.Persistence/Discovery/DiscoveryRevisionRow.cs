namespace Atlas.Persistence.Discovery;

/// <summary>Database-wide committed generations, shared by every app instance.</summary>
public sealed class DiscoveryRevisionRow
{
    /// <summary>Singleton key: the model seeds the row with Id = 1.</summary>
    public int Id { get; set; }
    /// <summary>Advances when membership, ranking, totals or visibility can change.</summary>
    public long PageVersion { get; set; }
    /// <summary>Advances when public card text or metadata can change.</summary>
    public long DetailVersion { get; set; }
}
