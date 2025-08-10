using atlas_the_public_think_tank.Models.Database;
using System;
using System.ComponentModel.DataAnnotations;

namespace atlas_the_public_think_tank.Models.ViewModel
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
    /// Represent the App User in entirety minus sensitive info
    /// </summary>
    public class AppUser_ReadVM
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
        public List<Issue_ReadVM> Issues { get; set; } = new List<Issue_ReadVM>();
        public ContentCount_ReadVM ContentCount { get; set; } = new ContentCount_ReadVM();
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
    }

    /// <summary>
    /// Used to paginate solutions for single issue's issue page
    /// </summary>
    public class PaginatedSolutionsResponse
    {
        public List<Solution_ReadVM> Solutions { get; set; } = new List<Solution_ReadVM>();

        public ContentCount_ReadVM ContentCount { get; set; } = new ContentCount_ReadVM();
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
    }



    /// <summary>
    /// Represents a Cacheable version of the Issue
    /// </summary>
    public class Issue_Cacheable : ContentItem_Cacheable
    {
        public Guid IssueID { get; set; }
        public Guid? ParentIssueID { get; set; }
        public Guid? ParentSolutionID { get; set; }

        public required UserVote_Issue_ReadVM VoteStats { get; set; }

        public List<Category_ReadVM> IssueCategories { get; set; } = new List<Category_ReadVM>();


    }

    public class Issue_ReadVM : Issue_Cacheable
    { 
        public Issue_ReadVM? ParentIssue { get; set; }
        public Solution_ReadVM? ParentSolution { get; set; }
        public PaginatedIssuesResponse PaginatedSubIssues { get; set; } = new PaginatedIssuesResponse();
        public PaginatedSolutionsResponse PaginatedSolutions { get; set; } = new PaginatedSolutionsResponse();
    
    }




    /// <summary>
    /// ViewModel for the reading a solution
    /// </summary>
    public class Solution_Cacheable : ContentItem_Cacheable
    {
        public Guid SolutionID { get; set; }     
        public Guid ParentIssueID { get; set; }


        public required UserVote_Solution_ReadVM VoteStats { get; set; }

        public List<Category_ReadVM> SolutionCategories { get; set; } = new List<Category_ReadVM>();

    }

    public class Solution_ReadVM : Solution_Cacheable
    {
        public Issue_ReadVM? ParentIssue { get; set; }
        public PaginatedIssuesResponse PaginatedSubIssues { get; set; } = new PaginatedIssuesResponse();
        //public Issue_Cacheable ParentIssue { get; set; }
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

        public ContentType ContentType { get; set; }
   

        //public string UserId { get; set; } // This is provided by ASP.NET Identity

        public int TotalVotes { get; set; } = 0;


        public double AverageVote { get; set; } = 0;
    }

   

    public class UserVote_Issue_ReadVM : UserVote_Generic_Cacheable_ReadVM
    {
        /// <summary>
        /// IssueVotes are stored as a map in memory for ease of update in the cache.
        /// </summary>
        public Dictionary<Guid, Vote_ReadVM> IssueVotes { get; set; } = new Dictionary<Guid, Vote_ReadVM>();
    }

    public class UserVote_Solution_ReadVM : UserVote_Generic_Cacheable_ReadVM
    {
        /// <summary>
        /// IssueVotes are stored as a map in memory for ease of update in the cache.
        /// </summary>
        public Dictionary<Guid, Vote_ReadVM> SolutionVotes { get; set; } = new Dictionary<Guid, Vote_ReadVM>();


    }

    public class UserVote_Generic_ReadVM : UserVote_Generic_Cacheable_ReadVM
    {
        /// <summary>
        /// IssueVotes are stored as a map in memory for ease of update in the cache.
        /// </summary>
        public Dictionary<Guid, Vote_ReadVM> GenericContentVotes { get; set; } = new Dictionary<Guid, Vote_ReadVM>();
    }





    public class Vote_ReadVM
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


    public class ContentCount_ReadVM
    { 
        /// <summary>
        /// Represents a count of a content with the filters applied
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Represents a count of a content without any filters applied
        /// </summary>
        public int AbsoluteCount { get; set; }
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