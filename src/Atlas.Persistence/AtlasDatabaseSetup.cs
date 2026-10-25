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
        // Search reads opt into row versions. This database option is outside EF's table model.
        // Enable only during explicit setup, never on a search request or normal API startup.
        database.Database.ExecuteSqlRaw("""
            IF EXISTS (SELECT 1 FROM sys.databases WHERE database_id = DB_ID() AND snapshot_isolation_state = 0)
            BEGIN
                DECLARE @sql nvarchar(max) = N'ALTER DATABASE ' + QUOTENAME(DB_NAME()) + N' SET ALLOW_SNAPSHOT_ISOLATION ON';
                EXEC sys.sp_executesql @sql;
            END;
            IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE database_id = DB_ID() AND snapshot_isolation_state = 1)
                THROW 51000, 'Snapshot isolation is not ready. Finish open transactions and rerun database setup.', 1;
            """);
        // CREATE OR ALTER procedures/triggers must be the first statement in their batch.
        foreach (var script in new[] { "SearchDocumentsSchema.sql", "SearchDocumentsRefreshNodes.sql",
            "SearchDocumentsRefreshComments.sql", "SearchDocumentsTrackNodes.sql", "SearchDocumentsTrackComments.sql",
            "SearchDocumentsTrackBlocks.sql", "SearchDocumentsTrackLinks.sql", "SearchDocumentsBackfill.sql" })
            database.Database.ExecuteSqlRaw(SqlDiscoveryReadQuery.ReadResource(script));
        database.Database.ExecuteSqlRaw(DiscoveryFullTextSchema.CreateSql);
        DiscoveryFullTextSchema.EnsureAvailable(database, includeComments: true);
    }

    private static void RequireNoTransaction(AtlasDataContext database)
    {
        ArgumentNullException.ThrowIfNull(database);
        if (database.Database.CurrentTransaction is not null || Transaction.Current is not null)
            throw new InvalidOperationException("Database setup must run outside an explicit or ambient transaction.");
    }
}
