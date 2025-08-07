using Microsoft.Extensions.Caching.Memory;
using repository_pattern_experiment.Data.RepositoryPattern.IRepository;
using repository_pattern_experiment.Models.ViewModel;

namespace repository_pattern_experiment.Data.RepositoryPattern.Cache
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


        /// <summary>
        /// Calls base implantation (Caching thie value might not be worth it)
        /// </summary>
        public async Task<int?> GetActiveUserIssueVote(Guid id)
        {
            return await _inner.GetActiveUserIssueVote(id);
        }

        public async Task<UserVote_Issue_ReadVM?> GetIssueVoteStats(Guid id)
        {

            return await _cache.GetOrCreateAsync($"vote-stats:{id}", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                return await _inner.GetIssueVoteStats(id);
            });
        }


    }
}
