using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Persistence.Identity;

internal static class IdentityModel
{
    public static void Configure(ModelBuilder model)
    {
        model.Entity<AtlasIdentityUser>(entity =>
        {
            entity.ToTable("Users", "identity");
            entity.Property(user => user.Id).ValueGeneratedNever();
            entity.HasOne<ParticipantRow>().WithOne().HasForeignKey<AtlasIdentityUser>(user => user.Id)
                .OnDelete(DeleteBehavior.NoAction);
            // Identity checks uniqueness in application code; SQL also protects concurrent registration.
            entity.HasIndex(user => user.NormalizedEmail).IsUnique()
                .HasFilter("[NormalizedEmail] IS NOT NULL").HasDatabaseName("EmailIndex");
        });
        model.Entity<IdentityRole<Guid>>().ToTable("Roles", "identity");
        model.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles", "identity");
        model.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims", "identity");
        model.Entity<IdentityUserLogin<Guid>>(entity =>
        {
            entity.ToTable("UserLogins", "identity");
            entity.Property(login => login.LoginProvider).HasMaxLength(128);
            entity.Property(login => login.ProviderKey).HasMaxLength(128);
        });
        model.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims", "identity");
        model.Entity<IdentityUserToken<Guid>>(entity =>
        {
            entity.ToTable("UserTokens", "identity");
            entity.Property(token => token.LoginProvider).HasMaxLength(128);
            entity.Property(token => token.Name).HasMaxLength(128);
        });
        model.Entity<IdentityRole<Guid>>().HasData(
            Role("94000000-0000-4000-8000-000000000001", AtlasRoles.Member),
            Role("94000000-0000-4000-8000-000000000002", AtlasRoles.GlobalModerator),
            Role("94000000-0000-4000-8000-000000000003", AtlasRoles.Administrator));
    }

    private static IdentityRole<Guid> Role(string id, string name) => new()
    {
        Id = Guid.Parse(id), Name = name, NormalizedName = name.ToUpperInvariant(), ConcurrencyStamp = id
    };
}
