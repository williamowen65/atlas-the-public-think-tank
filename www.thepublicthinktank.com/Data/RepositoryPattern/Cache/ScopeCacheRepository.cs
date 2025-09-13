using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;
using atlas_the_public_think_tank.Data.RepositoryPattern.IRepository;
using atlas_the_public_think_tank.Models.ViewModel.CRUD_VM.ContentItem_Common;
using Microsoft.Extensions.Caching.Memory;

namespace atlas_the_public_think_tank.Data.RepositoryPattern.Cache
{
    public class ScopeCacheRepository : IScopeRepository
    {
        private readonly IScopeRepository _inner;
        private readonly IMemoryCache _cache;
        private readonly ILogger _cacheLogger;
        private readonly IConfiguration _configuration;
        public ScopeCacheRepository(IScopeRepository inner, IMemoryCache cache, ILoggerFactory loggerFactory, IConfiguration configuration)
        {
            _cache = cache;
            _inner = inner;
            _cacheLogger = loggerFactory.CreateLogger("CacheLog");
            _configuration = configuration;
        }

        

        public async Task<Scope_ReadVM> AddScopeAsync(Scope scope)
        {
            return await _inner.AddScopeAsync(scope);  
        }

        public async Task<Scope_ReadVM?> UpdateScopeAsync(Scope scope)
        {
            return await _inner.UpdateScopeAsync(scope);
        }
    }
}
