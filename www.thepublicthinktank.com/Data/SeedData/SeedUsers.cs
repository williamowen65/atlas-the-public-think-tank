using atlas_the_public_think_tank.Models;
using Microsoft.EntityFrameworkCore;

namespace atlas_the_public_think_tank.Data.SeedData
{
    public class SeedUsers
    {
        public SeedUsers(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AppUser>().HasData(
            new AppUser
            {
                Id = SeedIds.Users.CooperBarker,
                UserName = "Cooper.Barker",
                NormalizedUserName = "COOPER.BARKER",
                Email = "whoLetTheDogsOut@barker.com",
                NormalizedEmail = "WHOLETTHEDOGSOUT@BARKER.COM",
                EmailConfirmed = true,
                PasswordHash = "AQAAAAEAACcQAAAAEExamplePasswordHash==",
                SecurityStamp = "d12ef04d-5b83-4aab-8661-567ffb12e15",
                ConcurrencyStamp = "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
                PhoneNumber = null,
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                LockoutEnd = null,
                LockoutEnabled = true,
                AccessFailedCount = 0
            },
             new AppUser
             {
                 Id = SeedIds.Users.AmeliaKnight,
                 UserName = "amelia.knight",
                 NormalizedUserName = "AMELIA.KNIGHT",
                 Email = "amelia.knight@example.org",
                 NormalizedEmail = "AMELIA.KNIGHT@EXAMPLE.ORG",
                 EmailConfirmed = true,
                 PasswordHash = "AQAAAAEAACcQAAAAEExamplePasswordHash==",
                 SecurityStamp = "d12ef04d-5b83-4aab-8661-567ffb12e11",
                 ConcurrencyStamp = "a1b2c3e4-e5f6-7890-acsd-ef1234567891",
                 PhoneNumber = null,
                 PhoneNumberConfirmed = false,
                 TwoFactorEnabled = false,
                 LockoutEnd = null,
                 LockoutEnabled = true,
                 AccessFailedCount = 0
             });
        }
    }
}