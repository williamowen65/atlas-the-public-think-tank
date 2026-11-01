using System.Diagnostics;
using Atlas.Persistence;
using Atlas.Persistence.Discovery;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Testing;

/// <summary>Test-only readiness barriers and captured diagnostics for the asynchronous full-text indexer.</summary>
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
        // Index population is asynchronous and competes with other disposable databases.
        // This is setup readiness, not a search-query performance budget.
        var readinessTimeout = TimeSpan.FromMinutes(3);
        var timer = Stopwatch.StartNew();
        var history = new List<string>();
        var nextSnapshot = TimeSpan.FromSeconds(15);
        void Snapshot(string phase)
        {
            // Print to MSTest's captured standard output and retain the same timeline
            // in the exception, so failed Test Explorer results include it as well.
            // Never log connection strings or credentials.
            var entry = $"[{DateTimeOffset.UtcNow:O}] full-text {phase}; " +
                $"server={connection.DataSource}; database={connection.Database}; " +
                $"elapsed={timer.Elapsed.TotalSeconds:F1}s; {ReadIndexStates(connection)}";
            history.Add(entry);
            Console.WriteLine(entry);
        }
        Snapshot("START");
        if (Convert.ToInt32(schema.ExecuteScalar()) != 4)
        {
            Snapshot("SCHEMA ERROR");
            throw new InvalidOperationException(
                "Expected four full-text indexes. Run explicit database setup after applying the EF schema." +
                Environment.NewLine + string.Join(Environment.NewLine, history));
        }
        do
        {
            if (Convert.ToInt32(command.ExecuteScalar()) == 0)
            {
                Snapshot("READY");
                return;
            }
            if (timer.Elapsed >= nextSnapshot)
            {
                Snapshot("WAITING");
                nextSnapshot = timer.Elapsed + TimeSpan.FromSeconds(15);
            }
            Thread.Sleep(100);
        } while (timer.Elapsed < readinessTimeout);
        Snapshot("TIMEOUT");
        throw new TimeoutException($"SQL Server full-text population was not ready after {timer.Elapsed.TotalSeconds:F1}s " +
            $"(budget {readinessTimeout.TotalSeconds:F0}s). " +
            string.Join(Environment.NewLine, history) +
            ". Check SQL full-text population logs; query assertions have not been bypassed.");
    }

    private static string ReadIndexStates(SqlConnection connection)
    {
        using var diagnostics = connection.CreateCommand();
        diagnostics.CommandTimeout = 5;
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
        return states.Count == 0 ? "No matching full-text indexes." : string.Join("; ", states);
    }
}
