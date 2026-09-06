using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;
using atlas_the_public_think_tank.Data.DbContext;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace atlas_the_public_think_tank.Data.DatabaseEntities.Content.Solution
{
    /// <summary>
    /// Defines a vote on solutions
    /// </summary>
    public class SolutionVote : VoteBase
    {
        public required Guid SolutionID { get; set; }

        // Navigation property
        [JsonIgnore]
        public virtual Solution? Solution { get; set; }

       
    }

    public class SolutionVoteModel : IModelComposer
    {
        public static void Declare(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SolutionVote>().ToTable("SolutionVotes", "solutions", (tb) => {
                tb.HasCheckConstraint("CK_SolutionVote_VoteValue_Range", "[VoteValue] >= 0 AND [VoteValue] <= 10");
            });
        }
        public static void Build(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SolutionVote>(entity =>
            {
                entity.HasKey(e => e.VoteID);

                // Ensure a user can only cast one vote per issue
                entity.HasIndex(e => new { e.SolutionID, e.UserID }).IsUnique();

                entity.HasOne(e => e.Solution)
                    .WithMany(s => s.SolutionVotes)
                    .HasForeignKey(e => e.SolutionID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserID)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

    }
}
