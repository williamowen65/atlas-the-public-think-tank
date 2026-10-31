using Atlas.Persistence;
using Atlas.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Atlas.Persistence.Tests;

internal sealed class SqlTestDatabase : IDisposable
{
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<(string Template, bool Model, bool FullText), System.Collections.Concurrent.ConcurrentStack<string>> Pools = new();
    private readonly System.Collections.Concurrent.ConcurrentStack<string>? _pool;
    private bool _disposed;
    private static long _created, _reused, _resetCount, _waitCount, _waitTicks, _resetTicks;
    private readonly DbContextOptions<AtlasDataContext> _options;
    private SqlTestDatabase(string connection, bool useCurrentModel, bool installFullText, System.Collections.Concurrent.ConcurrentStack<string>? pool = null, bool initialize = true)
    {
        _pool = pool;
        _options = new DbContextOptionsBuilder<AtlasDataContext>().UseSqlServer(connection).Options;
        if (!initialize) { Interlocked.Increment(ref _reused); return; }
        Interlocked.Increment(ref _created);
        using var database = Open();
        if (useCurrentModel)
        {
            database.Database.EnsureCreated();
            if (installFullText) SqlFullTextSupport.CreateIndexes(database);
        }
        else AtlasDatabaseSetup.Apply(database);
    }
    public static SqlTestDatabase Create(bool useCurrentModel = false, bool installFullText = true, bool reuse = true)
    {
        // Local projects share one secrets file; CI environment values take precedence.
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<SqlTestDatabase>(optional: true)
            .AddEnvironmentVariables()
            .Build();
        var template = configuration["ATLAS_SQL_TEST_CONNECTION_STRING"];
        if (string.IsNullOrWhiteSpace(template))
            Assert.Inconclusive("Set ATLAS_SQL_TEST_CONNECTION_STRING in user secrets or environment variables to run SQL Server tests.");
        // Lease exclusively: concurrent tests never see one another's rows.
        var pool = reuse ? Pools.GetOrAdd((template!, useCurrentModel, installFullText), _ => new()) : null;
        if (pool is not null && pool.TryPop(out var existing))
            return new SqlTestDatabase(existing, useCurrentModel, installFullText, pool, initialize: false);
        var connection = new SqlConnectionStringBuilder(template)
        {
            InitialCatalog = $"AtlasRepositoryTests_{Guid.NewGuid():N}"
        }.ConnectionString;
        try { return new SqlTestDatabase(connection, useCurrentModel, installFullText, pool); }
        catch
        {
            using var failed = new AtlasDataContext(new DbContextOptionsBuilder<AtlasDataContext>().UseSqlServer(connection).Options);
            failed.Database.EnsureDeleted();
            throw;
        }
    }
    public AtlasDataContext Open() => new(_options);
    public AtlasDataContext OpenForDiscovery()
    {
        WaitForDiscoveryIndexing();
        return Open();
    }
    /// <summary>Explicit barrier after committed searchable writes, before text assertions.</summary>
    public void WaitForDiscoveryIndexing()
    {
        var watch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            using var configuration = Open();
            SqlFullTextSupport.WaitForIndexing(configuration.Database.GetConnectionString()!);
        }
        finally
        {
            Interlocked.Increment(ref _waitCount);
            Interlocked.Add(ref _waitTicks, watch.Elapsed.Ticks);
        }
    }

    public Guid AddParticipant(Guid? id = null)
    {
        var key = id ?? Guid.NewGuid();
        using var database = Open();
        database.ParticipantRows.Add(new ParticipantRow {
            Id = key,
            DisplayName = "Test participant",
            Bio = "",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        database.SaveChanges();
        return key;
    }
    public Guid AddNode(Guid? id = null)
    {
        var key = id ?? Guid.NewGuid();
        using var database = Open();
        database.NodeRows.Add(new NodeRow {
            Id = key,
            Title = "Test node",
            Status = "Active",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        database.SaveChanges();
        return key;
    }
    public Guid AddReaction()
    {
        var actor = AddParticipant();
        var node = AddNode();
        var now = DateTimeOffset.UtcNow;
        var definitions = new Atlas.Application.Storage.SqlReactionDefinitionRepository(Open);
        var definition = Atlas.Graph.Reactions.ReactionDefinition.Create(
            "Helpful",
            "👍",
            "Helpful",
            actor,
            now
        );
        definitions.Save(definition);
        var reaction = Atlas.Graph.Reactions.NodeReaction.Create(
            new Atlas.Graph.Nodes.NodeId(node),
            definition.Id,
            actor,
            actor,
            now
        );
        new Atlas.Application.Storage.SqlNodeReactionRepository(Open).Save(reaction);
        return reaction.Id.Value;
    }
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        using var database = Open();
        if (_pool is null)
        {
            database.Database.EnsureDeleted();
            return;
        }
        try
        {
            ResetData();
            _pool.Push(database.Database.GetConnectionString()!);
        }
        catch
        {
            // A failed reset is never returned to the pool.
            database.Database.EnsureDeleted();
            throw;
        }
    }

    /// <summary>Reset an exclusively held lease; also exercised directly by isolation tests.</summary>
    internal void ResetData()
    {
        var resetWatch = System.Diagnostics.Stopwatch.StartNew();
        using var database = Open();
            // Reset committed data, not a surrounding rollback transaction: full-text must
            // observe real commits. Transactional DDL restores triggers/constraints on failure.
            using var transaction = database.Database.BeginTransaction();
            database.Database.ExecuteSqlRaw("""
                DECLARE @disable nvarchar(max) = N'', @delete nvarchar(max) = N'', @enable nvarchar(max) = N'';
                SELECT @disable += N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(schema_id)) + N'.' + QUOTENAME(name)
                        + N' NOCHECK CONSTRAINT ALL; DISABLE TRIGGER ALL ON '
                        + QUOTENAME(SCHEMA_NAME(schema_id)) + N'.' + QUOTENAME(name) + N';',
                    @delete += N'DELETE FROM ' + QUOTENAME(SCHEMA_NAME(schema_id)) + N'.' + QUOTENAME(name) + N';',
                    @enable += N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(schema_id)) + N'.' + QUOTENAME(name)
                        + N' WITH CHECK CHECK CONSTRAINT ALL; ENABLE TRIGGER ALL ON '
                        + QUOTENAME(SCHEMA_NAME(schema_id)) + N'.' + QUOTENAME(name) + N';'
                FROM sys.tables WHERE is_ms_shipped = 0
                    AND name NOT IN (N'__EFMigrationsHistory', N'DiscoveryRevisions');
                EXEC sys.sp_executesql @disable;
                EXEC sys.sp_executesql @delete;
                EXEC sys.sp_executesql @enable;
                UPDATE dbo.DiscoveryRevisions SET PageVersion = PageVersion + 1, DetailVersion = DetailVersion + 1 WHERE Id = 1;
                """);
            transaction.Commit();
        Interlocked.Increment(ref _resetCount);
        Interlocked.Add(ref _resetTicks, resetWatch.Elapsed.Ticks);
    }

    internal static void DeletePooledDatabases()
    {
        Console.WriteLine($"SQL fixture totals: created={_created}, reused={_reused}, resets={_resetCount}, full-text waits={_waitCount}, wait seconds={TimeSpan.FromTicks(_waitTicks).TotalSeconds:F2}, reset seconds={TimeSpan.FromTicks(_resetTicks).TotalSeconds:F2}. Durations sum across parallel workers; independently managed SQL fixtures are excluded.");
        var failures = new List<Exception>();
        foreach (var pool in Pools.Values)
            while (pool.TryPop(out var connection))
                try
                {
                    using var db = new AtlasDataContext(new DbContextOptionsBuilder<AtlasDataContext>().UseSqlServer(connection).Options);
                    db.Database.EnsureDeleted();
                }
                catch (Exception error) { failures.Add(error); }
        if (failures.Count > 0) throw new AggregateException("Could not delete pooled test databases.", failures);
    }
}
