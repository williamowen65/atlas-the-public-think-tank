using Atlas.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

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
        // Local projects share one secrets file; CI environment values take precedence.
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<SqlTestDatabase>(optional: true)
            .AddEnvironmentVariables()
            .Build();
        var template = configuration["ATLAS_SQL_TEST_CONNECTION_STRING"];
        if (string.IsNullOrWhiteSpace(template))
            Assert.Inconclusive("Set ATLAS_SQL_TEST_CONNECTION_STRING in user secrets or environment variables to run SQL Server tests.");
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
    public Guid AddReaction()
    {
        var actor = AddParticipant();
        var node = AddNode();
        var now = DateTimeOffset.UtcNow;
        var definitions = new Atlas.ConsoleApp.Storage.SqlReactionDefinitionRepository(Open);
        var definition = Atlas.Graph.Reactions.ReactionDefinition.Create("Helpful", "👍", "Helpful", actor, now);
        definitions.Save(definition);
        var reaction = Atlas.Graph.Reactions.NodeReaction.Create(new Atlas.Graph.Nodes.NodeId(node), definition.Id, actor, actor, now);
        new Atlas.ConsoleApp.Storage.SqlNodeReactionRepository(Open).Save(reaction);
        return reaction.Id.Value;
    }
    public void Dispose()
    {
        using var database = Open();
        database.Database.EnsureDeleted();
    }
}
