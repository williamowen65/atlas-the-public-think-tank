using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Issue;
using atlas_the_public_think_tank.Data.RepositoryPattern.Cache.Helpers;
using atlas_the_public_think_tank.Data.RepositoryPattern.IRepository;
using atlas_the_public_think_tank.Data.RepositoryPattern.Repository.Helpers;
using atlas_the_public_think_tank.Models.ViewModel;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.ContentItem_Common;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Issue;
using Microsoft.Extensions.Caching.Memory;
using repository_pattern_experiment.Controllers;
using static atlas_the_public_think_tank.Data.SeedData.SeedIds;

namespace atlas_the_public_think_tank.Data.RepositoryPattern.Cache
{
    public class IssueCacheRepository : IIssueRepository
    {
        private readonly IIssueRepository _inner;
        private readonly IMemoryCache _cache;
        private readonly ILogger _cacheLogger;
        private readonly IFilterIdSetRepository _filterIdSetRepository;
        private readonly IConfiguration _configuration;
        private readonly CacheHelper _cacheHelper;

        public IssueCacheRepository(IIssueRepository inner, IMemoryCache cache, ILoggerFactory loggerFactory, IFilterIdSetRepository filterIdSetRepository, IConfiguration configuration, CacheHelper cacheHelper)
        {
            _cache = cache;
            _inner = inner;
            _cacheLogger = loggerFactory.CreateLogger("CacheLog");
            _filterIdSetRepository = filterIdSetRepository;
            _configuration = configuration;
            _cacheHelper = cacheHelper;
        }


        /*
         
        Entity: Issue
        Access Frequency: High
        Cacheable?: Yes
        Cache Update:
            When the Issue content, title, or scope is updated
            
            Not when a direct child content is added
                SubIssue, Solution, Comment
                  These update the counts on the issue.
                  Requires adding the entry to the cache 
                    These have pointers to a ParentIssueID
                    That should update the count 
                Adding a subIssue, should invalidate an ID set,
                Adding a solution, should invalidate an ID set,
                Adding a comment, should invalidate an ID set,
        */

        /// <summary>
        /// Caching layer for "reading an issue" from the cache.
        /// </summary>
        /// <remarks>
        /// However, a full issue read is performed with
        /// <see cref="CRUD.Read.Issue(Guid, ContentFilter, bool)"/> which pulls from multiple parts of the cache.
        /// </remarks>
        public async Task<IssueRepositoryViewModel?> GetIssueById(Guid id)
        {

            if (_configuration.GetValue<bool>("Caching:Enabled") == false)
            {
                _cacheLogger.LogInformation("[~] Cache skip for issue {IssueId}", id);
                return await _inner.GetIssueById(id);
            }

            var cacheKey = $"{CacheKeyPrefix.Issue}:{id}";
            if (_cache.TryGetValue(cacheKey, out IssueRepositoryViewModel? cachedIssue))
            {
                _cacheLogger.LogInformation("[+] Cache hit for issue {IssueId}", id);
                return cachedIssue;
            }
            else
            {
                _cacheLogger.LogInformation("[!] Cache miss for issue {IssueId}", id);
                return await _cache.GetOrCreateAsync(cacheKey, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                    return await _inner.GetIssueById(id);
                });
            }
        }


        /// <summary>
        /// Cache layer for "creating a new issue"
        /// </summary>
        /// <remarks>
        /// Clears related parent content cache entities <br/>
        /// </remarks>
        public async Task<Issue_ReadVM> AddIssueAsync(Issue issue)
        {

            // TODO Improve cache validation on a per ContentStatus basis (draft, published, etc)
            #region cache invalidation
            // When creating an issue invalidate all related filterIdSets in the cache
            // This can later be optimized

            if (issue.ParentIssueID != null) {
                // Clear CacheHelper.CacheKeysPrefix.FeedIds.SubIssueOfIssue cache for parent issue
                _cacheHelper.ClearSubIssueFeedIdsForIssue((Guid)issue.ParentIssueID);
                _cacheHelper.ClearContentCountSubIssuesForIssue((Guid)issue.ParentIssueID);
            }
            if(issue.ParentSolutionID != null)
            { 
                _cacheHelper.ClearSubIssueFeedIdsForSolution((Guid)issue.ParentSolutionID);
                _cacheHelper.ClearContentCountSubIssuesForSolution((Guid)issue.ParentSolutionID);
            }

            // Main page
            _cacheHelper.ClearContentCountForMainPage();
            _cacheHelper.ClearMainPageFeedIds();

            // User Profile FeedId
            _cacheHelper.ClearIssueFeedIdsForUser(issue.AuthorID);
            // User Profile Content count
            _cacheHelper.ClearContentCountIssuesForUser(issue.AuthorID);
            #endregion

            return await _inner.AddIssueAsync(issue);
        }




