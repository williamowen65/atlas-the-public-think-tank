using Microsoft.Extensions.Caching.Memory;
using atlas_the_public_think_tank.Data.RepositoryPattern.IRepository;
using atlas_the_public_think_tank.Models.ViewModel;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.User;

namespace atlas_the_public_think_tank.Data.RepositoryPattern.Cache
{
    public class AppUserCacheRepository : IAppUserRepository
    {
        private readonly IAppUserRepository _inner;
        private readonly IMemoryCache _cache;
        private readonly ILogger _cacheLogger;
        public AppUserCacheRepository(IAppUserRepository inner, IMemoryCache cache, ILoggerFactory loggerFactory)
        {
            _cache = cache;
            _inner = inner;
            _cacheLogger = loggerFactory.CreateLogger("CacheLog");
        }

        public async Task<AppUser_ReadVM?> GetAppUser(Guid UserId)
        {
            //return await _cache.GetOrCreateAsync($"app-user:{UserId}", async entry =>
            //{
            //    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            //    return await _inner.GetAppUser(UserId);
            //});                     
            _cacheLogger.LogInformation($"[!] Cache miss for GetAppUser {UserId}");
            return await _inner.GetAppUser(UserId);
        }
    }
}
