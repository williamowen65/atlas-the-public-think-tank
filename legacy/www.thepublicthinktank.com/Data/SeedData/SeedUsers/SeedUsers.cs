using atlas_the_public_think_tank.Data.DatabaseEntities.Users;
using atlas_the_public_think_tank.Data.DbContext;
using atlas_the_public_think_tank.Data.SeedData.SeedUsers.Data;
using Microsoft.EntityFrameworkCore;

namespace atlas_the_public_think_tank.Data.SeedData.SeedUsers
{
    public static class SeedUsers
    {
        public static void Seed(ApplicationDbContext context)
        {
            // Collect all seed users into a list
            var users = new[]
            {
                SeedUserOne.user,
                SeedUserTwo.user,
                SeedUserThree.user,
                SeedUserFour.user,
                SeedUserFive.user,
                SeedUserSix.user,
                SeedUserSeven.user,
                SeedUserEight.user,
                SeedUserNine.user,
                SeedUserTen.user,
                SeedUserEleven.user,
                SeedUserTwelve.user,
                SeedUserThirteen.user,
                SeedUserFourteen.user,
                SeedUserFifteen.user,
                SeedUserSixteen.user,
                SeedUserSeventeen.user,
                SeedUserEighteen.user,
                SeedUserNineteen.user,
                SeedUserTwenty.user
            };

            // Add users that don't already exist (by Id or unique property)
            var existingIds = context.Users.Select(u => u.Id).ToHashSet();
            var newUsers = users.Where(u => !existingIds.Contains(u.Id)).ToList();

            if (newUsers.Any())
            {
                context.Users.AddRange(newUsers);
            }
        }
    }
}