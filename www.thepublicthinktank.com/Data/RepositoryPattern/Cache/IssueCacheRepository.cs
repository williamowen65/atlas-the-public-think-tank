using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Issue;
using atlas_the_public_think_tank.Data.RepositoryPattern.IRepository;
using atlas_the_public_think_tank.Data.RepositoryPattern.Repository.Helpers;
using atlas_the_public_think_tank.Models.ViewModel;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Issue;
using Microsoft.Extensions.Caching.Memory;

namespace atlas_the_public_think_tank.Data.RepositoryPattern.Cache
{



    public class IssueCacheRepository : IIssueRepository
    {
        private readonly IIssueRepository _inner;
        private readonly IMemoryCache _cache;
        private readonly ILogger _cacheLogger;
        private readonly IFilterIdSetRepository _filterIdSetRepository;

        public IssueCacheRepository(IIssueRepository inner, IMemoryCache cache, ILoggerFactory loggerFactory, IFilterIdSetRepository filterIdSetRepository)
        {
            _cache = cache;
            _inner = inner;
            _cacheLogger = loggerFactory.CreateLogger("CacheLog");
            _filterIdSetRepository = filterIdSetRepository;
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
            var cacheKey = $"issue:{id}";
            if (_cache.TryGetValue(cacheKey, out IssueRepositoryViewModel? cachedIssue))
            {
                _cacheLogger.LogInformation("[+] Cache hit for issue {IssueId}", id);
                return cachedIssue;
            }
            else
            {
                _cacheLogger.LogInformation("[!] Cache miss for issue {IssueId}", id);
                var issue = await _cache.GetOrCreateAsync(cacheKey, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                    return await _inner.GetIssueById(id);
                });
                return issue;
            }
        }


        /*
            AddIssueAsync sets cache implicitly by nested call to Read.Issue
        */
        public async Task<Issue_ReadVM> AddIssueAsync(Issue issue)
        {
            // When creating an issue invalidate all filterIdSets in the cache
            //CacheHelper.ClearAllFeedIdSets();

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

            return updatedIssueVM;
        }

        public async Task<int> GetIssueVersionHistoryCount(Guid issueID)
        {
            _cacheLogger.LogInformation("[!] Cache miss for issue VersionHistoryCount {IssueId}", issueID);
            return await _inner.GetIssueVersionHistoryCount(issueID);
        }
    }
}
