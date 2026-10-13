namespace Atlas.Persistence.Discovery;

/// <summary>Database-wide committed generations, shared by every app instance.</summary>
public sealed class DiscoveryRevisionRow
{
    public int Id { get; set; }
    public long PageVersion { get; set; }
    public long DetailVersion { get; set; }
}
