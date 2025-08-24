using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

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
    }
}