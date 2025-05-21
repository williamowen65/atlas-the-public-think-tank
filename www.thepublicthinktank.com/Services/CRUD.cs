using atlas_the_public_think_tank.Data;
using atlas_the_public_think_tank.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace atlas_the_public_think_tank.Services
{

    public class CRUD
    {
        public Issues Issues { get; }
        public Solutions Solutions { get; }

        public CRUD(Issues issues, Solutions solutions)
        {
            Issues = issues;
            Solutions = solutions;
        }
    }


    public class Issues
    {

        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly Solutions _solutions;

        public Issues(ApplicationDbContext context, UserManager<AppUser> userManager, Solutions solutions)
        {
            _context = context;
            _userManager = userManager;
            _solutions = solutions;
        }


        public async Task<List<Issue_ReadVM>> GetEveryIssue()
        {
            List<Issue> posts = await _context.Issues
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
               .ToListAsync();

            // Map the results to view models after retrieving from the database
            List<Issue_ReadVM> postsViewModel = ConvertIssueEntitiesToVM(posts);

            return postsViewModel;
        }

        /// <summary>
        /// Converts a list of issues to a list of issueVMs
        /// </summary>
        public  List<Issue_ReadVM> ConvertIssueEntitiesToVM(List<Issue> posts)
        {
            return posts.Select(p => new Issue_ReadVM
            {
                IssueID = p.IssueID,
                Title = p.Title,
                Content = p.Content,
                CreatedAt = p.CreatedAt,
                ModifiedAt = p.ModifiedAt,
                AuthorID = p.AuthorID,
                ScopeID = p.ScopeID,
                ParentIssueID = p.ParentIssueID,
                BlockedContentID = p.BlockedContentID,
                Author = p.Author,
                Scope = p.Scope,
                Categories = GetIssuesCategories(p),
                SubIssueCount = _context.Issues.Count(i => i.ParentIssueID == p.IssueID),
                SubIssues = GetIssuesSubIssues(p),
                Solutions = _solutions.GetIssuesSolutions(p),
            }).ToList();
        }

        /// <summary>
        /// Converts a single issue to a single issueVM 
        /// </summary>
        public Issue_ReadVM ConvertIssueEntityToVM(Issue issue)
        {
            return new Issue_ReadVM
            {
                IssueID = issue.IssueID,
                Title = issue.Title,
                Content = issue.Content,
                CreatedAt = issue.CreatedAt,

                // These navigation properties come directly from the query results
                Author = issue.Author,
                Scope = issue.Scope,
                ParentIssue = GetParentIssue(issue),
                ParentSolution = GetParentSolution(issue),

                SubIssues = GetIssuesSubIssues(issue),
                SubIssueCount = _context.Issues.Count(i => i.ParentIssueID == issue.IssueID),
                BlockedContent = issue.BlockedContent,
                Solutions = _solutions.GetIssuesSolutions(issue),
                Comments = issue.Comments,
                IssueVotes = issue.IssueVotes,
                IssueCategories = issue.IssueCategories,

                // This is transformed from the many-to-many relationship
                Categories = GetIssuesCategories(issue)

            };
        }

        public  List<Category_ReadVM> GetIssuesCategories(Issue currentIssue)
        {
            return currentIssue.IssueCategories.Select(fc => new Category_ReadVM
            {
                CategoryID = fc.Category.CategoryID,
                CategoryName = fc.Category.CategoryName
            }).ToList() ?? new List<Category_ReadVM>();
        }

        public  List<Issue_ReadVM> GetIssuesSubIssues(Issue currentIssue)
        {
            return currentIssue.ChildIssues == null
                        ? new List<Issue_ReadVM>()
                        : currentIssue.ChildIssues.Select(child => new Issue_ReadVM
                        {
                            IssueID = child.IssueID,
                            Title = child.Title,
                            Content = child.Content,
                            CreatedAt = child.CreatedAt,
                            ModifiedAt = child.ModifiedAt,
                            AuthorID = child.AuthorID,
                            ScopeID = child.ScopeID,
                            ParentIssueID = child.ParentIssueID,
                            Scope = child.Scope,
                            SubIssueCount = _context.Issues.Count(i => i.ParentIssueID == child.IssueID),
                        })
                        .ToList();
        }

        public List<Issue_ReadVM> GetSolutionSubIssues(Solution currentSolution)
        {
            return currentSolution.ChildIssues == null
                        ? new List<Issue_ReadVM>()
                        : currentSolution.ChildIssues.Select(child => new Issue_ReadVM
                        {
                            IssueID = child.IssueID,
                            Title = child.Title,
                            Content = child.Content,
                            CreatedAt = child.CreatedAt,
                            ModifiedAt = child.ModifiedAt,
                            AuthorID = child.AuthorID,
                            ScopeID = child.ScopeID,
                            ParentIssueID = child.ParentIssueID,
                            Scope = child.Scope,
                            SubIssueCount = _context.Issues.Count(i => i.ParentIssueID == child.IssueID),
                        })
                        .ToList();
        }


        public  Issue_ReadVM? GetParentIssue(Issue currentIssue)
        {
            return currentIssue.ParentIssue == null ? null : new Issue_ReadVM
            {
                IssueID = currentIssue.ParentIssue.IssueID,
                Title = currentIssue.ParentIssue.Title,
                Content = currentIssue.ParentIssue.Content,
                CreatedAt = currentIssue.ParentIssue.CreatedAt,
                ModifiedAt = currentIssue.ParentIssue.ModifiedAt,
                AuthorID = currentIssue.ParentIssue.AuthorID,
                ScopeID = currentIssue.ParentIssue.ScopeID,
                ParentIssueID = currentIssue.ParentIssue.ParentIssueID,
                ParentSolutionID = currentIssue.ParentIssue.ParentSolutionID,
                // Map other properties as needed
                Scope = currentIssue.ParentIssue.Scope,
                SubIssueCount = _context.Issues.Count(i => i.ParentIssueID == currentIssue.ParentIssue.IssueID),
                Solutions = _solutions.GetIssuesSolutions(currentIssue.ParentIssue)
            };
        }

        public Solution_ReadVM? GetParentSolution(Issue currentIssue)
        {
            if (currentIssue.ParentSolution == null)
                return null;

            var parent = currentIssue.ParentSolution;
            return new Solution_ReadVM
            {
                SolutionID = parent.SolutionID,
                Title = parent.Title,
                Content = parent.Content,
                CreatedAt = parent.CreatedAt,
                ModifiedAt = parent.ModifiedAt,
                AuthorID = parent.AuthorID,
                IssueID = parent.IssueID,
                ScopeID = parent.ScopeID,
                Scope = parent.Scope,
                SubIssueCount = _context.Solutions.Count(s => s.IssueID == parent.SolutionID),
                SubIssues = GetSolutionSubIssues(parent),
                Categories = _solutions.GetSolutionCategories(parent),
                Comments = parent.Comments,
                SolutionCategories = parent.SolutionCategories
                // Add other properties as needed
            };
        }

        public Issue_ReadVM? GetParentIssue(Solution currentIssue)
        {
            // Issue == Parent Issue for solutions 
            return currentIssue.Issue == null ? null : new Issue_ReadVM
            {
                IssueID = currentIssue.Issue.IssueID,
                Title = currentIssue.Issue.Title,
                Content = currentIssue.Issue.Content,
                CreatedAt = currentIssue.Issue.CreatedAt,
                ModifiedAt = currentIssue.Issue.ModifiedAt,
                AuthorID = currentIssue.Issue.AuthorID,
                ScopeID = currentIssue.Issue.ScopeID,
                ParentIssueID = currentIssue.Issue.ParentIssueID,
                ParentSolutionID = currentIssue.Issue.ParentSolutionID,
                // Map other properties as needed
                Scope = currentIssue.Issue.Scope,
                Solutions = _solutions.GetIssuesSolutions(currentIssue.Issue),
                SubIssueCount = _context.Issues.Count(i => i.ParentIssueID == currentIssue.Issue.IssueID),
            };
        }

    }
    public class Solutions
    {

        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IServiceProvider _serviceProvider;

        public Solutions(ApplicationDbContext context, UserManager<AppUser> userManager, IServiceProvider serviceProvider)
        {
            _context = context;
            _userManager = userManager;
            _serviceProvider = serviceProvider;
        }

        private Issues? _issues;
        private Issues Issues => _issues ??= _serviceProvider.GetRequiredService<Issues>();

        public List<Solution_ReadVM> GetIssuesSolutions(Issue currentIssue)
        {

            if (currentIssue.Solutions == null)
                return new List<Solution_ReadVM>();

            return currentIssue.Solutions.Select(s => new Solution_ReadVM
            {
                SolutionID = s.SolutionID,
                Title = s.Title,
                Content = s.Content,
                CreatedAt = s.CreatedAt,
                ModifiedAt = s.ModifiedAt,
                AuthorID = s.AuthorID,
                IssueID = s.IssueID,
                ContentStatus = s.ContentStatus,
                BlockedContentID = s.BlockedContentID,
                Scope = s.Scope,
                ScopeID = s.ScopeID,
                SubIssueCount = _context.Issues.Count(i => i.ParentSolutionID == s.SolutionID),
                SolutionCategories = s.SolutionCategories,
                Categories = GetSolutionCategories(s)

            }).ToList() ?? new List<Solution_ReadVM>();
        }

        /// <summary>
        /// Converts a single solution to a single Solution_ReadVM
        /// </summary>
        public Solution_ReadVM ConvertSolutionEntityToVM(Solution solution)
        {
            return new Solution_ReadVM
            {
                SolutionID = solution.SolutionID,
                Title = solution.Title,
                Content = solution.Content,
                CreatedAt = solution.CreatedAt,
                ModifiedAt = solution.ModifiedAt,
                AuthorID = solution.AuthorID,
                IssueID = solution.IssueID,
                Issue = Issues.GetParentIssue(solution),
                ContentStatus = solution.ContentStatus,
                BlockedContentID = solution.BlockedContentID,
                Scope = solution.Scope,
                ScopeID = solution.ScopeID,
                BlockedContent = solution.BlockedContent,
                Comments = solution.Comments,
                // You may want to include logic for categories if you have a many-to-many relationship
                Categories = GetSolutionCategories(solution),
                SubIssueCount = _context.Issues.Count(i => i.ParentSolutionID == solution.SolutionID),
                SubIssues = Issues.GetSolutionSubIssues(solution)
                // Add other properties as needed
            };
        }


        /// <summary>
        /// Gets the categories for a solution 
        /// </summary>
        public List<Category_ReadVM> GetSolutionCategories(Solution currentSolution)
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




    }

}

