using Atlas.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Atlas.Persistence.Tests;

[TestClass]
public sealed class SqlReferentialIntegrityTests
{
    [TestMethod]
    public void Sql_rejects_missing_description_and_community_owner()
    {
        using var database = SqlTestDatabase.Create();
        var nodeId = database.AddNode();
        using (var context = database.Open())
        {
            context.NodeRows.Single(row => row.Id == nodeId).DescriptionId = Guid.NewGuid();
            AssertForeignKeyViolation(() => context.SaveChanges());
        }
        using (var context = database.Open())
        {
            context.CommunityRows.Add(new CommunityRow { Id = Guid.NewGuid(), Name = "Invalid community",
                Description = "", OwnerParticipantId = Guid.NewGuid(), Status = "Active",
                CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow });
            AssertForeignKeyViolation(() => context.SaveChanges());
        }
        using var reloaded = database.Open();
        Assert.IsNull(reloaded.NodeRows.Single(row => row.Id == nodeId).DescriptionId);
        Assert.AreEqual(0, reloaded.CommunityRows.Count());
    }

    [TestMethod]
    public void Sql_preserves_referenced_participant_and_document_on_delete()
    {
        using var database = SqlTestDatabase.Create();
        var participantId = database.AddParticipant();
        var nodeId = database.AddNode();
        var documentId = Guid.NewGuid();
        using (var context = database.Open())
        {
            context.DocumentRows.Add(new DocumentRow { Id = documentId,
                CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow });
            var node = context.NodeRows.Single(row => row.Id == nodeId);
            node.DescriptionId = documentId;
            node.AuthorId = participantId;
            context.SaveChanges();
        }
        using (var context = database.Open())
        {
            context.DocumentRows.Remove(context.DocumentRows.Single(row => row.Id == documentId));
            AssertForeignKeyViolation(() => context.SaveChanges());
        }
        using (var context = database.Open())
        {
            context.ParticipantRows.Remove(context.ParticipantRows.Single(row => row.Id == participantId));
            AssertForeignKeyViolation(() => context.SaveChanges());
        }
        using var reloaded = database.Open();
        Assert.AreEqual(documentId, reloaded.NodeRows.Single(row => row.Id == nodeId).DescriptionId);
        Assert.IsTrue(reloaded.ParticipantRows.Any(row => row.Id == participantId));
        Assert.IsTrue(reloaded.DocumentRows.Any(row => row.Id == documentId));
    }

    [TestMethod]
    public void Generated_migration_upgrades_existing_rows_without_recreating_database()
    {
        using var database = SqlTestDatabase.Create();
        using var context = database.Open();
        var migrator = context.GetService<IMigrator>();
        migrator.Migrate("20260929180000_InitialRelationalStorage");
        var participantId = database.AddParticipant();
        var nodeId = database.AddNode();
        migrator.Migrate();
        Assert.IsTrue(context.ParticipantRows.Any(row => row.Id == participantId));
        context.NodeRows.Single(row => row.Id == nodeId).AuthorId = Guid.NewGuid();
        AssertForeignKeyViolation(() => context.SaveChanges());
    }

    private static void AssertForeignKeyViolation(Action save)
    {
        var error = Assert.Throws<DbUpdateException>(save);
        Assert.IsInstanceOfType<SqlException>(error.InnerException);
        Assert.AreEqual(547, ((SqlException)error.InnerException!).Number);
    }
}
