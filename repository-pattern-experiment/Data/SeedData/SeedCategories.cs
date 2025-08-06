using repository_pattern_experiment.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace repository_pattern_experiment.Data.SeedData
{
    public class SeedCategories
    {
        public SeedCategories(ModelBuilder modelBuilder)
        {
            // Seed Categories
            modelBuilder.Entity<Category>().HasData(
              
            ); 
        }
    }
}