using System;
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

        public List<Guid> SelectedCategoryIds { get; set; } = new List<Guid>();
        public Guid? ParentIssueID { get; set; }
        public Guid? ParentSolutionID { get; set; }

        public Guid ScopeID { get; set; }
        public List<Scope> Scopes { get; set; } = new List<Scope>();


    }



    public interface ICardStatsViewModel
    {
        public List<Solution_ReadVM>? SolutionVM { get => null; }
        public int SubIssueCount { get; }

        public ICollection<UserComment> Comments { get; set; }
        public Scope Scope { get; }
    }


    public class Issue_ReadVM : ICardStatsViewModel
    {
        public Guid IssueID { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public DateTime? LastActivity { get; set; }
        public Guid AuthorID { get; set; }
        public Guid ScopeID { get; set; }
        public Guid? ParentIssueID { get; set; }
        public Guid? ParentSolutionID { get; set; }
        public Guid? BlockedContentID { get; set; }
        public List<Category_ReadVM> Categories { get; set; } = new List<Category_ReadVM>();
        public List<Issue_ReadVM> SubIssues { get; set; } = new List<Issue_ReadVM>();
        public List<Solution_ReadVM> Solutions { get; set; } = new List<Solution_ReadVM>();

        public Issue_ReadVM? ParentIssue { get; set; }
        public Solution_ReadVM? ParentSolution { get; set; }

        public required int SubIssueCount { get; set; }

        public required List<Breadcrumb_ReadVM> BreadcrumbTags { get; set; }


        // Navigation properties
        public AppUser Author { get; set; }
        public Scope Scope { get; set; }
        public ICollection<Issue> ChildIssues { get; set; }
        public BlockedContent BlockedContent { get; set; }
        public ICollection<UserComment> Comments { get; set; }
        public ICollection<IssueVote> IssueVotes { get; set; }
        public ICollection<IssueCategory> IssueCategories { get; set; }
    }

    public class Breadcrumb_ReadVM
    {
        public string Title { get; set; }

        public Guid ContentID { get; set; }

        public ContentType ContentType { get; set; }

        public string Url
        {
            get
            {
                // Adjust base paths as needed for your Razor Pages routes
                return ContentType switch
                {
                    ContentType.Issue => $"/issue/{ContentID}",
                    ContentType.Solution => $"/solution/{ContentID}",
                    _ => "#"
                };
            }
        }
    }
    
    public class Solution_ReadVM : ICardStatsViewModel
    {
        public Guid SolutionID { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public Guid AuthorID { get; set; }
        public Guid IssueID { get; set; }
        public ContentStatus ContentStatus { get; set; }
        public Guid? BlockedContentID { get; set; }

        public required int SubIssueCount { get; set; }
        public required Guid ScopeID { get; set; }
        public required Scope Scope { get; set; }

        public required List<Breadcrumb_ReadVM> BreadcrumbTags { get; set; }

        public List<Issue_ReadVM> SubIssues { get; set; } = new List<Issue_ReadVM>();

        // Navigation properties
        public AppUser Author { get; set; }
        public Issue_ReadVM Issue { get; set; }
        public BlockedContent BlockedContent { get; set; }
        public ICollection<UserComment> Comments { get; set; } = new List<UserComment>();
        // public ICollection<UserVote> UserVotes { get; set; } = new List<UserVote>();
        public List<Category_ReadVM> Categories { get; set; } = new List<Category_ReadVM>();

        public ICollection<SolutionCategory> SolutionCategories { get; set; }

        // Statistics
        // public int TotalVotes { get; set; } = 0;
        // public double AverageVote { get; set; } = 0;
    }

    public class Category_ReadVM
    {
        public Guid CategoryID { get; set; }
        public string CategoryName { get; set; }
    }



    public class UserVote_Generic_ReadVM
    {

        public string ContentType { get; set; } // "Issue", "Solution", or "Comment"
        public Guid ContentID { get; set; }

        // A user may have voted and if so, when loading the dial, their vote should be cast
        public int? UserVote { get; set; }

        //public string UserId { get; set; } // This is provided by ASP.NET Identity

        public int TotalVotes { get; set; } = 0;

        public ICollection<IssueVote> IssueVotes { get; set; } = new List<IssueVote>();

        public double AverageVote { get; set; } = 0;
    }

    public class UserVote_Issue_CreateVM
    {
        public Guid IssueID { get; set; }
        public int VoteValue { get; set; }

        //public AppUser User { get; set; } // The user is captured via injection
    }
    public class UserVote_Solution_CreateVM
    {
        public Guid SolutionID { get; set; }
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
        public Guid? IssueID { get; set; }

        [Required(ErrorMessage = "Status is required")]
        public ContentStatus ContentStatus { get; set; } = ContentStatus.Draft;
        public Guid ScopeID { get; set; }

        // For category selection
        public IEnumerable<Category> Categories { get; set; } = new List<Category>();
        public List<Guid> SelectedCategoryIds { get; set; } = new List<Guid>();

        public List<Scope> Scopes { get; set; } = new List<Scope>();


    }

    public class Scope_ReadVM
    { 
        public string ScopeName { get; set; }
        public Guid ScopeID { get; set; }
    
    }
}