using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;
using atlas_the_public_think_tank.Data.DbContext;
using Microsoft.EntityFrameworkCore;

namespace atlas_the_public_think_tank.Data.DatabaseEntities.Content.Issue
{
    public class IssueTag : TagBase
    {
        public required Guid IssueID { get; set; }
    }


    public class IssueTagModel : IModelComposer
    {
        public static void Declare(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IssueTag>().ToTable("IssuesTags", "issues");
        }
        public static void Build(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IssueTag>(entity =>
            {
                entity.HasKey(e => new { e.TagID, e.IssueID });

                // Relationships
                //entity.HasOne(e => e.TagName)
                //    .WithMany(e => e.IssueCategories)
                //    .HasForeignKey(e => e.CategoryID)
                //    .OnDelete(DeleteBehavior.Restrict);

                //entity.HasOne(e => e.Issue)
                //    .WithMany(e => e.IssueCategories)
                //    .HasForeignKey(e => e.IssueID)
                //    .OnDelete(DeleteBehavior.Restrict);
            });

        }

    }
}
