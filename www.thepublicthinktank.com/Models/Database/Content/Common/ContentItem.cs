using System.Text.Json.Serialization;

namespace atlas_the_public_think_tank.Models.Database.Content.Common
{

    /// <summary>
    /// Issues and Solutions, but not Comments
    /// </summary>
    public abstract class ContentItem : ContentBase
    {
        public required string Title { get; set; }
        public required string Content { get; set; }

        public required Guid ScopeID { get; set; }
        public virtual required Scope Scope { get; set; }

    
        public virtual required ICollection<Comment.Comment> Comments { get; set; }
    }
}
