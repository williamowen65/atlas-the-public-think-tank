using atlas_the_public_think_tank.Models.Database;

namespace atlas_the_public_think_tank.Data.SeedData.SeedUsers.Data
{
    public static class SeedUserNine
    {
        public static AppUser user {
            get {
                return new AppUser
                {
                    Id = new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee38"),
                    UserName = "Seed.User.Nine",
                    NormalizedUserName = "SEED.USER.NINE",
                    Email = "seed.user9@example.com",
                    NormalizedEmail = "SEED.USER9@EXAMPLE.COM",
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
                };
            }
        } 
    }
}