using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Issue;
using atlas_the_public_think_tank.Data.RepositoryPattern.Cache.Helpers;
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

        /// <summary>
        /// Cache layer for "Paginated sub-issue ids for an issue"
        /// </summary>
        /// <remarks>
        /// Cache should be invalidated by:<br/>
        /// <see cref="IssueCacheRepository.AddIssueAsync(Issue)"/><br/>
        /// <see cref="VoteStatsCacheRepository.UpsertIssueVote(Models.ViewModel.CRUD_VM.Issue.IssueVote.IssueVote_UpsertVM, DatabaseEntities.Users.AppUser)"/><br/>
        /// </remarks>
        public async Task<List<Guid>?> GetPagedSubIssueIdsOfIssueById(Guid issueId, ContentFilter filter, int pageNumber = 1, int pageSize = 3)
        {

            if (_configuration.GetValue<bool>("Caching:Enabled") == false)
            {
                _cacheLogger.LogInformation($"[~] Cache skip for filterIdSet.GetPagedSubIssueIdsOfIssueById {issueId}");
                return await _inner.GetPagedSubIssueIdsOfIssueById(issueId, filter, pageNumber, pageSize);
            }

            string filterCacheString = filter.ToCacheString();
            var cacheKey = $"{CacheKeyPrefix.SubIssueOfIssueOrSolutionFeedIds}:{issueId}:{filterCacheString}:page({pageNumber})";
            if (_cache.TryGetValue(cacheKey, out List<Guid>? cachedPagedSubIssueIds))
            {
                _cacheLogger.LogInformation($"[+] Cache hit for filterIdSet.GetPagedSubIssueIdsOfIssueById {issueId}");
                return cachedPagedSubIssueIds;
            }
            else 
            {
                _cacheLogger.LogWarning($"[!] Cache miss for filterIdSet.GetPagedSubIssueIdsOfIssueById {issueId}");
                return await _cache.GetOrCreateAsync(cacheKey, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                    return await _inner.GetPagedSubIssueIdsOfIssueById(issueId, filter, pageNumber, pageSize);
                });
            }
        }

        /// <summary>
        /// Cache layer for "Sub-issue count of an issue"
        /// </summary>
        /// <remarks>
        /// Cache should be updated by: <br/>
        /// <see cref="IssueCacheRepository.AddIssueAsync(Issue)"/> <br/>
        /// </remarks>
        public async Task<ContentCount_VM?> GetContentCountSubIssuesOfIssueById(Guid issueId, ContentFilter filter)
        {

            if (_configuration.GetValue<bool>("Caching:Enabled") == false)
            {
                _cacheLogger.LogInformation($"[~] Cache skip for filterIdSet.GetContentCountSubIssuesOfIssueById {issueId}");
                return await _inner.GetContentCountSubIssuesOfIssueById(issueId, filter);
            }

            string filterCacheString = filter.ToCacheString();
            var cacheKey = $"{CacheKeyPrefix.SubIssueForIssueContentCount}:{issueId}:{filterCacheString}";
            if (_cache.TryGetValue(cacheKey, out ContentCount_VM? subIssueContentCounts))
            {
                _cacheLogger.LogInformation($"[+] Cache hit for filterIdSet.GetContentCountSubIssuesOfIssueById {issueId}");
                return subIssueContentCounts;
            }
            else
            {
                _cacheLogger.LogWarning($"[!] Cache miss for filterIdSet.GetContentCountSubIssuesOfIssueById {issueId}");
                return await _cache.GetOrCreateAsync(cacheKey, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                    return await _inner.GetContentCountSubIssuesOfIssueById(issueId, filter);
                });
            }
           
        }
        #endregion

        #region Issue solution cache

        /// <summary>
        /// Cache layer for "Paginated solution ids of an issue"
        /// </summary>
        /// <remarks>
        /// Cache should be invalided by: <br/>
        /// <see cref="SolutionCacheRepository.AddSolutionAsync(DatabaseEntities.Content.Solution.Solution)"/><br/>
        /// <see cref="VoteStatsCacheRepository.UpsertSolutionVote(Models.ViewModel.CRUD_VM.Solution.SolutionVote.SolutionVote_UpsertVM, DatabaseEntities.Users.AppUser)"/><br/>
        /// </remarks>
        public async Task<List<Guid>?> GetPagedSolutionIdsOfIssueById(Guid issueId, ContentFilter filter, int pageNumber = 1, int pageSize = 3)
        {
            if (_configuration.GetValue<bool>("Caching:Enabled") == false)
            {
                _cacheLogger.LogInformation($"[~] Cache skip for filterIdSet.GetPagedSolutionIdsOfIssueById {issueId}");
                return await _inner.GetPagedSolutionIdsOfIssueById(issueId, filter, pageNumber, pageSize);
            }

            string filterCacheString = filter.ToCacheString();
            var cacheKey = $"{CacheKeyPrefix.SolutionsOfIssueFeedIds}:{issueId}:{filterCacheString}:page({pageNumber})";
            if (_cache.TryGetValue(cacheKey, out List<Guid>? cachedPagedSolutionIds))
            {
                _cacheLogger.LogInformation($"[+] Cache hit for filterIdSet.GetPagedSolutionIdsOfIssueById {issueId}");
                return cachedPagedSolutionIds;
            }
            else
            {
                _cacheLogger.LogWarning($"[!] Cache miss for filterIdSet.GetPagedSolutionIdsOfIssueById {issueId}");
                return await _cache.GetOrCreateAsync(cacheKey, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                    return await _inner.GetPagedSolutionIdsOfIssueById(issueId, filter, pageNumber, pageSize);
                });
            }
        }


        /// <summary>
        /// Cache layer for "Solution count for an issue"
        /// </summary>
        /// <remarks>
        /// Cache should be updated by: <br/>
        /// <see cref="SolutionCacheRepository.AddSolutionAsync(DatabaseEntities.Content.Solution.Solution)"/> <br/>
        /// </remarks>
        public async Task<ContentCount_VM?> GetContentCountSolutionsOfIssueById(Guid issueId, ContentFilter filter)
        {
            if (_configuration.GetValue<bool>("Caching:Enabled") == false)
            {
                _cacheLogger.LogInformation($"[~] Cache skip for filterIdSet.GetContentCountSubIssuesOfIssueById {issueId}");
                return await _inner.GetContentCountSolutionsOfIssueById(issueId, filter);
            }

            string filterCacheString = filter.ToCacheString();
            var cacheKey = $"{CacheKeyPrefix.SolutionForIssueContentCount}:{issueId}:{filterCacheString}";
            if (_cache.TryGetValue(cacheKey, out ContentCount_VM? subIssueContentCounts))
            {
                _cacheLogger.LogInformation($"[+] Cache hit for filterIdSet.GetContentCountSolutionsOfIssueById {issueId}");
                return subIssueContentCounts;
            }
            else
            {
                _cacheLogger.LogWarning($"[!] Cache miss for filterIdSet.GetContentCountSolutionsOfIssueById {issueId}");
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

        /// <summary>
        /// Cache layer for "Paginated sub-issue ids of a solution
        /// </summary>
        /// <remarks>
        /// Cache invalidated by:<br/>
        /// <see cref="IssueCacheRepository.AddIssueAsync(Issue)"/><br/>
        /// <see cref="VoteStatsCacheRepository.UpsertIssueVote(atlas_the_public_think_tank.Models.ViewModel.CRUD_VM.Issue.IssueVote.IssueVote_UpsertVM, atlas_the_public_think_tank.Data.DatabaseEntities.Users.AppUser)"/>
        /// </remarks>
        public async Task<List<Guid>?> GetPagedSubIssueIdsOfSolutionById(Guid solutionId, ContentFilter filter, int pageNumber = 1, int pageSize = 3)
        {

            if (_configuration.GetValue<bool>("Caching:Enabled") == false)
            {
                _cacheLogger.LogInformation($"[~] Cache miss for filterIdSet.GetPagedSubIssueIdsOfSolutionById {solutionId}");
                return await _inner.GetPagedSubIssueIdsOfSolutionById(solutionId, filter, pageNumber, pageSize);
            }

            string filterCacheString = filter.ToCacheString();
            var cacheKey = $"{CacheKeyPrefix.SubIssueOfIssueOrSolutionFeedIds}:{solutionId}:{filterCacheString}:page({pageNumber})";
            if (_cache.TryGetValue(cacheKey, out List<Guid>? cachedPagedSubIssueIds))
            {
                _cacheLogger.LogInformation($"[+] Cache hit for filterIdSet.GetPagedSubIssueIdsOfSolutionById {solutionId}");
                return cachedPagedSubIssueIds;
            }
            else
            {
                _cacheLogger.LogWarning($"[!] Cache miss for filterIdSet.GetPagedSubIssueIdsOfSolutionById {solutionId}");
                return await _cache.GetOrCreateAsync(cacheKey, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                    return await _inner.GetPagedSubIssueIdsOfSolutionById(solutionId, filter, pageNumber, pageSize);
                });
            }

        }

        /// <summary>
        /// Caching layer of "Sub-issue content counts of a solution"
        /// </summary>
        /// <remarks>
        /// Cache should be updated in during specific actions: <br/> 
        /// <see cref="IssueCacheRepository.AddIssueAsync(Issue)"/>
        /// </remarks>
        public async Task<ContentCount_VM?> GetContentCountSubIssuesOfSolutionById(Guid solutionId, ContentFilter filter)
        {
            if (_configuration.GetValue<bool>("Caching:Enabled") == false)
            {
                _cacheLogger.LogInformation($"[~] Cache skip for filterIdSet.GetContentCountSubIssuesOfSolutionById {solutionId}");
                return await _inner.GetContentCountSubIssuesOfSolutionById(solutionId, filter);
            }

            string filterCacheString = filter.ToCacheString();
            var cacheKey = $"{CacheKeyPrefix.SubIssueForIssueContentCount}:{solutionId}:{filterCacheString}";
            if (_cache.TryGetValue(cacheKey, out ContentCount_VM? subIssueContentCounts))
            {
                _cacheLogger.LogInformation($"[+] Cache hit for filterIdSet.GetContentCountSubIssuesOfSolutionById {solutionId}");
                return subIssueContentCounts;
            }
            else
            {
                _cacheLogger.LogWarning($"[!] Cache miss for filterIdSet.GetContentCountSubIssuesOfSolutionById {solutionId}");
                return await _cache.GetOrCreateAsync(cacheKey, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                    return await _inner.GetContentCountSubIssuesOfSolutionById(solutionId, filter);
                });
            }
        }

        #endregion

        #region home page cache

        /// <summary>
        /// Cache layer for "Paginated main page content ids (issues and/or solutions)"
        /// </summary>
        /// <remarks>
        /// Cache should be invalided by: <br/>
        /// <see cref="IssueCacheRepository.AddIssueAsync(Issue)"/><br/>
        /// <see cref="VoteStatsCacheRepository.UpsertIssueVote(Models.ViewModel.CRUD_VM.Issue.IssueVote.IssueVote_UpsertVM, DatabaseEntities.Users.AppUser)"/><br/>
        /// <see cref="SolutionCacheRepository.AddSolutionAsync(DatabaseEntities.Content.Solution.Solution)"/><br/>
        /// <see cref="VoteStatsCacheRepository.UpsertSolutionVote(Models.ViewModel.CRUD_VM.Solution.SolutionVote.SolutionVote_UpsertVM, DatabaseEntities.Users.AppUser)"/><br/>
        /// </remarks>
        public async Task<List<ContentIdentifier>?> GetPagedMainContentFeedIds(ContentFilter filter, int pageNumber = 1, int pageSize = 3)
        {
            if (_configuration.GetValue<bool>("Caching:Enabled") == false)
            {
                _cacheLogger.LogInformation($"[~] Cache miss for filterIdSet.GetPagedMainContentFeedIds");
                return await _inner.GetPagedMainContentFeedIds(filter, pageNumber, pageSize);
            }

            string filterCacheString = filter.ToCacheString();
            var cacheKey = $"{CacheKeyPrefix.MainContentFeedIds}:{filterCacheString}:page({pageNumber})";
            if (_cache.TryGetValue(cacheKey, out List<ContentIdentifier>? cachedPagedMainPageContentFeedIds))
            {
                _cacheLogger.LogInformation($"[+] Cache miss for filterIdSet.GetPagedMainContentFeedIds");
                return cachedPagedMainPageContentFeedIds;
            }
            else
            {
                _cacheLogger.LogWarning($"[!] Cache miss for filterIdSet.GetPagedMainContentFeedIds");
                return await _cache.GetOrCreateAsync(cacheKey, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                    return await _inner.GetPagedMainContentFeedIds(filter, pageNumber, pageSize);
                });
            }
        }


        /// <summary>
        /// Cache layer for "Main page content count for an issues and/or solutions"
        /// </summary>
        /// <remarks>
        /// Cache should be updated by: <br/>
        /// <see cref="SolutionCacheRepository.AddSolutionAsync(DatabaseEntities.Content.Solution.Solution)"/> <br/>
        /// <see cref="IssueCacheRepository.AddIssueAsync(Issue)"/>
        /// </remarks>
        public async Task<ContentCount_VM?> GetContentCountMainContentFeed(ContentFilter filter)
        {
            if (_configuration.GetValue<bool>("Caching:Enabled") == false)
            {
                _cacheLogger.LogInformation($"[~] Cache skip for filterIdSet.GetContentCountMainContentFeed");
                return await _inner.GetContentCountMainContentFeed(filter);
            }

            string filterCacheString = filter.ToCacheString();
            var cacheKey = $"main-content-total-count:{filterCacheString}";
            if (_cache.TryGetValue(cacheKey, out ContentCount_VM? cachedPagedMainPageContentFeedIds))
            {
                _cacheLogger.LogInformation($"[+] Cache hit for filterIdSet.GetContentCountMainContentFeed");
                return cachedPagedMainPageContentFeedIds;
            }
            else
            {
                _cacheLogger.LogWarning($"[!] Cache miss for filterIdSet.GetContentCountMainContentFeed");
                return await _cache.GetOrCreateAsync(cacheKey, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                    return await _inner.GetContentCountMainContentFeed(filter);
                });
            }
        }

        #endregion


        public async Task<List<Guid>?> GetPagedIssueIdsOfIssuesCreatedByUser(Guid userId, ContentFilter filter, int pageNumber = 1, int pageSize = 3)
        {

            if (_configuration.GetValue<bool>("Caching:Enabled") == false)
            {
                _cacheLogger.LogInformation($"[~] Cache skip for filterIdSet.GetPagedIssueIdsOfIssuesCreatedByUser");
                return await _inner.GetPagedIssueIdsOfIssuesCreatedByUser(userId, filter, pageNumber, pageSize);
            }

            string filterCacheString = filter.ToCacheString();
            var cacheKey = $"{CacheKeyPrefix.UserIssuesFeedIds}:{userId}:{filterCacheString}:page({pageNumber})";
            if (_cache.TryGetValue(cacheKey, out List<Guid>? cachedPagedIdsOfIssuesCreatedByUser))
            {
                _cacheLogger.LogInformation($"[+] Cache hit for filterIdSet.GetPagedIssueIdsOfIssuesCreatedByUser");
                return cachedPagedIdsOfIssuesCreatedByUser;
            }
            else
            {
                _cacheLogger.LogWarning($"[!] Cache miss for filterIdSet.GetPagedIssueIdsOfIssuesCreatedByUser");
                return await _cache.GetOrCreateAsync(cacheKey, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                    return await _inner.GetPagedIssueIdsOfIssuesCreatedByUser(userId, filter, pageNumber, pageSize);
                });
            }
        }

        public async Task<ContentCount_VM?> GetContentCountIssuesOfIssuesCreatedByUser(Guid userId, ContentFilter filter)
        {
            if (_configuration.GetValue<bool>("Caching:Enabled") == false)
            {
                _cacheLogger.LogInformation($"[~] Cache skip for filterIdSet.GetContentCountIssuesOfIssuesCreatedByUser");
                return await _inner.GetContentCountIssuesOfIssuesCreatedByUser(userId, filter);
            }

            string filterCacheString = filter.ToCacheString();
            var cacheKey = $"{CacheKeyPrefix.UserIssuesContentCount}:{userId}:{filterCacheString}";
            if (_cache.TryGetValue(cacheKey, out ContentCount_VM? cachedPagedUserIssueFeedCounts))
            {
                _cacheLogger.LogInformation($"[+] Cache hit for filterIdSet.GetContentCountIssuesOfIssuesCreatedByUser");
                return cachedPagedUserIssueFeedCounts;
            }
            else
            {
                _cacheLogger.LogWarning($"[!] Cache miss for filterIdSet.GetContentCountIssuesOfIssuesCreatedByUser");
                return await _cache.GetOrCreateAsync(cacheKey, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                    return await _inner.GetContentCountIssuesOfIssuesCreatedByUser(userId, filter);
                });
            }
        }

        public async Task<List<Guid>?> GetPagedSolutionIdsOfSolutionsCreatedByUser(Guid userId, ContentFilter filter, int pageNumber = 1, int pageSize = 3)
        {
            if (_configuration.GetValue<bool>("Caching:Enabled") == false)
            {
                _cacheLogger.LogInformation($"[~] Cache skip for filterIdSet.GetPagedSolutionIdsOfSolutionsCreatedByUser");
                return await _inner.GetPagedSolutionIdsOfSolutionsCreatedByUser(userId, filter, pageNumber, pageSize);
            }

            string filterCacheString = filter.ToCacheString();
            var cacheKey = $"{CacheKeyPrefix.UserSolutionsFeedIds}:{userId}:{filterCacheString}:page({pageNumber})";
            if (_cache.TryGetValue(cacheKey, out List<Guid>? cachedPagedIdsOfSolutionsCreatedByUser))
            {
                _cacheLogger.LogInformation($"[+] Cache hit for filterIdSet.GetPagedSolutionIdsOfSolutionsCreatedByUser");
                return cachedPagedIdsOfSolutionsCreatedByUser;
            }
            else
            {
                _cacheLogger.LogWarning($"[!] Cache miss for filterIdSet.GetPagedSolutionIdsOfSolutionsCreatedByUser");
                return await _cache.GetOrCreateAsync(cacheKey, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                    return await _inner.GetPagedSolutionIdsOfSolutionsCreatedByUser(userId, filter, pageNumber, pageSize);
                });
            }
        }

        public async Task<ContentCount_VM?> GetContentCountSolutionsOfSolutionsCreatedByUser(Guid userId, ContentFilter filter)
        {
            if (_configuration.GetValue<bool>("Caching:Enabled") == false)
            {
                _cacheLogger.LogInformation($"[~] Cache skip for filterIdSet.GetContentCountSolutionsOfSolutionsCreatedByUser");
                return await _inner.GetContentCountSolutionsOfSolutionsCreatedByUser(userId, filter);
            }

            string filterCacheString = filter.ToCacheString();
            var cacheKey = $"{CacheKeyPrefix.UserSolutionsContentCount}:{userId}:{filterCacheString}";
            if (_cache.TryGetValue(cacheKey, out ContentCount_VM? cachedPagedUserSolutionFeedCounts))
            {
                _cacheLogger.LogInformation($"[+] Cache hit for filterIdSet.GetContentCountSolutionsOfSolutionsCreatedByUser");
                return cachedPagedUserSolutionFeedCounts;
            }
            else
            {
                _cacheLogger.LogWarning($"[!] Cache miss for filterIdSet.GetContentCountSolutionsOfSolutionsCreatedByUser");
                return await _cache.GetOrCreateAsync(cacheKey, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                    return await _inner.GetContentCountSolutionsOfSolutionsCreatedByUser(userId, filter);
                });
            }
        }
    }
}
