
using atlas_the_public_think_tank.Data.DatabaseEntities.History;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Issue;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.User;

namespace atlas_the_public_think_tank.Models.ViewModel.PageVM
{
    public class UserProfile_PageVM 
    {
        public required AppUser_ReadVM appUserVM { get; set; }

        public required List<UserHistory> userHistory { get; set; }

        public required UserStats userStats { get; set; }

        public required List<Issue_ReadVM> usersIssues { get; set; }
    }

    public class UserStats
    {
        public int Issues { get; set; }
        public int Solutions { get; set; }
        public int Followers { get; set; }
        public int Following { get; set; }
        public int Projects { get; set; }
    }
}
