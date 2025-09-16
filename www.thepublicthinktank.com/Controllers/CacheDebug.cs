using atlas_the_public_think_tank.Data.DatabaseEntities.Users;
using atlas_the_public_think_tank.Models.Cacheable;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Issue.IssueVote;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Solution.SolutionVote;
using atlas_the_public_think_tank.Models.ViewModel.CRUD_VM.Issue.IssueVote;
using atlas_the_public_think_tank.Models.ViewModel.CRUD_VM.Solution.SolutionVote;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using static repository_pattern_experiment.Controllers.CacheDebug;

namespace repository_pattern_experiment.Controllers
{
    public class CacheDebug : Controller
    {
        private readonly IMemoryCache _cache;

        public CacheDebug(IMemoryCache cache)
        {
            _cache = cache;
        }

        #region Cache Api Routes

        [Route("api/cache-log/keys")]
        public IActionResult GetKeys()
        {
            List<string> CacheKeys = CacheHelper.GetAllCacheKeys();
            CacheKeys.Sort();
            return Json(CacheKeys);
        }

        [Route("api/cache-log/entries")]
        public IActionResult GetEntries()
        {
            var cacheEntries = CacheHelper.GetAllCacheKeys();
            cacheEntries.Sort();
            var cacheItems = new List<CacheItem>();

            foreach (var key in cacheEntries)
            {
                if (_cache.TryGetValue(key, out var value))
                {
                    cacheItems.Add(new CacheItem
                    {
                        Key = key,
                        Type = value?.GetType().FullName,
                        Value = value // Keep the original object
                    });
                }
            }

            return Json(cacheItems, new JsonSerializerOptions
            {
                WriteIndented = true,
                MaxDepth = 10
            });
        }

        [Route("api/cache-log/entry")]
        public IActionResult GetEntry(string key)
        {
            if (_cache.TryGetValue(key, out var value))
            {
                return Json(new CacheItem
                {
                    Key = key,
                    Type = value?.GetType().FullName,
                    Value = value // Keep the original object
                },
                new JsonSerializerOptions
                {
                    WriteIndented = true,
                    MaxDepth = 10
                });
            }
            return Json("Entry Not Found");
        }

        #endregion


        #region Cache Controller Helpers


        // Model for view
        public class CacheItem
        {
            public string Key { get; set; }
            public string Type { get; set; }
            public object Value { get; set; } // Changed to object to maintain structure
        }

        #endregion

    }


    /// <summary>
    /// TODO: Move this to repository helpers
    /// </summary>
    public static class CacheHelper
    {
        private static IMemoryCache _cache;

        public static void Initialize(IMemoryCache cache)
        {
            _cache = cache;
        }

        /// <summary>
        /// For debugging: Get all cached keys
        /// </summary>
        public static List<string> GetAllCacheKeys()
        {
            if (_cache is MemoryCache memoryCache)
            {
                return memoryCache.Keys.Cast<object>().Select(k => k.ToString()).ToList();
            }
            return new List<string>();
        }

        public static void ClearAllFeedIdSets()
        {
            // When creating an issue invalidate all filterIdSets in the cache
            List<string> keys = CacheHelper.GetAllCacheKeys();
            // all of the keys with "feed-ids" should be invalidated
            foreach (var key in keys)
            {
                if (key.Contains("feed-ids", StringComparison.OrdinalIgnoreCase))
                {
                    _cache.Remove(key);
                }
            }
        }

        public static void ClearSubIssueFeedIdsForIssue(Guid issueId)
        {
            List<string> keys = CacheHelper.GetAllCacheKeys();
            foreach (var key in keys)
            {
                // Invlidate the cache no matter the filter type, or page number
                if (key.Contains($"sub-issue-feed-ids:{issueId}", StringComparison.OrdinalIgnoreCase))
                {
                    _cache.Remove(key);
                }
            }
        }
        
