
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis;

namespace atlas_the_public_think_tank.Models
{

    /*
     Don't use Data Annotations for EF - Use Fluent API only
     */

    public class AppUser : IdentityUser
    {

        // Navigation properties
        public virtual ICollection<Forum> Forums { get; set; }
        public virtual ICollection<Solution> Solutions { get; set; }
        public virtual ICollection<UserComment> Comments { get; set; }
        public virtual ICollection<UserVote> UserVotes { get; set; }
        public virtual ICollection<UserHistory> UserHistory { get; set; }
    }


    public class Scope
    {
        public int ScopeID { get; set; }
        public string ScopeName { get; set; }

        // Navigation properties
        public virtual ICollection<Forum> Forums { get; set; }
    }

    public class BlockedContent
    {
        public int BlockedContentID { get; set; }
        public short? ReasonID { get; set; }

        // Navigation properties
        public virtual ICollection<Forum> Forums { get; set; }
        public virtual ICollection<Solution> Solutions { get; set; }
        public virtual ICollection<UserComment> Comments { get; set; }
    }

    public class Category
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }

        // Navigation properties
        public virtual ICollection<ForumCategory> ForumCategories { get; set; }
    }

    public class Forum
    {
        public int ForumID { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }

        public ContentStatus ContentStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string AuthorID { get; set; }
        public int ScopeID { get; set; }
        public int? ParentForumID { get; set; }
        public int? BlockedContentID { get; set; }

        // Navigation properties
        public virtual AppUser Author { get; set; }
        public virtual Scope Scope { get; set; }
        public virtual Forum ParentForum { get; set; }
        public virtual ICollection<Forum> ChildForums { get; set; }
        public virtual BlockedContent BlockedContent { get; set; }
        public virtual ICollection<Solution> Solutions { get; set; }
        public virtual ICollection<UserComment> Comments { get; set; }
        public virtual ICollection<UserVote> UserVotes { get; set; }
        public virtual ICollection<ForumCategory> ForumCategories { get; set; }
    }




    public class Solution
    {
        public int SolutionID { get; set; }
        public int ForumID { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public ContentStatus ContentStatus { get; set; }
        public string AuthorID { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public int? BlockedContentID { get; set; }

        // Navigation properties
        public virtual Forum Forum { get; set; }
        public virtual AppUser Author { get; set; }
        public virtual BlockedContent BlockedContent { get; set; }
        public virtual ICollection<UserComment> Comments { get; set; }
        public virtual ICollection<UserVote> UserVotes { get; set; }
    }

    public enum ContentStatus
    {
        Draft,
        Published,
        Archived
    }
    public class UserComment
    {
        public int CommentID { get; set; }
        public int? ForumID { get; set; }
        public int? ForumSolutionID { get; set; }
        public string Comment { get; set; }
        public ContentStatus ContentStatus { get; set; }
        public string AuthorID { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public int? ParentCommentID { get; set; }
        public int? BlockedContentID { get; set; }

        // Navigation properties
        public virtual Forum Forum { get; set; }
        public virtual Solution Solution { get; set; }
        public virtual AppUser Author { get; set; }
        public virtual UserComment ParentComment { get; set; }
        public virtual ICollection<UserComment> ChildComments { get; set; }
        public virtual BlockedContent BlockedContent { get; set; }
        public virtual ICollection<UserVote> UserVotes { get; set; }
    }

    public class UserVote
    {
        public int ForumID { get; set; }
        public int ForumSolutionID { get; set; }
        public int CommentID { get; set; }
        public string UserID { get; set; }
        public int Vote { get; set; }

        // Navigation properties
        public virtual Forum Forum { get; set; }
        public virtual Solution Solution { get; set; }
        public virtual UserComment Comment { get; set; }
        public virtual AppUser User { get; set; }
    }

    public class ForumCategory
    {
        public int CategoryID { get; set; }
        public int ForumID { get; set; }

        // Navigation properties
        public virtual Category Category { get; set; }
        public virtual Forum Forum { get; set; }
    }

    public class UserHistory
    {
        public int UserHistoryID { get; set; }
        public string UserID { get; set; }
        public string Action { get; set; }
        public string Link { get; set; }
        public int? ForumID { get; set; }
        public int? ForumSolutionID { get; set; }
        public int? CommentID { get; set; }
        public int? UserVote { get; set; }
        public DateTime Timestamp { get; set; }

        // Navigation properties
        public virtual AppUser User { get; set; }
        public virtual Forum Forum { get; set; }
        public virtual Solution Solution { get; set; }
        public virtual UserComment Comment { get; set; }
    }
}