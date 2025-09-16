using atlas_the_public_think_tank.Data.DatabaseEntities.Users;
using atlas_the_public_think_tank.Data.RepositoryPattern.IRepository;
using atlas_the_public_think_tank.Models;
using atlas_the_public_think_tank.Models.Cacheable;
using atlas_the_public_think_tank.Models.ViewModel;
using atlas_the_public_think_tank.Models.ViewModel.AjaxVM;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Issue.IssueVote;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Solution.SolutionVote;
using atlas_the_public_think_tank.Models.ViewModel.CRUD_VM.Issue.IssueVote;
using atlas_the_public_think_tank.Models.ViewModel.CRUD_VM.Solution.SolutionVote;

namespace atlas_the_public_think_tank.Data.CRUD
{
    public static class Upsert
    {
        private static IServiceProvider? _serviceProvider;

        // Initialize method to be called at startup
        public static void Initialize(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }


        public static async Task<VoteResponse_AjaxVM?> IssueVote(IssueVote_UpsertVM model, AppUser user)
        {
            if (_serviceProvider == null)
            { 
                throw new InvalidOperationException("Upsert class has not been initialized with a service provider.");
            }

            // Create the response object
            VoteResponse_AjaxVM voteResponse = new VoteResponse_AjaxVM();
            
            // Create a scope to resolve scoped services
            using var scope = _serviceProvider.CreateScope();
            var services = scope.ServiceProvider;

            var voteStatsRepository = services.GetRequiredService<IVoteStatsRepository>();

           
            try
            {
                Vote_Cacheable? issueVote = await voteStatsRepository.UpsertIssueVote(model, user);

                IssueVotes_Cacheable_ReadVM? issueVoteStats = await voteStatsRepository.GetIssueVoteStats(model.IssueID);

                voteResponse.Success = true;
                voteResponse.Message = "Vote successfully upserted";
                voteResponse.Average = issueVoteStats!.AverageVote;
                voteResponse.Count = issueVoteStats!.TotalVotes;
            }
            catch (Exception ex)
            {
                voteResponse.Success = false;
                voteResponse.Message = ex.Message;
            }


            return voteResponse;

        }

        internal static async Task<VoteResponse_AjaxVM?> SolutionVote(SolutionVote_UpsertVM model, AppUser user)
        {
            if (_serviceProvider == null)
            {
                throw new InvalidOperationException("Upsert class has not been initialized with a service provider.");
            }

            // Create the response object
            VoteResponse_AjaxVM voteResponse = new VoteResponse_AjaxVM();

            // Create a scope to resolve scoped services
            using var scope = _serviceProvider.CreateScope();
            var services = scope.ServiceProvider;

            var voteStatsRepository = services.GetRequiredService<IVoteStatsRepository>();

            try
            {
                Vote_Cacheable? solutionVote = await voteStatsRepository.UpsertSolutionVote(model, user);

                SolutionVotes_Cacheable_ReadVM? solutionVoteStats = await voteStatsRepository.GetSolutionVoteStats(model.SolutionID);

                voteResponse.Success = true;
                voteResponse.Message = "Vote successfully upserted";
                voteResponse.Average = solutionVoteStats!.AverageVote;
                voteResponse.Count = solutionVoteStats!.TotalVotes;
            }
            catch (Exception ex)
            {
                voteResponse.Success = false;
                voteResponse.Message = ex.Message;
            }


            return voteResponse;
        }
    }

  
}
