using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using repository_pattern_experiment.Data.RepositoryPattern.IRepository;
using repository_pattern_experiment.Data.RepositoryPattern.Repository;
using repository_pattern_experiment.Models.Database;
using repository_pattern_experiment.Models.ViewModel;
using System;
using System.Threading.Tasks;

namespace repository_pattern_experiment.Data.CRUD
{
    /// <summary>
    /// Helper methods to read items from the database via repository pattern
    /// </summary>
    /// <remarks>
    /// The data returned from these methods are combinations of cacheable items.
    /// These methods can also accept the ContentFilters (for Feed Ids - which are cached on a per filter request)
    /// </remarks>
    public static class Read
    {
        private static IServiceProvider? _serviceProvider;

        // Initialize method to be called at startup
        public static void Initialize(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public static async Task<Issue_ReadVM> Issue(Guid issueId)
        {
            if (_serviceProvider == null)
                throw new InvalidOperationException("Read class has not been initialized with a service provider.");

            // Create a scope to resolve scoped services
            using var scope = _serviceProvider.CreateScope();
            var services = scope.ServiceProvider;

            // Get the required repositories from the service provider
            var issueRepository = services.GetRequiredService<IIssueRepository>();
            var voteStatsRepository = services.GetRequiredService<IVoteStatsRepository>();
            var appUserRepository = services.GetRequiredService<IAppUserRepository>();
            var breadcrumbRepository = services.GetRequiredService<IBreadcrumbRepository>();

            var issueContent = await issueRepository.GetIssueById(issueId);

            if (issueContent == null)
            {
                throw new InvalidOperationException("Issue not found");
            }

            UserVote_Issue_ReadVM? issueVoteStats = await voteStatsRepository.GetIssueVoteStats(issueId);
            AppUser_ReadVM? appUser = await appUserRepository.GetAppUser(issueContent.AuthorID);

            // Assemble an issue from the IRepository
            Issue_ReadVM issue = new Issue_ReadVM()
            {
                Content = issueContent.Content,
                ParentIssueID = issueContent.ParentIssueID,
                ParentSolutionID = issueContent.ParentSolutionID,
                Title = issueContent.Title,
                Author = new AppUser_ContentItem_ReadVM()
                {
                    email = appUser!.email,
                    Id = appUser.Id,
                    UserName = appUser.UserName
                },
                ContentStatus = issueContent.ContentStatus,
                CreatedAt = issueContent.CreatedAt,
                ModifiedAt = issueContent.ModifiedAt,
                Scope = issueContent.Scope,
                IssueID = issueContent.Id,
                VoteStats = issueVoteStats!,
                BreadcrumbTags = await breadcrumbRepository.GetBreadcrumbPagedAsync(issueContent.ParentIssueID ?? issueContent.ParentSolutionID)
            };

            return issue;
        }



        public static async Task<Solution_ReadVM> Solution(Guid solutionId)
        {

            if (_serviceProvider == null)
                throw new InvalidOperationException("Read class has not been initialized with a service provider.");

            // Create a scope to resolve scoped services
            using var scope = _serviceProvider.CreateScope();
            var services = scope.ServiceProvider;

            var solutionRepository = services.GetRequiredService<ISolutionRepository>();
            var voteStatsRepository = services.GetRequiredService<IVoteStatsRepository>();
            var appUserRepository = services.GetRequiredService<IAppUserRepository>();
            var breadcrumbRepository = services.GetRequiredService<IBreadcrumbRepository>();


            var solutionContent = await solutionRepository.GetSolutionById(solutionId);

            if (solutionContent == null)
            {
                throw new InvalidOperationException("Issue not found");
            }

            AppUser_ReadVM? appUser = await appUserRepository.GetAppUser(solutionContent.AuthorID);

            UserVote_Solution_ReadVM? solutionVoteStats = await voteStatsRepository.GetSolutionVoteStats(solutionId);


            Solution_ReadVM solution = new Solution_ReadVM()
            {
                Content = solutionContent.Content,
                ParentIssueID = solutionContent.ParentIssueID,
                Title = solutionContent.Title,
                Author = new AppUser_ContentItem_ReadVM()
                {
                    email = appUser!.email,
                    Id = appUser.Id,
                    UserName = appUser.UserName
                },
                ContentStatus = solutionContent.ContentStatus,
                CreatedAt = solutionContent.CreatedAt,
                ModifiedAt = solutionContent.ModifiedAt,
                Scope = solutionContent.Scope,
                SolutionID = solutionContent.Id,
                VoteStats = solutionVoteStats!,
                BreadcrumbTags = await breadcrumbRepository.GetBreadcrumbPagedAsync(solutionContent.ParentIssueID)
            };


            return solution;
        }
    
    }
}