using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Solution;
using atlas_the_public_think_tank.Data.RepositoryPattern.IRepository;
using atlas_the_public_think_tank.Data.RepositoryPattern.Repository.Helpers;
using atlas_the_public_think_tank.Models.ViewModel;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Solution;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace atlas_the_public_think_tank.Data.RepositoryPattern.Cache
{
    public class SolutionCacheRepository : ISolutionRepository
    {

        private readonly ISolutionRepository _inner;
        private readonly IMemoryCache _cache;
        private readonly ILogger _cacheLogger;
        private readonly IFilterIdSetRepository _filterIdSetRepository;
        private readonly IConfiguration _configuration;

        public SolutionCacheRepository(ISolutionRepository inner, IMemoryCache cache, ILoggerFactory loggerFactory, IFilterIdSetRepository filterIdSetRepository, IConfiguration configuration)
        {
            _cache = cache;
            _inner = inner;
            _cacheLogger = loggerFactory.CreateLogger("CacheLog");
            _filterIdSetRepository = filterIdSetRepository;
            _configuration = configuration;
        }

        public async Task<SolutionRepositoryViewModel?> GetSolutionById(Guid id)
        {

            if (_configuration.GetValue<bool>("Caching:Enabled") == false)
            {
                _cacheLogger.LogInformation("[~] Cache skip for solution {SolutionId}", id);
                return await _inner.GetSolutionById(id);
            }

            var cacheKey = $"solution:{id}";
            if (_cache.TryGetValue(cacheKey, out SolutionRepositoryViewModel? cachedSolution))
            {
                _cacheLogger.LogInformation("[+] Cache hit for solution {SolutionId}", id);
                return cachedSolution;
            }
            else
            {
                _cacheLogger.LogInformation("[!] Cache miss for solution {SolutionId}", id);
                var solution = await _cache.GetOrCreateAsync(cacheKey, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                    return await _inner.GetSolutionById(id);
                });
                return solution;
            }
        }


        public async Task<Solution_ReadVM> AddSolutionAsync(Solution solution)
        {
            // When creating an issue invalidate all filterIdSets in the cache
            //CacheHelper.ClearAllFeedIdSets();

            return await _inner.AddSolutionAsync(solution);
        }

        public async Task<Solution_ReadVM> UpdateSolutionAsync(Solution solution)
        {
            if (_configuration.GetValue<bool>("Caching:Enabled") == false)
            {
                _cacheLogger.LogInformation($"[~] Cache skip for updating solution {solution.SolutionID}");
                return await _inner.UpdateSolutionAsync(solution);
            }

            var cacheKey = $"solution:{solution.SolutionID}";

            // Convert solution to SolutionRepositoryViewModel
            SolutionRepositoryViewModel cacheableSolution = Converter.ConvertSolutionToSolutionRepositoryViewModel(solution);

            // Update Cache
            _cache.Set(cacheKey, cacheableSolution, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            });

           return await _inner.UpdateSolutionAsync(solution);

        }

        public async Task<int> GetSolutionVersionHistoryCount(Guid solutionID)
        {
            _cacheLogger.LogInformation($"[!] Cache miss for solution VersionHistoryCount {solutionID}");
            return await _inner.GetSolutionVersionHistoryCount(solutionID);
        }
    }
}
