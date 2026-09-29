using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Issue;
 
using Microsoft.EntityFrameworkCore;
using System;

namespace atlas_the_public_think_tank.Data.SeedData
{
    public class SeedIssueCategories
    {
        public SeedIssueCategories(ModelBuilder modelBuilder)
        {
            // Seed Issue Categories relationships
            modelBuilder.Entity<IssueTag>().HasData(
               
            );
        }
    }
}