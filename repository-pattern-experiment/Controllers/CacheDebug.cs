using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
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

        [Route("api/keys")]
        public IActionResult GetKeys()
        {
            return Json(GetAllCacheKeys());
        }

        [Route("api/entries")]
        public IActionResult GetEntries()
        {
            var cacheEntries = GetAllCacheKeys();
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

        /// <summary>
        /// For debugging: Get all cached keys
        /// </summary>
        private List<string> GetAllCacheKeys()
        {
            if (_cache is MemoryCache memoryCache)
            {
                return memoryCache.Keys.Cast<object>().Select(k => k.ToString()).ToList();
            }
            return new List<string>();
        }

        // Model for view
        public class CacheItem
        {
            public string Key { get; set; }
            public string Type { get; set; }
            public object Value { get; set; } // Changed to object to maintain structure
        }
    }
}