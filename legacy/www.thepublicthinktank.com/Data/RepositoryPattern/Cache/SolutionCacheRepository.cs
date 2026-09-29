using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Issue;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Solution;
using atlas_the_public_think_tank.Data.RepositoryPattern.Cache.Helpers;
using atlas_the_public_think_tank.Data.RepositoryPattern.IRepository;
using atlas_the_public_think_tank.Data.RepositoryPattern.Repository.Helpers;
using atlas_the_public_think_tank.Models.ViewModel;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.ContentItem_Common;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Issue;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Solution;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using repository_pattern_experiment.Controllers;

namespace atlas_the_public_think_tank.Data.RepositoryPattern.Cache
{
    public class SolutionCacheRepository : ISolutionRepository
    {

        private readonly ISolutionRepository _inner;
        private readonly IMemoryCache _cache;
        private readonly ILogger _cacheLogger;
        private readonly IFilterIdSetRepository _filterIdSetRepository;
        private readonly IConfiguration _configuration;
        private readonly CacheHelper _cacheHelper;

        public SolutionCacheRepository(ISolutionRepository inner, IMemoryCache cache, ILoggerFactory loggerFactory, IFilterIdSetRepository filterIdSetRepository, IConfiguration configuration, CacheHelper cacheHelper)
        {
            _cache = cache;
            _inner = inner;
            _cacheLogger = loggerFactory.CreateLogger("CacheLog");
            _filterIdSetRepository = filterIdSetRepository;
            _configuration = configuration;
            _cacheHelper = cacheHelper;
        }

        /// <summary>
        /// Caching layer for "reading a solution" from the cache.
        /// </summary>
        /// <remarks>
        /// However, a full solution read is performed with
        /// <see cref="CRUD.Read.Solution(Guid, ContentFilter, bool)"/> which pulls from multiple parts of the cache.
        /// </remarks>
        public async Task<SolutionRepositoryViewModel?> GetSolutionById(Guid id)
        {
            if (_configuration.GetValue<bool>("Caching:Enabled") == false)
            {
                _cacheLogger.LogInformation("[~] Cache skip for solution {SolutionId}", id);
                return await _inner.GetSolutionById(id);
            }

            var cacheKey = $"{CacheKeyPrefix.Solution}:{id}";
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

        /// <summary>
        /// Cache layer for "creating a new solution"
        /// </summary>
        /// <remarks>
        /// Clears related cache entities: <br/>
        /// Solutions only belong to issues. Adding a solution should update the solution
        /// feed for that issue, and the counts of solutions for that issue
        /// </remarks>
        public async Task<Solution_ReadVM> AddSolutionAsync(Solution solution)
        {
            // TODO Improve cache validation on a per ContentStatus basis (draft, published, etc)
            #region cache invalidation

            _cacheHelper.ClearSolutionFeedIdsForIssue((Guid)solution.ParentIssueID);
            _cacheHelper.ClearContentCountSolutionsForIssue((Guid)solution.ParentIssueID);

            // Main page
            _cacheHelper.ClearContentCountForMainPage();
            _cacheHelper.ClearMainPageFeedIds();

            // User profile page
            _cacheHelper.ClearSolutionFeedIdsForUser(solution.AuthorID);
            _cacheHelper.ClearContentCountSolutionsForUser(solution.AuthorID);

            #endregion

            return await _inner.AddSolutionAsync(solution);
        }


        /// <summary>
        /// Cache layer for "Updating a solution"
        /// </summary>
        /// <remarks>
        /// Directly updates the cache, preventing a database read
        /// </remarks>
        public async Task<Solution_ReadVM> UpdateSolutionAsync(Solution solution)
        {
            if (_configuration.GetValue<bool>("Caching:Enabled") == false)
            {
                _cacheLogger.LogInformation($"[~] Cache skip for updating solution {solution.SolutionID}");
                return await _inner.UpdateSolutionAsync(solution);
            }

            #region cache invalidation

            var cacheKey = $"{CacheKeyPrefix.Solution}:{solution.SolutionID}";

            // Convert solution to SolutionRepositoryViewModel
            SolutionRepositoryViewModel cacheableSolution = Converter.ConvertSolutionToSolutionRepositoryViewModel(solution);


            // TODO Move this to cache helper
            // Update Cache
            _cache.Set(cacheKey, cacheableSolution, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            });


            // Content Version History
            // Note: This method may be better suited to be Update instead of clear.
            _cacheHelper.ClearSolutionContentVersionHistoryCache(solution.SolutionID);

            // User Profile Page
            //If the user updates a post from Draft to Published the feed should be updated
            _cacheHelper.ClearSolutionFeedIdsForUser(solution.AuthorID);
            _cacheHelper.ClearContentCountSolutionsForUser(solution.AuthorID);

            // Main Page
            _cacheHelper.ClearContentCountForMainPage();
            _cacheHelper.ClearMainPageFeedIds();

            // Read Issue Page
            _cacheHelper.ClearSubIssueFeedIdsForIssue((Guid)solution.ParentIssueID);
            _cacheHelper.ClearContentCountSubIssuesForIssue((Guid)solution.ParentIssueID);

            #endregion

            return await _inner.UpdateSolutionAsync(solution);

        }

        public async Task<int> GetSolutionVersionHistoryCount(Guid solutionID)
        {
            _cacheLogger.LogWarning($"[!] Cache miss for solution VersionHistoryCount {solutionID}");
            return await _inner.GetSolutionVersionHistoryCount(solutionID);
        }


        /// <summary>
        /// Cache layer for "The version history of a solution"
        /// </summary>
        /// <remarks>
        /// Cache should be updated/invalidated by: <br/>
        /// <see cref="SolutionCacheRepository.UpdateSolutionAsync(Solution)"/>
        /// </remarks>
        public async Task<List<ContentItem_ReadVM>?> GetSolutionVersionHistoryBySolutionVM(Solution_ReadVM solution)
        {

            if (_configuration.GetValue<bool>("Caching:Enabled") == false)
            {
                _cacheLogger.LogInformation($"[~] Cache skip for solution VersionHistory {solution.SolutionID}");
                return await _inner.GetSolutionVersionHistoryBySolutionVM(solution);
            }

            var cacheKey = $"{CacheKeyPrefix.SolutionVersionHistory}:{solution.SolutionID}";
            if (_cache.TryGetValue(cacheKey, out List<ContentItem_ReadVM>? cachedIssueVersionHistory))
            {
                _cacheLogger.LogInformation($"[+] Cache hit for solution VersionHistory {solution.SolutionID}");
                return cachedIssueVersionHistory;
            }
            else
            {
                _cacheLogger.LogWarning($"[!] Cache miss for solution VersionHistory {solution.SolutionID}");
                return await _cache.GetOrCreateAsync(cacheKey, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                    return await _inner.GetSolutionVersionHistoryBySolutionVM(solution);
                });
            }
        }
    }
}
