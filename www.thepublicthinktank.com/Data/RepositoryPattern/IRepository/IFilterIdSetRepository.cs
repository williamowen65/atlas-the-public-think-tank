using atlas_the_public_think_tank.Data.RepositoryPattern.Repository.Helpers;
using atlas_the_public_think_tank.Models;
using atlas_the_public_think_tank.Models.ViewModel;
using atlas_the_public_think_tank.Models.ViewModel.UI_VM;

namespace atlas_the_public_think_tank.Data.RepositoryPattern.IRepository
{

    /// <summary>
    /// This repository is meant for getting a list of IDs for a feeds
    /// And caching ids per content with filter/sorting overhead
    /// </summary>
    public interface IFilterIdSetRepository
    {
        // ContentCount_ReadVM?
        public Task<List<Guid>?> GetPagedSubIssueIdsOfIssueById(Guid issueId, ContentFilter filter, int pageNumber = 1, int pageSize = 3);
        public Task<ContentCount_VM?> GetContentCountSubIssuesOfIssueById(Guid issueId, ContentFilter filter);
        public Task<List<Guid>?> GetPagedSolutionIdsOfIssueById(Guid issueId, ContentFilter filter, int pageNumber = 1, int pageSize = 3);
        public Task<ContentCount_VM?> GetContentCountSolutionsOfIssueById(Guid issueId, ContentFilter filter);
        public Task<List<Guid>?> GetPagedSubIssueIdsOfSolutionById(Guid solutionId, ContentFilter filter, int pageNumber = 1, int pageSize = 3);
        public Task<ContentCount_VM?> GetContentCountSubIssuesOfSolutionById(Guid solutionId, ContentFilter filter);
        public Task<List<ContentIdentifier>?> GetPagedMainContentFeedIds(ContentFilter filter, int pageNumber = 1, int pageSize = 3);
        public Task<ContentCount_VM?> GetContentCountMainContentFeed(ContentFilter filter);

    }
}
