using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Issue;
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

        public static void Seed(ApplicationDbContext context)
        {
            if (!context.Solutions.Any())
            {
                foreach (var solution in SeedSolutionData)
                {
                    // Guaranteeing that the entity doesn't contain navigation properties
                    context.Solutions.Add(new Solution()
                    {
                        SolutionID = solution.SolutionID,
                        ParentIssueID = solution.ParentIssueID,
                        Title = solution.Title,
                        Content = solution.Content,
                        CreatedAt = solution.CreatedAt,
                        ModifiedAt = solution.ModifiedAt,
                        ScopeID = solution.ScopeID,
                        AuthorID = solution.AuthorID,
                    });
                }
            }
        }
    }

    public interface SeedSolutionContainer
    {
        Solution solution { get; }
        SolutionVote[] solutionVotes { get; }

        Scope scope { get; }
    }
}
