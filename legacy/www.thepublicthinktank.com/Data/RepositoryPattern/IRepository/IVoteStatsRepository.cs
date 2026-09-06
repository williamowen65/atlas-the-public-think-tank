using atlas_the_public_think_tank.Data.DatabaseEntities.Users;
using atlas_the_public_think_tank.Models.Cacheable;
using atlas_the_public_think_tank.Models.ViewModel;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Issue.IssueVote;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Solution.SolutionVote;
using atlas_the_public_think_tank.Models.ViewModel.CRUD_VM.Issue.IssueVote;
using atlas_the_public_think_tank.Models.ViewModel.CRUD_VM.Solution.SolutionVote;

namespace atlas_the_public_think_tank.Data.RepositoryPattern.IRepository
{

    public interface IVoteStatsRepository
    {

        Task<IssueVotes_Cacheable_ReadVM?> GetIssueVoteStats(Guid id);
        Task<SolutionVotes_Cacheable_ReadVM?> GetSolutionVoteStats(Guid id);


        /// <summary>
        /// This Upsert creates or update issue votes
        /// </summary>
        /// <remarks>
        /// This must trigger an update to GetIssueVoteStats cache (invalidate or update)
        /// </remarks>
        Task<Vote_Cacheable?> UpsertIssueVote(IssueVote_UpsertVM model, AppUser user);
        Task<Vote_Cacheable?> UpsertSolutionVote(SolutionVote_UpsertVM model, AppUser user);
    }
}
