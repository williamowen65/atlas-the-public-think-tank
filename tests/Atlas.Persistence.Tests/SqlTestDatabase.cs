using Atlas.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Persistence.Tests;

internal sealed class SqlTestDatabase : IDisposable
{
    private readonly DbContextOptions<AtlasDataContext> _options;
    private SqlTestDatabase(string connection)
    {
        _options = new DbContextOptionsBuilder<AtlasDataContext>().UseSqlServer(connection).Options;
        using var database = Open();
        database.Database.Migrate();
    }
    public static SqlTestDatabase Create()
    {
        var template = Environment.GetEnvironmentVariable("ATLAS_SQL_TEST_CONNECTION_STRING");
        if (string.IsNullOrWhiteSpace(template))
            Assert.Inconclusive("Set ATLAS_SQL_TEST_CONNECTION_STRING to run SQL Server persistence tests.");
        var connection = new SqlConnectionStringBuilder(template)
        { InitialCatalog = $"AtlasRepositoryTests_{Guid.NewGuid():N}" }.ConnectionString;
        return new SqlTestDatabase(connection);
    }
    public AtlasDataContext Open() => new(_options);
    public Guid AddParticipant(Guid? id = null)
    {
        var key = id ?? Guid.NewGuid();
        using var database = Open();
        database.ParticipantRows.Add(new ParticipantRow { Id = key, DisplayName = "Test participant",
            Bio = "", IsActive = true, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow });
        database.SaveChanges();
        return key;
    }
    public Guid AddNode(Guid? id = null)
    {
        var key = id ?? Guid.NewGuid();
        using var database = Open();
        database.NodeRows.Add(new NodeRow { Id = key, Title = "Test node", Status = "Active",
            CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow });
        database.SaveChanges();
        return key;
    }
    public void Dispose()
    {
        using var database = Open();
        database.Database.EnsureDeleted();
    }
}
