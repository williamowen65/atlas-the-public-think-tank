using Atlas.Infrastructure.Storage;
using Atlas.Identity;
using Microsoft.Extensions.DependencyInjection;
using Atlas.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Atlas.Persistence.Tests;

[TestClass]
public sealed class SqlPersistenceTests
{
    [TestMethod]
    public async Task DemoSeed_creates_domain_rows_and_sql_enforces_vote_uniqueness()
    {
        // Match the other SQL fixtures: shared local secrets, overridden by CI's environment.
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<SqlPersistenceTests>(optional: true)
            .AddEnvironmentVariables()
            .Build();
        var serverConnection = configuration["ATLAS_SQL_TEST_CONNECTION_STRING"];
        if (string.IsNullOrWhiteSpace(serverConnection))
            Assert.Inconclusive("Set ATLAS_SQL_TEST_CONNECTION_STRING in user secrets or environment variables to run the SQL Server integration test.");

        var connection = new SqlConnectionStringBuilder(serverConnection)
        {
            InitialCatalog = $"AtlasPersistenceTests_{Guid.NewGuid():N}"
        }.ConnectionString;
        var options = new DbContextOptionsBuilder<AtlasDataContext>().UseSqlServer(connection).Options;
        try
        {
            SqlStorage.Configure(connection); // EF creates an isolated database and applies the migration.
            using var identityServices = new ServiceCollection().AddAtlasIdentity(connection)
                .BuildServiceProvider(new ServiceProviderOptions {
                    ValidateScopes = true
                });
            await SqlStorage.SeedDemoDataAsync(identityServices.GetRequiredService<IServiceScopeFactory>());
            using var database = new AtlasDataContext(options);
            Assert.IsTrue(database.NodeRows.Any());
            Assert.IsTrue(database.ParticipantRows.Any());
            Assert.IsTrue(database.DocumentRows.Any());
            Assert.IsTrue(database.VoteRows.Any());
            Assert.IsNotNull(new SqlNodeRepository(() => new AtlasDataContext(options)).GetById(new Atlas.Graph.Nodes.NodeId(database.NodeRows.First().Id)));

            var factory = () => new AtlasDataContext(options);
            Assert.IsNotEmpty(new SqlNodeTypeRepository(factory).GetAll());
            Assert.IsNotEmpty(new SqlReactionDefinitionRepository(factory).GetAll());
            var reactions = new SqlNodeReactionRepository(factory).GetAll();
            Assert.IsNotEmpty(reactions);
            Assert.IsTrue(reactions.Any(reaction => reaction.AuditHistory.Count > 0));
            Assert.IsNotEmpty(new SqlCommunityRepository(factory).GetAll());
            Assert.IsTrue(new SqlParticipantRepository(factory).GetAll().All(participant => participant.DisplayName.StartsWith("Demo User")));
            Assert.IsTrue(database.Set<NodeParentRow>().Any());
            Assert.IsTrue(database.Set<DocumentBlockRow>().Any());

            // Walk the actual seeded graph: 3 + 3 + 3 + 1 ancestors, with branch notices.
            var cursor = Guid.Parse("b1000000-0000-4000-8000-000000000110");
            var visited = new List<Guid> { cursor };
            var segmentSizes = new List<int>();
            var alternativeSegments = 0;
            do
            {
                var segment = Atlas.Persistence.Discovery.DiscoveryBreadcrumbReader.ReadPage(database, cursor, visited);
                segmentSizes.Add(segment.Items.Count);
                if (segment.HasAlternativeRoutes)
                    alternativeSegments++;
                Assert.IsFalse(segment.CycleDetected);
                visited.AddRange(segment.Items.Select(item => item.NodeId));
                if (segment.ContinuationNodeId is not Guid next)
                    break;
                Assert.Contains("…", segment.DisplayText, StringComparison.Ordinal);
                cursor = next;
            } while (segmentSizes.Count < 5);
            CollectionAssert.AreEqual(new[] { 3, 3, 3, 1 }, segmentSizes.ToArray());
            Assert.AreEqual(2, alternativeSegments);
            Assert.AreEqual(Guid.Parse("b1000000-0000-4000-8000-000000000023"), visited.Last());

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
