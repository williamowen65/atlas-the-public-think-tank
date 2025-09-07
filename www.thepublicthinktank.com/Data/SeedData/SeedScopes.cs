using Microsoft.EntityFrameworkCore;
using System;
 
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;
using atlas_the_public_think_tank.Data.SeedData.SeedIssues.Data;
using atlas_the_public_think_tank.Data.SeedData.SeedSolutions.Data;

namespace atlas_the_public_think_tank.Data.SeedData
{
    // Pseudocode:
    // 1. Access SeedIssuesDataContainers collection.
    // 2. For each item, map its 'scope' attribute to a new Scope object.
    // 3. Add each Scope to the issueScopes list.
    // 4. Use issueScopes in modelBuilder.Entity<Scope>().HasData().

    public class SeedScopes
    {
        public SeedScopes(ModelBuilder modelBuilder)
        {
            // Collect scopes from SeedIssuesDataContainers
            List<Scope> issueScopes = new List<Scope>();
            foreach (var issueData in SeedIssues.SeedIssues.SeedIssuesDataContainers)
            {
                issueScopes.Add(issueData.scope);
            }

            List<Scope> solutionScopes = new List<Scope>();
            foreach (var solutionData in SeedSolutions.SeedSolutions.SeedSolutionDataContainers)
            {
                issueScopes.Add(solutionData.scope);
            }

            // Seed Scopes
            modelBuilder.Entity<Scope>().HasData(
                issueScopes.ToArray()
            );

            modelBuilder.Entity<Scope>().HasData(
                solutionScopes.ToArray()
            );
        }
    }
}