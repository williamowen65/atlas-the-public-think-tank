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
        private readonly IConfiguration _configuration;
        public AppUserCacheRepository(IAppUserRepository inner, IMemoryCache cache, ILoggerFactory loggerFactory, IConfiguration configuration)
        {
            _cache = cache;
            _inner = inner;
            _cacheLogger = loggerFactory.CreateLogger("CacheLog");
            _configuration = configuration;
        }

        public async Task<AppUser_ReadVM?> GetAppUser(Guid UserId)
        {

            if (_configuration.GetValue<bool>("Caching:Enabled") == false)
            {
                _cacheLogger.LogInformation($"[~] Cache skip for AppUser {UserId}");
                return await _inner.GetAppUser(UserId);
            }

            var cacheKey = $"app-user:{UserId}";

            if (_cache.TryGetValue(cacheKey, out AppUser_ReadVM? cachedAppUser))
            {
                _cacheLogger.LogInformation($"[+] Cache hit for AppUser {UserId}");
                return cachedAppUser;
            }
            else
            { 
                _cacheLogger.LogInformation($"[!] Cache miss for GetAppUser {UserId}");
                return await _cache.GetOrCreateAsync(cacheKey, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                    return await _inner.GetAppUser(UserId);
                });
            }
        }
    }
}
