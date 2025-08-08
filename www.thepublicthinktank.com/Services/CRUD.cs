using atlas_the_public_think_tank.Data;
using atlas_the_public_think_tank.Models.Database;
using atlas_the_public_think_tank.Models.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using static atlas_the_public_think_tank.Data.SeedData.SeedIds;
using Solution = atlas_the_public_think_tank.Models.Database.Solution;

namespace atlas_the_public_think_tank.Services
{

    public class ContentIndexEntry 
    {
        public Guid ContentId { get; set; }
        public ContentType ContentType { get; set; }

        // The below properties are filterable properties.
        // They can all be optional and depend on the specific query

        public double AverageVote { get; set; }
        public int TotalVotes { get; set; }
        public DateTime CreatedAt { get; set; }


        /// <summary>
        /// WeightedScore: score = (averageVote * totalVotes) / (totalVotes + k)
        /// </summary>
        /// <remarks>
        /// k = tuning parameter (how much you penalize low-vote content) <br/>
        /// Content with a low vote gets discounted more <br/>
        /// </remarks>
        public double WeightedScore { get; set; }

        /// <summary>
        /// score = (v / (v + m)) * R + (m / (v + m)) * C
        /// </summary>
        /// <remarks>
        /// R = average vote for the item <br/>
        /// V = number of votes for the item <br/>
        /// m = minimum votes required for credibility (eg, 10, 20) <br/>
        /// C = mean vote across all content <br/>
        /// </remarks>
        public double BayesianAverage { get; set; }

    }

    /// <summary>
    /// Below are alternatives to fairly score content based on average vote and total number of vote
    /// Quality(average rating) with Confidence(How many people agree)
    /// </summary>
    //public class ContentScore
    //{
    //    /// <summary>
    //    /// WeightedScore: score = (averageVote * totalVotes) / (totalVotes + k)
    //    /// </summary>
    //    /// <remarks>
    //    /// k = tuning parameter (how much you penalize low-vote content) <br/>
    //    /// Content with a low vote gets discounted more <br/>
    //    /// </remarks>
    //    public double WeightedScore { get; set; }

    //    /// <summary>
    //    /// score = (v / (v + m)) * R + (m / (v + m)) * C
    //    /// </summary>
    //    /// <remarks>
    //    /// R = average vote for the item <br/>
    //    /// V = number of votes for the item <br/>
    //    /// m = minimum votes required for credibility (eg, 10, 20) <br/>
    //    /// C = mean vote across all content <br/>
    //    /// </remarks>
    //    public double BayesianAverage { get; set; }
    //}

  

    /// <summary>
    /// A service to encapsulate CRUD logic for the app (Accessible by dependency injection)
    /// </summary>
    /// <remarks>
    /// This class helps to keep code dry/changes to CRUD logic to apply to 
    /// Ideally the endpoints can be lean and this would have all the CRUD logic
    /// At the moment, some CRUD logic remains in controllers.
    /// The CRUD class is a convenience class for accessing logic in this file
    /// </remarks>
    public class CRUD
    {
        public Issues Issues { get; }
        public Solutions Solutions { get; }
        public BreadcrumbAccessor BreadcrumbAccessor{ get; }

        private readonly ApplicationDbContext _context;

        public CRUD(Issues issues, Solutions solutions, ApplicationDbContext context, BreadcrumbAccessor breadcrumbAccessor)
        {
            Issues = issues;
            Solutions = solutions;
            _context = context;
            BreadcrumbAccessor = breadcrumbAccessor;
        }

        /// <summary>
        /// Creates a filtering utility that can be shared between Issues and Solutions
        /// </summary>
        public static class ContentFiltering
        {
            /// <summary>
            /// Applies common filtering logic to a query of Issues
            /// </summary>
            public static IQueryable<Issue> ApplyIssueFilters(IQueryable<Issue> query, ContentFilter filter)
            {
                if (filter == null)
                    return query;

                // Log filter info
                Console.WriteLine("Applying content filters to issues");
                Console.WriteLine(filter);
                Console.WriteLine(filter.ToJson());

                // Filter by average vote range
                if (filter?.AvgVoteRange != null)
                {
                    query = query.Where(i =>
                        (i.IssueVotes.Any() ? i.IssueVotes.Average(v => v.VoteValue) : 0) >= filter.AvgVoteRange.Min &&
                        (i.IssueVotes.Any() ? i.IssueVotes.Average(v => v.VoteValue) : 0) <= filter.AvgVoteRange.Max);
                }

                // Filter by total vote count
                if (filter?.TotalVoteCount != null)
                {
                    query = query.Where(i => i.IssueVotes.Count >= filter.TotalVoteCount.Min);

                    // Apply max filter only if it has a value
                    if (filter.TotalVoteCount.Max.HasValue)
                    {
                        query = query.Where(i => i.IssueVotes.Count <= filter.TotalVoteCount.Max.Value);
                    }
                }

                // Filter by date range
                if (filter?.DateRange != null)
                {
                    // Apply "from date" filter if specified
                    if (filter.DateRange.From.HasValue)
                    {
                        query = query.Where(i => i.CreatedAt >= filter.DateRange.From.Value.AddDays(-1));
                    }

                    // Apply "to date" filter if specified
                    if (filter.DateRange.To.HasValue)
                    {
                        // Add one day to include the entire end date (up to 23:59:59)
                        var toDateInclusive = filter.DateRange.To.Value.AddDays(1).AddSeconds(-1);
                        query = query.Where(i => i.CreatedAt <= toDateInclusive);
                    }
                }

                return query;
            }


