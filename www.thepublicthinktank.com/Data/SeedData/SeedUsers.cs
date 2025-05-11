using atlas_the_public_think_tank.Models;
using Microsoft.EntityFrameworkCore;

namespace atlas_the_public_think_tank.Data.SeedData
{
    public class SeedUsers
    {

       public static Dictionary<string, string> UserIds { get; } = new Dictionary<string, string> {
            { "user1", "1a61454c-5b83-4aab-8661-96d6dffbee30" },
            { "user2", "1a61454c-5b83-4aab-8661-96d6dffbe31" }
        };
        public SeedUsers(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AppUser>().HasData(
            new AppUser
            {
                Id = "1a61454c-5b83-4aab-8661-96d6dffbee30", // Static GUID
                UserName = "Cooper.Barker",
                NormalizedUserName = "COOPER.BARKER",
                Email = "whoLetTheDogsOut@barker.com",
                NormalizedEmail = "WHOLETTHEDOGSOUT@BARKER.COM",
                EmailConfirmed = true,
                PasswordHash = "AQAAAAEAACcQAAAAEExamplePasswordHash==", // Example hash for a password
                SecurityStamp = "d12ef04d-5b83-4aab-8661-567ffb12e15", // Static SecurityStamp
                ConcurrencyStamp = "a1b2c3d4-e5f6-7890-abcd-ef1234567890", // Static ConcurrencyStamp
                PhoneNumber = null, // Optional
                PhoneNumberConfirmed = false, // Optional
                TwoFactorEnabled = false, // Optional
                LockoutEnd = null, // Optional
                LockoutEnabled = true, // Optional
                AccessFailedCount = 0 // Optional
            },
             new AppUser
             {
                 Id = "1a61454c-5b83-4aab-8661-96d6dffbe31", // Static GUID
                 UserName = "amelia.knight",
                 NormalizedUserName = "AMELIA.KNIGHT",
                 Email = "amelia.knight@example.org",
                 NormalizedEmail = "AMELIA.KNIGHT@EXAMPLE.ORG",
                 EmailConfirmed = true,
                 PasswordHash = "AQAAAAEAACcQAAAAEExamplePasswordHash==", // Example hash for a password
                 SecurityStamp = "d12ef04d-5b83-4aab-8661-567ffb12e11", // Static SecurityStamp
                 ConcurrencyStamp = "a1b2c3e4-e5f6-7890-acsd-ef1234567891", // Static ConcurrencyStamp
                 PhoneNumber = null, // Optional
                 PhoneNumberConfirmed = false, // Optional
                 TwoFactorEnabled = false, // Optional
                 LockoutEnd = null, // Optional
                 LockoutEnabled = true, // Optional
                 AccessFailedCount = 0 // Optional
             });
        }
    }
}
