using atlas_the_public_think_tank.Models;
using Microsoft.EntityFrameworkCore;

namespace atlas_the_public_think_tank.Data.SeedData
{
    public class SeedIssueCategories
    {
        public SeedIssueCategories(ModelBuilder modelBuilder)
        {
            // Seed Issue Categories relationships
            modelBuilder.Entity<IssueCategory>().HasData(
                // Climate Change Solutions (Issue 1) categories
                new IssueCategory { IssueID = 1, CategoryID = 1 }, // Global Cooperation
                new IssueCategory { IssueID = 1, CategoryID = 2 }, // Sustainable Development
                new IssueCategory { IssueID = 1, CategoryID = 8 }, // Resilience and Adaptability
                
                // Urban Planning Innovations (Issue 2) categories
                new IssueCategory { IssueID = 2, CategoryID = 2 }, // Sustainable Development
                new IssueCategory { IssueID = 2, CategoryID = 4 }, // Innovation and Technology
                new IssueCategory { IssueID = 2, CategoryID = 5 }, // Effective Governance
                
                // Renewable Energy Transition (Issue 3) categories 
                new IssueCategory { IssueID = 3, CategoryID = 1 }, // Global Cooperation
                new IssueCategory { IssueID = 3, CategoryID = 2 }, // Sustainable Development
                new IssueCategory { IssueID = 3, CategoryID = 4 }  // Innovation and Technology
            );
        }
    }
}