            /// <summary>
            /// Applies common filtering logic to a query of Solutions
            /// </summary>
            public static IQueryable<Solution> ApplySolutionFilters(IQueryable<Solution> query, ContentFilter filter)
            {
                if (filter == null)
                    return query;

                // Log filter info
                Console.WriteLine("Applying content filters to solutions");
                Console.WriteLine(filter);
                Console.WriteLine(filter.ToJson());

                // Filter by average vote range
                if (filter?.AvgVoteRange != null)
                {
                    query = query.Where(s =>
                        (s.SolutionVotes.Any() ? s.SolutionVotes.Average(v => v.VoteValue) : 0) >= filter.AvgVoteRange.Min &&
                        (s.SolutionVotes.Any() ? s.SolutionVotes.Average(v => v.VoteValue) : 0) <= filter.AvgVoteRange.Max);
                }

                // Filter by total vote count
                if (filter?.TotalVoteCount != null)
                {
                    query = query.Where(s => s.SolutionVotes.Count >= filter.TotalVoteCount.Min);

                    // Apply max filter only if it has a value
                    if (filter.TotalVoteCount.Max.HasValue)
                    {
                        query = query.Where(s => s.SolutionVotes.Count <= filter.TotalVoteCount.Max.Value);
                    }
                }

                // Filter by date range
                if (filter?.DateRange != null)
                {
                    // Apply "from date" filter if specified
                    if (filter.DateRange.From.HasValue)
                    {
                        query = query.Where(i => i.CreatedAt >= filter.DateRange.From.Value.AddDays(-1));
                    }

                    // Apply "to date" filter if specified
                    if (filter.DateRange.To.HasValue)
                    {
                        // Add one day to include the entire end date (up to 23:59:59)
                        var toDateInclusive = filter.DateRange.To.Value.AddDays(1).AddSeconds(-1);
                        query = query.Where(s => s.CreatedAt <= toDateInclusive);
                    }
                }

                return query;
            }

            /// <summary>
            /// Applies filtering to ContentIndexEntry queries that combine both issues and solutions
            /// </summary>
            public static IQueryable<ContentIndexEntry> ApplyCombinedContentFilters(
                IQueryable<ContentIndexEntry> query,
                ContentFilter filter)
            {
                if (filter == null)
                    return query;

                // Log filter info
                Console.WriteLine("Applying combined content filters");
                Console.WriteLine(filter);
                Console.WriteLine(filter.ToJson());

                // Filter for ContentType ("both", "issues", "solutions)
                if (filter.ContentType != null && filter.ContentType != "both")
                {
                    if (filter.ContentType.Equals("issues", StringComparison.OrdinalIgnoreCase))
                    {
                        query = query.Where(c => c.ContentType == ContentType.Issue);
                    }
                    else if (filter.ContentType.Equals("solutions", StringComparison.OrdinalIgnoreCase))
                    {
                        query = query.Where(c => c.ContentType == ContentType.Solution);
                    }
                    // "both" is the default and doesn't need filtering as it includes all content types
                }

                // Filter by average vote range
                if (filter.AvgVoteRange != null)
                {
                    query = query.Where(c =>
                        c.AverageVote >= filter.AvgVoteRange.Min &&
                        c.AverageVote <= filter.AvgVoteRange.Max);
                }

                // Filter by total vote count
                if (filter.TotalVoteCount != null)
                {
                    query = query.Where(c => c.TotalVotes >= filter.TotalVoteCount.Min);

                    // Apply max filter only if it has a value
                    if (filter.TotalVoteCount.Max.HasValue)
                    {
                        query = query.Where(c => c.TotalVotes <= filter.TotalVoteCount.Max.Value);
                    }
                }

                // Filter by date range
                if (filter.DateRange != null)
                {
                    // Apply "from date" filter if specified
                    if (filter.DateRange.From.HasValue)
                    {
                        query = query.Where(i => i.CreatedAt >= filter.DateRange.From.Value.AddDays(-1));
                    }

                    // Apply "to date" filter if specified
                    if (filter.DateRange.To.HasValue)
                    {
                        // Add one day to include the entire end date (up to 23:59:59)
                        var toDateInclusive = filter.DateRange.To.Value.AddDays(1).AddSeconds(-1);
                        query = query.Where(c => c.CreatedAt <= toDateInclusive);
                    }
                }

                return query;
            }
            /// <summary>
            /// Calculate weighted scores and apply standard sorting to ContentIndexEntry query
            /// </summary>
            public static IOrderedQueryable<ContentIndexEntry> ApplyCombinedContentSorting(
                IQueryable<ContentIndexEntry> query,
                int k = 5)
            {
                return query
                    .Select(entry => new ContentIndexEntry
                    {
                        ContentId = entry.ContentId,
                        ContentType = entry.ContentType,
                        CreatedAt = entry.CreatedAt,
                        AverageVote = entry.AverageVote,
                        TotalVotes = entry.TotalVotes,
                        WeightedScore = (entry.TotalVotes + k) != 0
                            ? (entry.AverageVote * entry.TotalVotes) / (entry.TotalVotes + k)
                            : 0
                    })
                    .OrderByDescending(c => c.WeightedScore)
                    .ThenByDescending(c => c.AverageVote)
                    .ThenByDescending(c => c.CreatedAt);
            }

            /// <summary>
            /// Applies sorting by weighted score to an issue query
            /// </summary>
            public static IOrderedQueryable<Issue> ApplyWeightedScoreSorting(IQueryable<Issue> query, int k = 5)
            {
                return query
                    .OrderByDescending(i => i.IssueVotes.Any()
                        ? (i.IssueVotes.Average(v => v.VoteValue) * i.IssueVotes.Count) / (i.IssueVotes.Count + k) // Weighted score
                        : 0)
                    .ThenByDescending(i => i.IssueVotes.Any() ? i.IssueVotes.Average(v => v.VoteValue) : 0) // Average vote as tiebreaker
                    .ThenByDescending(i => i.CreatedAt); // Creation date as final tiebreaker
            }

            /// <summary>
            /// Applies sorting by weighted score to a solution query
            /// </summary>
            public static IOrderedQueryable<Solution> ApplyWeightedScoreSorting(IQueryable<Solution> query, int k = 5)
            {
                return query
                    .OrderByDescending(s => s.SolutionVotes.Any()
                        ? (s.SolutionVotes.Average(v => v.VoteValue) * s.SolutionVotes.Count) / (s.SolutionVotes.Count + k) // Weighted score
                        : 0)
                    .ThenByDescending(s => s.SolutionVotes.Any() ? s.SolutionVotes.Average(v => v.VoteValue) : 0) // Average vote as tiebreaker
                    .ThenByDescending(s => s.CreatedAt); // Creation date as final tiebreaker
            }
        }



