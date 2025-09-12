using atlas_the_public_think_tank.Data;
using atlas_the_public_think_tank.Data.SeedData;
using atlas_the_public_think_tank.Data.SeedData.SeedIssues;
using atlas_the_public_think_tank.Data.SeedData.SeedSolutions;
using atlas_the_public_think_tank.Data.SeedData.SeedUsers;
using atlas_the_public_think_tank.Data.SeedData.SeedVotes;
using Microsoft.EntityFrameworkCore;

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
        SeedScopes.Seed(context);
        SeedIssues.Seed(context);
        SeedSolutions.Seed(context);
        SeedIssueVotes.Seed(context);
        SeedSolutionVotes.Seed(context);

        // Save changes if needed
        context.SaveChanges();
    }
}