using atlas_the_public_think_tank.Data.DatabaseEntities.Users;
using atlas_the_public_think_tank.Models.Cacheable;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Issue.IssueVote;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Solution.SolutionVote;
using atlas_the_public_think_tank.Models.ViewModel.CRUD_VM.Issue.IssueVote;
using atlas_the_public_think_tank.Models.ViewModel.CRUD_VM.Solution.SolutionVote;
using Microsoft.Extensions.Caching.Memory;

namespace atlas_the_public_think_tank.Data.RepositoryPattern.Cache.Helpers
{


    public class CacheHelper
    {
        private IMemoryCache _cache;

        public CacheHelper(IMemoryCache cache)
        {
            _cache = cache;
        }



        /// <summary>
        /// For debugging: Get all cached keys
        /// </summary>
        public List<string> GetAllCacheKeys()
        {
            if (_cache is MemoryCache memoryCache)
            {
                return memoryCache.Keys.Cast<object>().Select(k => k.ToString()).ToList();
            }
            return new List<string>();
        }

        public void ClearAllFeedIdSets()
        {
            // When creating an issue invalidate all filterIdSets in the cache
            List<string> keys = GetAllCacheKeys();
            // all of the keys with "feed-ids" should be invalidated
            foreach (var key in keys)
            {
                if (key.Contains("feed-ids", StringComparison.OrdinalIgnoreCase))
                {
                    _cache.Remove(key);
                }
            }
        }

        public void ClearEntireCache()
        {
            List<string> keys = GetAllCacheKeys();
            foreach (var key in keys)
            {
                _cache.Remove(key);
            }
        }
        public void ClearSubIssueFeedIdsForIssue(Guid issueId)
        {
            List<string> keys = GetAllCacheKeys();
            foreach (var key in keys)
            {
                // Invalidate the cache no matter the filter type, or page number
                if (key.Contains($"{CacheKeyPrefix.SubIssueOfIssueOrSolutionFeedIds}:{issueId}", StringComparison.OrdinalIgnoreCase))
                {
                    _cache.Remove(key);
                }
            }
        }

        public void ClearIssueFeedIdsForUser(Guid userId)
        {
            List<string> keys = GetAllCacheKeys();
            foreach (var key in keys)
            {
                // Invalidate the cache no matter the filter type, or page number
                if (key.Contains($"{CacheKeyPrefix.UserIssuesFeedIds}:{userId}", StringComparison.OrdinalIgnoreCase))
                {
                    _cache.Remove(key);
                }
            }
        }

        public void ClearSubIssueFeedIdsForSolution(Guid solutionId)
        {
            List<string> keys = GetAllCacheKeys();
            foreach (var key in keys)
            {
                // Invalidate the cache no matter the filter type, or page number
                if (key.Contains($"{CacheKeyPrefix.SubIssueOfIssueOrSolutionFeedIds}:{solutionId}", StringComparison.OrdinalIgnoreCase))
                {
                    _cache.Remove(key);
                }
            }
        }
        public void ClearSolutionFeedIdsForIssue(Guid issueId)
        {
            List<string> keys = GetAllCacheKeys();
            foreach (var key in keys)
            {
                // Invalidate the cache no matter the filter type, or page number
                if (key.Contains($"{CacheKeyPrefix.SolutionsOfIssueFeedIds}:{issueId}", StringComparison.OrdinalIgnoreCase))
                {
                    _cache.Remove(key);
                }
            }
        }
        public void ClearSolutionFeedIdsForUser(Guid userId)
        {
            List<string> keys = GetAllCacheKeys();
            foreach (var key in keys)
            {
                // Invalidate the cache no matter the filter type, or page number
                if (key.Contains($"{CacheKeyPrefix.UserSolutionsFeedIds}:{userId}", StringComparison.OrdinalIgnoreCase))
                {
                    _cache.Remove(key);
                }
            }
        }
        public void ClearContentCountSubIssuesForIssue(Guid issueId)
        {
            List<string> keys = GetAllCacheKeys();
            foreach (var key in keys)
            {
                // Invalidate the cache no matter the filter type, or page number
                if (key.Contains($"{CacheKeyPrefix.SubIssueForIssueContentCount}:{issueId}", StringComparison.OrdinalIgnoreCase))
                {
                    _cache.Remove(key);
                }
            }
        }

        public void ClearContentCountIssuesForUser(Guid userId)
        {
            List<string> keys = GetAllCacheKeys();
            foreach (var key in keys)
            {
                // Invalidate the cache no matter the filter type, or page number
                if (key.Contains($"{CacheKeyPrefix.UserIssuesContentCount}:{userId}", StringComparison.OrdinalIgnoreCase))
                {
                    _cache.Remove(key);
                }
            }
        }
        public void ClearContentCountSolutionsForUser(Guid userId)
        {
            List<string> keys = GetAllCacheKeys();
            foreach (var key in keys)
            {
                // Invalidate the cache no matter the filter type, or page number
                if (key.Contains($"{CacheKeyPrefix.UserSolutionsContentCount}:{userId}", StringComparison.OrdinalIgnoreCase))
                {
                    _cache.Remove(key);
                }
            }
        }
        public void ClearContentCountSubIssuesForSolution(Guid solutionId)
        {
            List<string> keys = GetAllCacheKeys();
            foreach (var key in keys)
            {
                // Invalidate the cache no matter the filter type, or page number
                if (key.Contains($"{CacheKeyPrefix.SubIssueForIssueContentCount}:{solutionId}", StringComparison.OrdinalIgnoreCase))
                {
                    _cache.Remove(key);
                }
            }
        }
        public void ClearContentCountSolutionsForIssue(Guid issueId)
        {
            List<string> keys = GetAllCacheKeys();
            foreach (var key in keys)
            {
                // Invalidate the cache no matter the filter type, or page number
                if (key.Contains($"{CacheKeyPrefix.SolutionForIssueContentCount}:{issueId}", StringComparison.OrdinalIgnoreCase))
                {
                    _cache.Remove(key);
                }
            }
        }

