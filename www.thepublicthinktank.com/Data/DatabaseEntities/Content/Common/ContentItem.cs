using System.Text.Json.Serialization;

namespace atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common
{

    /// <summary>
    /// Issues and Solutions, but not Comments
    /// </summary>
    public abstract class ContentItem : ContentBase
    {
        public required string Title { get; set; }
        public required string Content { get; set; }

        // ScopeId may not exist until the scope is created in the db.
        public Guid? ScopeID { get; set; }
        public virtual Scope? Scope { get; set; }
    
        public virtual ICollection<Comment.Comment>? Comments { get; set; }

        public int? RANK { get; set; }
    }
}
