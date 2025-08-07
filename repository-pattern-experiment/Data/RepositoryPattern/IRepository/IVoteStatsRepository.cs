using repository_pattern_experiment.Models.ViewModel;

namespace repository_pattern_experiment.Data.RepositoryPattern.IRepository
{

    public interface IVoteStatsRepository
    {

        Task<UserVote_Issue_ReadVM?> GetIssueVoteStats(Guid id);
        Task<UserVote_Solution_ReadVM?> GetSolutionVoteStats(Guid id);

       
    }
}