        public void ClearMainPageFeedIds()
        {
            List<string> keys = GetAllCacheKeys();
            foreach (var key in keys)
            {
                // Invalidate the cache no matter the filter type, or page number
                if (key.Contains($"{CacheKeyPrefix.MainContentFeedIds}", StringComparison.OrdinalIgnoreCase))
                {
                    _cache.Remove(key);
                }
            }
        }

        public void ClearContentCountForMainPage()
        {
            List<string> keys = GetAllCacheKeys();
            foreach (var key in keys)
            {
                // Invalidate the cache no matter the filter type, or page number
                if (key.Contains($"{CacheKeyPrefix.MainPageContentCount}", StringComparison.OrdinalIgnoreCase))
                {
                    _cache.Remove(key);
                }
            }
        }

        public void ClearIssueContentVersionHistoryCache(Guid issueId)
        {
            List<string> keys = GetAllCacheKeys();
            foreach (var key in keys)
            {
                // Invalidate the cache no matter the filter type, or page number
                if (key.Contains($"{CacheKeyPrefix.IssueVersionHistory}:{issueId}", StringComparison.OrdinalIgnoreCase))
                {
                    _cache.Remove(key);
                }
            }
        }

        public void ClearSolutionContentVersionHistoryCache(Guid solutionId)
        {
            List<string> keys = GetAllCacheKeys();
            foreach (var key in keys)
            {
                // Invalidate the cache no matter the filter type, or page number
                if (key.Contains($"{CacheKeyPrefix.SolutionVersionHistory}:{solutionId}", StringComparison.OrdinalIgnoreCase))
                {
                    _cache.Remove(key);
                }
            }
        }

        public void ClearUserHistoryCache(Guid userId)
        {
            List<string> keys = GetAllCacheKeys();
            foreach (var key in keys)
            {
                // Invalidate the cache no matter the filter type, or page number
                if (key.Contains($"{CacheKeyPrefix.UserHistory}:{userId}", StringComparison.OrdinalIgnoreCase))
                {
                    _cache.Remove(key);
                }
            }
        }




        /// <summary>
        /// Updates the "vote-stats" cache <br/>
        /// See <see cref="IssueVotes_Cacheable_ReadVM"/>
        /// </summary>
        public void UpdateCache_IssueVoteStats(Vote_Cacheable? newOrUpdateVote, IssueVote_UpsertVM model, AppUser user)
        {
            if (newOrUpdateVote != null)
            {
                // Get the cache key for this issue's vote stats
                string cacheKey = $"{CacheKeyPrefix.VoteStats}:{model.IssueID}";
                if (_cache.TryGetValue<IssueVotes_Cacheable_ReadVM>(cacheKey, out var cachedStats))
                {
                    // Update the cached stats
                    if (cachedStats != null)
                    {
                        // Update or add the vote in the dictionary
                        cachedStats.IssueVotes[user.Id] = newOrUpdateVote;
                        // This will update the count automatically


                        // Recalculate averages and totals
                        cachedStats.TotalVotes = cachedStats.IssueVotes.Count;// + (isNewVote ? 1 : 0); // <-- If the is truly a new vote then add 1. if it is an update add 0
                        cachedStats.AverageVote = cachedStats.IssueVotes.Any()
                            ? cachedStats.IssueVotes.Values.Average(v => v.VoteValue)
                            : 0;

                        // Update the cache with new expiration
                        var cacheEntryOptions = new MemoryCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
                        };
                        _cache.Set(cacheKey, cachedStats, cacheEntryOptions);
                    }
                }
            }
        }

        /// <summary>
        /// Updates the "vote-stats" cache <br/>
        /// See <see cref="IssueVotes_Cacheable_ReadVM"/>
        /// </summary>
        public void UpdateCache_SolutionVoteStats(Vote_Cacheable? newVote, SolutionVote_UpsertVM model, AppUser user)
        {

            if (newVote != null)
            {
                // Get the cache key for this issue's vote stats
                string cacheKey = $"{CacheKeyPrefix.VoteStats}:{model.SolutionID}";

                if (_cache.TryGetValue<SolutionVotes_Cacheable_ReadVM>(cacheKey, out var cachedStats))
                {
                    // Update the cached stats
                    if (cachedStats != null)
                    {
                        // Update or add the vote in the dictionary
                        cachedStats.SolutionVotes[user.Id] = newVote;

                        // Recalculate averages and totals
                        cachedStats.TotalVotes = cachedStats.SolutionVotes.Count;
                        cachedStats.AverageVote = cachedStats.SolutionVotes.Any()
                            ? cachedStats.SolutionVotes.Values.Average(v => v.VoteValue)
                            : 0;

                        // Update the cache with new expiration
                        var cacheEntryOptions = new MemoryCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
                        };

                        _cache.Set(cacheKey, cachedStats, cacheEntryOptions);
                    }
                }

            }

        }
    }

}
