using Microsoft.Extensions.Caching.Memory;
using atlas_the_public_think_tank.Data.RepositoryPattern.IRepository;
using atlas_the_public_think_tank.Models.ViewModel;

namespace atlas_the_public_think_tank.Data.RepositoryPattern.Cache
{
    public class VoteStatsCacheRepository : IVoteStatsRepository
    {
        private readonly IVoteStatsRepository _inner;
        private readonly IMemoryCache _cache;

        public VoteStatsCacheRepository(IVoteStatsRepository inner, IMemoryCache cache)
        {
            _cache = cache;
            _inner = inner;
        }


     

        public async Task<UserVote_Issue_ReadVM?> GetIssueVoteStats(Guid id)
        {

            return await _cache.GetOrCreateAsync($"vote-stats:{id}", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                return await _inner.GetIssueVoteStats(id);
            });
        }
        public async Task<UserVote_Solution_ReadVM?> GetSolutionVoteStats(Guid id)
        {

            return await _cache.GetOrCreateAsync($"vote-stats:{id}", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                return await _inner.GetSolutionVoteStats(id);
            });
        }


    }
}
