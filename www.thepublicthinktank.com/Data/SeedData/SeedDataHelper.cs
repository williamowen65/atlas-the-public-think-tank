using atlas_the_public_think_tank.Data.DbContext;
using atlas_the_public_think_tank.Data.RepositoryPattern.Repository.Helpers;
using atlas_the_public_think_tank.Data.SeedData;
using atlas_the_public_think_tank.Data.SeedData.SeedIssues;
using atlas_the_public_think_tank.Data.SeedData.SeedSolutions;
using atlas_the_public_think_tank.Data.SeedData.SeedUsers;
using atlas_the_public_think_tank.Data.SeedData.SeedVotes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

public static class SeedDataHelper
{
    public static void SeedDatabase(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Only seed if database is created and empty (customize as needed)
        context.Database.Migrate();

        // Example: Seed users
        SeedUsers.Seed(context);
        context.SaveChanges(); // Save Users
        SeedScopes.Seed(context);
        SeedIssues.Seed(context);
        SeedSolutions.Seed(context);
        context.SaveChanges(); // Save issues, solutions, and their scopes
        SeedIssueVotes.Seed(context);
        SeedSolutionVotes.Seed(context);
        context.SaveChanges(); // Save votes

        // If running seed data clear the cache after creating seed data so I can populate on its own with app functionality
        CacheHelper.ClearEntireCache();
        // this is important for testing
    }
}