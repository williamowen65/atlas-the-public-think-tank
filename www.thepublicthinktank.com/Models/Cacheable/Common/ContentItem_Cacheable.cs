using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Comment;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;
using atlas_the_public_think_tank.Data.DatabaseEntities.Moderation;
using atlas_the_public_think_tank.Models.Enums;
using atlas_the_public_think_tank.Models.ViewModel;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.User;
using atlas_the_public_think_tank.Models.ViewModel.CRUD_VM.ContentItem_Common;

namespace atlas_the_public_think_tank.Models.Cacheable.Common
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
        public int? VersionHistoryCount { get; set; }

        // LastActivity could be when last comment or vote was
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
        public AppUser_ReadVM Author { get; set; }
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public required Scope Scope { get; set; }

        public BlockedContent BlockedContent { get; set; }
    }
}
