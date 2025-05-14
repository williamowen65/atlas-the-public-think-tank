using System.ComponentModel.DataAnnotations;

namespace atlas_the_public_think_tank.Models
{

    public class HomeIndexViewModel
    {
        public List<Issue_ReadVM> Issues { get; set; } = new List<Issue_ReadVM>();
        public List<Category_ReadVM> Categories { get; set; } = new List<Category_ReadVM>();
    }

    public class Issue_CreateVM
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public ContentStatus ContentStatus { get; set; }

        public List<Category> Categories { get; set; } = new List<Category>();

        public List<int> SelectedCategoryIds { get; set; } = new List<int>();
        public int ScopeID { get; set; }
        public int? ParentIssueID { get; set; }

        public List<Scope> Scopes { get; set; } = new List<Scope>();
    }



    public class Issue_ReadVM
    {
        public int IssueID { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public DateTime? LastActivity { get; set; }
        public string AuthorID { get; set; }
        public int ScopeID { get; set; }
        public int? ParentIssueID { get; set; }
        public int? BlockedContentID { get; set; }
        public List<Category_ReadVM> Categories { get; set; } = new List<Category_ReadVM>();
        public List<Issue_ReadVM> SubIssues { get; set; } = new List<Issue_ReadVM>();
        public List<Solution_ReadVM> SolutionVM { get; set; } = new List<Solution_ReadVM>();

        public Issue_ReadVM? ParentIssueVM { get; set; }

        public required int SubIssueCount { get; set; }


        // Navigation properties
        public AppUser Author { get; set; }
        public Scope Scope { get; set; }
        public Issue ParentIssue { get; set; }
        public ICollection<Issue> ChildIssues { get; set; }
        public BlockedContent BlockedContent { get; set; }
        public ICollection<Solution> Solutions { get; set; }
        public ICollection<UserComment> Comments { get; set; }
        public ICollection<UserVote> UserVotes { get; set; }
        public ICollection<IssueCategory> IssueCategories { get; set; }
    }
    
    public class Solution_ReadVM
    {
        public int SolutionID { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string AuthorID { get; set; }
        public int IssueID { get; set; }
        public ContentStatus ContentStatus { get; set; }
        public int? BlockedContentID { get; set; }

        // Navigation properties
        public AppUser Author { get; set; }
        public Issue Issue { get; set; }
        public BlockedContent BlockedContent { get; set; }
        public ICollection<UserComment> Comments { get; set; } = new List<UserComment>();
        // public ICollection<UserVote> UserVotes { get; set; } = new List<UserVote>();
        public List<Category_ReadVM> Categories { get; set; } = new List<Category_ReadVM>();

        // Statistics
        // public int TotalVotes { get; set; } = 0;
        // public double AverageVote { get; set; } = 0;
    }

    public class Category_ReadVM
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
    }



    public class UserVote_Generic_ReadVM
    {

        public string ContentType { get; set; } // "Issue", "Solution", or "Comment"
        public int ContentID { get; set; }

        // A user may have voted and if so, when loading the dial, their vote should be cast
        public int? UserVote { get; set; }

        //public string UserId { get; set; } // This is provided by ASP.NET Identity

        public int TotalVotes { get; set; } = 0;

        public ICollection<UserVote> UserVotes { get; set; } = new List<UserVote>();

        public double AverageVote { get; set; } = 0;
    }

    public class UserVote_Issue_CreateVM
    {
        public int IssueID { get; set; }
        public int VoteValue { get; set; }

        //public AppUser User { get; set; } // The user is captured via injection
    }
    public enum AlertType
    {
        success,
        info,
        warning,
        danger,
        error,
        plaintext
    }


    public class Alert_ReadVM
    {
        public string Message { get; set; }

        public bool Dismissible { get; set; } = true;

        public int Timeout { get; set; }

        public string Id { get; set; } = Guid.NewGuid().ToString();

        public AlertType Type { get; set; } = AlertType.info;
    }
    
       public class Solution_CreateVM
    {

        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, ErrorMessage = "Title cannot be longer than 100 characters")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Content is required")]
        public string Content { get; set; }

        [Required(ErrorMessage = "An issue must be selected")]
        public int? IssueID { get; set; }

        [Required(ErrorMessage = "Status is required")]
        public ContentStatus ContentStatus { get; set; } = ContentStatus.Draft;

        // For category selection
        public IEnumerable<Category> Categories { get; set; } = new List<Category>();
        public List<int> SelectedCategoryIds { get; set; } = new List<int>();

    }

    public class Scope_ReadVM
    { 
        public string ScopeName { get; set; }
        public int ScopeID { get; set; }
    
    }
}