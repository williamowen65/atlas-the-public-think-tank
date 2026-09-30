using Atlas.ConsoleApp.Storage;
using Atlas.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Persistence.Tests;

[TestClass]
public sealed class SqlPersistenceTests
{
    [TestMethod]
    public void DemoSeed_creates_domain_rows_and_sql_enforces_vote_uniqueness()
    {
        var serverConnection = Environment.GetEnvironmentVariable("ATLAS_SQL_TEST_CONNECTION_STRING");
        if (string.IsNullOrWhiteSpace(serverConnection))
            Assert.Inconclusive("Set ATLAS_SQL_TEST_CONNECTION_STRING to run the SQL Server integration test.");

        var connection = new SqlConnectionStringBuilder(serverConnection)
        {
            InitialCatalog = $"AtlasPersistenceTests_{Guid.NewGuid():N}"
        }.ConnectionString;
        var options = new DbContextOptionsBuilder<AtlasDataContext>().UseSqlServer(connection).Options;
        try
        {
            SqlStorage.Configure(connection); // EF creates an isolated database and applies the migration.
            SqlStorage.SeedDemoData();
            using var database = new AtlasDataContext(options);
            Assert.IsTrue(database.NodeRows.Any());
            Assert.IsTrue(database.ParticipantRows.Any());
            Assert.IsTrue(database.DocumentRows.Any());
            Assert.IsTrue(database.VoteRows.Any());
            Assert.IsNotNull(new SqlNodeRepository(() => new AtlasDataContext(options)).GetById(new Atlas.Graph.Nodes.NodeId(database.NodeRows.First().Id)));

            var factory = () => new AtlasDataContext(options);
            Assert.IsTrue(new SqlNodeTypeRepository(factory).GetAll().Count > 0);
            Assert.IsTrue(new SqlReactionDefinitionRepository(factory).GetAll().Count > 0);
            var reactions = new SqlNodeReactionRepository(factory).GetAll();
            Assert.IsTrue(reactions.Count > 0);
            Assert.IsTrue(reactions.Any(reaction => reaction.AuditHistory.Count > 0));
            Assert.IsTrue(new SqlCommunityRepository(factory).GetAll().Count > 0);
            Assert.IsTrue(new SqlParticipantRepository(factory).GetAll().All(participant => participant.DisplayName.StartsWith("Demo User")));
            Assert.IsTrue(database.Set<NodeParentRow>().Any());
            Assert.IsTrue(database.Set<DocumentBlockRow>().Any());

            var original = database.VoteRows.AsNoTracking().First();
            database.VoteRows.Add(new VoteRow
            {
                Id = Guid.NewGuid(),
                ParticipantId = original.ParticipantId,
                TargetType = original.TargetType,
                TargetId = original.TargetId,
                Value = original.Value,
                CreatedAt = original.CreatedAt,
                UpdatedAt = original.UpdatedAt
            });
            Assert.Throws<DbUpdateException>(() => database.SaveChanges());
        }
        finally
        {
            using var database = new AtlasDataContext(options);
            database.Database.EnsureDeleted(); // Only the uniquely named database created above.
        }
    }
}
