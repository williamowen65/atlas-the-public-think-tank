using repository_pattern_experiment.Models.ViewModel;

namespace repository_pattern_experiment.Data.RepositoryPattern.IRepository
{

    /// <summary>
    /// This repository is meant for getting a list of IDs for a feeds
    /// And caching ids per content with filter/sorting overhead
    /// </summary>
    public interface IFilterIdSetRepository
    {

        public Task<List<Guid>?> GetPagedSubIssueIdsOfIssueById(Guid issueId, int pageNumber = 1, int pageSize = 3);
        public Task<int> GetTotalCountSubIssueIdsOfIssueById(Guid issueId);
        public Task<List<Guid>?> GetPagedSolutionIdsOfIssueById(Guid issueId, int pageNumber = 1, int pageSize = 3);
        public Task<List<Guid>?> GetPagedSubIssueIdsOfSolutionById(Guid solutionId, int pageNumber = 1, int pageSize = 3);
    }
}
