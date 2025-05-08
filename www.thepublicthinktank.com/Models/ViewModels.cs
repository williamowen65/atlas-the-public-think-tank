namespace atlas_the_public_think_tank.Models
{

    public class Forum_CreateVM
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public ContentStatus ContentStatus { get; set; }

        public List<Category> Categories { get; set; } = new List<Category>();
        public int ScopeID { get; set; }
        public int? ParentForumID { get; set; }

        public List<Scope> Scopes { get; set; } = new List<Scope>();
    }

    public class Forum_ReadVM
    {
        public int ForumID { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string AuthorID { get; set; }
        public int ScopeID { get; set; }
        public int? ParentForumID { get; set; }
        public int? BlockedContentID { get; set; }

        // Navigation properties
        public AppUser Author { get; set; }
        public Scope Scope { get; set; }
        public Forum ParentForum { get; set; }
        public ICollection<Forum> ChildForums { get; set; }
        public BlockedContent BlockedContent { get; set; }
        public ICollection<Solution> Solutions { get; set; }
        public ICollection<UserComment> Comments { get; set; }
        public ICollection<UserVote> UserVotes { get; set; }
        public ICollection<ForumCategory> ForumCategories { get; set; }
    }

    public class Category_ReadVM
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
    }

}