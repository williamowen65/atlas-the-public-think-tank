using atlas_the_public_think_tank.Models.ViewModel.CRUD.Common;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Issue;
using atlas_the_public_think_tank.Models.ViewModel.CRUD_VM.Solution;
using System.ComponentModel.DataAnnotations;

namespace atlas_the_public_think_tank.Models.ViewModel.CRUD.Solution
{

    public class Solution_CreateVM : ContentItem_CreateVM_EditVM
    {
        public Guid? SolutionID { get; set; }
        public Issue_ReadVM? ParentIssue { get; set; }
        public List<SolutionTags> SolutionTags { get; set; } = new List<SolutionTags>();

        [Required(ErrorMessage = "Parent Issue is required when creating a solution")]
        public Guid? ParentIssueID { get; set; }
    }
}
