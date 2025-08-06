using repository_pattern_experiment.Models.Database;
using Microsoft.EntityFrameworkCore;
using System;

namespace repository_pattern_experiment.Data.SeedData
{
    public class SeedIssueCategories
    {
        public SeedIssueCategories(ModelBuilder modelBuilder)
        {
            // Seed Issue Categories relationships
            modelBuilder.Entity<IssueCategory>().HasData(
               
            );
        }
    }
}