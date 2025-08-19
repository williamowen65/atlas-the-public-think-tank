using atlas_the_public_think_tank.Models.Database;
using System.ComponentModel.DataAnnotations;

namespace atlas_the_public_think_tank.Models.ViewModel
{


    public class CreateContentItemBase
    {
        [Display(Name = "Title")]
        [Required(ErrorMessage = "Title is required")]
        [StringLength(150, MinimumLength = 15, ErrorMessage = "Title must be between 15 and 150 characters")]
        public string Title { get; set; }

        [Display(Name = "Body text")]
        [Required(ErrorMessage = "Content is required")]
        public string Content { get; set; }

        public List<SolutionTags> SolutionTags { get; set; } = new List<SolutionTags>();

        public Scope Scope { get; set; }

        public Guid? ParentIssueID { get; set; }

        public ContentStatus ContentStatus { get; set; }
    }


    public class CreateIssueViewModel : CreateContentItemBase
    {

       
        public Guid? ParentSolutionID { get; set; }

        public List<CreateSolutionViewModel> Solutions { get; set; } = new List<CreateSolutionViewModel>();
    }


    public class CreateSolutionViewModel : CreateContentItemBase
    {
        
    }

    public class IssueTags
    { }
    public class SolutionTags
    { }
}
