using Microsoft.EntityFrameworkCore;
using repository_pattern_experiment.Data.CRUD;
using repository_pattern_experiment.Data.RepositoryPattern.IRepository;
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

        public Task<List<Guid>?> GetPagedSolutionIdsOfIssueById(Guid issueId, int pageNumber = 1, int pageSize = 3)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Guid>?> GetPagedSubIssueIdsOfIssueById(Guid issueId, int pageNumber = 1, int pageSize = 3)
        {
            var query = _context.Issues
                .Where(i => i.ParentIssueID == issueId);

            // TODO Apply Filter / Sorting
            // TODO Apply Weighted Score

            var paginatedChildIssuesIds = await query.Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(i => i.IssueID)
            .ToListAsync();

            return (paginatedChildIssuesIds);

        }

        public Task<List<Guid>?> GetPagedSubIssueIdsOfSolutionById(Guid solutionId, int pageNumber = 1, int pageSize = 3)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetTotalCountSubIssueIdsOfIssueById(Guid issueId)
        {
            var query = _context.Issues
                .Where(i => i.ParentIssueID == issueId);

            return query.CountAsync();
        }
    }
}