        public async Task<PaginatedContentItemsResponse> GetContentItemsPagedAsync(int pageNumber, ContentFilter filter, int pageSize = 3)
        {
            // First, get all the issues and solutions IDs with their creation dates and vote averages
            // This allows efficient sorting and pagination at the database level
            var issuesIndexQuery = _context.Issues
                .Select(i => new ContentIndexEntry
                {
                    ContentId = i.IssueID,
                    ContentType = ContentType.Issue,
                    CreatedAt = i.CreatedAt,
                    AverageVote = i.IssueVotes.Any() ? i.IssueVotes.Average(v => v.VoteValue) : 0,
                    TotalVotes = i.IssueVotes.Any() ? i.IssueVotes.Count() : 0,
                });

            var solutionsIndexQuery = _context.Solutions
                .Select(s => new ContentIndexEntry
                {
                    ContentId = s.SolutionID,
                    ContentType = ContentType.Solution,
                    CreatedAt = s.CreatedAt,
                    AverageVote = s.SolutionVotes.Any() ? s.SolutionVotes.Average(v => v.VoteValue) : 0,
                    TotalVotes = s.SolutionVotes.Any() ? s.SolutionVotes.Count() : 0
                });

            // Combine queries
            var combinedQuery = issuesIndexQuery.Union(solutionsIndexQuery);

            // Apply filters using the centralized filtering method
            combinedQuery = ContentFiltering.ApplyCombinedContentFilters(combinedQuery, filter);

            // Get total count after applying filters
            int totalCount = await combinedQuery.CountAsync();

            // Apply sorting, calculate weighted scores, and paginate
            int k = 5; // tuning constant
            var pagedIndexEntries = await ContentFiltering.ApplyCombinedContentSorting(combinedQuery, k)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Now fetch the full content items for each ID in the paged results
            var contentItems = new List<ContentItem_ReadVM>();

            foreach (var entry in pagedIndexEntries)
            {
                if (entry.ContentType == ContentType.Issue)
                {
                    // Get the issue with all needed includes
                    var issue = await _context.Issues
                        .Include(p => p.Scope)
                        .Include(p => p.Author)
                        .Include(f => f.ParentIssue)
                        .Include(f => f.ChildIssues)
                        .Include(f => f.BlockedContent)
                        .Include(f => f.Solutions)
                            .ThenInclude(s => s.Scope)
                        .Include(f => f.Comments)
                        .Include(f => f.IssueVotes)
                        .Include(p => p.IssueCategories)
                            .ThenInclude(fc => fc.Category)
                        .FirstOrDefaultAsync(i => i.IssueID == entry.ContentId);

                    if (issue != null)
                    {
                        var issueVM = new Issue_ReadVM
                        {
                            IssueID = issue.IssueID,
                            Title = issue.Title,
                            Content = issue.Content,
                            CreatedAt = issue.CreatedAt,
                            ModifiedAt = issue.ModifiedAt,
                            AuthorID = issue.AuthorID,
                            VoteStats = await Issues.GetIssueVoteStats(issue.IssueID),
                            ScopeID = issue.ScopeID,
                            ParentIssueID = issue.ParentIssueID,
                            ContentStatus = issue.ContentStatus,
                            BlockedContentID = issue.BlockedContentID,
                            Author = issue.Author,
                            Scope = issue.Scope,
                            Categories = Issues.GetIssuesCategories(issue),
                            SubIssueCount = _context.Issues.Count(i => i.ParentIssueID == issue.IssueID),
                            SubIssues = await Issues.GetIssuesSubIssuesAsync(issue),
                            Solutions = await Solutions.GetIssuesSolutions(issue),
                            BreadcrumbTags = await BreadcrumbAccessor.GetIssueBreadcrumbTags(issue)
                        };
                        contentItems.Add(issueVM);
                    }
                }
                else if (entry.ContentType == ContentType.Solution)
                {
                    // Get the solution with all needed includes
                    var solution = await _context.Solutions
                        .Include(p => p.Scope)
                        .Include(p => p.Author)
                        .Include(f => f.ParentIssue)
                        .Include(f => f.ChildIssues)
                        .Include(f => f.BlockedContent)
                        .Include(f => f.Comments)
                        .Include(f => f.SolutionVotes)
                        .Include(p => p.SolutionCategories)
                            .ThenInclude(fc => fc.Category)
                        .FirstOrDefaultAsync(s => s.SolutionID == entry.ContentId);

                    if (solution != null)
                    {
                        var solutionVM = new Solution_ReadVM
                        {
                            SolutionID = solution.SolutionID,
                            Title = solution.Title,
                            Content = solution.Content,
                            CreatedAt = solution.CreatedAt,
                            ModifiedAt = solution.ModifiedAt,
                            AuthorID = solution.AuthorID,
                            VoteStats = await Solutions.GetSolutionVoteStats(solution.SolutionID),
                            ScopeID = solution.ScopeID,
                            ParentIssueID = solution.ParentIssueID,
                            ContentStatus = solution.ContentStatus,
                            BlockedContentID = solution.BlockedContentID,
                            Author = solution.Author,
                            Scope = solution.Scope,
                            Categories = Solutions.GetSolutionCategories(solution),
                            SubIssueCount = _context.Issues.Count(i => i.ParentSolutionID == solution.SolutionID),
                            SubIssues = await Issues.GetSolutionSubIssues(solution),
                            BreadcrumbTags = await BreadcrumbAccessor.GetSolutionBreadcrumbTags(solution)
                        };
                        contentItems.Add(solutionVM);
                    }
                }
            }

            return new PaginatedContentItemsResponse
            {
                ContentItems = contentItems,
                CurrentPage = pageNumber,
                TotalCount = totalCount,
                PageSize = pageSize
            };
        }

    }



    /// <summary>
    /// The breadcrumbs reflect the nesting of content and offer a way to navigate 
    /// The breadcrumbAccessor helps retrieve info about that nesting. <br/>
    /// When requesting breadcrumb data, make sure to pass the parent solution or parent issue
    /// in order to not include the current content in the breadcrumb
    /// 
    /// </summary>
    public class BreadcrumbAccessor
    {
        private readonly ApplicationDbContext _context;
      

        public BreadcrumbAccessor(ApplicationDbContext context )
        {
            _context = context;
        }

