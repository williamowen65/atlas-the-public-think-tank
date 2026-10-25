using System.Diagnostics;
using Atlas.Persistence;
using Atlas.Persistence.Discovery;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Testing;

/// <summary>Only disposable test databases wait for the asynchronous full-text indexer.</summary>
internal static class SqlFullTextSupport
{
    internal static void CreateIndexes(AtlasDataContext db) =>
        AtlasDatabaseSetup.InstallSupplementalSchema(db);

    internal static void WaitForIndexing(string connectionString)
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT COUNT(*) FROM sys.fulltext_indexes
            WHERE object_id IN (OBJECT_ID(N'dbo.NodeRows'), OBJECT_ID(N'dbo.BlockRows'), OBJECT_ID(N'dbo.CommentRows'))
              AND (has_crawl_completed = 0
                OR COALESCE(OBJECTPROPERTYEX(object_id, 'TableFulltextPopulateStatus'), 1) <> 0
                OR COALESCE(OBJECTPROPERTYEX(object_id, 'TableFulltextPendingChanges'), 1) <> 0)
            """;
        // Missing raw-SQL migration steps must fail, rather than silently bypassing coverage.
        using var schema = connection.CreateCommand();
        schema.CommandText = "SELECT COUNT(*) FROM sys.fulltext_indexes WHERE object_id IN (OBJECT_ID(N'dbo.NodeRows'), OBJECT_ID(N'dbo.BlockRows'), OBJECT_ID(N'dbo.CommentRows'))";
        if (Convert.ToInt32(schema.ExecuteScalar()) != 3)
            throw new InvalidOperationException("Run the explicit database setup command after applying the EF schema before running discovery tests.");
        var timer = Stopwatch.StartNew();
        do
        {
            if (Convert.ToInt32(command.ExecuteScalar()) == 0) return;
            Thread.Sleep(100);
        } while (timer.Elapsed < TimeSpan.FromSeconds(45));
        throw new TimeoutException("SQL Server did not finish full-text indexing within 45 seconds. Check full-text population errors and the CI container logs.");
    }
}
