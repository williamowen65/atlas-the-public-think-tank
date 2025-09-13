using atlas_the_public_think_tank.Data.CRUD;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Issue;
using atlas_the_public_think_tank.Data.DatabaseEntities.Users;
using atlas_the_public_think_tank.Data.RepositoryPattern.IRepository;
using atlas_the_public_think_tank.Data.RepositoryPattern.Repository.Helpers;
using atlas_the_public_think_tank.Models.Cacheable;
using atlas_the_public_think_tank.Models.ViewModel;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Issue;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Issue.IssueVote;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Solution.SolutionVote;
using atlas_the_public_think_tank.Models.ViewModel.CRUD_VM.Issue.IssueVote;
using atlas_the_public_think_tank.Models.ViewModel.CRUD_VM.Solution.SolutionVote;
using Microsoft.Extensions.Caching.Memory;
using repository_pattern_experiment.Controllers;

namespace atlas_the_public_think_tank.Data.RepositoryPattern.Cache
{
    public class VoteStatsCacheRepository : IVoteStatsRepository
    {
        private readonly IVoteStatsRepository _inner;
        private readonly IMemoryCache _cache;
        private readonly ILogger _cacheLogger;

        public VoteStatsCacheRepository(IVoteStatsRepository inner, IMemoryCache cache, ILoggerFactory loggerFactory)
        {
            _cache = cache;
            _inner = inner;
            _cacheLogger = loggerFactory.CreateLogger("CacheLog");
        }




        public async Task<IssueVotes_ReadVM?> GetIssueVoteStats(Guid id)
        {

            //return await _cache.GetOrCreateAsync($"vote-stats:{id}", async entry =>
            //{
            //    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            //});
            _cacheLogger.LogInformation($"[!] Cache miss for GetIssueVoteStats {id}");
            return await _inner.GetIssueVoteStats(id);
        }
        public async Task<SolutionVotes_ReadVM?> GetSolutionVoteStats(Guid id)
        {

            //return await _cache.GetOrCreateAsync($"vote-stats:{id}", async entry =>
            //{
            //    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            //    return await _inner.GetSolutionVoteStats(id);
            //});
            _cacheLogger.LogInformation($"[!] Cache miss for GetIssueVoteStats {id}");
            return await _inner.GetSolutionVoteStats(id);
        }

        public async Task<Vote_Cacheable?> UpsertIssueVote(IssueVote_UpsertVM model, AppUser user)
        {
            // First, call the inner repository to update the database
            var result = await _inner.UpsertIssueVote(model, user);

            #region clear vote-stat cache

            //if (result != null)
            //{
            //    // Get the cache key for this issue's vote stats
            //    string cacheKey = $"vote-stats:{model.IssueID}";

            //    if (_cache.TryGetValue<UserVote_Issue_ReadVM>(cacheKey, out var cachedStats))
            //    {
            //        // Update the cached stats
            //        if (cachedStats != null)
            //        {
            //            // Update or add the vote in the dictionary
            //            cachedStats.IssueVotes[user.Id] = result;

            //            // Recalculate averages and totals
            //            cachedStats.TotalVotes = cachedStats.IssueVotes.Count;
            //            cachedStats.AverageVote = cachedStats.IssueVotes.Any() 
            //                ? cachedStats.IssueVotes.Values.Average(v => v.VoteValue) 
            //                : 0;

            //            // Update the cache with new expiration
            //            var cacheEntryOptions = new MemoryCacheEntryOptions
            //            {
            //                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
            //            };

            //            _cache.Set(cacheKey, cachedStats, cacheEntryOptions);
            //        }
            //    }

            //}
            #endregion

            //TODO Invalidate all paginated filter sets with the filter hash.
            Issue_ReadVM? issue = await Read.Issue(model.IssueID, new ContentFilter());

            if (issue!.ParentIssueID != null) {
                CacheHelper.ClearSubIssueFeedIdsForIssue((Guid)issue.ParentIssueID!);
                CacheHelper.ClearContentCountSubIssuesForIssue((Guid)issue.ParentIssueID);
            }

            return result;
        }

        public async Task<Vote_Cacheable?> UpsertSolutionVote(SolutionVote_UpsertVM model, AppUser user)
        {
            // First, call the inner repository to update the database
            var result = await _inner.UpsertSolutionVote(model, user);

            //if (result != null)
            //{
            //    // Get the cache key for this issue's vote stats
            //    string cacheKey = $"vote-stats:{model.SolutionID}";

            //    if (_cache.TryGetValue<UserVote_Solution_ReadVM>(cacheKey, out var cachedStats))
            //    {
            //        // Update the cached stats
            //        if (cachedStats != null)
            //        {
            //            // Update or add the vote in the dictionary
            //            cachedStats.SolutionVotes[user.Id] = result;

            //            // Recalculate averages and totals
            //            cachedStats.TotalVotes = cachedStats.SolutionVotes.Count;
            //            cachedStats.AverageVote = cachedStats.SolutionVotes.Any()
            //                ? cachedStats.SolutionVotes.Values.Average(v => v.VoteValue)
            //                : 0;

            //            // Update the cache with new expiration
            //            var cacheEntryOptions = new MemoryCacheEntryOptions
            //            {
            //                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
            //            };

            //            _cache.Set(cacheKey, cachedStats, cacheEntryOptions);
            //        }
            //    }

            //}

            //TODO Invalidate all paginated filter sets with the filter hash.

            return result;
        }
    }
}