        public async Task<List<Breadcrumb_ReadVM>> GetIssueBreadcrumbTags(Issue issue)
        {
            return (issue.ParentIssueID.HasValue || issue.ParentSolutionID.HasValue)
                ? await GetContentBreadcrumb(issue.ParentSolutionID ?? issue.ParentIssueID ?? issue.IssueID)
                : new List<Breadcrumb_ReadVM>();
        }
        public async Task<List<Breadcrumb_ReadVM>> GetSolutionBreadcrumbTags(Models.Database.Solution solution)
        {
            return solution.ParentIssueID != Guid.Empty
             ? await GetContentBreadcrumb(
                 solution.ParentIssueID
               )
             : new List<Breadcrumb_ReadVM>();
        }




        /// <summary>
        /// This method recursively retrieves the breadcrumb tags for a given content ID by traversing the content hierarchy via the parent posts.  
        /// </summary>
        /// <param name="contentId"></param>
        /// <returns></returns>
        public async Task<List<Breadcrumb_ReadVM>> GetContentBreadcrumb(Guid contentId)
        {
            var breadcrumbs = new List<Breadcrumb_ReadVM>();

            // First check if the content exists as an Issue
            var issue = await _context.Issues
                .Include(i => i.ParentIssue)
                .Include(i => i.ParentSolution)
                .FirstOrDefaultAsync(i => i.IssueID == contentId);

            if (issue != null)
            {
                breadcrumbs.Add(new Breadcrumb_ReadVM
                {
                    Title = issue.Title,
                    ContentID = issue.IssueID,
                    ContentType = ContentType.Issue
                });

                // If this issue has a parent issue, recursively get its breadcrumbs
                if (issue.ParentIssueID.HasValue)
                {
                    var parentBreadcrumbs = await GetContentBreadcrumb(issue.ParentIssueID.Value);
                    breadcrumbs.AddRange(parentBreadcrumbs);
                }
                // If this issue has a parent solution, recursively get its breadcrumbs
                else if (issue.ParentSolutionID.HasValue)
                {
                    var parentBreadcrumbs = await GetContentBreadcrumb(issue.ParentSolutionID.Value);
                    breadcrumbs.AddRange(parentBreadcrumbs);
                }

                return breadcrumbs;
            }

            // If not an issue, check if it's a solution
            var solution = await _context.Solutions
                .Include(s => s.ParentIssue)
                .FirstOrDefaultAsync(s => s.SolutionID == contentId);

            if (solution != null)
            {
                breadcrumbs.Add(new Breadcrumb_ReadVM
                {
                    Title = solution.Title,
                    ContentID = solution.SolutionID,
                    ContentType = ContentType.Solution
                });

                // Solutions have parent issues, so get the breadcrumbs for the parent issue
                if (solution.ParentIssueID != Guid.Empty)
                {
                    var parentBreadcrumbs = await GetContentBreadcrumb(solution.ParentIssueID);
                    breadcrumbs.AddRange(parentBreadcrumbs);
                }

                return breadcrumbs;
            }

            // If we get here, no content was found with the given ID
            return breadcrumbs;
        }
    }

    /// <summary>
    /// Access issues
    /// </summary>
    public class Issues
    {

        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly Solutions _solutions;
        private readonly BreadcrumbAccessor _breadcrumbAccessor;
        private readonly IHttpContextAccessor _httpContextAccessor; // Inject IHttpContextAccessor

        public Issues(ApplicationDbContext context, UserManager<AppUser> userManager, Solutions solutions, BreadcrumbAccessor breadcrumbAccessor, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userManager = userManager;
            _solutions = solutions;
            _breadcrumbAccessor = breadcrumbAccessor;
            _httpContextAccessor = httpContextAccessor; // Assign IHttpContextAccessor
        }


        //public async Task<List<Issue_ReadVM>> GetEveryIssue()
        //{
        //    List<Issue> posts = await _context.Issues
        //       .Include(p => p.Scope)
        //       .Include(p => p.Author)
        //       .Include(f => f.ParentIssue)
        //       .Include(f => f.ChildIssues)
        //       .Include(f => f.BlockedContent)
        //       .Include(f => f.Solutions)
        //            .ThenInclude(s => s.Scope)
        //       .Include(f => f.Comments)
        //       .Include(f => f.IssueVotes)
        //       .Include(p => p.IssueCategories)
        //           .ThenInclude(fc => fc.Category)
        //       .ToListAsync();

        //    // Map the results to view models after retrieving from the database
        //    List<Issue_ReadVM> postsViewModel = await ConvertIssueEntitiesToVM(posts);

        //    return postsViewModel;
        //}

        public async Task<UserVote_Generic_ReadVM> GetIssueVoteStats(Guid? issueId = null)
        {

            if (!issueId.HasValue)
            {
                throw new ArgumentException("Issue ID must be provided", nameof(issueId));
            }

            int? userVote = null;

            // Use IHttpContextAccessor to access HttpContext
            var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext?.User);

            if (user != null)
            {
                var existingVote = await _context.IssueVotes
                    .OfType<IssueVote>()
                    .FirstOrDefaultAsync(v => v.UserID == user.Id && v.IssueID == issueId);

                if (existingVote != null)
                {
                    userVote = existingVote.VoteValue;
                }
            }

            // Retrieve vote data for the issue
            var votes = await _context.IssueVotes
                .OfType<IssueVote>()
                .Where(v => v.IssueID == issueId)
                .ToListAsync();

            double averageVote = votes.Any() ? votes.Average(v => v.VoteValue) : 0;
            int totalVotes = votes.Count;

            var model = new UserVote_Generic_ReadVM
            {
                ContentType = "Issue",
                ContentID = issueId.Value,
                AverageVote = averageVote,
                TotalVotes = totalVotes,
                UserVote = userVote
            };

            return model;

        }

