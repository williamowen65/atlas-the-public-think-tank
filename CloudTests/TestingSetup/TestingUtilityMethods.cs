using atlas_the_public_think_tank.Data;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Issue;
using atlas_the_public_think_tank.Data.RepositoryPattern.Repository.Helpers;
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

        public static SeedIssueContainer[] GetSubIssuesOf(Issue issue)
        {
            SeedIssueContainer[] seedIssuesDataContainers = SeedIssues.SeedIssuesDataContainers;

            // return all issues that have a ParentIssueID of issue.IssueID
            return seedIssuesDataContainers
                .Where(idc => idc.issue.ParentIssueID == issue.IssueID)
                .ToArray();
        }
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


        public static IEnumerable<object[]> GetIssues()
        {
            int max = 5;
            int count = 0;
            foreach (var container in SeedIssues.SeedIssuesDataContainers)
            {
                if (count++ >= max) break;
                yield return new object[] { container.issue };
            }
        }
        public static IEnumerable<object[]> GetSolutions()
        {
            int max = 5;
            int count = 0;
            // Iterate through all SeedIssueContainer instances in SeedIssuesDataContainers
            foreach (var container in SeedSolutions.SeedSolutionDataContainers)
            {
                if (count++ >= max) break;
                yield return new object[] { container.solution };
            }
        }

        public static IEnumerable<object[]> GetIssueUrls()
        {
            int max = 5;
            int count = 0;
            // Iterate through all SeedIssueContainer instances in SeedIssuesDataContainers
            foreach (var container in SeedIssues.SeedIssuesDataContainers)
            {
                if (count++ >= max) break;
                yield return new object[] { "/issue/" + container.issue.IssueID.ToString() };
            }
        }

        public static IEnumerable<object[]> GetFilterDataRows()
        {
            yield return new object[] {
                new ContentFilter()
                {
                    AvgVoteRange = new RangeFilter<double> { Min = 1, Max = 10 }
                },
                new string[] { "Vote Range: 1 to 10" }
            };
            yield return new object[] {
                new ContentFilter()
                {
                    AvgVoteRange = new RangeFilter<double> { Min = 4.5, Max = 5.5 }
                },
                new string[] { "Vote Range: 4.5 to 5.5" }
            };
            yield return new object[] {
                new ContentFilter()
                {
                    AvgVoteRange = new RangeFilter<double> { Min = 4.5, Max = 5.5 },
                    TotalVoteCount = new NullableMaxRangeFilter<int> { Min = 3}

                },
                new string[] { "Vote Range: 4.5 to 5.5", "Vote Count: 3 and up" }
            };
        }

    }
}
