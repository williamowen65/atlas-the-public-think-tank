using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Issue;
using atlas_the_public_think_tank.Data.DbContext;
using atlas_the_public_think_tank.Data.SeedData.SeedIssues.Data;

using Microsoft.EntityFrameworkCore;
using System;
using System.Reflection;

namespace atlas_the_public_think_tank.Data.SeedData.SeedIssues
{
    public class SeedIssues
    {

        // file path Data/SeedData/SeedIssues/Data
        public static SeedIssueContainer?[] SeedIssuesDataContainers { get; } =
             typeof(SeedIssues).Assembly // or typeof(SomeClass).Assembly if it's another DLL
             .GetTypes()
             .Where(t => t.IsClass && !t.IsAbstract && typeof(SeedIssueContainer).IsAssignableFrom(t))
             .Select(t =>
             {
                 try
                 {
                     return (SeedIssueContainer)Activator.CreateInstance(t);
                 }
                 catch (Exception ex)
                 {
                     // Log the error or handle it appropriately
                     Console.WriteLine($"Failed to create instance of {t.Name}: {ex.Message}");
                     return null;
                 }
             })
             .Where(instance => instance != null)
             .ToArray();


        public static Issue[] SeedIssuesData = SeedIssuesDataContainers
          .Select(container => container.issue)
          .ToArray();

        public static void Seed(ApplicationDbContext context)
        {
            // Only seed if there are no issues
            if (!context.Issues.Any())
            {
                foreach (var issue in SeedIssuesData)
                {
                    try
                    {
                        // Guaranteeing that the entity doesn't contain navigation properties
                        context.Issues.Add(new Issue()
                        {
                            ContentStatus = issue.ContentStatus,
                            IssueID = issue.IssueID,
                            Title = issue.Title,
                            Content = issue.Content,
                            CreatedAt = issue.CreatedAt,
                            ModifiedAt = issue.ModifiedAt,
                            AuthorID = issue.AuthorID,
                            ScopeID = issue.ScopeID,
                            ParentIssueID = issue.ParentIssueID,
                            ParentSolutionID = issue.ParentSolutionID
                        });
                    }
                    catch (Exception ex)
                    {
                        // Log the error or handle it appropriately
                        Console.WriteLine($"Failed to add issue '{issue?.Title}': {ex.Message}");
                    }
                }
            }
        }
    }


    public interface SeedIssueContainer
    {
        Issue issue { get; }
        IssueVote[] issueVotes { get; }

        Scope scope { get; }
    }

}