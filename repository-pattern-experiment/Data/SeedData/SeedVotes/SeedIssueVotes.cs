using repository_pattern_experiment.Data.SeedData.SeedIssues.Data;
using repository_pattern_experiment.Data.SeedData.SeedSolutions;
using repository_pattern_experiment.Data.SeedData.SeedSolutions.Data;
using repository_pattern_experiment.Data.SeedData.SeedUsers.Data;
using repository_pattern_experiment.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace repository_pattern_experiment.Data.SeedData.SeedVotes
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
