using repository_pattern_experiment.Models.ViewModel;

namespace repository_pattern_experiment.Data.RepositoryPattern.IRepository
{

    public interface IVoteStatsRepository
    {

        //Task<UserVote_Generic_Cacheable_ReadVM?> getContentVoteStats(Guid id);
        Task<UserVote_Issue_ReadVM?> GetIssueVoteStats(Guid id);

        Task<int?> GetActiveUserIssueVote(Guid id);
    }
}
