using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Issue;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Solution;
using atlas_the_public_think_tank.Data.RepositoryPattern.IRepository;
using atlas_the_public_think_tank.Data.RepositoryPattern.Repository;
using atlas_the_public_think_tank.Data.RepositoryPattern.Repository.Helpers;
using atlas_the_public_think_tank.Models.Enums;
using atlas_the_public_think_tank.Models.ViewModel;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.ContentItem_Common;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.ContentItem_Common.ContentItemVote;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Issue;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Issue.IssueVote;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Solution;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Solution.SolutionVote;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.User;
using atlas_the_public_think_tank.Models.ViewModel.CRUD_VM.Solution;
using Microsoft.EntityFrameworkCore;

namespace atlas_the_public_think_tank.Data.CRUD
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

        #region Pagination Read methods

        public static async Task<ContentItems_Paginated_ReadVM> PaginatedMainContentFeed(ContentFilter filter, int pageNumber = 1) {

            if (_serviceProvider == null)
                throw new InvalidOperationException("Read class has not been initialized with a service provider.");

            // Create a scope to resolve scoped services
            using var scope = _serviceProvider.CreateScope();
            var services = scope.ServiceProvider;
            var filterIdRepository = services.GetRequiredService<IFilterIdSetRepository>();
            var issueRepository = services.GetRequiredService<IIssueRepository>();
            var solutionRepository = services.GetRequiredService<ISolutionRepository>();

            var paginatedMainContentIds = await filterIdRepository.GetPagedMainContentFeedIds(filter, pageNumber);
            var counts = await filterIdRepository.GetContentCountMainContentFeed(filter);

            // Create the response object
            var response = new ContentItems_Paginated_ReadVM
            {
                ContentItems = new List<ContentItem_ReadVM>(),
                TotalCount = counts!.TotalCount,
                PageSize = paginatedMainContentIds?.Count ?? 0,
                CurrentPage = pageNumber
            };

            // If there are no content IDs, return the empty response
            if (paginatedMainContentIds == null || paginatedMainContentIds.Count == 0)
            {
                return response;
            }

            // Process each content identifier and fetch the appropriate content
            foreach (var contentId in paginatedMainContentIds)
            {
                try
                {
                    ContentItem_ReadVM item;

                    // Based on the content type, fetch either an issue or a solution
                    if (contentId.Type == ContentType.Issue)
                    {
                        var issue = await Read.Issue(contentId.Id, filter);

                        item = new ContentItem_ReadVM()
                        {
                            ContentID = issue.IssueID,
                            ContentType = ContentType.Issue,
                            Author = issue.Author,
                            BreadcrumbTags = issue.BreadcrumbTags,
                            Content = issue.Content,
                            ContentStatus = issue.ContentStatus,
                            CreatedAt = issue.CreatedAt,
                            ModifiedAt = issue.ModifiedAt,
                            VersionHistoryCount = issue.ModifiedAt != null ? await issueRepository.GetIssueVersionHistoryCount(issue.IssueID): null,
                            Scope = issue.Scope,
                            Title = issue.Title,
                            BlockedContent = issue.BlockedContent,
                            Comments = issue.Comments,
                            LastActivity = issue.LastActivity,
                            PaginatedSubIssues = issue.PaginatedSubIssues,
                            PaginatedSolutions = issue.PaginatedSolutions,
                            VoteStats = new ContentItemVotes_ReadVM() {
                                GenericContentVotes = issue.VoteStats.IssueVotes,
                                AverageVote = issue.VoteStats.AverageVote,
                                ContentID = issue.VoteStats.ContentID,
                                TotalVotes = issue.VoteStats.TotalVotes,
                                ContentType = ContentType.Issue
                            }
                        };
                    }
                    else if (contentId.Type == ContentType.Solution)
                    {
                        var solution = await Read.Solution(contentId.Id, filter);
                        item = new ContentItem_ReadVM() 
                        {
                            ContentID = solution.SolutionID,
                            ContentType = ContentType.Solution,
                            Author = solution.Author,
                            BreadcrumbTags = solution.BreadcrumbTags,
                            Content = solution.Content,
                            ContentStatus = solution.ContentStatus,
                            CreatedAt = solution.CreatedAt,
                            ModifiedAt = solution.ModifiedAt,
                            VersionHistoryCount = solution.ModifiedAt != null ? await solutionRepository.GetSolutionVersionHistoryCount(solution.SolutionID) : null,
                            Scope = solution.Scope,
                            Title = solution.Title,
                            BlockedContent= solution.BlockedContent,
                            Comments = solution.Comments,
                            LastActivity = solution.LastActivity,
                            PaginatedSubIssues = solution.PaginatedSubIssues,
                            VoteStats = new ContentItemVotes_ReadVM()
                            {
                                GenericContentVotes = solution.VoteStats.SolutionVotes,
                                AverageVote = solution.VoteStats.AverageVote,
                                ContentID = solution.VoteStats.ContentID,
                                TotalVotes = solution.VoteStats.TotalVotes,
                                ContentType = ContentType.Solution
                            }
                        };
                    }
                    else
                    {
                        // Skip unknown content types
                        continue;
                    }

                    // Add the item to the response
                    response.ContentItems.Add(item);
                }
                catch (Exception ex)
                {
                    // Log the error but continue processing other items
                    // Consider adding proper logging here
                    Console.WriteLine($"Error loading content item {contentId.Id}: {ex.Message}");
                }
            }

            return response;


        }

        /// <summary>
        /// Returns a paginated issue response with paginated sub issue feed and meta data
        /// </summary>
        public static async Task<Issues_Paginated_ReadVM> PaginatedSubIssueFeedForIssue(Guid issueId, ContentFilter filter, int pageNumber = 1)
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

            Issues_Paginated_ReadVM paginatedIssuesResponse = new Issues_Paginated_ReadVM();

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

            }
            var counts = await filterIdRepository.GetContentCountSubIssuesOfIssueById(issueId, filter);
            paginatedIssuesResponse.ContentCount = counts!;

            return paginatedIssuesResponse;
        }

        public static async Task<Issues_Paginated_ReadVM> PaginatedSubIssueFeedForSolution(Guid solutionId, ContentFilter filter, int pageNumber = 1)
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

            Issues_Paginated_ReadVM paginatedIssuesResponse = new Issues_Paginated_ReadVM();

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

            }
            var counts = await filterIdRepository.GetContentCountSubIssuesOfSolutionById(solutionId, filter);
            paginatedIssuesResponse.ContentCount = counts!;

            return paginatedIssuesResponse;
        }

        public static async Task<Solutions_Paginated_ReadVM> PaginatedSolutionFeedForIssue(Guid issueId, ContentFilter filter, int pageNumber = 1)
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

            Solutions_Paginated_ReadVM paginatedSolutionResponse = new Solutions_Paginated_ReadVM();

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

            }
            var counts = await filterIdRepository.GetContentCountSolutionsOfIssueById(issueId, filter);
            paginatedSolutionResponse.ContentCount = counts!;

            return paginatedSolutionResponse;
        }

        #endregion


        #region Read An Issue or Solution


        /// <summary>
        /// Read an issue from the Cache or Database
        /// </summary>
        /// <remarks>
        /// Fetch parent is only ever done for the root issue of an issue page
        /// </remarks>
        public static async Task<Issue_ReadVM?> Issue(Guid issueId, ContentFilter filter, bool fetchParent = false)
        {


            if (_serviceProvider == null)
                throw new InvalidOperationException("Read class has not been initialized with a service provider.");

            // Create a scope to resolve scoped services
            using var scope = _serviceProvider.CreateScope();
            var services = scope.ServiceProvider;

            // Get the required repositories from the service provider
            var issueRepository = services.GetRequiredService<IIssueRepository>();
            var solutionRepository = services.GetRequiredService<ISolutionRepository>();
            var voteStatsRepository = services.GetRequiredService<IVoteStatsRepository>();
            var appUserRepository = services.GetRequiredService<IAppUserRepository>();
            var breadcrumbRepository = services.GetRequiredService<IBreadcrumbRepository>();

            var issueContent = await issueRepository.GetIssueById(issueId);

            if (issueContent == null)
            {
                throw new InvalidOperationException("Issue not found");
            }

            IssueVotes_ReadVM? issueVoteStats = await voteStatsRepository.GetIssueVoteStats(issueId);
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
                Author = new AppUser_ReadVM()
                {
                    email = appUser!.email,
                    Id = appUser.Id,
                    UserName = appUser.UserName
                },
                ContentStatus = issueContent.ContentStatus,
                CreatedAt = issueContent.CreatedAt,
                ModifiedAt = issueContent.ModifiedAt,
                VersionHistoryCount = issueContent.ModifiedAt != null ? await issueRepository.GetIssueVersionHistoryCount(issueId) : null,
                Scope = issueContent.Scope,
                IssueID = issueContent.Id,
                VoteStats = issueVoteStats!,
                BreadcrumbTags = await breadcrumbRepository.GetBreadcrumbPagedAsync(issueContent.ParentIssueID ?? issueContent.ParentSolutionID),
                PaginatedSubIssues = paginatedSubIssues!,
                PaginatedSolutions = paginatedSolutions!,
            };

            if (fetchParent)
            {
                if (issue.ParentIssueID != null)
                {
                    issue.ParentIssue = await Read.Issue(issue.ParentIssueID.Value, filter);
                }
                else if (issue.ParentSolutionID != null)
                {
                    issue.ParentSolution = await Read.Solution(issue.ParentSolutionID.Value, filter);
                }
            }

            return issue;
        }

        /// <summary>
        /// Read a solution from the Cache or Database
        /// </summary>
        /// <remarks>
        /// Fetch parent is only ever done for the root Solution of a solution page
        /// </remarks>
        public static async Task<Solution_ReadVM> Solution(Guid solutionId, ContentFilter filter, bool fetchParent = false)
        {

            if (_serviceProvider == null)
                throw new InvalidOperationException("Read class has not been initialized with a service provider.");

            // Create a scope to resolve scoped services
            using var scope = _serviceProvider.CreateScope();
            var services = scope.ServiceProvider;

            var issueRepository = services.GetRequiredService<IIssueRepository>();
            var solutionRepository = services.GetRequiredService<ISolutionRepository>();
            var voteStatsRepository = services.GetRequiredService<IVoteStatsRepository>();
            var appUserRepository = services.GetRequiredService<IAppUserRepository>();
            var breadcrumbRepository = services.GetRequiredService<IBreadcrumbRepository>();


            var solutionContent = await solutionRepository.GetSolutionById(solutionId);

            if (solutionContent == null)
            {
                throw new InvalidOperationException("Solution not found");
            }

            AppUser_ReadVM? appUser = await appUserRepository.GetAppUser(solutionContent.AuthorID);

            SolutionVotes_ReadVM? solutionVoteStats = await voteStatsRepository.GetSolutionVoteStats(solutionId);

            var paginatedSubIssues = await Read.PaginatedSubIssueFeedForSolution(solutionId, filter);

            

            Solution_ReadVM solution = new Solution_ReadVM()
            {
                Content = solutionContent.Content,
                ParentIssueID = solutionContent.ParentIssueID,
               
                Title = solutionContent.Title,
                Author = new AppUser_ReadVM()
                {
                    email = appUser!.email,
                    Id = appUser.Id,
                    UserName = appUser.UserName
                },
                ContentStatus = solutionContent.ContentStatus,
                CreatedAt = solutionContent.CreatedAt,
                ModifiedAt = solutionContent.ModifiedAt,
                VersionHistoryCount = solutionContent.ModifiedAt != null ? await solutionRepository.GetSolutionVersionHistoryCount(solutionId) : null,
                Scope = solutionContent.Scope,
                SolutionID = solutionContent.Id,
                VoteStats = solutionVoteStats!,
                BreadcrumbTags = await breadcrumbRepository.GetBreadcrumbPagedAsync(solutionContent.ParentIssueID),
                PaginatedSubIssues = paginatedSubIssues
            };


            if (fetchParent)
            {
                solution.ParentIssue = await Read.Issue(solutionContent.ParentIssueID, filter);
            }

            return solution;
        }

        #endregion


        #region Get Content Items Version History

        public static async Task<List<ContentItem_ReadVM>> IssueVersionHistory(Issue_ReadVM issue) 
        {
            if (_serviceProvider == null)
                throw new InvalidOperationException("Read class has not been initialized with a service provider.");

            using var methodScope = _serviceProvider.CreateScope();
            var services = methodScope.ServiceProvider;
            var _context = services.GetRequiredService<ApplicationDbContext>();
            var _appUserRepository = services.GetRequiredService<IAppUserRepository>();
            var _breadcrumbRepository = services.GetRequiredService<IBreadcrumbRepository>();


            // TODO: Potentially move the temporal issues to the cache layer
            // This currently skips the repository pattern


            // Pull all temporal versions of the issue with their period information
            var versionsWithPeriodStart = await _context.Issues
                .TemporalAll()
                .Where(i => i.IssueID == issue.IssueID)
                .OrderBy(i => EF.Property<DateTime>(i, "PeriodStart"))
                .Select(i => new
                {
                    Issue = i,
                    PeriodStart = EF.Property<DateTime>(i, "PeriodStart")
                })
                .ToListAsync();

            List<ContentItem_ReadVM> contentItemVersions = new();

            foreach (var versionData in versionsWithPeriodStart)
            {
                var version = versionData.Issue;
                var versionPeriodStart = versionData.PeriodStart;

                AppUser_ReadVM? appUser = await _appUserRepository.GetAppUser(version.AuthorID);

                // Now use the captured versionPeriodStart in the scope query
                Scope? scope = await _context.Scopes
                    .TemporalAll()
                    .Where(s => s.ScopeID == version.ScopeID)
                    .Where(s =>
                        EF.Property<DateTime>(s, "PeriodStart") <= versionPeriodStart &&
                        EF.Property<DateTime>(s, "PeriodEnd") > versionPeriodStart)
                    .OrderByDescending(s => EF.Property<DateTime>(s, "PeriodStart"))
                    .FirstOrDefaultAsync();

                var contentItem = new ContentItem_ReadVM
                {
                    ContentID = version.IssueID,
                    Title = version.Title,
                    Content = version.Content,
                    ContentType = ContentType.Issue,
                    ContentStatus = version.ContentStatus,
                    CreatedAt = version.CreatedAt,
                    ModifiedAt = version.ModifiedAt,
                    Author = appUser!,
                    BreadcrumbTags = await _breadcrumbRepository.GetBreadcrumbPagedAsync(version.ParentIssueID ?? version.ParentSolutionID),
                    Scope = scope,
                    VoteStats = new ContentItemVotes_ReadVM
                    {
                        GenericContentVotes = issue.VoteStats.IssueVotes,
                        AverageVote = issue.VoteStats.AverageVote,
                        ContentID = issue.VoteStats.ContentID,
                        TotalVotes = issue.VoteStats.TotalVotes,
                        ContentType = ContentType.Issue
                    }
                };

                contentItemVersions.Add(contentItem);
            }


            return contentItemVersions;
        }

        public static async Task<List<ContentItem_ReadVM>> SolutionVersionHistory(Solution_ReadVM solution) 
        {
            if (_serviceProvider == null)
                throw new InvalidOperationException("Read class has not been initialized with a service provider.");

            using var methodScope = _serviceProvider.CreateScope();
            var services = methodScope.ServiceProvider;
            var _context = services.GetRequiredService<ApplicationDbContext>();
            var _appUserRepository = services.GetRequiredService<IAppUserRepository>();
            var _breadcrumbRepository = services.GetRequiredService<IBreadcrumbRepository>();


            // TODO: Potentially move the temporal issues to the cache layer
            // This currently skips the repository pattern


            // Pull all temporal versions of the solution with their period information
            var versionsWithPeriodStart = await _context.Solutions
                .TemporalAll()
                .Where(s => s.SolutionID == solution.SolutionID)
                .OrderBy(s => EF.Property<DateTime>(s, "PeriodStart"))
                .Select(s => new
                {
                    Solution = s,
                    PeriodStart = EF.Property<DateTime>(s, "PeriodStart")
                })
                .ToListAsync();

            List<ContentItem_ReadVM> contentItemVersions = new();

            foreach (var versionData in versionsWithPeriodStart)
            {
                var version = versionData.Solution;
                var versionPeriodStart = versionData.PeriodStart;

                AppUser_ReadVM? appUser = await _appUserRepository.GetAppUser(version.AuthorID);
              
                // Now use the captured versionPeriodStart in the scope query
                Scope? scope = await _context.Scopes
                    .TemporalAll()
                    .Where(s => s.ScopeID == version.ScopeID)
                    .Where(s =>
                        EF.Property<DateTime>(s, "PeriodStart") <= versionPeriodStart &&
                        EF.Property<DateTime>(s, "PeriodEnd") > versionPeriodStart)
                    .OrderByDescending(s => EF.Property<DateTime>(s, "PeriodStart"))
                    .FirstOrDefaultAsync();

                var contentItem = new ContentItem_ReadVM
                {
                    ContentID = version.SolutionID,
                    Title = version.Title,
                    Content = version.Content,
                    ContentType = ContentType.Solution,
                    ContentStatus = version.ContentStatus,
                    CreatedAt = version.CreatedAt,
                    ModifiedAt = version.ModifiedAt,
                    Author = appUser!,
                    BreadcrumbTags = await _breadcrumbRepository.GetBreadcrumbPagedAsync(version.ParentIssueID),
                    Scope = scope,
                    VoteStats = new ContentItemVotes_ReadVM
                    {
                        GenericContentVotes = solution.VoteStats.SolutionVotes,
                        AverageVote = solution.VoteStats.AverageVote,
                        ContentID = solution.VoteStats.ContentID,
                        TotalVotes = solution.VoteStats.TotalVotes,
                        ContentType = ContentType.Issue
                    }
                };

                contentItemVersions.Add(contentItem);
            }


            return contentItemVersions;
        }



        #endregion

    }
}