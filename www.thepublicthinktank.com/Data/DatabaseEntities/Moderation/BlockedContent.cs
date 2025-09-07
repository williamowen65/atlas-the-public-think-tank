using atlas_the_public_think_tank.Data;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Comment;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Issue;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Solution;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace atlas_the_public_think_tank.Data.DatabaseEntities.Moderation
{
    /// <summary>
    /// Defines content that was deemed inappropriate or another reason to be blocked
    /// </summary>
    public class BlockedContent
    {
        public Guid BlockedContentID { get; set; }
        public short? ReasonID { get; set; }


        // Navigation property
        [JsonIgnore]
        public virtual ICollection<Issue>? Issues { get; set; }
       
        // Navigation property
        [JsonIgnore]
        public virtual ICollection<Solution>? Solutions { get; set; }

        // Navigation property
        [JsonIgnore]
        public virtual ICollection<Comment>? Comments { get; set; }
    }


    public class BlockedContentModel : IModelComposer
    {
        public static void Declare(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BlockedContent>().ToTable("BlockedContent", "app");
        }
        public static void Build(ModelBuilder modelBuilder)
        {
            throw new NotImplementedException();
        }

    }
}
