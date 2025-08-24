using atlas_the_public_think_tank.Data.RepositoryPattern.IRepository;
using atlas_the_public_think_tank.Models.Database;
using atlas_the_public_think_tank.Models.ViewModel;
using Microsoft.Extensions.Caching.Memory;
using repository_pattern_experiment.Controllers;

namespace atlas_the_public_think_tank.Data.RepositoryPattern.Cache
{
    public class SolutionCacheRepository : ISolutionRepository
    {

        private readonly ISolutionRepository _inner;
        private readonly IMemoryCache _cache;
        public SolutionCacheRepository(ISolutionRepository inner, IMemoryCache cache)
        {
            _cache = cache;
            _inner = inner;
        }


        public async Task<SolutionRepositoryViewModel?> GetSolutionById(Guid id)
        {
            return await _cache.GetOrCreateAsync($"solution:{id}", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
                return await _inner.GetSolutionById(id);
            });
        }



        public async Task<Solution_ReadVM> AddSolutionAsync(Solution solution)
        {
            // When creating an issue invalidate all filterIdSets in the cache
            CacheHelper.ClearAllFeedIdSets();

            return await _inner.AddSolutionAsync(solution);
        }

        public async Task<Solution_ReadVM> UpdateSolutionAsync(Solution solution)
        {
            // Invalidate the cache related to this issue
            // issue, and possibly other related ones

            _cache.Remove($"solution:{solution.SolutionID}");

            // Update any nested breadcrumbs

            return await _inner.UpdateSolutionAsync(solution);
        }
    }
}
