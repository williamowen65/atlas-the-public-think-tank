using repository_pattern_experiment.Models.Database;

namespace repository_pattern_experiment.Data.SeedData.SeedUsers.Data
{
    public static class SeedUserOne
    {
        public static AppUser user {
            get {
                return new AppUser
                {
                    Id = new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"),
                    UserName = "Seed.User.One",
                    NormalizedUserName = "SEED.USER.ONE",
                    Email = "seed.user1@example.com",
                    NormalizedEmail = "SEED.USER1@EXAMPLE.COM",
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