        /// <summary>
        /// Cache layer for "Updating an issue"
        /// </summary>
        /// <remarks>
        /// Directly updates the cache, preventing a database read
        /// </remarks>
        public async Task<Issue_ReadVM?> UpdateIssueAsync(Issue issue)
        {
            if (_configuration.GetValue<bool>("Caching:Enabled") == false)
            {
                _cacheLogger.LogInformation($"[~] Cache skip for updating issue {issue.IssueID}");
                return await _inner.UpdateIssueAsync(issue);
            }

            #region cache invalidation

            var cacheKey = $"{CacheKeyPrefix.Issue}:{issue.IssueID}";

            //convert issue to IssueRepositoryViewModel
            IssueRepositoryViewModel cacheableIssue = Converter.ConvertIssueToIssueRepositoryViewModel(issue);
            
            // TODO: Create cache helper method for this.
            // Update Cache
            _cache.Set(cacheKey, cacheableIssue, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            });


            // Content Version History
            // Note: This method may be better suited to be Update instead of clear.
            _cacheHelper.ClearIssueContentVersionHistoryCache(issue.IssueID);

            // User Profile Page
            //If the user updates a post from Draft to Published the feed should be updated
            _cacheHelper.ClearIssueFeedIdsForUser(issue.AuthorID);
            _cacheHelper.ClearContentCountIssuesForUser(issue.AuthorID);

            // Main Page
            _cacheHelper.ClearContentCountForMainPage(); 
            _cacheHelper.ClearMainPageFeedIds();

            // Read Issue Page
            if (issue.ParentIssueID != null)
            { 
                // Clear CacheHelper.CacheKeysPrefix.FeedIds.SubIssueOfIssue cache for parent issue
                _cacheHelper.ClearSubIssueFeedIdsForIssue((Guid)issue.ParentIssueID);
                _cacheHelper.ClearContentCountSubIssuesForIssue((Guid)issue.ParentIssueID);
            }

            // Read Solution Page
            if (issue.ParentSolutionID != null)
            {
                _cacheHelper.ClearSubIssueFeedIdsForSolution((Guid)issue.ParentSolutionID);
                _cacheHelper.ClearContentCountSubIssuesForSolution((Guid)issue.ParentSolutionID);
            }
            #endregion

            return await _inner.UpdateIssueAsync(issue);
        }

        public async Task<int> GetIssueVersionHistoryCount(Guid issueID)
        {
            _cacheLogger.LogInformation("[!] Cache miss for issue VersionHistoryCount {IssueId}", issueID);
            return await _inner.GetIssueVersionHistoryCount(issueID);
        }

        /// <summary>
        /// Cache layer for "The version history of an issue"
        /// </summary>
        /// <remarks>
        /// Cache should be updated/invalidated by: <br/>
        /// <see cref="IssueCacheRepository.UpdateIssueAsync(Issue)"/>
        /// </remarks>
        public async Task<List<ContentItem_ReadVM>?> GetIssueVersionHistoryByIssueVM(Issue_ReadVM issue)
        {
            if (_configuration.GetValue<bool>("Caching:Enabled") == false)
            {
                _cacheLogger.LogInformation($"[~] Cache skip for issue VersionHistory {issue.IssueID}");
                return await _inner.GetIssueVersionHistoryByIssueVM(issue);
            }

            var cacheKey = $"{CacheKeyPrefix.IssueVersionHistory}:{issue.IssueID}";
            if (_cache.TryGetValue(cacheKey, out List<ContentItem_ReadVM>? cachedIssueVersionHistory))
            {
                _cacheLogger.LogInformation($"[+] Cache hit for issue VersionHistory {issue.IssueID}");
                return cachedIssueVersionHistory;
            }
            else
            {
                _cacheLogger.LogWarning($"[!] Cache miss for issue VersionHistory {issue.IssueID}");
                return await _cache.GetOrCreateAsync(cacheKey, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                    return await _inner.GetIssueVersionHistoryByIssueVM(issue);
                });
            }
        }
    }
}
