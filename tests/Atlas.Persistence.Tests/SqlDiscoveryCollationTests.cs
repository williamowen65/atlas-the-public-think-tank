using Atlas.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Persistence.Tests;

[TestClass]
public sealed class SqlDiscoveryCollationTests
{
    [TestMethod]
    public void Migrated_columns_match_case_and_accents_and_preserve_unicode()
    {
        using var fixture = SqlTestDatabase.Create();
        using var database = fixture.Open();
        var now = DateTimeOffset.UtcNow;
        var node = new NodeRow { Id = Guid.NewGuid(), Title = "Café 世界 🐋", Status = "Active", CreatedAt = now, UpdatedAt = now };
        var block = new BlockRow {
            Id = Guid.NewGuid(), Kind = "Markdown", Markdown = "Résumé 世界 🐋",
            AltText = "Résumé", Caption = "Résumé", Title = "Résumé", Description = "Résumé",
            CreatedAt = now, UpdatedAt = now
        };
        database.NodeRows.Add(node);
        database.BlockRows.Add(block);
        database.SaveChanges();
        database.ChangeTracker.Clear();
        Assert.AreEqual(node.Id, database.NodeRows.Single(row => row.Title == "CAFE 世界 🐋").Id);
        Assert.AreEqual("Café 世界 🐋", database.NodeRows.AsNoTracking().Single(row => row.Id == node.Id).Title);
        Assert.AreEqual(block.Id, database.BlockRows.Single(row => row.Markdown!.Contains("RESUME") &&
            row.AltText == "resume" && row.Caption == "resume" && row.Title == "resume" && row.Description == "resume").Id);
        Assert.AreEqual("Résumé 世界 🐋", database.BlockRows.AsNoTracking().Single(row => row.Id == block.Id).Markdown);
        var columns = database.Database.SqlQueryRaw<string>(
            "SELECT collation_name AS [Value] FROM sys.columns WHERE object_id = OBJECT_ID(N'NodeRows') AND name = N'Title'").ToList();
        Assert.AreEqual(DiscoveryTextCollation.Name, columns.Single());
    }
}
