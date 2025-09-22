using atlas_the_public_think_tank.Data.DatabaseEntities.Users;
using atlas_the_public_think_tank.Data.RepositoryPattern.Cache.Helpers;
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
            return Json(CacheKeys); // <--- Json serialized the keys, and confused interpretation by changing + to ascii
            //return Content(string.Join("\n", CacheKeys));
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


}