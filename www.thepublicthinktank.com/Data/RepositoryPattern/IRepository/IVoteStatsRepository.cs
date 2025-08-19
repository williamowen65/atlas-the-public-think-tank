using atlas_the_public_think_tank.Models;
using atlas_the_public_think_tank.Models.Database;
using atlas_the_public_think_tank.Models.ViewModel;

namespace atlas_the_public_think_tank.Data.RepositoryPattern.IRepository
{

    public interface IVoteStatsRepository
    {

        Task<UserVote_Issue_ReadVM?> GetIssueVoteStats(Guid id);
        Task<UserVote_Solution_ReadVM?> GetSolutionVoteStats(Guid id);


        /// <summary>
        /// This Upsert creates or update issue votes
        /// </summary>
        /// <remarks>
        /// This must trigger an update to GetIssueVoteStats cache (invalidate or update)
        /// </remarks>
        Task<Vote_Cacheable_ReadVM?> UpsertIssueVote(UserVote_Issue_UpsertVM model, AppUser user);
        Task<Vote_Cacheable_ReadVM?> UpsertSolutionVote(UserVote_Solution_UpsertVM model, AppUser user);
    }
}
