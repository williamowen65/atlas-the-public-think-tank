using repository_pattern_experiment.Data.SeedData.SeedUsers.Data;
using repository_pattern_experiment.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace repository_pattern_experiment.Data.SeedData.SeedUsers
{
    public class SeedUsers
    {
        public SeedUsers(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AppUser>().HasData(
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
            );
        }
    }
}