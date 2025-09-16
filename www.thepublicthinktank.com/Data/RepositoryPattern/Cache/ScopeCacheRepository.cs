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

        /*
         
            Scope is cached with related issue or solution entities
            
            Scopes exist as their own entity in the database so they can be versioned separately and 
            and reduce redundancy in the DB
            (This pattern may soon apply to content and title to reduce redundancy in the db. 
            Why store a copy of the content description if only the title was updated?)

            It would probably be a good idea to reflect this in the cache for the same reasons.
            But for right not, the in memory cache stores an issues/solutions scope with the content.
         */

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
