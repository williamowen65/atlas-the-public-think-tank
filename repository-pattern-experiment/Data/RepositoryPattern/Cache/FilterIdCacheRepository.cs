using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Caching.Memory;
using repository_pattern_experiment.Data.RepositoryPattern.IRepository;
using repository_pattern_experiment.Data.RepositoryPattern.Repository.Helpers;
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

        public async Task<List<Guid>?> GetPagedSolutionIdsOfIssueById(Guid issueId, ContentFilter filter, int pageNumber = 1, int pageSize = 3)
        {
            string filterHash = filter.ToJson().GetHashCode().ToString();
            return await _cache.GetOrCreateAsync($"solution-feed-ids:{issueId}:{filterHash}:{pageNumber}", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                return await _inner.GetPagedSolutionIdsOfIssueById(issueId, filter, pageNumber, pageSize);
            });
        }

        public async Task<List<Guid>?> GetPagedSubIssueIdsOfIssueById(Guid issueId, ContentFilter filter, int pageNumber = 1, int pageSize = 3)
        {
            string filterHash = filter.ToJson().GetHashCode().ToString();
            return await _cache.GetOrCreateAsync($"sub-issue-feed-ids:{issueId}:{filterHash}:{pageNumber}", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                var pagedSubIssueIds= await _inner.GetPagedSubIssueIdsOfIssueById(issueId, filter, pageNumber, pageSize);
                return pagedSubIssueIds;
            });
        }

        public async Task<List<Guid>?> GetPagedSubIssueIdsOfSolutionById(Guid solutionId, ContentFilter filter, int pageNumber = 1, int pageSize = 3)
        {
            string filterHash = filter.ToJson().GetHashCode().ToString();
            return await _cache.GetOrCreateAsync($"sub-issue-feed-ids:{solutionId}:{filterHash}:{pageNumber}", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                return await _inner.GetPagedSubIssueIdsOfSolutionById(solutionId, filter, pageNumber, pageSize);
            });
        }

        public async Task<int> GetTotalCountSubIssuesOfIssueById(Guid issueId)
        {
            return await _cache.GetOrCreateAsync($"sub-issue-total-count:{issueId}", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                return await _inner.GetTotalCountSubIssuesOfIssueById(issueId);
            });
        }
        public async Task<int> GetTotalCountSubIssuesOfSolutionById(Guid solutionId)
        {
            return await _cache.GetOrCreateAsync($"sub-issue-total-count:{solutionId}", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                return await _inner.GetTotalCountSubIssuesOfSolutionById(solutionId);
            });
        }
        public async Task<int> GetTotalCountSolutionsOfIssueById(Guid issueId)
        {
            return await _cache.GetOrCreateAsync($"solutions-total-count:{issueId}", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                return await _inner.GetTotalCountSolutionsOfIssueById(issueId);
            });
        }
    }
}
