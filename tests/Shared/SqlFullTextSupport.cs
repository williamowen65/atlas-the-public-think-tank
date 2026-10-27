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
            WHERE object_id IN (OBJECT_ID(N'dbo.NodeRows'), OBJECT_ID(N'dbo.BlockRows'), OBJECT_ID(N'dbo.CommentRows'), OBJECT_ID(N'dbo.SearchDocuments'))
              AND (has_crawl_completed = 0
                OR COALESCE(OBJECTPROPERTYEX(object_id, 'TableFulltextPopulateStatus'), 1) <> 0
                OR COALESCE(OBJECTPROPERTYEX(object_id, 'TableFulltextPendingChanges'), 1) <> 0)
            """;
        // Missing raw-SQL migration steps must fail, rather than silently bypassing coverage.
        using var schema = connection.CreateCommand();
        schema.CommandText = "SELECT COUNT(*) FROM sys.fulltext_indexes WHERE object_id IN (OBJECT_ID(N'dbo.NodeRows'), OBJECT_ID(N'dbo.BlockRows'), OBJECT_ID(N'dbo.CommentRows'), OBJECT_ID(N'dbo.SearchDocuments'))";
        if (Convert.ToInt32(schema.ExecuteScalar()) != 4)
            throw new InvalidOperationException("Run the explicit database setup command after applying the EF schema before running discovery tests.");
        // Index population is asynchronous and competes with other disposable databases.
        // This is setup readiness, not a search-query performance budget.
        var readinessTimeout = TimeSpan.FromMinutes(3);
        var timer = Stopwatch.StartNew();
        do
        {
            if (Convert.ToInt32(command.ExecuteScalar()) == 0) return;
            Thread.Sleep(100);
        } while (timer.Elapsed < readinessTimeout);
        using var diagnostics = connection.CreateCommand();
        diagnostics.CommandText = """
            SELECT OBJECT_SCHEMA_NAME(object_id) + N'.' + OBJECT_NAME(object_id),
                is_enabled, has_crawl_completed,
                CONVERT(int, OBJECTPROPERTYEX(object_id, 'TableFulltextPopulateStatus')),
                CONVERT(bigint, OBJECTPROPERTYEX(object_id, 'TableFulltextPendingChanges'))
            FROM sys.fulltext_indexes
            WHERE object_id IN (OBJECT_ID(N'dbo.NodeRows'), OBJECT_ID(N'dbo.BlockRows'),
                OBJECT_ID(N'dbo.CommentRows'), OBJECT_ID(N'dbo.SearchDocuments'))
            """;
        var states = new List<string>();
        try
        {
            using var reader = diagnostics.ExecuteReader();
            while (reader.Read())
                states.Add($"{reader.GetString(0)}: enabled={reader.GetValue(1)}, crawlCompleted={reader.GetValue(2)}, populateStatus={reader.GetValue(3)}, pendingChanges={reader.GetValue(4)}");
        }
        catch (SqlException error)
        {
            states.Add($"Index diagnostics unavailable (SQL error {error.Number}).");
        }
        throw new TimeoutException($"SQL Server full-text population was not ready after {timer.Elapsed.TotalSeconds:F1}s " +
            $"(budget {readinessTimeout.TotalSeconds:F0}s). Database: {connection.Database}. " +
            string.Join("; ", states) + ". Check SQL full-text population logs; query assertions have not been bypassed.");
    }
}
