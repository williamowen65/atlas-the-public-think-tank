using atlas_the_public_think_tank.Data.RepositoryPattern.IRepository;
using atlas_the_public_think_tank.Data.RepositoryPattern.Repository.Helpers;
using atlas_the_public_think_tank.Models;
using atlas_the_public_think_tank.Models.Enums;
using atlas_the_public_think_tank.Models.ViewModel;
using atlas_the_public_think_tank.Models.ViewModel.UI_VM;
using Microsoft.EntityFrameworkCore;

namespace atlas_the_public_think_tank.Data.RepositoryPattern.Repository
{
    public class FilterIdRepository : IFilterIdSetRepository
    {

        private ApplicationDbContext _context;
        public FilterIdRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Guid>?> GetPagedSolutionIdsOfIssueById(Guid issueId, ContentFilter filter, int pageNumber = 1, int pageSize = 3)
        {
            var query = _context.Solutions
               .Where(i => i.ParentIssueID == issueId);

            // TODO Apply Filter
            var filteredQuery = FilterQueryService.ApplySolutionFilters(query, filter);
            // TODO Apply Weighted Score  / Sorting
            var sortedQuery = SortQueryService.ApplyWeightedScoreSorting(filteredQuery);

            var paginatedSolutionFeedIds = await sortedQuery.Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(i => i.SolutionID)
            .ToListAsync();

            return paginatedSolutionFeedIds;
        }

        public async Task<List<Guid>?> GetPagedSubIssueIdsOfIssueById(Guid issueId, ContentFilter filter, int pageNumber = 1, int pageSize = 3)
        {
            var query = _context.Issues
                .Where(i => i.ParentIssueID == issueId);

            // TODO Apply Filter / Sorting
            var filteredQuery = FilterQueryService.ApplyIssueFilters(query, filter);
            // TODO Apply Weighted Score
            var sortedQuery = SortQueryService.ApplyWeightedScoreSorting(filteredQuery);

            var paginatedChildIssuesIds = await sortedQuery.Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(i => i.IssueID)
            .ToListAsync();

            return (paginatedChildIssuesIds);

        }

        public async Task<List<Guid>?> GetPagedSubIssueIdsOfSolutionById(Guid solutionId, ContentFilter filter, int pageNumber = 1, int pageSize = 3)
        {
            var query = _context.Issues
                .Where(i => i.ParentSolutionID == solutionId);

            // TODO Apply Filter / Sorting (NOTE: Using Issue filters because working with sub issues)
            var filteredQuery = FilterQueryService.ApplyIssueFilters(query, filter);
            // TODO Apply Weighted Score
            var sortedQuery = SortQueryService.ApplyWeightedScoreSorting(filteredQuery);

            var paginatedChildIssuesIds = await sortedQuery.Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(s => s.IssueID)
            .ToListAsync();

            return (paginatedChildIssuesIds);
        }

        public async Task<ContentCount_VM?> GetContentCountSubIssuesOfIssueById(Guid issueId, ContentFilter filter)
        {

            ContentCount_VM counts = new ContentCount_VM();

            var query = _context.Issues
                .Where(i => i.ParentIssueID == issueId);
            var filteredQuery = FilterQueryService.ApplyIssueFilters(query, filter);

            counts.AbsoluteCount = await query.CountAsync();
            counts.FilteredCount = await filteredQuery.CountAsync();

            return counts;
        }
        public async Task<ContentCount_VM?> GetContentCountSubIssuesOfSolutionById(Guid solutionId, ContentFilter filter)
        {
            ContentCount_VM counts = new ContentCount_VM();
            var query = _context.Issues
                .Where(i => i.ParentSolutionID == solutionId);
            // Note: Using issue filter since this is for sub issues
            var filteredQuery = FilterQueryService.ApplyIssueFilters(query, filter);

            counts.AbsoluteCount = await query.CountAsync();
            counts.FilteredCount  = await filteredQuery.CountAsync();

            return counts;
        }
        public async Task<ContentCount_VM?> GetContentCountSolutionsOfIssueById(Guid solutionId, ContentFilter filter)
        {
            ContentCount_VM counts = new ContentCount_VM();
            var query = _context.Solutions
                .Where(i => i.ParentIssueID == solutionId);
            var filteredQuery = FilterQueryService.ApplySolutionFilters(query, filter);

            counts.AbsoluteCount = await query.CountAsync();
            counts.FilteredCount = await filteredQuery.CountAsync();

            return counts;
        }

