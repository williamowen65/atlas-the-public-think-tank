using atlas_the_public_think_tank.Models;
using Microsoft.EntityFrameworkCore;

namespace atlas_the_public_think_tank.Data.SeedData
{
    public class SeedCategories
    {
        public SeedCategories(ModelBuilder modelBuilder)
        {
            // Seed Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryID = 1, CategoryName = "Global Cooperation" },
                new Category { CategoryID = 2, CategoryName = "Sustainable Development" },
                new Category { CategoryID = 3, CategoryName = "Equitable Access" },
                new Category { CategoryID = 4, CategoryName = "Innovation and Technology" },
                new Category { CategoryID = 5, CategoryName = "Effective Governance" },
                new Category { CategoryID = 6, CategoryName = "Education and Awareness" },
                new Category { CategoryID = 7, CategoryName = "Cultural Understanding" },
                new Category { CategoryID = 8, CategoryName = "Resilience and Adaptability" }
            );
        }
    }
}
