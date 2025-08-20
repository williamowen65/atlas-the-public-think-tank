using atlas_the_public_think_tank.Models.Database;
using System.ComponentModel.DataAnnotations;

namespace atlas_the_public_think_tank.Models.ViewModel
{

    public class CreateIssuePageViewModel()
    {

        
        public List<Scope> Scopes { get; set; } = new List<Scope>();

        /// <summary>
        /// Represents the main issue content that the user is creating
        /// </summary>
        public CreateIssueViewModel MainIssue { get; set; } = new CreateIssueViewModel();

        /// <summary>
        /// When creating an issue, the user has the options to create solutions to that issue
        /// via the same interface. This is to help the user to not write out solutions inside the content for the issue.
        /// It's okay for this list to be empty. 
        /// </summary>
        /// <remarks>
        /// Special business logic: For any of the solutions to be published, their parents issue must be published
        /// </remarks>
        public List<CreateSolutionViewModel> Solutions { get; set; } = new List<CreateSolutionViewModel>();
    }


    public class ContentCreationResponseBase
    {
        public bool Success { get; set; }

      

        /// <summary>
        /// Collection of validation errors or other error messages that occurred during content creation
        /// </summary>
        public List<List<string>> Errors { get; set; } = new List<List<string>>();
    }

    public class IssueCreationResponse : ContentCreationResponseBase
    {

        /// <summary>
        /// Content would be the newly created content (plus the content ID)
        /// </summary>
        public string Content { get; set; }

    }


    public class CreateContentItemBase
    {
        [Display(Name = "Title")]
        [Required(ErrorMessage = "Title is required")]
        [StringLength(150, MinimumLength = 15, ErrorMessage = "Title must be between 15 and 150 characters")]
        public string Title { get; set; }

        [Display(Name = "Body text")]
        [MinLength(30, ErrorMessage = "Content must be at least 30 characters")]
        [Required(ErrorMessage = "Content is required")]
        public string Content { get; set; }


        /// <summary>
        /// ScopeId is Guid? with an DataAnnotation of Required 
        /// to intentionally throw the correct error response.
        /// This is because Guid is a non nullable type.
        /// </summary>
        [Required(ErrorMessage = "Scope is required")]
        [Display(Name = "Scope")]
        public Guid? ScopeID { get; set; }


        public ContentStatus ContentStatus { get; set; }
    }


    public class CreateIssueViewModel : CreateContentItemBase
    {

        public List<IssueTags> IssueTags { get; set; } = new List<IssueTags>();
       
        public Guid? ParentSolutionID { get; set; }
        public Guid? ParentIssueID { get; set; }
    }


    public class CreateSolutionViewModel : CreateContentItemBase
    {
        public Guid ParentIssueID { get; set; }
        public List<SolutionTags> SolutionTags { get; set; } = new List<SolutionTags>();
        
    }

    public class IssueTags
    { }
    public class SolutionTags
    { }

    class CreateIssueWrapper
    {
        public CreateIssueViewModel? Issue { get; set; }
        public List<Scope> Scopes { get; set; }
    }
    class EditIssueWrapper
    {
        public Issue_ReadVM? Issue { get; set; }
        public List<Scope> Scopes { get; set; }
    }
}
