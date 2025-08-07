
using Microsoft.Extensions.Caching.Memory;
using repository_pattern_experiment.Data.RepositoryPattern.IRepository;
using repository_pattern_experiment.Models.ViewModel;

namespace repository_pattern_experiment.Data.RepositoryPattern.Cache
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
        public async Task<List<Breadcrumb_ReadVM>> GetBreadcrumbPagedAsync(Guid itemId)
        {
            return await _cache.GetOrCreateAsync($"breadcrumb:{itemId}", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                return await _inner.GetBreadcrumbPagedAsync(itemId);
            });
        }
    }
}


