using Atlas.Participants.Participants;
using Atlas.Persistence;
using Atlas.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Identity.Tests;

[TestClass]
public sealed class IdentityModelTests
{
    [TestMethod]
    public void Accounts_use_identity_schema_shared_guid_and_restricted_profile_delete()
    {
        using var context = new AtlasDataContext(new DbContextOptionsBuilder<AtlasDataContext>()
            .UseSqlServer("Server=localhost;Database=ModelOnly;Integrated Security=True;TrustServerCertificate=True").Options);
        var user = context.Model.FindEntityType(typeof(AtlasIdentityUser))!;
        Assert.AreEqual("identity", user.GetSchema());
        var link = user.GetForeignKeys().Single(key => key.PrincipalEntityType.ClrType == typeof(ParticipantRow));
        Assert.IsTrue(link.IsUnique);
        Assert.AreEqual(DeleteBehavior.NoAction, link.DeleteBehavior);
        Assert.AreEqual(nameof(AtlasIdentityUser.Id), link.Properties.Single().Name);
        Assert.IsTrue(user.GetIndexes().Single(index => index.GetDatabaseName() == "EmailIndex").IsUnique);
        Assert.AreEqual(typeof(Guid), user.FindPrimaryKey()!.Properties.Single().ClrType);
    }

    [TestMethod]
    public void Participants_remain_independent_of_identity_and_ef()
    {
        var references = typeof(Participant).Assembly.GetReferencedAssemblies().Select(reference => reference.Name!).ToArray();
        Assert.IsFalse(references.Any(name => name.Contains("Identity") || name.Contains("EntityFramework")));
        Assert.IsFalse(typeof(IdentityUser<Guid>).IsAssignableFrom(typeof(Participant)));
    }
}
