using atlas_the_public_think_tank.Data.DatabaseEntities.Moderation;
using atlas_the_public_think_tank.Data.DatabaseEntities.Users;
using atlas_the_public_think_tank.Models.Enums;

namespace atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common
{
    /// <summary>
    /// Issues, Solutions, and Comments
    /// </summary>
    public abstract class ContentBase
    {
        public ContentStatus ContentStatus { get; set; }
        public Guid AuthorID { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }

        public Guid? BlockedContentID { get; set; }

        // Navigation properties common to both
        public virtual AppUser Author { get; set; }
        public virtual BlockedContent? BlockedContent { get; set; }

    }
}
