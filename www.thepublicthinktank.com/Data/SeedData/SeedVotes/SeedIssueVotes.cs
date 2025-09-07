using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Issue;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Solution;
using atlas_the_public_think_tank.Data.SeedData.SeedIssues.Data;
using atlas_the_public_think_tank.Data.SeedData.SeedSolutions;
using atlas_the_public_think_tank.Data.SeedData.SeedSolutions.Data;
using atlas_the_public_think_tank.Data.SeedData.SeedUsers.Data;
 
using Microsoft.EntityFrameworkCore;

namespace atlas_the_public_think_tank.Data.SeedData.SeedVotes
{
    public class SeedIssueVotes
    {
       

        public static IssueVote[] SeedIssuesVoteData = SeedIssues.SeedIssues.SeedIssuesDataContainers
         .SelectMany(container => container.issueVotes)
         .ToArray();

        public SeedIssueVotes(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IssueVote>().HasData(SeedIssuesVoteData);
        }
    }
    public class SeedSolutionVotes
    {
        

        public static SolutionVote[] SeedSolutionVoteData = SeedSolutions.SeedSolutions.SeedSolutionDataContainers
          .SelectMany(container => container.solutionVotes)
          .ToArray();

        public SeedSolutionVotes(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SolutionVote>().HasData(SeedSolutionVoteData);
        }
    }


}
