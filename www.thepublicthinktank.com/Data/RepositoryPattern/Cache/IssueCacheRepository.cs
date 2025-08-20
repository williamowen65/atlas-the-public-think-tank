using atlas_the_public_think_tank.Data.RepositoryPattern.IRepository;
using atlas_the_public_think_tank.Models.Database;
using atlas_the_public_think_tank.Models.ViewModel;
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
            return await _cache.GetOrCreateAsync($"issue:{id}", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                return await _inner.GetIssueById(id);
            });
        }

        public async Task<Issue_ReadVM> AddIssueAsync(Issue issue)
        {
            // Could optionally invalidate cache here
            return await _inner.AddIssueAsync(issue);
        }
    }
}
