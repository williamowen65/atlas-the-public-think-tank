using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Issue;
using atlas_the_public_think_tank.Data.RepositoryPattern.IRepository;
using atlas_the_public_think_tank.Data.RepositoryPattern.Repository.Helpers;
using atlas_the_public_think_tank.Models;
using atlas_the_public_think_tank.Models.ViewModel;
using atlas_the_public_think_tank.Models.ViewModel.UI_VM;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using static atlas_the_public_think_tank.Data.SeedData.SeedIds;

namespace atlas_the_public_think_tank.Data.RepositoryPattern.Cache
{
    public class FilterIdCacheRepository : IFilterIdSetRepository
    {
        // These EntityNames are used for invalidation purposes
        public readonly string FilterIdSet_EntityName = "feed-ids";
        public readonly string ContentCount_EntityName = "total-count";

        private readonly IFilterIdSetRepository _inner;
        private readonly IMemoryCache _cache;
        private readonly ILogger _cacheLogger;
        private readonly IConfiguration _configuration;
        public FilterIdCacheRepository(IFilterIdSetRepository inner, IMemoryCache cache, ILoggerFactory loggerFactory, IConfiguration configuration)
        {
            _cache = cache;
            _inner = inner;
            _cacheLogger = loggerFactory.CreateLogger("CacheLog");
            _configuration = configuration;
        }

        /*
             Entity: FilterIdSet
             Access Frequency: High
             Cacheable?: Yes
             Cache Invalidation Frequency: High (Note, invalidation, not update -- Letting the database do the work of the sorting order)
                Almost every action on the website cause an invalidation.
                    Adding root issue, sub-issue, solution, comment
                    A vote on content
                Anything that would change the order of the sorting contents
         */

        #region Paged ID Sets Of Issues

        #region Issue subissue cache
        public async Task<List<Guid>?> GetPagedSubIssueIdsOfIssueById(Guid issueId, ContentFilter filter, int pageNumber = 1, int pageSize = 3)
        {

            if (_configuration.GetValue<bool>("Caching:Enabled") == false)
            {
                _cacheLogger.LogInformation($"[~] Cache skip for filterIdSet.GetPagedSubIssueIdsOfIssueById {issueId}");
                return await _inner.GetPagedSubIssueIdsOfIssueById(issueId, filter, pageNumber, pageSize);
            }

            string filterHash = filter.ToJson().GetHashCode().ToString();
            var cacheKey = $"sub-issue-feed-ids:{issueId}:{filterHash}:{pageNumber}";
            if (_cache.TryGetValue(cacheKey, out List<Guid>? cachedPagedSubIssueIds))
            {
                _cacheLogger.LogInformation($"[+] Cache hit for filterIdSet.GetPagedSubIssueIdsOfIssueById {issueId}");
                return cachedPagedSubIssueIds;
            }
            else 
            {
                _cacheLogger.LogInformation($"[!] Cache miss for filterIdSet.GetPagedSubIssueIdsOfIssueById {issueId}");
                return await _cache.GetOrCreateAsync(cacheKey, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                    return await _inner.GetPagedSubIssueIdsOfIssueById(issueId, filter, pageNumber, pageSize);
                });
            }
        }

