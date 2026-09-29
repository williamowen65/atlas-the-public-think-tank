using atlas_the_public_think_tank.Data.DatabaseEntities.Users;
 

namespace atlas_the_public_think_tank.Data.SeedData.SeedUsers.Data
{
    public static class SeedUserTwelve
    {
        public static AppUser user {
            get {
                return new AppUser
                {
                    Id = new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee41"),
                    UserName = "Seed.User.Twelve",
                    NormalizedUserName = "SEED.USER.TWELVE",
                    Email = "seed.user12@example.com",
                    NormalizedEmail = "SEED.USER12@EXAMPLE.COM",
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