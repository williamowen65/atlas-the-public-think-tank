
using atlas_the_public_think_tank.Data.DatabaseEntities.History;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Issue;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.User;
using atlas_the_public_think_tank.Models.ViewModel.CRUD_VM.Solution;
using atlas_the_public_think_tank.Models.ViewModel.UI_VM;

namespace atlas_the_public_think_tank.Models.ViewModel.PageVM
{
    public class UserProfile_PageVM 
    {
        public required AppUser_ReadVM appUserVM { get; set; }

        public required List<UserHistory> userHistory { get; set; }

        public required UserStats userStats { get; set; }

        public required Issues_Paginated_ReadVM paginatedUserIssues { get; set; }
        public required Solutions_Paginated_ReadVM paginatedUserSolutions { get; set; }

        //public required List<Issue_ReadVM> paginatedUserIssues { get; set; }
    }

    public class UserStats
    {
        public int Followers { get; set; }
        public int Following { get; set; }
        public int Projects { get; set; }
    }
}
