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
            Assert.AreEqual(database.NodeRows.Count(),
                System.Text.Json.JsonSerializer.Deserialize<List<StoredNode>>(SqlStorage.ReadText("nodes"))!.Count);

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
