using atlas_the_public_think_tank.Data;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;
using Microsoft.EntityFrameworkCore;

namespace atlas_the_public_think_tank.Data.DatabaseEntities.Content.Solution
{
    public class SolutionTag : TagBase
    {
        public required Guid SolutionID { get; set; }
    }


    public class SolutionTagModel : IModelComposer
    {
        public static void Declare(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SolutionTag>().ToTable("SolutionsTags", "solutions");
        }
        public static void Build(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SolutionTag>(entity =>
            {
                entity.HasKey(e => new { e.TagID, e.SolutionID });

                // Relationships
                //entity.HasOne(e => e.TagName)
                //    .WithMany(e => e.SolutionTag)
                //    .HasForeignKey(e => e.TagID)
                //    .OnDelete(DeleteBehavior.Restrict);

                //entity.HasOne(e => e.Solution)
                //    .WithMany(e => e.SolutionCategories)
                //    .HasForeignKey(e => e.SolutionID)
                //    .OnDelete(DeleteBehavior.Restrict);
            });
        }

    }
}
