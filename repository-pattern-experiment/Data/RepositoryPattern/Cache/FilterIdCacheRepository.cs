using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Caching.Memory;
using repository_pattern_experiment.Data.RepositoryPattern.IRepository;
using repository_pattern_experiment.Models.ViewModel;

namespace repository_pattern_experiment.Data.RepositoryPattern.Cache
{
    public class FilterIdCacheRepository : IFilterIdSetRepository
    {

        private readonly IFilterIdSetRepository _inner;
        private readonly IMemoryCache _cache;
        public FilterIdCacheRepository(IFilterIdSetRepository inner, IMemoryCache cache)
        {
            _cache = cache;
            _inner = inner;
        }

        public async Task<List<Guid>?> GetPagedSolutionIdsOfIssueById(Guid issueId, int pageNumber = 1, int pageSize = 3)
        {
            return await _cache.GetOrCreateAsync($"solution-feed-ids:{issueId}:{pageNumber}", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                return await _inner.GetPagedSolutionIdsOfIssueById(issueId, pageNumber, pageSize);
            });
        }

        public async Task<List<Guid>?> GetPagedSubIssueIdsOfIssueById(Guid issueId, int pageNumber = 1, int pageSize = 3)
        {
            return await _cache.GetOrCreateAsync($"sub-issue-feed-ids:{issueId}:{pageNumber}", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                var pagedSubIssueIds= await _inner.GetPagedSubIssueIdsOfIssueById(issueId, pageNumber, pageSize);
                return pagedSubIssueIds;
            });
        }

        public async Task<List<Guid>?> GetPagedSubIssueIdsOfSolutionById(Guid solutionId, int pageNumber = 1, int pageSize = 3)
        {
            return await _cache.GetOrCreateAsync($"sub-issue-feed-ids:{solutionId}:{pageNumber}", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                return await _inner.GetPagedSubIssueIdsOfSolutionById(solutionId, pageNumber, pageSize);
            });
        }

        public async Task<int> GetTotalCountSubIssueIdsOfIssueById(Guid issueId)
        {
            return await _cache.GetOrCreateAsync($"sub-issue-total-count:{issueId}", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                return await _inner.GetTotalCountSubIssueIdsOfIssueById(issueId);
            });
        }
    }
}
