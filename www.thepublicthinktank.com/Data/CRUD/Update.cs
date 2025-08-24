using atlas_the_public_think_tank.Data.RepositoryPattern.IRepository;
using atlas_the_public_think_tank.Data.RepositoryPattern.Repository;
using atlas_the_public_think_tank.Models.Database;
using atlas_the_public_think_tank.Models.ViewModel;

namespace atlas_the_public_think_tank.Data.CRUD
{
    public static class Update
    {
        private static IServiceProvider? _serviceProvider;

        // Initialize method to be called at startup
        public static void Initialize(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async static Task<Solution_ReadVM> Solution(Solution solution) {
            if (_serviceProvider == null)
                throw new InvalidOperationException("Read class has not been initialized with a service provider.");

            // Create a scope to resolve scoped services
            using var scope = _serviceProvider.CreateScope();
            var services = scope.ServiceProvider;
            var solutionRepository = services.GetRequiredService<ISolutionRepository>();

            Solution_ReadVM? solutionVM = await solutionRepository.UpdateSolutionAsync(solution);

            return solutionVM;
        }
        
        public async static Task<Issue_ReadVM?> Issue(Issue issue) {
            if (_serviceProvider == null)
                throw new InvalidOperationException("Read class has not been initialized with a service provider.");

            // Create a scope to resolve scoped services
            using var scope = _serviceProvider.CreateScope();
            var services = scope.ServiceProvider;
            var issueRepository = services.GetRequiredService<IIssueRepository>();

            Issue_ReadVM? issueVM = await issueRepository.UpdateIssueAsync(issue);

            return issueVM;
        }

    }
}
