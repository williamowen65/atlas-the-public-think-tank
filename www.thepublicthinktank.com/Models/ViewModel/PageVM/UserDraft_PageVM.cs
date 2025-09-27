using atlas_the_public_think_tank.Models.ViewModel.CRUD.Issue;
using atlas_the_public_think_tank.Models.ViewModel.CRUD_VM.Solution;

namespace atlas_the_public_think_tank.Models.ViewModel.PageVM
{
    public class UserDraft_PageVM
    {
        public required Issues_Paginated_ReadVM paginatedUserIssuesDrafts { get; set; }
        public required Solutions_Paginated_ReadVM paginatedUserSolutionsDrafts { get; set; }
    }

}
