using Microsoft.Extensions.Caching.Memory;
using atlas_the_public_think_tank.Data.RepositoryPattern.IRepository;
using atlas_the_public_think_tank.Models.ViewModel;
using atlas_the_public_think_tank.Models.Database;
using atlas_the_public_think_tank.Models;

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

        public async Task<Vote_Cacheable_ReadVM?> UpsertIssueVote(UserVote_Issue_UpsertVM model, AppUser user)
        {
            // First, call the inner repository to update the database
            var result = await _inner.UpsertIssueVote(model, user);

            if (result != null)
            {
                // Get the cache key for this issue's vote stats
                string cacheKey = $"vote-stats:{model.IssueID}";
                
                if (_cache.TryGetValue<UserVote_Issue_ReadVM>(cacheKey, out var cachedStats))
                {
                    // Update the cached stats
                    if (cachedStats != null)
                    {
                        // Update or add the vote in the dictionary
                        cachedStats.IssueVotes[user.Id] = result;

                        // Recalculate averages and totals
                        cachedStats.TotalVotes = cachedStats.IssueVotes.Count;
                        cachedStats.AverageVote = cachedStats.IssueVotes.Any() 
                            ? cachedStats.IssueVotes.Values.Average(v => v.VoteValue) 
                            : 0;

                        // Update the cache with new expiration
                        var cacheEntryOptions = new MemoryCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                        };

                        _cache.Set(cacheKey, cachedStats, cacheEntryOptions);
                    }
                }
                
            }

            return result;
        }

        public async Task<Vote_Cacheable_ReadVM?> UpsertSolutionVote(UserVote_Solution_UpsertVM model, AppUser user)
        {
            // First, call the inner repository to update the database
            var result = await _inner.UpsertSolutionVote(model, user);

            if (result != null)
            {
                // Get the cache key for this issue's vote stats
                string cacheKey = $"vote-stats:{model.SolutionID}";

                if (_cache.TryGetValue<UserVote_Solution_ReadVM>(cacheKey, out var cachedStats))
                {
                    // Update the cached stats
                    if (cachedStats != null)
                    {
                        // Update or add the vote in the dictionary
                        cachedStats.SolutionVotes[user.Id] = result;

                        // Recalculate averages and totals
                        cachedStats.TotalVotes = cachedStats.SolutionVotes.Count;
                        cachedStats.AverageVote = cachedStats.SolutionVotes.Any()
                            ? cachedStats.SolutionVotes.Values.Average(v => v.VoteValue)
                            : 0;

                        // Update the cache with new expiration
                        var cacheEntryOptions = new MemoryCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                        };

                        _cache.Set(cacheKey, cachedStats, cacheEntryOptions);
                    }
                }

            }

            return result;
        }
    }
}
