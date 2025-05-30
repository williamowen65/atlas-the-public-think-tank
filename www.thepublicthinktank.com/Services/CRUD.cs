﻿using atlas_the_public_think_tank.Data;
using atlas_the_public_think_tank.Migrations;
using atlas_the_public_think_tank.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace atlas_the_public_think_tank.Services
{

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
    }

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
        public async Task<List<Breadcrumb_ReadVM>> GetSolutionBreadcrumbTags(Models.Solution solution)
        {
            return solution.IssueID != Guid.Empty
             ? await GetContentBreadcrumb(
                 solution.IssueID
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
                .Include(s => s.Issue)
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
                if (solution.IssueID != Guid.Empty)
                {
                    var parentBreadcrumbs = await GetContentBreadcrumb(solution.IssueID);
                    breadcrumbs.AddRange(parentBreadcrumbs);
                }

                return breadcrumbs;
            }

            // If we get here, no content was found with the given ID
            return breadcrumbs;
        }
    }


    public class Issues
    {

        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly Solutions _solutions;
        private readonly BreadcrumbAccessor _breadcrumbAccessor;

        public Issues(ApplicationDbContext context, UserManager<AppUser> userManager, Solutions solutions, BreadcrumbAccessor breadcrumbAccessor)
        {
            _context = context;
            _userManager = userManager;
            _solutions = solutions;
            _breadcrumbAccessor = breadcrumbAccessor;
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
            List<Issue_ReadVM> postsViewModel = await ConvertIssueEntitiesToVM(posts);

            return postsViewModel;
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
                    ScopeID = p.ScopeID,
                    ParentIssueID = p.ParentIssueID,
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
        public async Task<Issue_ReadVM> ConvertIssueEntityToVM(Issue issue)
        {
            List<Breadcrumb_ReadVM> breadcrumbTags = await _breadcrumbAccessor.GetIssueBreadcrumbTags(issue);

            return new Issue_ReadVM
            {
                IssueID = issue.IssueID,
                Title = issue.Title,
                Content = issue.Content,
                CreatedAt = issue.CreatedAt,
                Author = issue.Author,
                Scope = issue.Scope,
                ParentIssue = await GetParentIssue(issue),
                ParentSolution = await GetParentSolution(issue),
                SubIssues = await GetIssuesSubIssuesAsync(issue),
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

        public  List<Category_ReadVM> GetIssuesCategories(Issue currentIssue)
        {
            return currentIssue.IssueCategories.Select(fc => new Category_ReadVM
            {
                CategoryID = fc.Category.CategoryID,
                CategoryName = fc.Category.CategoryName
            }).ToList() ?? new List<Category_ReadVM>();
        }

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

        public async Task<List<Issue_ReadVM>> GetSolutionSubIssues(Models.Solution currentSolution)
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
                    ModifiedAt = child.ModifiedAt,
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
                ScopeID = currentIssue.ParentIssue.ScopeID,
                ParentIssueID = currentIssue.ParentIssue.ParentIssueID,
                ParentSolutionID = currentIssue.ParentIssue.ParentSolutionID,
                // Map other properties as needed
                Scope = currentIssue.ParentIssue.Scope,
                SubIssueCount = _context.Issues.Count(i => i.ParentIssueID == currentIssue.ParentIssue.IssueID),
                Solutions = await _solutions.GetIssuesSolutions(currentIssue.ParentIssue),
                BreadcrumbTags = breadcrumbTags
            };
        }

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
                IssueID = parent.IssueID,
                ScopeID = parent.ScopeID,
                Scope = parent.Scope,
                SubIssueCount = _context.Solutions.Count(s => s.IssueID == parent.SolutionID),
                SubIssues = await GetSolutionSubIssues(parent),
                Categories = _solutions.GetSolutionCategories(parent),
                Comments = parent.Comments,
                SolutionCategories = parent.SolutionCategories,
                BreadcrumbTags = breadcrumbTags
                // Add other properties as needed
            };
        }

        public async Task<Issue_ReadVM?> GetParentIssue(Models.Solution currentSolution)
        {

            var breadcrumbTags = await _breadcrumbAccessor.GetIssueBreadcrumbTags(currentSolution.Issue);

            // Issue == Parent Issue for solutions 
            return currentSolution.Issue == null ? null : new Issue_ReadVM
            {
                IssueID = currentSolution.Issue.IssueID,
                Title = currentSolution.Issue.Title,
                Content = currentSolution.Issue.Content,
                CreatedAt = currentSolution.Issue.CreatedAt,
                ModifiedAt = currentSolution.Issue.ModifiedAt,
                AuthorID = currentSolution.Issue.AuthorID,
                ScopeID = currentSolution.Issue.ScopeID,
                ParentIssueID = currentSolution.Issue.ParentIssueID,
                ParentSolutionID = currentSolution.Issue.ParentSolutionID,
                // Map other properties as needed
                Scope = currentSolution.Issue.Scope,
                Solutions = await _solutions.GetIssuesSolutions(currentSolution.Issue),
                SubIssueCount = _context.Issues.Count(i => i.ParentIssueID == currentSolution.Issue.IssueID),
                BreadcrumbTags = breadcrumbTags
            };
        }
    }
    public class Solutions
    {

        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly BreadcrumbAccessor _breadcrumbAccessor;

        public Solutions(ApplicationDbContext context, UserManager<AppUser> userManager, IServiceProvider serviceProvider, BreadcrumbAccessor breadcrumbAccessor)
        {
            _context = context;
            _userManager = userManager;
            _serviceProvider = serviceProvider;
            _breadcrumbAccessor = breadcrumbAccessor;
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
                    IssueID = s.IssueID,
                    ContentStatus = s.ContentStatus,
                    BlockedContentID = s.BlockedContentID,
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
        public async Task<Solution_ReadVM> ConvertSolutionEntityToVM(Models.Solution solution)
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
                IssueID = solution.IssueID,
                Issue = await Issues.GetParentIssue(solution),
                ContentStatus = solution.ContentStatus,
                BlockedContentID = solution.BlockedContentID,
                Scope = solution.Scope,
                ScopeID = solution.ScopeID,
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
        public List<Category_ReadVM> GetSolutionCategories(Models.Solution currentSolution)
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

