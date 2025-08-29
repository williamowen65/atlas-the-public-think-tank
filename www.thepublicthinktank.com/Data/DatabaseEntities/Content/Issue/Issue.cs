using atlas_the_public_think_tank.Data;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;
using Microsoft.EntityFrameworkCore;

namespace atlas_the_public_think_tank.Data.DatabaseEntities.Content.Issue
{

    /// <summary>
    /// Defines an issue
    /// </summary>
    public class Issue : ContentItem
    {
        public Guid IssueID { get; set; }
        public Guid? ParentIssueID { get; set; }
        public Guid? ParentSolutionID { get; set; }

        // Navigation properties
        public virtual Issue? ParentIssue { get; set; }
        public virtual Solution.Solution? ParentSolution { get; set; }

        public virtual ICollection<Issue>? ChildIssues { get; set; }
        public virtual ICollection<Solution.Solution>? Solutions { get; set; }
        public virtual ICollection<IssueVote>? IssueVotes { get; set; }
        public virtual ICollection<IssueTag>? IssueTags { get; set; }
    }


    /// <summary>
    /// Defined the SQL relationships of an issue
    /// </summary>
    /// <remarks>
    /// This content is versioned with SQL Server
    /// https://learn.microsoft.com/en-us/sql/relational-databases/tables/temporal-tables?view=sql-server-ver17
    /// </remarks>
    public class IssueModel : IModelComposer
    {
        public static void Declare(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Issue>().ToTable(
                "Issues",
                "issues",
                tb => tb.IsTemporal(temporal =>
                {
                    temporal.UseHistoryTable("IssuesHistory", "issues");
                }));
        }
        public static void Build(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Issue>(entity =>
            {
                entity.HasKey(e => e.IssueID);
                entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Content).IsRequired();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");

                // Self-referencing relationship
                entity.HasOne(e => e.ParentIssue)
                    .WithMany(e => e.ChildIssues)
                    .HasForeignKey(e => e.ParentIssueID)
                    .OnDelete(DeleteBehavior.Restrict);


                entity.HasOne(e => e.ParentSolution)
                    .WithMany()
                    .HasForeignKey(e => e.ParentSolutionID)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relationships
                entity.HasOne(e => e.Author)
                    .WithMany(e => e.Issues)
                    .HasForeignKey(e => e.AuthorID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Scope)
                    .WithMany(e => e.Issues)
                    .HasForeignKey(e => e.ScopeID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.BlockedContent)
                    .WithMany(e => e.Issues)
                    .HasForeignKey(e => e.BlockedContentID)
                    .OnDelete(DeleteBehavior.Restrict);

            });
        }

    }
}
