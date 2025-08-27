using atlas_the_public_think_tank.Models.Cacheable;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Solution;
using atlas_the_public_think_tank.Models.ViewModel.CRUD_VM.Solution;

namespace atlas_the_public_think_tank.Models.ViewModel.CRUD.Issue
{

    public class Issue_ReadVM : Issue_Cacheable
    {
        public Issue_ReadVM? ParentIssue { get; set; }
        public Solution_ReadVM? ParentSolution { get; set; }
        public Issues_Paginated_ReadVM PaginatedSubIssues { get; set; } = new Issues_Paginated_ReadVM();
        public Solutions_Paginated_ReadVM PaginatedSolutions { get; set; } = new Solutions_Paginated_ReadVM();
    }
}
