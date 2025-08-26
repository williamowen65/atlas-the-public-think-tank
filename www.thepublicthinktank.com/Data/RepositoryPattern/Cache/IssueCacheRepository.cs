using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Issue;
using atlas_the_public_think_tank.Data.RepositoryPattern.IRepository;
using atlas_the_public_think_tank.Models.ViewModel;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Issue;
using Microsoft.Extensions.Caching.Memory;

namespace atlas_the_public_think_tank.Data.RepositoryPattern.Cache
{



    public class IssueCacheRepository : IIssueRepository
    {
        private readonly IIssueRepository _inner;
        private readonly IMemoryCache _cache;
        public IssueCacheRepository(IIssueRepository inner, IMemoryCache cache)
        { 
            _cache = cache;
            _inner = inner;
        }

        public async Task<IssueRepositoryViewModel?> GetIssueById(Guid id)
        {
            //return await _cache.GetOrCreateAsync($"issue:{id}", async entry =>
            //{
            //    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            //});
            return await _inner.GetIssueById(id);
        }

        public async Task<Issue_ReadVM> AddIssueAsync(Issue issue)
        {
            // When creating an issue invalidate all filterIdSets in the cache
            //CacheHelper.ClearAllFeedIdSets();

            return await _inner.AddIssueAsync(issue);
        }

        public async Task<Issue_ReadVM?> UpdateIssueAsync(Issue issue)
        {
            // Invalidate the cache related to this issue
            // issue, and possibly other related ones

            //_cache.Remove($"issue:{issue.IssueID}");

            // Update any nested breadcrumbs



            return  await _inner.UpdateIssueAsync(issue);
        }
    }
}
