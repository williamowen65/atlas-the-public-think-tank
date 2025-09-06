using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Issue;
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
         Assembly.GetExecutingAssembly() // or typeof(SomeClass).Assembly if it's another DLL
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

        public SeedIssues(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Issue>().HasData(SeedIssuesData);
        }
    }


    public interface SeedIssueContainer
    {
        Issue issue { get; }
        IssueVote[] issueVotes { get; }

        Scope scope { get; }
    }

}