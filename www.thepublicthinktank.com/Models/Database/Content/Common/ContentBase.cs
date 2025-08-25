using atlas_the_public_think_tank.Models.Database.Moderation;
using atlas_the_public_think_tank.Models.Database.Users;

namespace atlas_the_public_think_tank.Models.Database.Content.Common
{
    /// <summary>
    /// Issues, Solutions, and Comments
    /// </summary>
    public abstract class ContentBase
    {
        public ContentStatusEnum ContentStatus { get; set; }
        public Guid AuthorID { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }

        public Guid? BlockedContentID { get; set; }

        // Navigation properties common to both
        public virtual required AppUser Author { get; set; }
        public virtual BlockedContent? BlockedContent { get; set; }

    }
}
