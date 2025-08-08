using repository_pattern_experiment.Data.RepositoryPattern.Repository.Helpers;
using repository_pattern_experiment.Models.ViewModel;

namespace repository_pattern_experiment.Data.RepositoryPattern.IRepository
{

    /// <summary>
    /// This repository is meant for getting a list of IDs for a feeds
    /// And caching ids per content with filter/sorting overhead
    /// </summary>
    public interface IFilterIdSetRepository
    {

        public Task<List<Guid>?> GetPagedSubIssueIdsOfIssueById(Guid issueId, ContentFilter filter, int pageNumber = 1, int pageSize = 3);
        public Task<int> GetTotalCountSubIssuesOfIssueById(Guid issueId);
        public Task<List<Guid>?> GetPagedSolutionIdsOfIssueById(Guid issueId, ContentFilter filter, int pageNumber = 1, int pageSize = 3);
        public Task<int> GetTotalCountSolutionsOfIssueById(Guid issueId);
        public Task<List<Guid>?> GetPagedSubIssueIdsOfSolutionById(Guid solutionId, ContentFilter filter, int pageNumber = 1, int pageSize = 3);
        public Task<int> GetTotalCountSubIssuesOfSolutionById(Guid solutionId);
    }
}
