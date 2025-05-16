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
                new Category { CategoryID = SeedIds.Categories.GlobalCooperation, CategoryName = "Global Cooperation" },
                new Category { CategoryID = SeedIds.Categories.SustainableDevelopment, CategoryName = "Sustainable Development" },
                new Category { CategoryID = SeedIds.Categories.EquitableAccess, CategoryName = "Equitable Access" },
                new Category { CategoryID = SeedIds.Categories.InnovationAndTechnology, CategoryName = "Innovation and Technology" },
                new Category { CategoryID = SeedIds.Categories.EffectiveGovernance, CategoryName = "Effective Governance" },
                new Category { CategoryID = SeedIds.Categories.EducationAndAwareness, CategoryName = "Education and Awareness" },
                new Category { CategoryID = SeedIds.Categories.CulturalUnderstanding, CategoryName = "Cultural Understanding" },
                new Category { CategoryID = SeedIds.Categories.ResilienceAndAdaptability, CategoryName = "Resilience and Adaptability" }
            ); 
        }
    }
}