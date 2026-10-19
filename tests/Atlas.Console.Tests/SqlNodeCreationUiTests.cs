using Atlas.Application.Eventing;
using Atlas.Application.Storage;
using Atlas.Persistence;
using Atlas.Graph.Nodes.NodeTypes;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Atlas.ConsoleApp.Tests;

[TestClass]
[DoNotParallelize] // Console input/output are process-wide, including when SQL tests are enabled.
public sealed class SqlNodeCreationUiTests
{
    [TestMethod]
    public void CreationWithNoRequestedTypesSucceeds()
    {
        var config = new ConfigurationBuilder().AddUserSecrets<SqlNodeCreationUiTests>(optional: true).AddEnvironmentVariables().Build();
        var template = config["ATLAS_SQL_TEST_CONNECTION_STRING"];
        if (string.IsNullOrWhiteSpace(template)) Assert.Inconclusive("SQL test connection is not configured.");
        var connection = new SqlConnectionStringBuilder(template) { InitialCatalog = "AtlasConsoleUi_" + Guid.NewGuid().ToString("N") }.ConnectionString;
        AtlasDataContext Open() => new(new DbContextOptionsBuilder<AtlasDataContext>().UseSqlServer(connection).Options);
        var actor = Guid.NewGuid();
        var originalIn = System.Console.In; var originalOut = System.Console.Out;
        try
        {
            using (var db = Open())
            {
                db.Database.Migrate();
                db.ParticipantRows.Add(new ParticipantRow { Id = actor, DisplayName = "Author", IsActive = true, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow });
                db.SaveChanges();
            }
            new SqlNodeTypeRepository(Open).Save(NodeTypeDefinition.CreateSystemDefined("Idea", "Ideas", DateTimeOffset.UtcNow));
            using var input = new StringReader("TEST NODE\ntest node\n1\n\n\n"); using var output = new StringWriter();
            System.Console.SetIn(input); System.Console.SetOut(output);
            var node = NodeCreationWorkflow.Create(new SqlNodeRepository(Open), new SqlNodeTypeRepository(Open), new SqlDocumentRepository(Open), actor, new InMemoryEventPublisher());
            Assert.IsNotNull(node); Assert.IsEmpty(node.RequestedSubNodeTypes);
            using var check = Open(); Assert.AreEqual(1, check.NodeRows.Count()); Assert.AreEqual(1, check.NodeCreationQuotaRows.Count());
        }
        finally
        {
            System.Console.SetIn(originalIn); System.Console.SetOut(originalOut);
            using var db = Open(); db.Database.EnsureDeleted();
        }
    }
}
