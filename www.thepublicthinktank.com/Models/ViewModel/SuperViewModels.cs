using atlas_the_public_think_tank.Models.Database;

namespace atlas_the_public_think_tank.Models.ViewModel
{

    /// <summary>
    /// Issues and Solutions inherit this super class
    /// </summary>
    /// <remarks>
    /// The home page serves this as content.
    /// </remarks>
    public class ContentItem_ReadVM
    {

        public required string Title { get; set; }
        public required string Content { get; set; }
        public required DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public DateTime? LastActivity { get; set; }
        public required Guid AuthorID { get; set; }
        public required Guid ScopeID { get; set; }

        public required ContentStatus ContentStatus { get; set; }
        public List<Issue_ReadVM> SubIssues { get; set; } = new List<Issue_ReadVM>();
        public required int SubIssueCount { get; set; }

        public Guid? BlockedContentID { get; set; }

        public required UserVote_Generic_ReadVM VoteStats { get; set; }
        public required List<Breadcrumb_ReadVM> BreadcrumbTags { get; set; }

        // Navigation Prop
        public AppUser Author { get; set; }
        public ICollection<UserComment> Comments { get; set; } = new List<UserComment>();
        public required Scope Scope { get; set; }

        public BlockedContent BlockedContent { get; set; }
    }
}
