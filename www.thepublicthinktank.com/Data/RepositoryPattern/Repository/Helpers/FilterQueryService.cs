using atlas_the_public_think_tank.Models;
using atlas_the_public_think_tank.Models.Database;

namespace atlas_the_public_think_tank.Data.RepositoryPattern.Repository.Helpers
{
    public static class FilterQueryService
    {
        /// <summary>
        /// Applies common filtering logic to a query of Issues
        /// </summary>
        public static IQueryable<Issue> ApplyIssueFilters(IQueryable<Issue> query, ContentFilter filter)
        {
            if (filter == null)
                return query;

            // Log filter info
            Console.WriteLine("Applying content filters to issues");
            Console.WriteLine(filter);
            Console.WriteLine(filter.ToJson());

            // Filter by average vote range
            if (filter?.AvgVoteRange != null)
            {
                query = query.Where(i =>
                    (i.IssueVotes.Any() ? i.IssueVotes.Average(v => v.VoteValue) : 0) >= filter.AvgVoteRange.Min &&
                    (i.IssueVotes.Any() ? i.IssueVotes.Average(v => v.VoteValue) : 0) <= filter.AvgVoteRange.Max);
            }

            // Filter by total vote count
            if (filter?.TotalVoteCount != null)
            {
                query = query.Where(i => i.IssueVotes.Count >= filter.TotalVoteCount.Min);

                // Apply max filter only if it has a value
                if (filter.TotalVoteCount.Max.HasValue)
                {
                    query = query.Where(i => i.IssueVotes.Count <= filter.TotalVoteCount.Max.Value);
                }
            }

            // Filter by date range
            if (filter?.DateRange != null)
            {
                // Apply "from date" filter if specified
                if (filter.DateRange.From.HasValue)
                {
                    query = query.Where(i => i.CreatedAt >= filter.DateRange.From.Value.AddDays(-1));
                }

                // Apply "to date" filter if specified
                if (filter.DateRange.To.HasValue)
                {
                    // Add one day to include the entire end date (up to 23:59:59)
                    var toDateInclusive = filter.DateRange.To.Value.AddDays(1).AddSeconds(-1);
                    query = query.Where(i => i.CreatedAt <= toDateInclusive);
                }
            }

            return query;
        }

        /// <summary>
        /// Applies common filtering logic to a query of Solutions
        /// </summary>
        public static IQueryable<Solution> ApplySolutionFilters(IQueryable<Solution> query, ContentFilter filter)
        {
            if (filter == null)
                return query;

            // Log filter info
            Console.WriteLine("Applying content filters to solutions");
            Console.WriteLine(filter);
            Console.WriteLine(filter.ToJson());

            // Filter by average vote range
            if (filter?.AvgVoteRange != null)
            {
                query = query.Where(s =>
                    (s.SolutionVotes.Any() ? s.SolutionVotes.Average(v => v.VoteValue) : 0) >= filter.AvgVoteRange.Min &&
                    (s.SolutionVotes.Any() ? s.SolutionVotes.Average(v => v.VoteValue) : 0) <= filter.AvgVoteRange.Max);
            }

            // Filter by total vote count
            if (filter?.TotalVoteCount != null)
            {
                query = query.Where(s => s.SolutionVotes.Count >= filter.TotalVoteCount.Min);

                // Apply max filter only if it has a value
                if (filter.TotalVoteCount.Max.HasValue)
                {
                    query = query.Where(s => s.SolutionVotes.Count <= filter.TotalVoteCount.Max.Value);
                }
            }

            // Filter by date range
            if (filter?.DateRange != null)
            {
                // Apply "from date" filter if specified
                if (filter.DateRange.From.HasValue)
                {
                    query = query.Where(i => i.CreatedAt >= filter.DateRange.From.Value.AddDays(-1));
                }

                // Apply "to date" filter if specified
                if (filter.DateRange.To.HasValue)
                {
                    // Add one day to include the entire end date (up to 23:59:59)
                    var toDateInclusive = filter.DateRange.To.Value.AddDays(1).AddSeconds(-1);
                    query = query.Where(s => s.CreatedAt <= toDateInclusive);
                }
            }

            return query;
        }

        /// <summary>
        /// Applies filtering to ContentIndexEntry queries that combine both issues and solutions
        /// </summary>
        public static IQueryable<ContentIndexEntry> ApplyCombinedContentFilters(
            IQueryable<ContentIndexEntry> query,
            ContentFilter filter)
        {
            if (filter == null)
                return query;

            // Log filter info
            Console.WriteLine("Applying combined content filters");
            Console.WriteLine(filter);
            Console.WriteLine(filter.ToJson());

            // Filter for ContentType ("both", "issues", "solutions)
            if (filter.ContentType != null && filter.ContentType != "both")
            {
                if (filter.ContentType.Equals("issues", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(c => c.ContentType == ContentType.Issue);
                }
                else if (filter.ContentType.Equals("solutions", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(c => c.ContentType == ContentType.Solution);
                }
                // "both" is the default and doesn't need filtering as it includes all content types
            }

            // Filter by average vote range
            if (filter.AvgVoteRange != null)
            {
                query = query.Where(c =>
                    c.AverageVote >= filter.AvgVoteRange.Min &&
                    c.AverageVote <= filter.AvgVoteRange.Max);
            }

            // Filter by total vote count
            if (filter.TotalVoteCount != null)
            {
                query = query.Where(c => c.TotalVotes >= filter.TotalVoteCount.Min);

                // Apply max filter only if it has a value
                if (filter.TotalVoteCount.Max.HasValue)
                {
                    query = query.Where(c => c.TotalVotes <= filter.TotalVoteCount.Max.Value);
                }
            }

            // Filter by date range
            if (filter.DateRange != null)
            {
                // Apply "from date" filter if specified
                if (filter.DateRange.From.HasValue)
                {
                    query = query.Where(i => i.CreatedAt >= filter.DateRange.From.Value.AddDays(-1));
                }

                // Apply "to date" filter if specified
                if (filter.DateRange.To.HasValue)
                {
                    // Add one day to include the entire end date (up to 23:59:59)
                    var toDateInclusive = filter.DateRange.To.Value.AddDays(1).AddSeconds(-1);
                    query = query.Where(c => c.CreatedAt <= toDateInclusive);
                }
            }

            return query;
        }


    }
}
