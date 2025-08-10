using atlas_the_public_think_tank.Models.Database;

namespace atlas_the_public_think_tank.Models.ViewModel
{

    /// <summary>
    /// Issues and Solutions inherit this super class
    /// </summary>
    /// <remarks>
    /// The home page serves this as content.
    /// </remarks>
    public class ContentItem_Cacheable
    {

        public required string Title { get; set; }
        public required string Content { get; set; }
        public required DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public DateTime? LastActivity { get; set; }
        //public required Guid AuthorID { get; set; }
        //public required Guid ScopeID { get; set; }
        public required ContentStatus ContentStatus { get; set; }

        public Guid? BlockedContentID { get; set; }
        

        /*
            Some of these below items shouldn't be in the cache via this item... Breadcrumb, Author, Comments
        */
        public required List<Breadcrumb_ReadVM> BreadcrumbTags { get; set; }

        // Navigation Prop
        public AppUser_ContentItem_ReadVM Author { get; set; }
        public ICollection<UserComment> Comments { get; set; } = new List<UserComment>();
        public required Scope Scope { get; set; }

        public BlockedContent BlockedContent { get; set; }
    }

    public class ContentItem_ReadVM : ContentItem_Cacheable
    {
        public Guid ContentID { get; set; }
        public required UserVote_Generic_ReadVM VoteStats { get; set; }
        public ContentType ContentType { get; set; }
        public PaginatedIssuesResponse PaginatedSubIssues { get; set; } = new PaginatedIssuesResponse();
        public PaginatedSolutionsResponse? PaginatedSolutions { get; set; } = new PaginatedSolutionsResponse();
    }
}
