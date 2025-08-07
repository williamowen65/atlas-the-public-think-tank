using Microsoft.Extensions.Caching.Memory;
using repository_pattern_experiment.Data.RepositoryPattern.IRepository;
using repository_pattern_experiment.Models.ViewModel;

namespace repository_pattern_experiment.Data.RepositoryPattern.Cache
{
    public class AppUserCacheRepository : IAppUserRepository
    {
        private readonly IAppUserRepository _inner;
        private readonly IMemoryCache _cache;
        public AppUserCacheRepository(IAppUserRepository inner, IMemoryCache cache)
        {
            _cache = cache;
            _inner = inner;
        }

        public async Task<AppUser_ReadVM?> GetAppUser(Guid UserId)
        {
            return await _cache.GetOrCreateAsync($"app-user:{UserId}", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                return await _inner.GetAppUser(UserId);
            });
        }
    }
}
