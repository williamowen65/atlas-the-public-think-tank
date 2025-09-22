
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Issue;
using atlas_the_public_think_tank.Data.RepositoryPattern.Cache.Helpers;
using atlas_the_public_think_tank.Data.RepositoryPattern.IRepository;
using atlas_the_public_think_tank.Models.ViewModel;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.ContentItem_Common;
using atlas_the_public_think_tank.Models.ViewModel.CRUD_VM.ContentItem_Common;
using Microsoft.Extensions.Caching.Memory;

namespace atlas_the_public_think_tank.Data.RepositoryPattern.Cache
{
    public class BreadcrumbCacheRepository : IBreadcrumbRepository
    {

        private readonly IBreadcrumbRepository _inner;
        private readonly IMemoryCache _cache;
        private readonly ILogger _cacheLogger;
        private readonly IConfiguration _configuration;
        public BreadcrumbCacheRepository(IBreadcrumbRepository inner, IMemoryCache cache, ILoggerFactory loggerFactory, IConfiguration configuration)
        {
            _cache = cache;
            _inner = inner;
            _cacheLogger = loggerFactory.CreateLogger("CacheLog");
            _configuration = configuration;
        }

        /// <summary>
        /// TODO: Create tests case <br/>
        /// What if a parent issue/solution title is updated <br/>
        /// What if a parent issue/solution is moved to point to a different parent.
        /// </summary>
        public async Task<List<Breadcrumb_ReadVM>> GetBreadcrumbPagedAsync(Guid? itemId)
        {
            if (itemId == null) {
                return await _inner.GetBreadcrumbPagedAsync(null);
            }

            if (_configuration.GetValue<bool>("Caching:Enabled") == false)
            {
                _cacheLogger.LogInformation($"[~] Cache skip for GetBreadcrumbPagedAsync {itemId}");
                return await _inner.GetBreadcrumbPagedAsync(itemId);
            }

            var cacheKey = $"{CacheKeyPrefix.Breadcrumb}:{itemId}";
            if (_cache.TryGetValue(cacheKey, out List<Breadcrumb_ReadVM>? cachedBreadcrumbList))
            {
                _cacheLogger.LogInformation($"[+] Cache hit for GetBreadcrumbPagedAsync {itemId}");
                return cachedBreadcrumbList!;
            }
            else
            {
                _cacheLogger.LogWarning($"[!] Cache miss for GetBreadcrumbPagedAsync {itemId}");
                #pragma warning disable CS8603 // Possible null reference return.
                return await _cache.GetOrCreateAsync(cacheKey, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                    return await _inner.GetBreadcrumbPagedAsync(itemId);
                });
                #pragma warning restore CS8603 // Possible null reference return.

            }
        }
    }
}