        /// <summary>
        /// Converts a list of issues to a list of issueVMs
        /// </summary>
        public async Task<List<Issue_ReadVM>> ConvertIssueEntitiesToVM(List<Issue> posts)
        {
            var issueReadVMs = new List<Issue_ReadVM>();
            foreach (var p in posts)
            {
                var vm = new Issue_ReadVM
                {
                    IssueID = p.IssueID,
                    Title = p.Title,
                    Content = p.Content,
                    CreatedAt = p.CreatedAt,
                    ModifiedAt = p.ModifiedAt,
                    AuthorID = p.AuthorID,
                    VoteStats = await GetIssueVoteStats(p.IssueID),
                    ScopeID = p.ScopeID,
                    ParentIssueID = p.ParentIssueID,
                    ContentStatus = p.ContentStatus,
                    BlockedContentID = p.BlockedContentID,
                    Author = p.Author,
                    Scope = p.Scope,
                    Categories = GetIssuesCategories(p),
                    SubIssueCount = _context.Issues.Count(i => i.ParentIssueID == p.IssueID),
                    SubIssues = await GetIssuesSubIssuesAsync(p),
                    Solutions = await _solutions.GetIssuesSolutions(p),
                    BreadcrumbTags = await _breadcrumbAccessor.GetIssueBreadcrumbTags(p)
                };
                issueReadVMs.Add(vm);
            }
            return issueReadVMs;
        }

        /// <summary>
        /// Converts a single issue to a single issueVM 
        /// </summary>
        public async Task<Issue_ReadVM> ConvertIssueEntityToVM(Issue issue, ContentFilter filter)
        {
            List<Breadcrumb_ReadVM> breadcrumbTags = await _breadcrumbAccessor.GetIssueBreadcrumbTags(issue);

            return new Issue_ReadVM
            {
                IssueID = issue.IssueID,
                Title = issue.Title,
                Content = issue.Content,
                CreatedAt = issue.CreatedAt,
                Author = issue.Author,
                AuthorID = issue.AuthorID,
                Scope = issue.Scope,
                ScopeID = issue.ScopeID,
                ContentStatus = issue.ContentStatus,
                VoteStats = await GetIssueVoteStats(issue.IssueID),
                ParentIssue = await GetParentIssue(issue),
                ParentSolution = await GetParentSolution(issue),
                SubIssues = await GetIssuesSubIssuesAsync(issue),
                PaginatedSubIssues = await GetSubIssuesPagedAsync(issue.IssueID, 1, filter),
                PaginatedSolutions = await _solutions.GetSolutionsPagedAsync(issue.IssueID, 1, filter),
                SubIssueCount = _context.Issues.Count(i => i.ParentIssueID == issue.IssueID),
                BlockedContent = issue.BlockedContent,
                Solutions = await _solutions.GetIssuesSolutions(issue),
                Comments = issue.Comments,
                IssueVotes = issue.IssueVotes,
                IssueCategories = issue.IssueCategories,
                BreadcrumbTags = breadcrumbTags,
                Categories = GetIssuesCategories(issue)
            };
        }

        /// <summary>
        /// Gets the categories (tags) for a specific issue
        /// </summary>
        /// <param name="currentIssue"></param>
        /// <returns></returns>
        public  List<Category_ReadVM> GetIssuesCategories(Issue currentIssue)
        {
            return currentIssue.IssueCategories.Select(fc => new Category_ReadVM
            {
                CategoryID = fc.Category.CategoryID,
                CategoryName = fc.Category.CategoryName
            }).ToList() ?? new List<Category_ReadVM>();
        }

        /// <summary>
        /// Get the sub issues for an issue
        /// </summary>
        /// <param name="currentIssue"></param>
        /// <returns></returns>
        public async Task<List<Issue_ReadVM>> GetIssuesSubIssuesAsync(Issue currentIssue)
        {
            if (currentIssue.ChildIssues == null)
                return new List<Issue_ReadVM>();

            var subIssues = new List<Issue_ReadVM>();
            foreach (var child in currentIssue.ChildIssues)
            {
                var breadcrumbTags = await _breadcrumbAccessor.GetIssueBreadcrumbTags(child );

                subIssues.Add(new Issue_ReadVM
                {
                    IssueID = child.IssueID,
                    Title = child.Title,
                    Content = child.Content,
                    CreatedAt = child.CreatedAt,
                    ModifiedAt = child.ModifiedAt,
                    ContentStatus = child.ContentStatus,
                    AuthorID = child.AuthorID,
                    VoteStats = await GetIssueVoteStats(child.IssueID),
                    ScopeID = child.ScopeID,
                    ParentIssueID = child.ParentIssueID,
                    Scope = child.Scope,
                    SubIssueCount = _context.Issues.Count(i => i.ParentIssueID == child.IssueID),
                    BreadcrumbTags = breadcrumbTags
                });
            }
            return subIssues;
        }

        /// <summary>
        /// Get the sub issues for a solution
        /// </summary>
        /// <param name="currentSolution"></param>
        /// <returns></returns>
        public async Task<List<Issue_ReadVM>> GetSolutionSubIssues(Models.Database.Solution currentSolution)
        {
            if (currentSolution.ChildIssues == null)
                return new List<Issue_ReadVM>();

            var subIssues = new List<Issue_ReadVM>();
            foreach (var child in currentSolution.ChildIssues)
            {
                var breadcrumbTags = await _breadcrumbAccessor.GetIssueBreadcrumbTags(child);
                subIssues.Add(new Issue_ReadVM
                {
                    IssueID = child.IssueID,
                    Title = child.Title,
                    Content = child.Content,
                    CreatedAt = child.CreatedAt,
                    ContentStatus = child.ContentStatus,
                    ModifiedAt = child.ModifiedAt,
                    VoteStats = await GetIssueVoteStats(child.IssueID),
                    AuthorID = child.AuthorID,
                    ScopeID = child.ScopeID,
                    ParentIssueID = child.ParentIssueID,
                    Scope = child.Scope,
                    SubIssueCount = _context.Issues.Count(i => i.ParentIssueID == child.IssueID),
                    BreadcrumbTags = breadcrumbTags
                });
            }
            return subIssues;
        }


