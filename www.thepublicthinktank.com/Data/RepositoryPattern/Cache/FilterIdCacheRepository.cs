using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Caching.Memory;
using atlas_the_public_think_tank.Data.RepositoryPattern.IRepository;
using atlas_the_public_think_tank.Data.RepositoryPattern.Repository.Helpers;
using atlas_the_public_think_tank.Models;
using atlas_the_public_think_tank.Models.ViewModel;

namespace atlas_the_public_think_tank.Data.RepositoryPattern.Cache
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
            //string filterHash = filter.ToJson().GetHashCode().ToString();
            //return await _cache.GetOrCreateAsync($"solution-feed-ids:{issueId}:{filterHash}:{pageNumber}", async entry =>
            //{
            //    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            //    return await _inner.GetPagedSolutionIdsOfIssueById(issueId, filter, pageNumber, pageSize);
            //});
            return await _inner.GetPagedSolutionIdsOfIssueById(issueId, filter, pageNumber, pageSize);
        }

        public async Task<List<Guid>?> GetPagedSubIssueIdsOfIssueById(Guid issueId, ContentFilter filter, int pageNumber = 1, int pageSize = 3)
        {
            //string filterHash = filter.ToJson().GetHashCode().ToString();
            //return await _cache.GetOrCreateAsync($"sub-issue-feed-ids:{issueId}:{filterHash}:{pageNumber}", async entry =>
            //{
            //    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            //    return await _inner.GetPagedSubIssueIdsOfIssueById(issueId, filter, pageNumber, pageSize);
            //});
            return await _inner.GetPagedSubIssueIdsOfIssueById(issueId, filter, pageNumber, pageSize);
        }

        public async Task<List<Guid>?> GetPagedSubIssueIdsOfSolutionById(Guid solutionId, ContentFilter filter, int pageNumber = 1, int pageSize = 3)
        {
            //string filterHash = filter.ToJson().GetHashCode().ToString();
            //return await _cache.GetOrCreateAsync($"sub-issue-feed-ids:{solutionId}:{filterHash}:{pageNumber}", async entry =>
            //{
            //    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            //    return await _inner.GetPagedSubIssueIdsOfSolutionById(solutionId, filter, pageNumber, pageSize);
            //});
            return await _inner.GetPagedSubIssueIdsOfSolutionById(solutionId, filter, pageNumber, pageSize);
        }

        public async Task<List<ContentIdentifier>?> GetPagedMainContentFeedIds(ContentFilter filter, int pageNumber = 1, int pageSize = 3)
        {
            //string filterHash = filter.ToJson().GetHashCode().ToString();
            //return await _cache.GetOrCreateAsync($"main-content-feed-ids:{filterHash}:{pageNumber}", async entry =>
            //{
            //    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            //    return await _inner.GetPagedMainContentFeedIds(filter, pageNumber, pageSize);
            //});
            return await _inner.GetPagedMainContentFeedIds(filter, pageNumber, pageSize);
        }

        public async Task<ContentCount_ReadVM?> GetContentCountSubIssuesOfIssueById(Guid issueId, ContentFilter filter)
        {
            //string filterHash = filter.ToJson().GetHashCode().ToString();
            //return await _cache.GetOrCreateAsync($"sub-issue-total-count:{issueId}:{filterHash}", async entry =>
            //{
            //    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            //    return await _inner.GetContentCountSubIssuesOfIssueById(issueId, filter);
            //});
            return await _inner.GetContentCountSubIssuesOfIssueById(issueId, filter);
        }
        public async Task<ContentCount_ReadVM?> GetContentCountSubIssuesOfSolutionById(Guid solutionId, ContentFilter filter)
        {
            //string filterHash = filter.ToJson().GetHashCode().ToString();
            //return await _cache.GetOrCreateAsync($"sub-issue-total-count:{solutionId}:{filterHash}", async entry =>
            //{
            //    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            //    return await _inner.GetContentCountSubIssuesOfSolutionById(solutionId, filter);
            //});
            return await _inner.GetContentCountSubIssuesOfSolutionById(solutionId, filter);
        }
        public async Task<ContentCount_ReadVM?> GetContentCountSolutionsOfIssueById(Guid issueId, ContentFilter filter )
        {
            //string filterHash = filter.ToJson().GetHashCode().ToString();
            //return await _cache.GetOrCreateAsync($"solutions-total-count:{issueId}:{filterHash}", async entry =>
            //{
            //    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            //    return await _inner.GetContentCountSolutionsOfIssueById(issueId, filter);
            //}); 
            return await _inner.GetContentCountSolutionsOfIssueById(issueId, filter);
        }

        public async Task<ContentCount_ReadVM?> GetContentCountMainContentFeed(ContentFilter filter)
        {
            //string filterHash = filter.ToJson().GetHashCode().ToString();
            //return await _cache.GetOrCreateAsync($"main-content-total-count:{filterHash}", async entry =>
            //{
            //    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            //    return await _inner.GetContentCountMainContentFeed(filter);
            //});
            return await _inner.GetContentCountMainContentFeed(filter);
        }
    }
}
