using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;


/**
* Don't use Data Annotations for EF - Use Fluent API only (ApplicationDbContext)
*/
namespace repository_pattern_experiment.Models.Database
{

  
    /// <summary>
    /// Defines a user of this application
    /// </summary>
    public class AppUser : IdentityUser<Guid>
    {
        public AppUser()
        {
            Issues = new List<Issue>();
            Solutions = new List<Solution>();
            Comments = new List<UserComment>();
            IssueVotes = new List<IssueVote>();
            SolutionVotes = new List<SolutionVote>();
            CommentVotes = new List<CommentVote>();
            UserHistory = new List<UserHistory>();
        }

        [JsonIgnore]
        public virtual ICollection<Issue> Issues { get; set; }
        [JsonIgnore]
        public virtual ICollection<Solution> Solutions { get; set; }
        [JsonIgnore]
        public virtual ICollection<UserComment> Comments { get; set; }
        [JsonIgnore]
        public virtual ICollection<IssueVote> IssueVotes { get; set; }
        public virtual ICollection<SolutionVote> SolutionVotes { get; set; }
        public virtual ICollection<CommentVote> CommentVotes { get; set; }
        [JsonIgnore]
        public virtual ICollection<UserHistory> UserHistory { get; set; }
    }

    /// <summary>
    /// Defines a scopes that can be defined on issues/solutions
    /// </summary>
    public class Scope
    {
        public Guid ScopeID { get; set; }
        public string ScopeName { get; set; }

        // Navigation properties
        [JsonIgnore]
        public virtual ICollection<Issue> Issues { get; set; }
    }

    /// <summary>
    /// Defines content that was deemed inappropriate or another reason to be blocked
    /// </summary>
    public class BlockedContent
    {
        public Guid BlockedContentID { get; set; }
        public short? ReasonID { get; set; }

        // Navigation properties
        public virtual ICollection<Issue> Issues { get; set; }
        public virtual ICollection<Solution> Solutions { get; set; }
        public virtual ICollection<UserComment> Comments { get; set; }
    }

    /// <summary>
    /// Defines a categories (tags) 
    /// These tags can be used to relate solutions or issues from other issue threads.
    /// </summary>
    public class Category
    {
        public Guid CategoryID { get; set; }
        public string CategoryName { get; set; }

        // Navigation properties
        public virtual ICollection<IssueCategory> IssueCategories { get; set; }
        public virtual ICollection<SolutionCategory> SolutionCategories { get; set; }
    }

    /// <summary>
    /// Defines an issue
    /// </summary>
    public class Issue : ContentItem
    {
        public Guid IssueID { get; set; }
        public Guid? ParentIssueID { get; set; }
        public Guid? ParentSolutionID { get; set; }

        // Navigation properties
        public virtual Issue? ParentIssue { get; set; }
        public virtual Solution? ParentSolution { get; set; }

        public virtual ICollection<Issue> ChildIssues { get; set; }
        public virtual ICollection<Solution> Solutions { get; set; }
        public virtual ICollection<IssueVote> IssueVotes { get; set; } = new List<IssueVote>();
        public virtual ICollection<IssueCategory> IssueCategories { get; set; }
    }




    /// <summary>
    /// Defines a solution
    /// </summary>
    public class Solution : ContentItem
    {
        public Guid SolutionID { get; set; }
        public required Guid ParentIssueID { get; set; }

        // Navigation properties
        [JsonIgnore]
        public virtual Issue ParentIssue { get; set; }

        public virtual ICollection<SolutionVote> SolutionVotes { get; set; }
        public virtual ICollection<Issue> ChildIssues { get; set; }
        public virtual ICollection<SolutionCategory> SolutionCategories { get; set; }
    }

    /// <summary>
    /// Defines an enum for ContentStatus with string representations
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ContentStatus
    {
        [EnumMember(Value = "Draft")]
        Draft,

        [EnumMember(Value = "Published")]
        Published,

        [EnumMember(Value = "Archived")]
        Archived
    }

    /// <summary>
    /// Defines an enum for ContentType with string representations
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ContentType
    {
        [EnumMember(Value = "Solution")]
        Solution,

        [EnumMember(Value = "Issue")]
        Issue,

        [EnumMember(Value = "Comment")]
        Comment
    }
    /// <summary>
    /// Defines a comment made on issues or solutions
    /// </summary>
    public class UserComment : ContentBase
    {
        public Guid CommentID { get; set; }
        public Guid? IssueID { get; set; }
        public Guid? SolutionID { get; set; }
        public string Comment { get; set; }

        public Guid? ParentCommentID { get; set; }

        // Navigation properties
        public virtual Issue Issue { get; set; }
        public virtual Solution Solution { get; set; }
        public virtual UserComment ParentComment { get; set; }
        public virtual ICollection<UserComment> ChildComments { get; set; }
        public virtual ICollection<CommentVote> CommentVotes { get; set; } = new List<CommentVote>();
    }


    /// <summary>
    /// Defines a vote on issues
    /// </summary>
    public class IssueVote : VoteBase
    {
        public Guid IssueID { get; set; }
        public virtual Issue Issue { get; set; }
    }



    /// <summary>
    /// Defines a vote on solutions
    /// </summary>
    public class SolutionVote : VoteBase
    {
        public Guid SolutionID { get; set; }
        public virtual Solution Solution { get; set; }
    }


    /// <summary>
    /// Defines a vote on comments
    /// </summary>
    public class CommentVote : VoteBase
    {
        public Guid CommentID { get; set; }
        public virtual UserComment Comment { get; set; }
    }



    /// <summary>
    /// Defines a a table for relating Issues and Categories
    /// </summary>
    public class IssueCategory
    {
        public Guid CategoryID { get; set; }
        public Guid IssueID { get; set; }

        // Navigation properties
        public virtual Category Category { get; set; }
        public virtual Issue Issue { get; set; }
    }

    /// <summary>
    /// Defines a a table for relating Solutions and Categories
    /// </summary>
    public class SolutionCategory
    {
        public Guid CategoryID { get; set; }
        public Guid SolutionID { get; set; }

        // Navigation properties
        public virtual Category Category { get; set; }
        public virtual Solution Solution { get; set; }
    }

    /// <summary>
    /// Defines a a table for  tracking user history
    /// History can include everything from creating account, casting votes, creating post, etc
    /// </summary>
    public class UserHistory
    {
        public Guid UserHistoryID { get; set; }
        public Guid UserID { get; set; }
        public string Action { get; set; }
        public string Link { get; set; }
        public Guid? IssueID { get; set; }
        public Guid? SolutionID { get; set; }
        public Guid? CommentID { get; set; }
        public Guid? UserVoteID { get; set; }
        public DateTime Timestamp { get; set; }

        // Navigation properties
        public virtual AppUser User { get; set; }
        public virtual Issue Issue { get; set; }
        public virtual Solution Solution { get; set; }
        public virtual UserComment Comment { get; set; }
    }
}