        /// <summary>
        /// Get the parent issue of an issue
        /// </summary>
        /// <param name="currentIssue"></param>
        /// <returns></returns>
        public  async Task<Issue_ReadVM?> GetParentIssue(Issue currentIssue)
        {
            if (currentIssue.ParentIssue == null)
                return null;

            var parentIssue = currentIssue.ParentIssue;

            List<Breadcrumb_ReadVM> breadcrumbTags = await _breadcrumbAccessor.GetIssueBreadcrumbTags(parentIssue);

            return currentIssue.ParentIssue == null ? null : new Issue_ReadVM
            {
                IssueID = currentIssue.ParentIssue.IssueID,
                Title = currentIssue.ParentIssue.Title,
                Content = currentIssue.ParentIssue.Content,
                CreatedAt = currentIssue.ParentIssue.CreatedAt,
                ModifiedAt = currentIssue.ParentIssue.ModifiedAt,
                AuthorID = currentIssue.ParentIssue.AuthorID,
                ContentStatus = currentIssue.ParentIssue.ContentStatus,
                ScopeID = currentIssue.ParentIssue.ScopeID,
                VoteStats = await GetIssueVoteStats(currentIssue.ParentIssue.IssueID),
                ParentIssueID = currentIssue.ParentIssue.ParentIssueID,
                ParentSolutionID = currentIssue.ParentIssue.ParentSolutionID,
                // Map other properties as needed
                Scope = currentIssue.ParentIssue.Scope,
                SubIssueCount = _context.Issues.Count(i => i.ParentIssueID == currentIssue.ParentIssue.IssueID),
                Solutions = await _solutions.GetIssuesSolutions(currentIssue.ParentIssue),
                BreadcrumbTags = breadcrumbTags
            };
        }

        /// <summary>
        /// Get the parent solution of an issue
        /// </summary>
        /// <param name="currentIssue"></param>
        /// <returns></returns>
        public async Task<Solution_ReadVM?> GetParentSolution(Issue currentIssue)
        {
            if (currentIssue.ParentSolution == null)
                return null;

            var parent = currentIssue.ParentSolution;

            var breadcrumbTags = await _breadcrumbAccessor.GetSolutionBreadcrumbTags(parent);


            return new Solution_ReadVM
            {
                SolutionID = parent.SolutionID,
                Title = parent.Title,
                Content = parent.Content,
                CreatedAt = parent.CreatedAt,
                ModifiedAt = parent.ModifiedAt,
                AuthorID = parent.AuthorID,
                ParentIssueID = parent.ParentIssueID,
                ContentStatus = parent.ContentStatus,
                ScopeID = parent.ScopeID,
                VoteStats = await _solutions.GetSolutionVoteStats(parent.SolutionID),
                Scope = parent.Scope,
                SubIssueCount = _context.Solutions.Count(s => s.ParentIssueID == parent.SolutionID),
                SubIssues = await GetSolutionSubIssues(parent),
                Categories = _solutions.GetSolutionCategories(parent),
                Comments = parent.Comments,
                SolutionCategories = parent.SolutionCategories,
                BreadcrumbTags = breadcrumbTags
                // Add other properties as needed
            };
        }

        /// <summary>
        /// Get the parent issue of a solution
        /// </summary>
        /// <param name="currentIssue"></param>
        /// <returns></returns>
        public async Task<Issue_ReadVM?> GetParentIssue(Models.Database.Solution currentSolution)
        {

            var breadcrumbTags = await _breadcrumbAccessor.GetIssueBreadcrumbTags(currentSolution.ParentIssue);

            // Issue == Parent Issue for solutions 
            return currentSolution.ParentIssue == null ? null : new Issue_ReadVM
            {
                IssueID = currentSolution.ParentIssue.IssueID,
                Title = currentSolution.ParentIssue.Title,
                Content = currentSolution.ParentIssue.Content,
                CreatedAt = currentSolution.ParentIssue.CreatedAt,
                ModifiedAt = currentSolution.ParentIssue.ModifiedAt,
                AuthorID = currentSolution.ParentIssue.AuthorID,
                ContentStatus = currentSolution.ParentIssue.ContentStatus,
                ScopeID = currentSolution.ParentIssue.ScopeID,
                VoteStats = await GetIssueVoteStats(currentSolution.ParentIssue.IssueID),
                ParentIssueID = currentSolution.ParentIssue.ParentIssueID,
                ParentSolutionID = currentSolution.ParentIssue.ParentSolutionID,
                // Map other properties as needed
                Scope = currentSolution.ParentIssue.Scope,
                Solutions = await _solutions.GetIssuesSolutions(currentSolution.ParentIssue),
                SubIssueCount = _context.Issues.Count(i => i.ParentIssueID == currentSolution.ParentIssue.IssueID),
                BreadcrumbTags = breadcrumbTags
            };
        }

        public async Task<PaginatedIssuesResponse> GetIssuesPagedAsync(int pageNumber, int pageSize = 3)
        {
            var query = _context.Issues
                .Include(p => p.Scope)
                .Include(p => p.Author)
                .Include(f => f.ParentIssue)
                .Include(f => f.ChildIssues)
                .Include(f => f.BlockedContent)
                .Include(f => f.Solutions)
                    .ThenInclude(s => s.Scope)
                .Include(f => f.Comments)
                .Include(f => f.IssueVotes)
                .Include(p => p.IssueCategories)
                    .ThenInclude(fc => fc.Category);

            int totalCount = await query.CountAsync();

            var pagedIssues = await query
                 .OrderByDescending(i => i.IssueVotes.Any()
                    ? i.IssueVotes.Average(v => v.VoteValue)
                    : 0)
                .ThenByDescending(i => i.CreatedAt) 
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var issuesVM = await ConvertIssueEntitiesToVM(pagedIssues);

            PaginatedIssuesResponse pr = new PaginatedIssuesResponse()
            {
                Issues = issuesVM,
                CurrentPage = pageNumber,
                TotalCount = totalCount,
                PageSize = pageSize
            };

            return pr;
        }

