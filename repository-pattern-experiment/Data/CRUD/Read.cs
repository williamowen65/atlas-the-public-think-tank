using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using repository_pattern_experiment.Data.RepositoryPattern.IRepository;
using repository_pattern_experiment.Data.RepositoryPattern.Repository;
using repository_pattern_experiment.Data.RepositoryPattern.Repository.Helpers;
using repository_pattern_experiment.Models.Database;
using repository_pattern_experiment.Models.ViewModel;
using System;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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

        /// <summary>
        /// Returns a paginated issue response with paginated sub issue feed and meta data
        /// </summary>
        public static async Task<PaginatedIssuesResponse> PaginatedSubIssueFeedForIssue(Guid issueId, ContentFilter filter, int pageNumber = 1)
        {
            if (_serviceProvider == null)
                throw new InvalidOperationException("Read class has not been initialized with a service provider.");

            // Create a scope to resolve scoped services
            using var scope = _serviceProvider.CreateScope();
            var services = scope.ServiceProvider;
            var filterIdRepository = services.GetRequiredService<IFilterIdSetRepository>();

            /*
            This is a recursive step of fetching sub issues, but it shouldn't go infinitely deep
            There needs to be something in place to stop the recursion at a depth of 3. 
            Nothing is in place for that at the moment.
            More seed data will be necessary.
            */

            var paginatedChildIssuesIds = await filterIdRepository.GetPagedSubIssueIdsOfIssueById(issueId, filter, pageNumber);

            PaginatedIssuesResponse paginatedIssuesResponse = new PaginatedIssuesResponse();

            // If there are any sub-issue IDs, recursively load them
            if (paginatedChildIssuesIds != null && paginatedChildIssuesIds.Count > 0)
            {

                // Recursively load each sub-issue
                foreach (var subIssueId in paginatedChildIssuesIds)
                {
                    // Call Read.Issue recursively to get the full sub-issue data
                    var subIssue = await Read.Issue(subIssueId, filter);

                    // Add the sub-issue to the PaginatedSubIssues.Issues collection
                    paginatedIssuesResponse.Issues.Add(subIssue);
                }

                paginatedIssuesResponse.CurrentPage = pageNumber;
                paginatedIssuesResponse.PageSize = paginatedChildIssuesIds.Count;
                paginatedIssuesResponse.TotalCount = await filterIdRepository.GetTotalCountSubIssuesOfIssueById(issueId);

            }

            return paginatedIssuesResponse;
        }

        public static async Task<PaginatedIssuesResponse> PaginatedSubIssueFeedForSolution(Guid solutionId, ContentFilter filter, int pageNumber = 1)
        {
            if (_serviceProvider == null)
                throw new InvalidOperationException("Read class has not been initialized with a service provider.");

            // Create a scope to resolve scoped services
            using var scope = _serviceProvider.CreateScope();
            var services = scope.ServiceProvider;
            var filterIdRepository = services.GetRequiredService<IFilterIdSetRepository>();

            /*
            This is a recursive step of fetching sub issues, but it shouldn't go infinitely deep
            There needs to be something in place to stop the recursion at a depth of 3. 
            Nothing is in place for that at the moment.
            More seed data will be necessary.
            */

            var paginatedChildIssuesIds = await filterIdRepository.GetPagedSubIssueIdsOfSolutionById(solutionId, filter, pageNumber);

            PaginatedIssuesResponse paginatedIssuesResponse = new PaginatedIssuesResponse();

            // If there are any sub-issue IDs, recursively load them
            if (paginatedChildIssuesIds != null && paginatedChildIssuesIds.Count > 0)
            {

                // Recursively load each sub-issue
                foreach (var subIssueId in paginatedChildIssuesIds)
                {
                    // Call Read.Issue recursively to get the full sub-issue data
                    var subIssue = await Read.Issue(subIssueId, filter);

                    // Add the sub-issue to the PaginatedSubIssues.Issues collection
                    paginatedIssuesResponse.Issues.Add(subIssue);
                }

                paginatedIssuesResponse.CurrentPage = pageNumber;
                paginatedIssuesResponse.PageSize = paginatedChildIssuesIds.Count;
                paginatedIssuesResponse.TotalCount = await filterIdRepository.GetTotalCountSubIssuesOfSolutionById(solutionId);

            }

            return paginatedIssuesResponse;
        }

        public static async Task<PaginatedSolutionsResponse> PaginatedSolutionFeedForIssue(Guid issueId, ContentFilter filter, int pageNumber = 1)
        {
            if (_serviceProvider == null)
                throw new InvalidOperationException("Read class has not been initialized with a service provider.");

            // Create a scope to resolve scoped services
            using var scope = _serviceProvider.CreateScope();
            var services = scope.ServiceProvider;
            var filterIdRepository = services.GetRequiredService<IFilterIdSetRepository>();

            /*
            This is a recursive step of fetching sub issues, but it shouldn't go infinitely deep
            There needs to be something in place to stop the recursion at a depth of 3. 
            Nothing is in place for that at the moment.
            More seed data will be necessary.
            */

            var paginatedSolutionFeedIds = await filterIdRepository.GetPagedSolutionIdsOfIssueById(issueId, filter, pageNumber);

            PaginatedSolutionsResponse paginatedSolutionResponse = new PaginatedSolutionsResponse();

            // If there are any sub-issue IDs, recursively load them
            if (paginatedSolutionFeedIds != null && paginatedSolutionFeedIds.Count > 0)
            {

                // Recursively load each sub-issue
                foreach (var solutionId in paginatedSolutionFeedIds)
                {
                    // Call Read.Issue recursively to get the full sub-issue data
                    var solution = await Read.Solution(solutionId, filter);

                    // Add the sub-issue to the PaginatedSubIssues.Issues collection
                    paginatedSolutionResponse.Solutions.Add(solution);
                }

                paginatedSolutionResponse.CurrentPage = pageNumber;
                paginatedSolutionResponse.PageSize = paginatedSolutionFeedIds.Count;
                paginatedSolutionResponse.TotalCount = await filterIdRepository.GetTotalCountSolutionsOfIssueById(issueId);

            }

            return paginatedSolutionResponse;
        }


        /*
         Below are the official api helpers for getting data
         Above are ones that may be made private in the future
         */

        public static async Task<Issue_ReadVM> Issue(Guid issueId, ContentFilter filter)
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
            var filterIdRepository = services.GetRequiredService<IFilterIdSetRepository>();

            var issueContent = await issueRepository.GetIssueById(issueId);

            if (issueContent == null)
            {
                throw new InvalidOperationException("Issue not found");
            }

            UserVote_Issue_ReadVM? issueVoteStats = await voteStatsRepository.GetIssueVoteStats(issueId);
            AppUser_ReadVM? appUser = await appUserRepository.GetAppUser(issueContent.AuthorID);

            // Get the sub-issue IDs (Page 1)
            var paginatedSubIssues = await Read.PaginatedSubIssueFeedForIssue(issueId, filter);
            var paginatedSolutions = await Read.PaginatedSolutionFeedForIssue(issueId, filter);

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
                BreadcrumbTags = await breadcrumbRepository.GetBreadcrumbPagedAsync(issueContent.ParentIssueID ?? issueContent.ParentSolutionID),
                PaginatedSubIssues = paginatedSubIssues!,
                PaginatedSolutions = paginatedSolutions!,
            };

            return issue;
        }

        public static async Task<Solution_ReadVM> Solution(Guid solutionId, ContentFilter filter)
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

            var paginatedSubIssues = await Read.PaginatedSubIssueFeedForSolution(solutionId, filter);

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
                BreadcrumbTags = await breadcrumbRepository.GetBreadcrumbPagedAsync(solutionContent.ParentIssueID),
                PaginatedSubIssues = paginatedSubIssues
            };


            return solution;
        }


        public static async Task<ContentItem_ReadVM> ContentItems(ContentFilter filter)
        {
            throw new NotImplementedException();
        }
    }
}