using System.Transactions;
using Atlas.Persistence.Discovery;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Persistence;

/// <summary>Explicit deployment/local setup: EF schema first, supplemental SQL objects second.</summary>
public static class AtlasDatabaseSetup
{
    /// <summary>Apply pending EF migrations, then reconcile supplemental infrastructure.</summary>
    public static void Apply(AtlasDataContext database)
    {
        RequireNoTransaction(database);
        if (!database.Database.GetMigrations().Any())
            throw new InvalidOperationException("No EF migrations are present. Generate and commit the InitialAtlasBaseline migration before running database setup.");
        database.Database.Migrate();
        InstallSupplementalSchema(database);
    }

    /// <summary>
    /// Install objects EF cannot scaffold. Also usable after a freshly generated baseline.
    /// Existing migration history remains authoritative; this does not modify that history.
    /// </summary>
    public static void InstallSupplementalSchema(AtlasDataContext database)
    {
        RequireNoTransaction(database);
        database.Database.ExecuteSqlRaw(DiscoveryFullTextSchema.CreateSql);
        DiscoveryFullTextSchema.EnsureAvailable(database);
    }

    private static void RequireNoTransaction(AtlasDataContext database)
    {
        ArgumentNullException.ThrowIfNull(database);
        if (database.Database.CurrentTransaction is not null || Transaction.Current is not null)
            throw new InvalidOperationException("Database setup must run outside an explicit or ambient transaction.");
    }
}
