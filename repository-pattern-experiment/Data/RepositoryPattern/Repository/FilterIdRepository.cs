using Microsoft.EntityFrameworkCore;
using repository_pattern_experiment.Data.CRUD;
using repository_pattern_experiment.Data.RepositoryPattern.IRepository;
using repository_pattern_experiment.Data.RepositoryPattern.Repository.Helpers;
using repository_pattern_experiment.Models.Database;
using repository_pattern_experiment.Models.ViewModel;

namespace repository_pattern_experiment.Data.RepositoryPattern.Repository
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

            // TODO Apply Filter / Sorting
            var filteredQuery = FilterQueryService.ApplyIssueFilters(query, filter);
            // TODO Apply Weighted Score
            var sortedQuery = SortQueryService.ApplyWeightedScoreSorting(filteredQuery);

            var paginatedChildIssuesIds = await sortedQuery.Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(s => s.IssueID)
            .ToListAsync();

            return (paginatedChildIssuesIds);
        }

        public Task<int> GetTotalCountSubIssuesOfIssueById(Guid issueId)
        {
            var query = _context.Issues
                .Where(i => i.ParentIssueID == issueId);

            return query.CountAsync();
        }
        public Task<int> GetTotalCountSubIssuesOfSolutionById(Guid solutionId)
        {
            var query = _context.Issues
                .Where(i => i.ParentSolutionID == solutionId);

            return query.CountAsync();
        }
        public Task<int> GetTotalCountSolutionsOfIssueById(Guid solutionId)
        {
            var query = _context.Solutions
                .Where(i => i.ParentIssueID == solutionId);

            return query.CountAsync();
        }
    }


    

}
