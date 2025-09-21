using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;
using atlas_the_public_think_tank.Data.DbContext;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace atlas_the_public_think_tank.Data.DatabaseEntities.Content.Comment
{
    /// <summary>
    /// Defines a vote on comments
    /// </summary>
    public class CommentVote : VoteBase
    {
        public Guid CommentID { get; set; }

        // Navigation property
        [JsonIgnore]
        public virtual Comment? Comment { get; set; }
    }


    public class CommentVoteModel : IModelComposer
    {
        public static void Declare(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CommentVote>().ToTable("CommentVotes", "comments");
        }

        public static void Build(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CommentVote>(entity =>
            {
                entity.HasKey(e => e.VoteID);

                // Ensure a user can only cast one vote per issue
                entity.HasIndex(e => new { e.CommentID, e.UserID }).IsUnique();

                entity.HasOne(e => e.Comment)
                    .WithMany(c => c.CommentVotes)
                    .HasForeignKey(e => e.CommentID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserID)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

    }
}
