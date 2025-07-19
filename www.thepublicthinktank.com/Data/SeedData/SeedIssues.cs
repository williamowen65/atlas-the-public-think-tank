using atlas_the_public_think_tank.Models.Database;
using Microsoft.EntityFrameworkCore;
using System;

namespace atlas_the_public_think_tank.Data.SeedData
{
    public class SeedIssues
    {
        public SeedIssues(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Issue>().HasData(
                new Issue
                {
                    IssueID = SeedIds.Issues.ClimateChangeSolutions,
                    Title = "Climate Change Solutions",
                    Content = "A issue to discuss practical solutions to climate change at individual and policy levels.",
                    ContentStatus = ContentStatus.Published,
                    CreatedAt = new DateTime(2024, 1, 15),
                    AuthorID = SeedIds.Users.CooperBarker, // Using centralized user ID
                    ScopeID = SeedIds.Scopes.Global // Using centralized scope ID
                },
                new Issue
                {
                    IssueID = SeedIds.Issues.UrbanPlanningInnovations,
                    Title = "Urban Planning Innovations",
                    Content = "Discussion on modern urban planning approaches for sustainable and livable cities.",
                    ContentStatus = ContentStatus.Published,
                    CreatedAt = new DateTime(2024, 2, 10),
                    AuthorID = SeedIds.Users.CooperBarker, // Using centralized user ID
                    ScopeID = SeedIds.Scopes.National // Using centralized scope ID
                },
                new Issue
                {
                    IssueID = SeedIds.Issues.RenewableEnergyTransition,
                    Title = "Renewable Energy Transition",
                    Content = "Strategies for transitioning to renewable energy sources at community and national levels.",
                    ContentStatus = ContentStatus.Published,
                    CreatedAt = new DateTime(2024, 3, 5),
                    ModifiedAt = new DateTime(2024, 3, 10),
                    AuthorID = SeedIds.Users.AmeliaKnight, // Using centralized user ID
                    ScopeID = SeedIds.Scopes.Global, // Using centralized scope ID
                    ParentIssueID = SeedIds.Issues.ClimateChangeSolutions // Using centralized issue ID for parent reference
                },
                new Issue
                {
                    IssueID = SeedIds.Issues.EndangeredSpeciesDecline,
                    Title = "Critical Decline of Endangered Species",
                    Content = "The world is experiencing a biodiversity crisis, with thousands of species teetering on the edge of extinction.",
                    ContentStatus = ContentStatus.Published,
                    CreatedAt = new DateTime(2024, 4, 1),
                    AuthorID = SeedIds.Users.AmeliaKnight, // Using centralized user ID
                    ScopeID = SeedIds.Scopes.Global, // Using centralized scope ID
                },
                new Issue 
                {
                    IssueID = SeedIds.Issues.OrcaPopulationDecline,
                    Title = "Decline of the Southern Resident orca population",
                    Content = "The Southern Resident orca population has dropped from 88 individuals in 2010 to just 74 as of late 2024. This decline is attributed to a combination of factors, including reduced prey availability, pollution, and vessel traffic.",
                    ContentStatus = ContentStatus.Published,
                    CreatedAt = new DateTime(2024, 5, 1),
                    AuthorID = SeedIds.Users.AmeliaKnight, // Using centralized user ID
                    ScopeID = SeedIds.Scopes.National, // Using centralized scope ID
                    ParentIssueID = SeedIds.Issues.EndangeredSpeciesDecline // Using centralized issue ID for parent reference
                }
            );
        }
    }
}