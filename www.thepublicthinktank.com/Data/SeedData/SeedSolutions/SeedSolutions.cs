using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Solution;
using atlas_the_public_think_tank.Data.SeedData.SeedIssues;
using atlas_the_public_think_tank.Data.SeedData.SeedIssues.Data;
using atlas_the_public_think_tank.Data.SeedData.SeedSolutions.Data;
 
using Microsoft.EntityFrameworkCore;

namespace atlas_the_public_think_tank.Data.SeedData.SeedSolutions
{
    public class SeedSolutions
    {


     

         public static SeedSolutionContainer?[] SeedSolutionDataContainers { get; } =
            typeof(SeedSolutions).Assembly
                .GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && typeof(SeedSolutionContainer).IsAssignableFrom(t))
                .Select(t =>
                {
                    try
                    {
                        return (SeedSolutionContainer)Activator.CreateInstance(t);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to create instance of {t.Name}: {ex.Message}");
                        return null;
                    }
                })
                .Where(instance => instance != null)
                .ToArray();

        public static Solution[] SeedSolutionData = SeedSolutionDataContainers
          .Select(container => container.solution)
          .ToArray();

        public SeedSolutions(ModelBuilder modelBuilder)
        {
            // Seed Solutions
            modelBuilder.Entity<Solution>().HasData(SeedSolutionData);
        }
    }

    public interface SeedSolutionContainer
    {
        Solution solution { get; }
        SolutionVote[] solutionVotes { get; }

        Scope scope { get; }
    }
}
