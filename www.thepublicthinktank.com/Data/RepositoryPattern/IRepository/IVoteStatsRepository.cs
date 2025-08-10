using atlas_the_public_think_tank.Models.ViewModel;

namespace atlas_the_public_think_tank.Data.RepositoryPattern.IRepository
{

    public interface IVoteStatsRepository
    {

        Task<UserVote_Issue_ReadVM?> GetIssueVoteStats(Guid id);
        Task<UserVote_Solution_ReadVM?> GetSolutionVoteStats(Guid id);

       
    }
}