        public Task<List<ContentIdentifier>?> GetPagedMainContentFeedIds(ContentFilter filter, int pageNumber = 1, int pageSize = 3)
        {
            // First, get all the issues and solutions IDs with their creation dates and vote averages
            // This allows efficient sorting and pagination at the database level
            var issuesIndexQuery = _context.Issues
                .Select(i => new ContentIndexEntry
                {
                    ContentId = i.IssueID,
                    ContentType = ContentType.Issue,
                    CreatedAt = i.CreatedAt,
                    AverageVote = i.IssueVotes.Any() ? i.IssueVotes.Average(v => v.VoteValue) : 0,
                    TotalVotes = i.IssueVotes.Any() ? i.IssueVotes.Count() : 0,
                });

            var solutionsIndexQuery = _context.Solutions
                .Select(s => new ContentIndexEntry
                {
                    ContentId = s.SolutionID,
                    ContentType = ContentType.Solution,
                    CreatedAt = s.CreatedAt,
                    AverageVote = s.SolutionVotes.Any() ? s.SolutionVotes.Average(v => v.VoteValue) : 0,
                    TotalVotes = s.SolutionVotes.Any() ? s.SolutionVotes.Count() : 0
                });

            // Combine queries
            var combinedQuery = issuesIndexQuery.Union(solutionsIndexQuery);

            var filteredQuery = FilterQueryService.ApplyCombinedContentFilters(combinedQuery, filter);
            var sortedQuery = SortQueryService.ApplyCombinedContentSorting(filteredQuery);


            var pagedIndexEntries = sortedQuery.Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(contentItem => new ContentIdentifier() { 
                    Id = contentItem.ContentId,
                    Type = contentItem.ContentType,
                })
                .ToListAsync();

            return pagedIndexEntries!;
        }

        public async Task<ContentCount_VM?> GetContentCountMainContentFeed(ContentFilter filter)
        {
            // First, get all the issues and solutions IDs with their creation dates and vote averages
            // This allows efficient sorting and pagination at the database level
            var issuesIndexQuery = _context.Issues
               .Select(i => new ContentIndexEntry
               {
                   ContentId = i.IssueID,
                   ContentType = ContentType.Issue,
                   CreatedAt = i.CreatedAt,
                   AverageVote = i.IssueVotes.Any() ? i.IssueVotes.Average(v => v.VoteValue) : 0,
                   TotalVotes = i.IssueVotes.Any() ? i.IssueVotes.Count() : 0,
               });

            var solutionsIndexQuery = _context.Solutions
                .Select(s => new ContentIndexEntry
                {
                    ContentId = s.SolutionID,
                    ContentType = ContentType.Solution,
                    CreatedAt = s.CreatedAt,
                    AverageVote = s.SolutionVotes.Any() ? s.SolutionVotes.Average(v => v.VoteValue) : 0,
                    TotalVotes = s.SolutionVotes.Any() ? s.SolutionVotes.Count() : 0
                });

            ContentCount_VM counts = new ContentCount_VM();

            // Combine queries
            var combinedQuery = issuesIndexQuery.Union(solutionsIndexQuery);
            var filteredQuery = FilterQueryService.ApplyCombinedContentFilters(combinedQuery, filter);

            counts.AbsoluteCount = await combinedQuery.CountAsync();
            counts.FilteredCount = await filteredQuery.CountAsync();

            return counts;
        }
    }


    

}
