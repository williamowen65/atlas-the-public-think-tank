using atlas_the_public_think_tank.Data;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Issue;
using atlas_the_public_think_tank.Data.SeedData.SeedIssues;
using atlas_the_public_think_tank.Data.SeedData.SeedSolutions;
 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudTests.TestingSetup
{
    public class TestingUtilityMethods
    {

        //public static Issue[] GetSubIssuesOf(Issue issue)
        //{
        //    Issue[] seedIssuesData = SeedIssues.SeedIssuesData;

        //    // return all issues that have a ParentIssueID of issue.IssueID
        //    return seedIssuesData
        //        .Where(i => i.ParentIssueID == issue.IssueID)
        //        .ToArray();
        //}
        public static SeedSolutionContainer[] GetSeedSolutionDataContainersOf(Issue issue)
        {
            SeedSolutionContainer[] seedSolutionDataContainers = SeedSolutions.SeedSolutionDataContainers;

            // return all issues that have a ParentIssueID of issue.IssueID
            return seedSolutionDataContainers
                .Where(sdc => sdc.solution.ParentIssueID == issue.IssueID)
                .ToArray();
        }

        public static object[] filterByAvgVoteRange(object[] contentItems, double min, double max)
        {
            return contentItems
                .Where(item =>
                {
                    double avgVote = 0;

                    if (item is SeedIssueContainer issue && issue.issueVotes.Any())
                    {
                        avgVote = issue.issueVotes.Average(v => v.VoteValue);
                    }
                    else if (item is SeedSolutionContainer solution && solution.solutionVotes.Any())
                    {
                        avgVote = solution.solutionVotes.Average(v => v.VoteValue);
                    }

                    // Filter based on average vote being within range
                    return avgVote >= min && avgVote <= max;
                })
                .ToArray();
        }

        public async static Task deleteDatabase(
            HttpClient _client,
            ApplicationDbContext _db

            )
        {

            if (_db != null)
            {
                try
                {
                    await _db.Database.EnsureDeletedAsync(); // Deletes the test database
                    Console.WriteLine("Test database deleted successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Database cleanup failed: {ex.Message}");
                }
                finally
                {
                    await _db.DisposeAsync();
                }
            }

            _client?.Dispose();
        }
    }
}
