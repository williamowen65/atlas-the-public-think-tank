using repository_pattern_experiment.Data.RepositoryPattern.Cache;
using repository_pattern_experiment.Data.RepositoryPattern.IRepository;
using repository_pattern_experiment.Data.RepositoryPattern.Repository;

namespace repository_pattern_experiment.Data.RepositoryPattern
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

            return services;
        }
    }
}
