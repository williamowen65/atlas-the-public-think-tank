using Microsoft.Extensions.Caching.Memory;
using repository_pattern_experiment.Data.RepositoryPattern.IRepository;
using repository_pattern_experiment.Models.Database;

namespace repository_pattern_experiment.Data.RepositoryPattern.Cache
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

        public Task AddIssueAsync(Issue issue, Guid? parentIssueId, Guid? parentSolutionId)
        {
            // Could optionally invalidate cache here
            return _inner.AddIssueAsync(issue, parentIssueId, parentSolutionId);
        }
    }
}
