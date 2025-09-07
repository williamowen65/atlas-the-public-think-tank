using atlas_the_public_think_tank.Models.Cacheable;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Issue;

namespace atlas_the_public_think_tank.Models.ViewModel.CRUD.Solution
{

    public class Solution_ReadVM : Solution_Cacheable
    {
        public Issue_ReadVM? ParentIssue { get; set; }
        public Issues_Paginated_ReadVM PaginatedSubIssues { get; set; } = new Issues_Paginated_ReadVM();
        //public Issue_Cacheable ParentIssue { get; set; }
    }
}
