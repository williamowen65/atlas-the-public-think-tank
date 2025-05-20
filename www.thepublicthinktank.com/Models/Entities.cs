using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis;
using System;
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace atlas_the_public_think_tank.Models
{

    /*
     Don't use Data Annotations for EF - Use Fluent API only
     */

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

    public class Scope
    {
        public Guid ScopeID { get; set; }
        public string ScopeName { get; set; }

        // Navigation properties
        [JsonIgnore]
        public virtual ICollection<Issue> Issues { get; set; }
    }

    public class BlockedContent
    {
        public Guid BlockedContentID { get; set; }
        public short? ReasonID { get; set; }

        // Navigation properties
        public virtual ICollection<Issue> Issues { get; set; }
        public virtual ICollection<Solution> Solutions { get; set; }
        public virtual ICollection<UserComment> Comments { get; set; }
    }

    public class Category
    {
        public Guid CategoryID { get; set; }
        public string CategoryName { get; set; }

        // Navigation properties
        public virtual ICollection<IssueCategory> IssueCategories { get; set; }
        public virtual ICollection<SolutionCategory> SolutionCategories { get; set; }
    }

    public class Issue
    {
        public Guid IssueID { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }

        public ContentStatus ContentStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public Guid AuthorID { get; set; }
        public Guid ScopeID { get; set; }
        public Guid? ParentIssueID { get; set; }
        public Guid? ParentSolutionID { get; set; }
        public Guid? BlockedContentID { get; set; }

        // Navigation properties
        public virtual AppUser Author { get; set; }
        public virtual Scope Scope { get; set; }
        public virtual Issue? ParentIssue { get; set; }
        public virtual Solution? ParentSolution { get; set; }
        public virtual ICollection<Issue> ChildIssues { get; set; }
        public virtual BlockedContent BlockedContent { get; set; }
        public virtual ICollection<Solution> Solutions { get; set; }
        public virtual ICollection<UserComment> Comments { get; set; }
        public virtual ICollection<IssueVote> IssueVotes { get; set; } = new List<IssueVote>();
        public virtual ICollection<IssueCategory> IssueCategories { get; set; }
    }




    public class Solution
    {
        public Guid SolutionID { get; set; }
        public Guid IssueID { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public ContentStatus ContentStatus { get; set; }
        public Guid AuthorID { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public Guid? BlockedContentID { get; set; }

        public Guid ScopeID { get; set; }

        // Navigation properties
        [JsonIgnore]
        public virtual Issue Issue { get; set; }
        public virtual AppUser Author { get; set; }
        public virtual BlockedContent BlockedContent { get; set; }
        public virtual ICollection<UserComment> Comments { get; set; } = new List<UserComment>();
        public virtual ICollection<SolutionVote> SolutionVotes { get; set; }

        public virtual ICollection<Issue> ChildIssues { get; set; }

        public virtual Scope Scope { get; set; }

        public virtual ICollection<SolutionCategory> SolutionCategories { get; set; }

    }

    public enum ContentStatus
    {
        Draft,
        Published,
        Archived
    }

    public enum ContentType
    {
        Solution,
        Issue,
        Comment
    }
    public class UserComment
    {
        public Guid CommentID { get; set; }
        public Guid? IssueID { get; set; }
        public Guid? SolutionID { get; set; }
        public string Comment { get; set; }
        public ContentStatus ContentStatus { get; set; }
        public Guid AuthorID { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public Guid? ParentCommentID { get; set; }
        public Guid? BlockedContentID { get; set; }

        // Navigation properties
        public virtual Issue Issue { get; set; }
        public virtual Solution Solution { get; set; }
        public virtual AppUser Author { get; set; }
        public virtual UserComment ParentComment { get; set; }
        public virtual ICollection<UserComment> ChildComments { get; set; }
        public virtual BlockedContent BlockedContent { get; set; }
        public virtual ICollection<CommentVote> CommentVotes { get; set; }  = new List<CommentVote>();
    }

    public class IssueVote
    {
        public Guid VoteID { get; set; }
        public Guid UserID { get; set; }
        public int VoteValue { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        
        public Guid IssueID { get; set; } 

        // Navigation property
        public virtual AppUser User { get; set; }
        public virtual Issue Issue { get; set; }
    }


    public class SolutionVote
    {
        public Guid VoteID { get; set; }
        public Guid UserID { get; set; }
        public int VoteValue { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }

        public Guid SolutionID { get; set; }

        // Navigation property
        public virtual AppUser User { get; set; }
        public virtual Solution Solution{ get; set; }
    }

    public class CommentVote
    {
        public Guid VoteID { get; set; }
        public Guid UserID { get; set; }
        public int VoteValue { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }

        public Guid CommentID { get; set; }

        // Navigation property
        public virtual AppUser User { get; set; }
        public virtual UserComment Comment { get; set; }
    }




    public class IssueCategory
    {
        public Guid CategoryID { get; set; }
        public Guid IssueID { get; set; }

        // Navigation properties
        public virtual Category Category { get; set; }
        public virtual Issue Issue { get; set; }
    }

    public class SolutionCategory
    {
        public Guid CategoryID { get; set; }
        public Guid SolutionID { get; set; }

        // Navigation properties
        public virtual Category Category { get; set; }
        public virtual Solution Solution { get; set; }
    }

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