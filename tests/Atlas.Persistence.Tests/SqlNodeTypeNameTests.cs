using Atlas.Graph.Nodes.NodeTypes;
using Atlas.Application.Storage;
using Atlas.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Persistence.Tests;

[TestClass]
public sealed class SqlNodeTypeNameTests
{
    /// <summary>Direct SQL writes cannot bypass case-insensitive node-type uniqueness.</summary>
    [TestMethod]
    public void DatabaseRejectsCaseVariantTypeNames()
    {
        using var database = SqlTestDatabase.Create();
        var at = DateTimeOffset.UtcNow;
        var original = NodeTypeDefinition.CreateSystemDefined("Issue", "Issues", at);
        new SqlNodeTypeRepository(database.Open).Save(original);
        using var context = database.Open();
        // Bypass the creation service to prove the database is authoritative under races.
        context.NodeTypeRows.Add(new NodeTypeRow
        {
            Id = Guid.NewGuid(), Name = "issue", Description = "Duplicate",
            IsSystemDefined = true, CreatedAt = at, UpdatedAt = at
        });
        Assert.Throws<DbUpdateException>(() => context.SaveChanges());
    }
}
