using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;
using atlas_the_public_think_tank.Data.DbContext;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace atlas_the_public_think_tank.Data.DatabaseEntities.Content.Issue
{
    /// <summary>
    /// Defines a vote on issues
    /// </summary>
    public class IssueVote : VoteBase
    {
        public required Guid IssueID { get; set; }

        // Navigation property
        [JsonIgnore]
        public virtual Issue? Issue { get; set; }
    }

    public class IssueVoteModel : IModelComposer
    {
        public static void Declare(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IssueVote>().ToTable("IssueVotes", "issues");
        }
        public static void Build(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IssueVote>(entity =>
            {
                entity.HasKey(e => e.VoteID);

                // Ensure a user can only cast one vote per issue
                entity.HasIndex(e => new { e.IssueID, e.UserID }).IsUnique();

                entity.HasOne(e => e.Issue)
                    .WithMany(i => i.IssueVotes)
                    .HasForeignKey(e => e.IssueID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserID)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

    }
}
