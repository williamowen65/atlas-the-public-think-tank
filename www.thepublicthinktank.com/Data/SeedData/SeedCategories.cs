using atlas_the_public_think_tank.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace atlas_the_public_think_tank.Data.SeedData
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