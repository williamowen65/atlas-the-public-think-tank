using Microsoft.Extensions.Caching.Memory;
using repository_pattern_experiment.Data.RepositoryPattern.IRepository;
using repository_pattern_experiment.Models.Database;

namespace repository_pattern_experiment.Data.RepositoryPattern.Cache
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
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                return await _inner.GetSolutionById(id);
            });
        }



        public Task AddSolutionAsync(Solution solution, Guid parentIssueId)
        {
            throw new NotImplementedException();
        }

    }
}
