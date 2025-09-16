using atlas_the_public_think_tank.Data.CRUD;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Issue;
using atlas_the_public_think_tank.Data.DatabaseEntities.Users;
using atlas_the_public_think_tank.Data.RepositoryPattern.IRepository;
using atlas_the_public_think_tank.Data.RepositoryPattern.Repository.Helpers;
using atlas_the_public_think_tank.Models.Cacheable;
using atlas_the_public_think_tank.Models.ViewModel;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.ContentItem_Common;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Issue;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Issue.IssueVote;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Solution;
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
        private readonly IConfiguration _configuration;
        public VoteStatsCacheRepository(IVoteStatsRepository inner, IMemoryCache cache, ILoggerFactory loggerFactory, IConfiguration configuration)
        {
            _cache = cache;
            _inner = inner;
            _cacheLogger = loggerFactory.CreateLogger("CacheLog");
            _configuration = configuration;
        }



        /// <summary>
        /// Cache layer for "Issue Vote Stats"
        /// </summary>
        /// <remarks>
        /// Votes are updated in the cache via:
        /// <see cref="VoteStatsCacheRepository.UpsertIssueVote(IssueVote_UpsertVM, AppUser)"/>
        /// </remarks>
        public async Task<IssueVotes_Cacheable_ReadVM?> GetIssueVoteStats(Guid id)
        {
            if (_configuration.GetValue<bool>("Caching:Enabled") == false)
            {
                _cacheLogger.LogInformation($"[~] Cache skip for GetIssueVoteStats {id}");
                return await _inner.GetIssueVoteStats(id);
            }

            var cacheKey = $"vote-stats:{id}";
            if (_cache.TryGetValue(cacheKey, out IssueVotes_Cacheable_ReadVM? cachedIssueVoteStats))
            {
                _cacheLogger.LogInformation($"[+] Cache hit for GetIssueVoteStats {id}");
                return cachedIssueVoteStats;
            }
            else
            {
                _cacheLogger.LogInformation($"[!] Cache miss for GetIssueVoteStats {id}");
                return await _cache.GetOrCreateAsync(cacheKey, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                    return await _inner.GetIssueVoteStats(id);
                });
            }
        }
        public async Task<SolutionVotes_Cacheable_ReadVM?> GetSolutionVoteStats(Guid id)
        {

            if (_configuration.GetValue<bool>("Caching:Enabled") == false)
            {
                _cacheLogger.LogInformation($"[~] Cache skip for GetSolutionVoteStats( {id}");
                return await _inner.GetSolutionVoteStats(id);
            }

            var cacheKey = $"vote-stats:{id}";
            if (_cache.TryGetValue(cacheKey, out SolutionVotes_Cacheable_ReadVM? cachedSolutionVoteStats))
            {
                _cacheLogger.LogInformation($"[+] Cache hit for GetSolutionVoteStats( {id}");
                return cachedSolutionVoteStats;
            }
            else
            {
                _cacheLogger.LogInformation($"[!] Cache miss for GetSolutionVoteStats( {id}");
                return await _cache.GetOrCreateAsync(cacheKey, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                    return await _inner.GetSolutionVoteStats(id);
                });
            }

        }


        /// <summary>
        /// Cache layer for "Update or inserts an issue vote"
        /// </summary>
        /// <remarks>
        /// Clears dependent caches: <br/>
        /// The voted issue may be a sub-issue of another issue, or a sub-issue of a solution. <br/>
        /// Content feed sorting needs to be updated <br/>
        /// Content counts need to be recalculated based on content filters <br/><br/>
        /// Note: <see cref="Vote_Cacheable"/> is stored in the cache as part of <see cref="IssueVotes_Cacheable_ReadVM"/>
        /// </remarks>
        public async Task<Vote_Cacheable?> UpsertIssueVote(IssueVote_UpsertVM model, AppUser user)
        {
            // First, call the inner repository to update the database
            var result = await _inner.UpsertIssueVote(model, user);

            // Update the vote stats in the cache
            CacheHelper.UpdateCache_IssueVoteStats(result, model, user);

            Issue_ReadVM? issue = await Read.Issue(model.IssueID, new ContentFilter());

            if (issue!.ParentIssueID != null) {
                CacheHelper.ClearSubIssueFeedIdsForIssue((Guid)issue.ParentIssueID!);
                CacheHelper.ClearContentCountSubIssuesForIssue((Guid)issue.ParentIssueID);
            }
            if (issue!.ParentSolutionID != null)
            { 
                CacheHelper.ClearSubIssueFeedIdsForSolution((Guid)issue.ParentSolutionID!);
                CacheHelper.ClearContentCountSubIssuesForSolution((Guid)issue.ParentSolutionID!);
            }

            return result;
        }

        /// <summary>
        /// Cache layer for "Update or inserts a solution vote"
        /// </summary>
        /// <remarks>
        /// Clears dependent caches: <br/>
        /// The voted solution would have a parent issue<br/>
        /// Content feed sorting needs to be invalidated </br>
        /// Content counts need to be recalculated based on content filters 
        /// </remarks>
        public async Task<Vote_Cacheable?> UpsertSolutionVote(SolutionVote_UpsertVM model, AppUser user)
        {
            // First, call the inner repository to update the database
            var result = await _inner.UpsertSolutionVote(model, user);

            // Update the vote stats in the cache
            CacheHelper.UpdateCache_SolutionVoteStats(result, model, user);

            Solution_ReadVM? solution = await Read.Solution(model.SolutionID, new ContentFilter());

            CacheHelper.ClearSolutionFeedIdsForIssue((Guid)solution.ParentIssueID!);
            CacheHelper.ClearContentCountSolutionsForIssue((Guid)solution.ParentIssueID);

            return result;
        }
    }
}
