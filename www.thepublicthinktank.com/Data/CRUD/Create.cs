using atlas_the_public_think_tank.Data.RepositoryPattern.IRepository;
using atlas_the_public_think_tank.Models.Database;
using atlas_the_public_think_tank.Models.ViewModel;

namespace atlas_the_public_think_tank.Data.CRUD
{
    public static class Create
    {
        private static IServiceProvider? _serviceProvider;

        // Initialize method to be called at startup
        public static void Initialize(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public static Solution Solution(Solution solution) {
            if (_serviceProvider == null)
                throw new InvalidOperationException("Read class has not been initialized with a service provider.");

            // Create a scope to resolve scoped services
            using var scope = _serviceProvider.CreateScope();
            var services = scope.ServiceProvider;
            var solutionRepository = services.GetRequiredService<ISolutionRepository>();


            return solution;


        }
        public async static Task<Issue_ReadVM> Issue(Issue issue) {
            if (_serviceProvider == null)
                throw new InvalidOperationException("Read class has not been initialized with a service provider.");

            // Create a scope to resolve scoped services
            using var scope = _serviceProvider.CreateScope();
            var services = scope.ServiceProvider;
            var issueRepository = services.GetRequiredService<IIssueRepository>();

            Issue_ReadVM issueVM = await issueRepository.AddIssueAsync(issue);

            return issueVM;
        }

    }
}
