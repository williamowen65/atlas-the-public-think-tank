using atlas_the_public_think_tank.Models.ViewModel.CRUD.Common;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Solution;
using atlas_the_public_think_tank.Models.ViewModel.CRUD_VM.Issue;

namespace atlas_the_public_think_tank.Models.ViewModel.CRUD.Issue
{


    public class Issue_CreateVM : ContentItem_CreateVM_EditVM
    {
        public Guid? IssueID { get; set; }
        public List<IssueTags> IssueTags { get; set; } = new List<IssueTags>();

        public Guid? ParentSolutionID { get; set; }
        public Guid? ParentIssueID { get; set; }

        public Issue_ReadVM? ParentIssue { get; set; }
        public Solution_ReadVM? ParentSolution { get; set; }
    }
}
