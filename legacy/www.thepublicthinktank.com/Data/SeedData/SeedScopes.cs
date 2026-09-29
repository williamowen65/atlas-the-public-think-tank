using Microsoft.EntityFrameworkCore;
using System;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;
using atlas_the_public_think_tank.Data.SeedData.SeedIssues.Data;
using atlas_the_public_think_tank.Data.SeedData.SeedSolutions.Data;
using atlas_the_public_think_tank.Data.DbContext;

namespace atlas_the_public_think_tank.Data.SeedData
{
    public static class SeedScopes
    {
        public static void Seed(ApplicationDbContext context)
        {
            // Only seed if there are no scopes
            if (!context.Scopes.Any())
            {
                // Collect scopes from SeedIssuesDataContainers
                var issueScopes = SeedIssues.SeedIssues.SeedIssuesDataContainers
                    .Where(x => x != null)
                    .Select(x => x.scope);

                var solutionScopes = SeedSolutions.SeedSolutions.SeedSolutionDataContainers
                    .Where(x => x != null)
                    .Select(x => x.scope);

                // Combine and remove duplicates by ScopeID
                var allScopes = issueScopes
                    .Concat(solutionScopes)
                    .GroupBy(s => s.ScopeID)
                    .Select(g => g.First())
                    .ToList();

                // Add scopes that don't already exist in the database
                var existingScopeIds = context.Scopes.Select(s => s.ScopeID).ToHashSet();
                var newScopes = allScopes.Where(s => !existingScopeIds.Contains(s.ScopeID)).ToList();

                if (newScopes.Any())
                {
                    context.Scopes.AddRange(newScopes);
                }
            }
        }
    }
}