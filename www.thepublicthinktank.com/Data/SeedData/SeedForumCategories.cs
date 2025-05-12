using atlas_the_public_think_tank.Models;
using Microsoft.EntityFrameworkCore;

namespace atlas_the_public_think_tank.Data.SeedData
{
    public class SeedForumCategories
    {
        public SeedForumCategories(ModelBuilder modelBuilder)
        {
            // Seed Forum Categories relationships
            modelBuilder.Entity<ForumCategory>().HasData(
                // Climate Change Solutions (Forum 1) categories
                new ForumCategory { ForumID = 1, CategoryID = 1 }, // Global Cooperation
                new ForumCategory { ForumID = 1, CategoryID = 2 }, // Sustainable Development
                new ForumCategory { ForumID = 1, CategoryID = 8 }, // Resilience and Adaptability
                
                // Urban Planning Innovations (Forum 2) categories
                new ForumCategory { ForumID = 2, CategoryID = 2 }, // Sustainable Development
                new ForumCategory { ForumID = 2, CategoryID = 4 }, // Innovation and Technology
                new ForumCategory { ForumID = 2, CategoryID = 5 }, // Effective Governance
                
                // Renewable Energy Transition (Forum 3) categories 
                new ForumCategory { ForumID = 3, CategoryID = 1 }, // Global Cooperation
                new ForumCategory { ForumID = 3, CategoryID = 2 }, // Sustainable Development
                new ForumCategory { ForumID = 3, CategoryID = 4 }  // Innovation and Technology
            );
        }
    }
}