        /// <summary>
        /// Returns an object with Total and Absolute count... 
        /// This will give info about how much content there is to be paginated, and how much content would be present if there were no filters set.
        /// </summary>
        /// <param name="issueId"></param>
        /// <param name="filter"></param>
        /// <returns>ContentCount_VM</returns>
        public async Task<ContentCount_VM?> GetContentCountSubIssuesOfIssueById(Guid issueId, ContentFilter filter)
        {

            if (_configuration.GetValue<bool>("Caching:Enabled") == false)
            {
                _cacheLogger.LogInformation($"[~] Cache skip for filterIdSet.GetContentCountSubIssuesOfIssueById {issueId}");
                return await _inner.GetContentCountSubIssuesOfIssueById(issueId, filter);
            }

            string filterHash = filter.ToJson().GetHashCode().ToString();
            var cacheKey = $"sub-issue-content-counts:{issueId}:{filterHash}";
            if (_cache.TryGetValue(cacheKey, out ContentCount_VM? subIssueContentCounts))
            {
                _cacheLogger.LogInformation($"[+] Cache hit for filterIdSet.GetContentCountSubIssuesOfIssueById {issueId}");
                return subIssueContentCounts;
            }
            else
            {
                _cacheLogger.LogInformation($"[!] Cache miss for filterIdSet.GetContentCountSubIssuesOfIssueById {issueId}");
                return await _cache.GetOrCreateAsync(cacheKey, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                    return await _inner.GetContentCountSubIssuesOfIssueById(issueId, filter);
                });
            }
           
        }
        #endregion

        #region Issue solution cache
        public async Task<List<Guid>?> GetPagedSolutionIdsOfIssueById(Guid issueId, ContentFilter filter, int pageNumber = 1, int pageSize = 3)
        {
            if (_configuration.GetValue<bool>("Caching:Enabled") == false)
            {
                _cacheLogger.LogInformation($"[~] Cache skip for filterIdSet.GetPagedSolutionIdsOfIssueById {issueId}");
                return await _inner.GetPagedSolutionIdsOfIssueById(issueId, filter, pageNumber, pageSize);
            }

            string filterHash = filter.ToJson().GetHashCode().ToString();
            var cacheKey = $"solution-feed-ids:{issueId}:{filterHash}:{pageNumber}";
            if (_cache.TryGetValue(cacheKey, out List<Guid>? cachedPagedSolutionIds))
            {
                _cacheLogger.LogInformation($"[+] Cache hit for filterIdSet.GetPagedSolutionIdsOfIssueById {issueId}");
                return cachedPagedSolutionIds;
            }
            else
            {
                _cacheLogger.LogInformation($"[!] Cache miss for filterIdSet.GetPagedSolutionIdsOfIssueById {issueId}");
                return await _cache.GetOrCreateAsync(cacheKey, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                    return await _inner.GetPagedSolutionIdsOfIssueById(issueId, filter, pageNumber, pageSize);
                });
            }
        }


        public async Task<ContentCount_VM?> GetContentCountSolutionsOfIssueById(Guid issueId, ContentFilter filter)
        {
            if (_configuration.GetValue<bool>("Caching:Enabled") == false)
            {
                _cacheLogger.LogInformation($"[~] Cache skip for filterIdSet.GetContentCountSubIssuesOfIssueById {issueId}");
                return await _inner.GetContentCountSolutionsOfIssueById(issueId, filter);
            }

            string filterHash = filter.ToJson().GetHashCode().ToString();
            var cacheKey = $"solution-content-counts:{issueId}:{filterHash}";
            if (_cache.TryGetValue(cacheKey, out ContentCount_VM? subIssueContentCounts))
            {
                _cacheLogger.LogInformation($"[+] Cache hit for filterIdSet.GetContentCountSolutionsOfIssueById {issueId}");
                return subIssueContentCounts;
            }
            else
            {
                _cacheLogger.LogInformation($"[!] Cache miss for filterIdSet.GetContentCountSolutionsOfIssueById {issueId}");
                return await _cache.GetOrCreateAsync(cacheKey, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                    return await _inner.GetContentCountSolutionsOfIssueById(issueId, filter);
                });
            }
        }

        #endregion

        #endregion

        #region Paged ID Sets of Solutions

        public async Task<List<Guid>?> GetPagedSubIssueIdsOfSolutionById(Guid solutionId, ContentFilter filter, int pageNumber = 1, int pageSize = 3)
        {

            if (_configuration.GetValue<bool>("Caching:Enabled") == false)
            {
                _cacheLogger.LogInformation($"[~] Cache miss for filterIdSet.GetPagedSubIssueIdsOfSolutionById {solutionId}");
                return await _inner.GetPagedSubIssueIdsOfSolutionById(solutionId, filter, pageNumber, pageSize);
            }

            string filterHash = filter.ToJson().GetHashCode().ToString();
            var cacheKey = $"sub-issue-feed-ids:{solutionId}:{filterHash}:{pageNumber}";
            if (_cache.TryGetValue(cacheKey, out List<Guid>? cachedPagedSubIssueIds))
            {
                _cacheLogger.LogInformation($"[+] Cache hit for filterIdSet.GetPagedSubIssueIdsOfSolutionById {solutionId}");
                return cachedPagedSubIssueIds;
            }
            else
            {
                _cacheLogger.LogInformation($"[!] Cache miss for filterIdSet.GetPagedSubIssueIdsOfSolutionById {solutionId}");
                return await _cache.GetOrCreateAsync(cacheKey, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                    return await _inner.GetPagedSubIssueIdsOfSolutionById(solutionId, filter, pageNumber, pageSize);
                });
            }

        }

        #endregion

        public async Task<List<ContentIdentifier>?> GetPagedMainContentFeedIds(ContentFilter filter, int pageNumber = 1, int pageSize = 3)
        {
            //string filterHash = filter.ToJson().GetHashCode().ToString();
            //return await _cache.GetOrCreateAsync($"main-content-feed-ids:{filterHash}:{pageNumber}", async entry =>
            //{
            //    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            //    return await _inner.GetPagedMainContentFeedIds(filter, pageNumber, pageSize);
            //});
            _cacheLogger.LogInformation($"[!] Cache miss for filterIdSet.GetPagedMainContentFeedIds");
            return await _inner.GetPagedMainContentFeedIds(filter, pageNumber, pageSize);
        }

      
        public async Task<ContentCount_VM?> GetContentCountSubIssuesOfSolutionById(Guid solutionId, ContentFilter filter)
        {


            if (_configuration.GetValue<bool>("Caching:Enabled") == false)
            {
                _cacheLogger.LogInformation($"[~] Cache skip for filterIdSet.GetContentCountSubIssuesOfSolutionById {solutionId}");
                return await _inner.GetContentCountSubIssuesOfSolutionById(solutionId, filter);
            }

            string filterHash = filter.ToJson().GetHashCode().ToString();
            var cacheKey = $"sub-issue-content-counts:{solutionId}:{filterHash}";
            if (_cache.TryGetValue(cacheKey, out ContentCount_VM? subIssueContentCounts))
            {
                _cacheLogger.LogInformation($"[+] Cache hit for filterIdSet.GetContentCountSubIssuesOfSolutionById {solutionId}");
                return subIssueContentCounts;
            }
            else
            {
                _cacheLogger.LogInformation($"[!] Cache miss for filterIdSet.GetContentCountSubIssuesOfSolutionById {solutionId}");
                return await _cache.GetOrCreateAsync(cacheKey, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                    return await _inner.GetContentCountSubIssuesOfSolutionById(solutionId, filter);
                });
            }

            //string filterHash = filter.ToJson().GetHashCode().ToString();
            //return await _cache.GetOrCreateAsync($"sub-issue-total-count:{solutionId}:{filterHash}", async entry =>
            //{
            //    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            //    return await _inner.GetContentCountSubIssuesOfSolutionById(solutionId, filter);
            //});
            _cacheLogger.LogInformation($"[!] Cache miss for filterIdSet.GetContentCountSubIssuesOfSolutionById {solutionId}");
            return await _inner.GetContentCountSubIssuesOfSolutionById(solutionId, filter);
        }
     
        public async Task<ContentCount_VM?> GetContentCountMainContentFeed(ContentFilter filter)
        {
            //string filterHash = filter.ToJson().GetHashCode().ToString();
            //return await _cache.GetOrCreateAsync($"main-content-total-count:{filterHash}", async entry =>
            //{
            //    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            //    return await _inner.GetContentCountMainContentFeed(filter);
            //});
            _cacheLogger.LogInformation($"[!] Cache miss for filterIdSet.GetContentCountMainContentFeed");
            return await _inner.GetContentCountMainContentFeed(filter);
        }
    }
}
