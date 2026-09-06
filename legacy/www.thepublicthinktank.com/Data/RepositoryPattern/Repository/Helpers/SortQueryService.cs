using Microsoft.EntityFrameworkCore;
using atlas_the_public_think_tank.Models;
 
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Issue;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Solution;

namespace atlas_the_public_think_tank.Data.RepositoryPattern.Repository.Helpers
{
    public static class SortQueryService
    {
        /// <summary>
        /// Applies sorting by weighted score to an issue query
        /// </summary>
        public static IOrderedQueryable<Issue> ApplyWeightedScoreSorting(IQueryable<Issue> query, int k = 5)
        {
            return query
                .Include(i => i.IssueVotes)
                .OrderByDescending(i => i.IssueVotes.Any()
                    ? (i.IssueVotes.Average(v => v.VoteValue) * i.IssueVotes.Count) / (i.IssueVotes.Count + k) // Weighted score
                    : 0)
                .ThenByDescending(i => i.IssueVotes.Any() ? i.IssueVotes.Average(v => v.VoteValue) : 0) // Average vote as tiebreaker
                .ThenByDescending(i => i.CreatedAt); // Creation date as final tiebreaker
        }

        /// <summary>
        /// Applies sorting by weighted score to a solution query
        /// </summary>
        public static IOrderedQueryable<Solution> ApplyWeightedScoreSorting(IQueryable<Solution> query, int k = 5)
        {
            return query
                .Include(s => s.SolutionVotes)
                .OrderByDescending(s => s.SolutionVotes.Any()
                    ? (s.SolutionVotes.Average(v => v.VoteValue) * s.SolutionVotes.Count) / (s.SolutionVotes.Count + k) // Weighted score
                    : 0)
                .ThenByDescending(s => s.SolutionVotes.Any() ? s.SolutionVotes.Average(v => v.VoteValue) : 0) // Average vote as tiebreaker
                .ThenByDescending(s => s.CreatedAt); // Creation date as final tiebreaker
        }


        /// <summary>
        /// Calculate weighted scores and apply standard sorting to ContentIndexEntry query
        /// </summary>
        public static IOrderedQueryable<ContentIndexEntry> ApplyCombinedContentSorting(
            IQueryable<ContentIndexEntry> query,
            int k = 5)
        {
            return query
                .Select(entry => new ContentIndexEntry
                {
                    ContentStatus = entry.ContentStatus,
                    ContentId = entry.ContentId,
                    ContentType = entry.ContentType,
                    CreatedAt = entry.CreatedAt,
                    AverageVote = entry.AverageVote,
                    TotalVotes = entry.TotalVotes,
                    WeightedScore = (entry.TotalVotes + k) != 0
                        ? (entry.AverageVote * entry.TotalVotes) / (entry.TotalVotes + k)
                        : 0
                })
                .OrderByDescending(c => c.WeightedScore)
                .ThenByDescending(c => c.AverageVote)
                .ThenByDescending(c => c.CreatedAt);
        }
    }
}
