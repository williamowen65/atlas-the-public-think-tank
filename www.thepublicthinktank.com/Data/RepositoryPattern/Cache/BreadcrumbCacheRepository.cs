
using Microsoft.Extensions.Caching.Memory;
using atlas_the_public_think_tank.Data.RepositoryPattern.IRepository;
using atlas_the_public_think_tank.Models.ViewModel;
using atlas_the_public_think_tank.Models.ViewModel.CRUD_VM.ContentItem_Common;

namespace atlas_the_public_think_tank.Data.RepositoryPattern.Cache
{
    public class BreadcrumbCacheRepository : IBreadcrumbRepository
    {

        private readonly IBreadcrumbRepository _inner;
        private readonly IMemoryCache _cache;
        private readonly ILogger _cacheLogger;
        public BreadcrumbCacheRepository(IBreadcrumbRepository inner, IMemoryCache cache, ILoggerFactory loggerFactory)
        {
            _cache = cache;
            _inner = inner;
            _cacheLogger = loggerFactory.CreateLogger("CacheLog");
        }
        public async Task<List<Breadcrumb_ReadVM>> GetBreadcrumbPagedAsync(Guid? itemId)
        {
            if (itemId == null) {
                return await _inner.GetBreadcrumbPagedAsync(null);
            }


            // Note: When implementing this, Create tests case
            // What if a parent issue/solution title is updated
            // What if a parent issue/solution is moved to point to a different parent.

            //#pragma warning disable CS8603 // Possible null reference return.
            //return await _cache.GetOrCreateAsync($"breadcrumb:{itemId}", async entry =>
            //{
            //    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            //    return await _inner.GetBreadcrumbPagedAsync(itemId);
            //});
            //#pragma warning restore CS8603 // Possible null reference return.
            _cacheLogger.LogInformation($"[!] Cache miss for GetBreadcrumbPagedAsync {itemId}");
            return await _inner.GetBreadcrumbPagedAsync(itemId);
        }
    }
}


