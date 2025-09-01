using atlas_the_public_think_tank.Data.RepositoryPattern.Cache;
using atlas_the_public_think_tank.Data.RepositoryPattern.IRepository;
using atlas_the_public_think_tank.Data.RepositoryPattern.Repository;

namespace atlas_the_public_think_tank.Data.RepositoryPattern
{
    public static class RepositoryServiceRegistration
    {
        /// <summary>
        /// Registers all repository services with caching decorators
        /// </summary>
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            // Register repositories with their cache decorators
            services.AddScoped<IIssueRepository, IssueRepository>();
            services.Decorate<IIssueRepository, IssueCacheRepository>();

            services.AddScoped<IBreadcrumbRepository, BreadcrumbRepository>();
            services.Decorate<IBreadcrumbRepository, BreadcrumbCacheRepository>();

            services.AddScoped<IVoteStatsRepository, VoteStatsRepository>();
            services.Decorate<IVoteStatsRepository, VoteStatsCacheRepository>();

            services.AddScoped<IAppUserRepository, AppUserRepository>();
            services.Decorate<IAppUserRepository, AppUserCacheRepository>();

            services.AddScoped<ISolutionRepository, SolutionRepository>();
            services.Decorate<ISolutionRepository, SolutionCacheRepository>();

            services.AddScoped<IFilterIdSetRepository, FilterIdRepository>();
            services.Decorate<IFilterIdSetRepository, FilterIdCacheRepository>();

            services.AddScoped<IScopeRepository, ScopeRepository>();
            services.Decorate<IScopeRepository, ScopeCacheRepository>();

            return services;
        }
    }
}