        public static void ClearSubIssueFeedIdsForSolution(Guid solutionId)
        {
            List<string> keys = CacheHelper.GetAllCacheKeys();
            foreach (var key in keys)
            {
                // Invlidate the cache no matter the filter type, or page number
                if (key.Contains($"sub-issue-feed-ids:{solutionId}", StringComparison.OrdinalIgnoreCase))
                {
                    _cache.Remove(key);
                }
            }
        }
        public static void ClearSolutionFeedIdsForIssue(Guid issueId)
        {
            List<string> keys = CacheHelper.GetAllCacheKeys();
            foreach (var key in keys)
            {
                // Invlidate the cache no matter the filter type, or page number
                if (key.Contains($"solution-feed-ids:{issueId}", StringComparison.OrdinalIgnoreCase))
                {
                    _cache.Remove(key);
                }
            }
        }
        public static void ClearContentCountSubIssuesForIssue(Guid issueId)
        {
            List<string> keys = CacheHelper.GetAllCacheKeys();
            foreach (var key in keys)
            {
                // Invlidate the cache no matter the filter type, or page number
                if (key.Contains($"sub-issue-content-counts:{issueId}", StringComparison.OrdinalIgnoreCase))
                {
                    _cache.Remove(key);
                }
            }
        }
        public static void ClearContentCountSubIssuesForSolution(Guid solutionId)
        {
            List<string> keys = CacheHelper.GetAllCacheKeys();
            foreach (var key in keys)
            {
                // Invlidate the cache no matter the filter type, or page number
                if (key.Contains($"sub-issue-content-counts:{solutionId}", StringComparison.OrdinalIgnoreCase))
                {
                    _cache.Remove(key);
                }
            }
        }
        public static void ClearContentCountSolutionsForIssue(Guid issueId)
        {
            List<string> keys = CacheHelper.GetAllCacheKeys();
            foreach (var key in keys)
            {
                // Invlidate the cache no matter the filter type, or page number
                if (key.Contains($"solution-content-counts:{issueId}", StringComparison.OrdinalIgnoreCase))
                {
                    _cache.Remove(key);
                }
            }
        }

        public static void ClearIssueContentVersionHistoryCache(Guid issueId)
        {
            List<string> keys = CacheHelper.GetAllCacheKeys();
            foreach (var key in keys)
            {
                // Invlidate the cache no matter the filter type, or page number
                if (key.Contains($"issue-version-history:{issueId}", StringComparison.OrdinalIgnoreCase))
                {
                    _cache.Remove(key);
                }
            }
        }

        public static void ClearSolutionContentVersionHistoryCache(Guid solutionId)
        {
            List<string> keys = CacheHelper.GetAllCacheKeys();
            foreach (var key in keys)
            {
                // Invlidate the cache no matter the filter type, or page number
                if (key.Contains($"solution-version-history:{solutionId}", StringComparison.OrdinalIgnoreCase))
                {
                    _cache.Remove(key);
                }
            }
        }


        /// <summary>
        /// Updates the "vote-stats" cache <br/>
        /// See <see cref="IssueVotes_Cacheable_ReadVM"/>
        /// </summary>
        public static void UpdateCache_IssueVoteStats(Vote_Cacheable? newVote, IssueVote_UpsertVM model, AppUser user)
        {
            if (newVote != null)
            {
                // Get the cache key for this issue's vote stats
                string cacheKey = $"vote-stats:{model.IssueID}";
                if (_cache.TryGetValue<IssueVotes_Cacheable_ReadVM>(cacheKey, out var cachedStats))
                {
                    // Update the cached stats
                    if (cachedStats != null)
                    {
                        // Update or add the vote in the dictionary
                        cachedStats.IssueVotes[user.Id] = newVote;

                        // Recalculate averages and totals
                        cachedStats.TotalVotes = cachedStats.IssueVotes.Count;
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
        public static void UpdateCache_SolutionVoteStats(Vote_Cacheable? newVote, SolutionVote_UpsertVM model, AppUser user)
        {

            if (newVote != null)
            {
                // Get the cache key for this issue's vote stats
                string cacheKey = $"vote-stats:{model.SolutionID}";

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