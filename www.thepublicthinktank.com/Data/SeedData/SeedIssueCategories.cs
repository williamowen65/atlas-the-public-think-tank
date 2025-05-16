using atlas_the_public_think_tank.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace atlas_the_public_think_tank.Data.SeedData
{
    public class SeedIssueCategories
    {
        public SeedIssueCategories(ModelBuilder modelBuilder)
        {
            // Seed Issue Categories relationships
            modelBuilder.Entity<IssueCategory>().HasData(
                // Climate Change Solutions (Issue 1) categories
                new IssueCategory { 
                    //IssueCategoryID = Guid.NewGuid(),
                    IssueID = SeedIds.Issues.ClimateChangeSolutions, 
                    CategoryID = SeedIds.Categories.GlobalCooperation 
                },
                new IssueCategory { 
                    //IssueCategoryID = Guid.NewGuid(),
                    IssueID = SeedIds.Issues.ClimateChangeSolutions, 
                    CategoryID = SeedIds.Categories.SustainableDevelopment 
                },
                new IssueCategory { 
                    //IssueCategoryID = Guid.NewGuid(),
                    IssueID = SeedIds.Issues.ClimateChangeSolutions, 
                    CategoryID = SeedIds.Categories.ResilienceAndAdaptability 
                },
                
                // Urban Planning Innovations (Issue 2) categories
                new IssueCategory { 
                    //IssueCategoryID = Guid.NewGuid(),
                    IssueID = SeedIds.Issues.UrbanPlanningInnovations, 
                    CategoryID = SeedIds.Categories.SustainableDevelopment 
                },
                new IssueCategory { 
                    //IssueCategoryID = Guid.NewGuid(),
                    IssueID = SeedIds.Issues.UrbanPlanningInnovations, 
                    CategoryID = SeedIds.Categories.InnovationAndTechnology 
                },
                new IssueCategory { 
                    //IssueCategoryID = Guid.NewGuid(),
                    IssueID = SeedIds.Issues.UrbanPlanningInnovations, 
                    CategoryID = SeedIds.Categories.EffectiveGovernance 
                },
                
                // Renewable Energy Transition (Issue 3) categories 
                new IssueCategory { 
                    //IssueCategoryID = Guid.NewGuid(),
                    IssueID = SeedIds.Issues.RenewableEnergyTransition, 
                    CategoryID = SeedIds.Categories.GlobalCooperation 
                },
                new IssueCategory { 
                    //IssueCategoryID = Guid.NewGuid(),
                    IssueID = SeedIds.Issues.RenewableEnergyTransition, 
                    CategoryID = SeedIds.Categories.SustainableDevelopment 
                },
                new IssueCategory { 
                    //IssueCategoryID = Guid.NewGuid(),
                    IssueID = SeedIds.Issues.RenewableEnergyTransition, 
                    CategoryID = SeedIds.Categories.InnovationAndTechnology 
                }
            );
        }
    }
}