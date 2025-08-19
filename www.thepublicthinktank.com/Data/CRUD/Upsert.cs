using atlas_the_public_think_tank.Data.RepositoryPattern.IRepository;
using atlas_the_public_think_tank.Models;
using atlas_the_public_think_tank.Models.Database;
using atlas_the_public_think_tank.Models.ViewModel;

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


        public static async Task<JsonVoteResponse?> IssueVote(UserVote_Issue_UpsertVM model, AppUser user)
        {
            if (_serviceProvider == null)
            { 
                throw new InvalidOperationException("Upsert class has not been initialized with a service provider.");
            }

            // Create the response object
            JsonVoteResponse voteResponse = new JsonVoteResponse();
            
            // Create a scope to resolve scoped services
            using var scope = _serviceProvider.CreateScope();
            var services = scope.ServiceProvider;

            var voteStatsRepository = services.GetRequiredService<IVoteStatsRepository>();

           
            try
            {
                Vote_Cacheable_ReadVM? issueVote = await voteStatsRepository.UpsertIssueVote(model, user);

                UserVote_Issue_ReadVM? issueVoteStats = await voteStatsRepository.GetIssueVoteStats(model.IssueID);

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

        internal static async Task<JsonVoteResponse?> SolutionVote(UserVote_Solution_UpsertVM model, AppUser user)
        {
            if (_serviceProvider == null)
            {
                throw new InvalidOperationException("Upsert class has not been initialized with a service provider.");
            }

            // Create the response object
            JsonVoteResponse voteResponse = new JsonVoteResponse();

            // Create a scope to resolve scoped services
            using var scope = _serviceProvider.CreateScope();
            var services = scope.ServiceProvider;

            var voteStatsRepository = services.GetRequiredService<IVoteStatsRepository>();

            try
            {
                Vote_Cacheable_ReadVM? solutionVote = await voteStatsRepository.UpsertSolutionVote(model, user);

                UserVote_Solution_ReadVM? solutionVoteStats = await voteStatsRepository.GetSolutionVoteStats(model.SolutionID);

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
