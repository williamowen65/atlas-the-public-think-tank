using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Issue;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Solution;
using atlas_the_public_think_tank.Data.RepositoryPattern.IRepository;
using atlas_the_public_think_tank.Models.ViewModel;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Issue;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Solution;
using atlas_the_public_think_tank.Models.ViewModel.CRUD_VM.ContentItem_Common;

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

        public async static Task<Solution_ReadVM> Solution(Solution solution) {
            if (_serviceProvider == null)
                throw new InvalidOperationException("Read class has not been initialized with a service provider.");

            // Create a scope to resolve scoped services
            using var scope = _serviceProvider.CreateScope();
            var services = scope.ServiceProvider;
            var solutionRepository = services.GetRequiredService<ISolutionRepository>();

            Solution_ReadVM solutionVM = await solutionRepository.AddSolutionAsync(solution);

            return solutionVM;


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

        public async static Task<Scope_ReadVM> Scope(Scope scope)
        {
            if (_serviceProvider == null)
                throw new InvalidOperationException("Read class has not been initialized with a service provider.");

            // Create a scope to resolve scoped services
            using var serviceScope = _serviceProvider.CreateScope();
            var services = serviceScope.ServiceProvider;
            var scopeRepository = services.GetRequiredService<IScopeRepository>();

            Scope_ReadVM scopeVM = await scopeRepository.AddScopeAsync(scope);
            return scopeVM;
        }
      
    }
}
