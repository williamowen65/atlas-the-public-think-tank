using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Issue;
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

        public IssueCacheRepository(IIssueRepository inner, IMemoryCache cache, ILoggerFactory loggerFactory, IFilterIdSetRepository filterIdSetRepository, IConfiguration configuration)
        {
            _cache = cache;
            _inner = inner;
            _cacheLogger = loggerFactory.CreateLogger("CacheLog");
            _filterIdSetRepository = filterIdSetRepository;
            _configuration = configuration;
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


        public async Task<IssueRepositoryViewModel?> GetIssueById(Guid id)
        {

            if (_configuration.GetValue<bool>("Caching:Enabled") == false)
            {
                _cacheLogger.LogInformation("[~] Cache skip for issue {IssueId}", id);
                return await _inner.GetIssueById(id);
            }

            var cacheKey = $"issue:{id}";
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


        /*
            AddIssueAsync sets cache implicitly by nested call to Read.Issue
        */
        public async Task<Issue_ReadVM> AddIssueAsync(Issue issue)
        {
            // When creating an issue invalidate all filterIdSets in the cache
            //CacheHelper.ClearAllFeedIdSets();

            if (issue.ParentIssueID != null) {
                // Clear sub-issue-feed-ids cache for parent issue
                CacheHelper.ClearSubIssueFeedIdsForIssue((Guid)issue.ParentIssueID);
                CacheHelper.ClearContentCountSubIssuesForIssue((Guid)issue.ParentIssueID);
            }


            return await _inner.AddIssueAsync(issue);
        }



        public async Task<Issue_ReadVM?> UpdateIssueAsync(Issue issue)
        {

            var cacheKey = $"issue:{issue.IssueID}";

            //convert issue to IssueRepositoryViewModel
            IssueRepositoryViewModel cacheableIssue = Converter.ConvertIssueToIssueRepositoryViewModel(issue);
            // Update Cache
            _cache.Set(cacheKey, cacheableIssue, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            });

            Issue_ReadVM? updatedIssueVM = await _inner.UpdateIssueAsync(issue);

            var cacheKeyIssueVersionHistory = $"issue-version-history:{issue.IssueID}";

            // Note: This method may be better suited to be Update instead of clear.
            CacheHelper.ClearIssueContentVersionHistoryCache(issue.IssueID);

            return updatedIssueVM;
        }

        public async Task<int> GetIssueVersionHistoryCount(Guid issueID)
        {
            _cacheLogger.LogInformation("[!] Cache miss for issue VersionHistoryCount {IssueId}", issueID);
            return await _inner.GetIssueVersionHistoryCount(issueID);
        }

        public async Task<List<ContentItem_ReadVM>?> GetIssueVersionHistoryById(Issue_ReadVM issue)
        {

            if (_configuration.GetValue<bool>("Caching:Enabled") == false)
            {
                _cacheLogger.LogInformation($"[~] Cache skip for issue VersionHistory {issue.IssueID}");
                return await _inner.GetIssueVersionHistoryById(issue);
            }

            var cacheKey = $"issue-version-history:{issue.IssueID}";
            if (_cache.TryGetValue(cacheKey, out List<ContentItem_ReadVM>? cachedIssueVersionHistory))
            {
                _cacheLogger.LogInformation($"[+] Cache hit for issue VersionHistory {issue.IssueID}");
                return cachedIssueVersionHistory;
            }
            else
            {
                _cacheLogger.LogInformation($"[!] Cache miss for issue VersionHistory {issue.IssueID}");
                return await _cache.GetOrCreateAsync(cacheKey, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                    return await _inner.GetIssueVersionHistoryById(issue);
                });
            }
        }
    }
}
