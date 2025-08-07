using repository_pattern_experiment.Models.Database;
using System;
using System.ComponentModel.DataAnnotations;

namespace repository_pattern_experiment.Models.ViewModel
{

    /// <summary>
    /// Represents info about the Author of a ContentItem
    /// </summary>
    /// <remarks>
    /// Hides personal information 
    /// and extra info that doesn't need to be cached per
    /// ContentItem Cache (Ex: SolutionVotes/commentVotes... 
    /// These can be found via a users profile instead)
    /// </remarks>
    public class AppUser_ContentItem_ReadVM
    { 
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string email { get; set; }

    }
  
    /// <summary>
    /// ViewModel for the creating an issue
    /// </summary>
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


    /// <summary>
    /// Used to paginate issues for single issue or solution page
    /// </summary>
    public class PaginatedIssuesResponse
    {
        public List<Issue_ReadVM> Issues { get; set; }
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
    }

    /// <summary>
    /// Used to paginate solutions for single issue's issue page
    /// </summary>
    public class PaginatedSolutionsResponse
    {
        public List<Solution_ReadVM> Solutions { get; set; }
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
    }



    public class Issue_ReadVM : ContentItem_ReadVM
    {
        public Guid IssueID { get; set; }
        public Guid? ParentIssueID { get; set; }
        public Guid? ParentSolutionID { get; set; }

        public required UserVote_Issue_ReadVM VoteStats { get; set; }

        public List<Category_ReadVM> IssueCategories { get; set; } = new List<Category_ReadVM>();

        public PaginatedIssuesResponse PaginatedSubIssues { get; set; } = new PaginatedIssuesResponse();
        public PaginatedSolutionsResponse PaginatedSolutions { get; set; } = new PaginatedSolutionsResponse();

    }



    /// <summary>
    /// ViewModel for the reading a solution
    /// </summary>
    public class Solution_ReadVM : ContentItem_ReadVM
    {
        public Guid SolutionID { get; set; }     
        public Guid ParentIssueID { get; set; }

        public PaginatedIssuesResponse PaginatedSubIssues { get; set; } = new PaginatedIssuesResponse();

        // Navigation properties
        public Issue_ReadVM ParentIssue { get; set; }
       
        // public ICollection<UserVote> UserVotes { get; set; } = new List<UserVote>();
        public List<Category_ReadVM> Categories { get; set; } = new List<Category_ReadVM>();

        public ICollection<SolutionCategory> SolutionCategories { get; set; }

        // Statistics
        // public int TotalVotes { get; set; } = 0;
        // public double AverageVote { get; set; } = 0;
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

    /// <summary>
    /// ViewModel for the reading a category
    /// </summary>
    public class Category_ReadVM
    {
        public Guid CategoryID { get; set; }
        public string CategoryName { get; set; }
    }



    /// <summary>
    /// A generic ViewModel for the reading the vote content (issues/solutions/comments)
    /// </summary>
    public class UserVote_Generic_Cacheable_ReadVM
    {

        //public ContentType ContentType { get; set; } // "Issue", "Solution", or "Comment"
        public Guid ContentID { get; set; }

   

        //public string UserId { get; set; } // This is provided by ASP.NET Identity

        public int TotalVotes { get; set; } = 0;


        public double AverageVote { get; set; } = 0;
    }

    /// <summary>
    /// This value is important for the user, but not needed for the cached entity
    /// </summary>
    public class UserVote_Generic_ReadVM : UserVote_Generic_Cacheable_ReadVM
    {
        // A user may have voted and if so, when loading the dial, their vote should be cast
        public int? UserVote { get; set; }

    }

    public class UserVote_Issue_ReadVM : UserVote_Generic_ReadVM
    { 
        public List<IssueVote_ReadVM> IssueVotes { get; set; } = new List<IssueVote_ReadVM>();

    }

    public class IssueVote_ReadVM
    {
        public Guid VoteID { get; set; }
        public Guid UserID { get; set; }
        public int VoteValue { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }


    /// <summary>
    /// ViewModel for creating a vote on an issue
    /// </summary>
    public class UserVote_Issue_CreateVM
    {
        public Guid IssueID { get; set; }
        public int VoteValue { get; set; }

        //public AppUser User { get; set; } // The user is captured via injection
    }

    /// <summary>
    /// ViewModel for creating a vote on an solution
    /// </summary>
    public class UserVote_Solution_CreateVM
    {
        public Guid SolutionID { get; set; }
        public int VoteValue { get; set; }

        //public AppUser User { get; set; } // The user is captured via injection
    }

    /// <summary>
    /// An Enum to describe possible alert types
    /// </summary>
    public enum AlertType
    {
        success,
        info,
        warning,
        danger,
        error,
        plaintext
    }


    /// <summary>
    /// ViewModel for reading an alert
    /// </summary>
    public class Alert_ReadVM
    {
        public string Message { get; set; }

        public bool Dismissible { get; set; } = true;

        public int Timeout { get; set; }

        public string Id { get; set; } = Guid.NewGuid().ToString();

        public AlertType Type { get; set; } = AlertType.info;
    }

    /// <summary>
    /// ViewModel for creating a solution
    /// </summary>
    public class Solution_CreateVM
    {

        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, ErrorMessage = "Title cannot be longer than 100 characters")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Content is required")]
        public string Content { get; set; }

        [Required(ErrorMessage = "An issue must be selected")]
        public Guid? ParentIssueID { get; set; }

        [Required(ErrorMessage = "Status is required")]
        public ContentStatus ContentStatus { get; set; } = ContentStatus.Draft;
        public Guid ScopeID { get; set; }

        // For category selection
        public IEnumerable<Category> Categories { get; set; } = new List<Category>();
        public List<Guid> SelectedCategoryIds { get; set; } = new List<Guid>();

        public List<Scope> Scopes { get; set; } = new List<Scope>();


    }

    /// <summary>
    /// ViewModel for reading the scope of an issue or solution
    /// </summary>
    public class Scope_ReadVM
    {
        public string ScopeName { get; set; }
        public Guid ScopeID { get; set; }

    }


    /// <summary>
    /// ViewModel for error handling
    /// </summary>
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }


    public class GradientBorderModel
    {
        public string Link { get; set; } = string.Empty;
        public string LinkText { get; set; } = "Default Link Text";
    }
}