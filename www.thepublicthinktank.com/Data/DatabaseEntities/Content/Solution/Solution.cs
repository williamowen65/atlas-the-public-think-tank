using atlas_the_public_think_tank.Data;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace atlas_the_public_think_tank.Data.DatabaseEntities.Content.Solution
{

    /// <summary>
    /// Defines a solution
    /// </summary>
    public class Solution : ContentItem
    {
        public Guid SolutionID { get; set; }
        public required Guid ParentIssueID { get; set; }

        // Navigation properties
        [JsonIgnore]
        public virtual Issue.Issue? ParentIssue { get; set; }

        public virtual ICollection<SolutionVote>? SolutionVotes { get; set; }
        public virtual ICollection<Issue.Issue>? ChildIssues { get; set; }
        public virtual ICollection<SolutionTag>? SolutionTags { get; set; }
    }

    public class SolutionModel : IModelComposer
    {
        public static void Build(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Solution>().ToTable("Solutions", "solutions");
        }

        public static void Declare(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Solution>(entity =>
            {
                entity.HasKey(e => e.SolutionID);
                entity.Property(e => e.Title).HasMaxLength(300).IsRequired();
                entity.Property(e => e.Content).IsRequired();
                entity.Property(e => e.ContentStatus).IsRequired();

                entity.HasMany(e => e.ChildIssues)
                  .WithOne(e => e.ParentSolution)
                  .HasForeignKey(e => e.ParentSolutionID)
                  .OnDelete(DeleteBehavior.Restrict);

                // Relationships
                entity.HasOne(e => e.ParentIssue)
                    .WithMany(e => e.Solutions)
                    .HasForeignKey(e => e.ParentIssueID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Author)
                    .WithMany(e => e.Solutions)
                    .HasForeignKey(e => e.AuthorID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.BlockedContent)
                    .WithMany(e => e.Solutions)
                    .HasForeignKey(e => e.BlockedContentID)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
