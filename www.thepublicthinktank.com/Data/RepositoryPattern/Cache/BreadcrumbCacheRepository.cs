
using Microsoft.Extensions.Caching.Memory;
using atlas_the_public_think_tank.Data.RepositoryPattern.IRepository;
using atlas_the_public_think_tank.Models.ViewModel;

namespace atlas_the_public_think_tank.Data.RepositoryPattern.Cache
{
    public class BreadcrumbCacheRepository : IBreadcrumbRepository
    {

        private readonly IBreadcrumbRepository _inner;
        private readonly IMemoryCache _cache;
        public BreadcrumbCacheRepository(IBreadcrumbRepository inner, IMemoryCache cache)
        {
            _cache = cache;
            _inner = inner;
        }
        public async Task<List<Breadcrumb_ReadVM>> GetBreadcrumbPagedAsync(Guid? itemId)
        {
            if (itemId == null) {
                return await _inner.GetBreadcrumbPagedAsync(null);
            }

            #pragma warning disable CS8603 // Possible null reference return.
            return await _cache.GetOrCreateAsync($"breadcrumb:{itemId}", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                return await _inner.GetBreadcrumbPagedAsync(itemId);
            });
            #pragma warning restore CS8603 // Possible null reference return.
        }
    }
}