        public async Task<PaginatedIssuesResponse> GetSubIssuesPagedAsync(Guid issueId, int pageNumber, ContentFilter filter, int pageSize = 3)
        {
            // Query for sub-issues directly from the database
            var query = _context.Issues
                .Include(i => i.Scope)
                .Include(i => i.Author)
                .Include(i => i.IssueVotes) // Include votes for filtering
                .AsNoTracking() // For better performance when reading
                .Where(i => i.ParentIssueID == issueId);

            // Apply filters using the centralized filtering method
            query = CRUD.ContentFiltering.ApplyIssueFilters(query, filter);

            // Get total count after applying filters
            var totalCount = await query.CountAsync();

            Console.WriteLine($"Found {totalCount} sub-issues for parent issue {issueId} after applying filters");

            // Apply sorting and paging
            int k = 5; // tuning constant
            var paginatedChildIssues = await CRUD.ContentFiltering.ApplyWeightedScoreSorting(query, k)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var subIssues = new List<Issue_ReadVM>();

            foreach (var child in paginatedChildIssues)
            {
                var breadcrumbTags = await _breadcrumbAccessor.GetIssueBreadcrumbTags(child);

                subIssues.Add(new Issue_ReadVM
                {
                    IssueID = child.IssueID,
                    Title = child.Title,
                    Content = child.Content,
                    CreatedAt = child.CreatedAt,
                    ModifiedAt = child.ModifiedAt,
                    ContentStatus = child.ContentStatus,
                    AuthorID = child.AuthorID,
                    Author = child.Author,
                    VoteStats = await GetIssueVoteStats(child.IssueID),
                    ScopeID = child.ScopeID,
                    ParentIssueID = child.ParentIssueID,
                    Scope = child.Scope,
                    SubIssueCount = await _context.Issues.CountAsync(i => i.ParentIssueID == child.IssueID),
                    BreadcrumbTags = breadcrumbTags
                });
            }

            return new PaginatedIssuesResponse()
            {
                Issues = subIssues,
                CurrentPage = pageNumber,
                TotalCount = totalCount,
                PageSize = pageSize
            };
        }
    }

    /// <summary>
    /// Provides methods for managing and retrieving solutions associated with issues,  including converting solution
    /// entities to view models and retrieving solution categories.
    /// </summary>
    /// <remarks>This class acts as a service layer for handling operations related to solutions,  such as
    /// fetching solutions for a specific issue, converting solution entities to  view models, and retrieving associated
    /// categories. It relies on injected dependencies  for database access, user management, and breadcrumb
    /// generation.</remarks>
    public class Solutions
    {

        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly BreadcrumbAccessor _breadcrumbAccessor;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public Solutions(ApplicationDbContext context, UserManager<AppUser> userManager, IServiceProvider serviceProvider, BreadcrumbAccessor breadcrumbAccessor, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userManager = userManager;
            _serviceProvider = serviceProvider;
            _breadcrumbAccessor = breadcrumbAccessor;
            _httpContextAccessor = httpContextAccessor;
        }

        private Issues? _issues;
        private Issues Issues => _issues ??= _serviceProvider.GetRequiredService<Issues>();

        public async Task<List<Solution_ReadVM>> GetIssuesSolutions(Issue currentIssue)
        {
            if (currentIssue.Solutions == null)
                return new List<Solution_ReadVM>();

            var solutionVMs = new List<Solution_ReadVM>();
            foreach (var s in currentIssue.Solutions)
            {
                // Fix: Correctly check if the IssueID is not null or empty
                // Note: IssueID == ParentIssueID for solutions....
                var breadcrumbTags = await _breadcrumbAccessor.GetSolutionBreadcrumbTags(s);

                    solutionVMs.Add(new Solution_ReadVM
                {
                    SolutionID = s.SolutionID,
                    Title = s.Title,
                    Content = s.Content,
                    CreatedAt = s.CreatedAt,
                    ModifiedAt = s.ModifiedAt,
                    AuthorID = s.AuthorID,
                    ParentIssueID = s.ParentIssueID,
                    ContentStatus = s.ContentStatus,
                    BlockedContentID = s.BlockedContentID,
                    VoteStats = await GetSolutionVoteStats(s.SolutionID),
                    Scope = s.Scope,
                    ScopeID = s.ScopeID,
                    SubIssueCount = _context.Issues.Count(i => i.ParentSolutionID == s.SolutionID),
                    SolutionCategories = s.SolutionCategories,
                    Categories = GetSolutionCategories(s),
                    BreadcrumbTags = breadcrumbTags,
                });
            }
            return solutionVMs;
        }

        /// <summary>
        /// Converts a single solution to a single Solution_ReadVM
        /// </summary>
        public async Task<Solution_ReadVM> ConvertSolutionEntityToVM(Models.Database.Solution solution, ContentFilter filter)
        {
            // Fix: Correctly check if the IssueID is not null or empty
            // Note: IssueID == ParentIssueID for solutions....
            var breadcrumbTags = await _breadcrumbAccessor.GetSolutionBreadcrumbTags(solution);

            return new Solution_ReadVM
            {
                SolutionID = solution.SolutionID,
                Title = solution.Title,
                Content = solution.Content,
                CreatedAt = solution.CreatedAt,
                ModifiedAt = solution.ModifiedAt,
                AuthorID = solution.AuthorID,
                ParentIssueID = solution.ParentIssueID,
                ParentIssue = await Issues.GetParentIssue(solution),
                PaginatedSubIssues = await GetSubIssuesPagedAsync(solution.SolutionID, 1, filter),
                ContentStatus = solution.ContentStatus,
                BlockedContentID = solution.BlockedContentID,
                Scope = solution.Scope,
                ScopeID = solution.ScopeID,
                VoteStats = await GetSolutionVoteStats(solution.SolutionID),
                BlockedContent = solution.BlockedContent,
                Comments = solution.Comments,
                // You may want to include logic for categories if you have a many-to-many relationship
                Categories = GetSolutionCategories(solution),
                SubIssueCount = _context.Issues.Count(i => i.ParentSolutionID == solution.SolutionID),
                SubIssues = await Issues.GetSolutionSubIssues(solution),
                BreadcrumbTags = breadcrumbTags
                // Add other properties as needed
            };
        }


        /// <summary>
        /// Gets the categories for a solution 
        /// </summary>
        public List<Category_ReadVM> GetSolutionCategories(Models.Database.Solution currentSolution)
        {
            if (currentSolution?.SolutionCategories == null)
                return new List<Category_ReadVM>();

            return currentSolution.SolutionCategories
                .Where(fc => fc.Category != null)
                .Select(fc => new Category_ReadVM
                {
                    CategoryID = fc.Category.CategoryID,
                    CategoryName = fc.Category.CategoryName
                }).ToList();
        }

        public async Task<UserVote_Generic_ReadVM> GetSolutionVoteStats(Guid? solutionId = null)
        {

            if (!solutionId.HasValue)
            {
                throw new ArgumentException("Issue ID must be provided", nameof(solutionId));
            }

            int? userVote = null;

            // Use IHttpContextAccessor to access HttpContext
            var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext?.User);

            if (user != null)
            {
                var existingVote = await _context.SolutionVotes
                    .OfType<SolutionVote>()
                    .FirstOrDefaultAsync(v => v.UserID == user.Id && v.SolutionID == solutionId);

                if (existingVote != null)
                {
                    userVote = existingVote.VoteValue;
                }
            }

            // Retrieve vote data for the issue
            var votes = await _context.SolutionVotes
                .OfType<SolutionVote>()
                .Where(v => v.SolutionID == solutionId)
                .ToListAsync();

            double averageVote = votes.Any() ? votes.Average(v => v.VoteValue) : 0;
            int totalVotes = votes.Count;

            var model = new UserVote_Generic_ReadVM
            {
                ContentType = "Solution",
                ContentID = solutionId.Value,
                AverageVote = averageVote,
                TotalVotes = totalVotes,
                UserVote = userVote
            };

            return model;

        }

        
        /// <summary>
        /// Get paginated solutions for a specific issue
        /// </summary>
        /// <param name="issueId">The ID of the parent issue</param>
        /// <param name="pageNumber">The page number to retrieve</param>
        /// <param name="filter">Optional filter criteria for solutions</param>
        /// <param name="pageSize">The number of solutions per page</param>
        /// <returns>A paginated response with solutions</returns>
        public async Task<PaginatedSolutionsResponse> GetSolutionsPagedAsync(Guid issueId, int pageNumber, ContentFilter filter, int pageSize = 3)
        {
            // Query for solutions for the given issue
            var query = _context.Solutions
                .Include(s => s.Scope)
                .Include(s => s.Author)
                .Include(s => s.SolutionCategories)
                    .ThenInclude(sc => sc.Category)
                .Include(s => s.SolutionVotes) // Include votes for filtering
                .AsNoTracking() // For better performance when reading
                .Where(s => s.ParentIssueID == issueId);

            // Apply filters using the centralized filtering method
            query = CRUD.ContentFiltering.ApplySolutionFilters(query, filter);

            // Get total count after applying filters
            var totalCount = await query.CountAsync();

            Console.WriteLine($"Found {totalCount} solutions for issue {issueId} after applying filters");

            // Apply sorting and paging
            int k = 5; // tuning constant
            var paginatedSolutions = await CRUD.ContentFiltering.ApplyWeightedScoreSorting(query, k)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var solutionVMs = new List<Solution_ReadVM>();

            foreach (var solution in paginatedSolutions)
            {
                var breadcrumbTags = await _breadcrumbAccessor.GetSolutionBreadcrumbTags(solution);

                solutionVMs.Add(new Solution_ReadVM
                {
                    SolutionID = solution.SolutionID,
                    Title = solution.Title,
                    Content = solution.Content,
                    CreatedAt = solution.CreatedAt,
                    ModifiedAt = solution.ModifiedAt,
                    AuthorID = solution.AuthorID,
                    Author = solution.Author,
                    ParentIssueID = solution.ParentIssueID,
                    ContentStatus = solution.ContentStatus,
                    BlockedContentID = solution.BlockedContentID,
                    VoteStats = await GetSolutionVoteStats(solution.SolutionID),
                    Scope = solution.Scope,
                    ScopeID = solution.ScopeID,
                    SubIssueCount = await _context.Issues.CountAsync(i => i.ParentSolutionID == solution.SolutionID),
                    Categories = GetSolutionCategories(solution),
                    SolutionCategories = solution.SolutionCategories,
                    BreadcrumbTags = breadcrumbTags
                });
            }

            return new PaginatedSolutionsResponse()
            {
                Solutions = solutionVMs,
                CurrentPage = pageNumber,
                TotalCount = totalCount,
                PageSize = pageSize
            };
        }
        public async Task<PaginatedIssuesResponse> GetSubIssuesPagedAsync(Guid solutionId, int pageNumber, ContentFilter filter, int pageSize = 3)
        {
            // Query for sub-issues directly from the database
            var query = _context.Issues
                .Include(i => i.Scope)
                .Include(i => i.Author)
                .Include(i => i.IssueVotes) // Include votes for filtering
                .AsNoTracking() // For better performance when reading
                .Where(i => i.ParentSolutionID == solutionId);

            // Apply filters using the centralized filtering method
            query = CRUD.ContentFiltering.ApplyIssueFilters(query, filter);

            // Get total count after applying filters
            var totalCount = await query.CountAsync();

            Console.WriteLine($"Found {totalCount} sub-issues for parent solution {solutionId} after applying filters");

            // Apply sorting and paging
            int k = 5; // tuning constant
            var paginatedChildIssues = await CRUD.ContentFiltering.ApplyWeightedScoreSorting(query, k)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var subIssues = new List<Issue_ReadVM>();

            foreach (var child in paginatedChildIssues)
            {
                var breadcrumbTags = await _breadcrumbAccessor.GetIssueBreadcrumbTags(child);

                subIssues.Add(new Issue_ReadVM
                {
                    IssueID = child.IssueID,
                    Title = child.Title,
                    Content = child.Content,
                    CreatedAt = child.CreatedAt,
                    ModifiedAt = child.ModifiedAt,
                    ContentStatus = child.ContentStatus,
                    AuthorID = child.AuthorID,
                    Author = child.Author,
                    VoteStats = await Issues.GetIssueVoteStats(child.IssueID),
                    ScopeID = child.ScopeID,
                    ParentSolutionID = child.ParentSolutionID,
                    Scope = child.Scope,
                    SubIssueCount = await _context.Issues.CountAsync(i => i.ParentIssueID == child.IssueID),
                    BreadcrumbTags = breadcrumbTags
                });
            }

            return new PaginatedIssuesResponse()
            {
                Issues = subIssues,
                CurrentPage = pageNumber,
                TotalCount = totalCount,
                PageSize = pageSize
            };
        }
    }
}

