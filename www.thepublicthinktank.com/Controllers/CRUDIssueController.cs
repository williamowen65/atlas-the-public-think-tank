using atlas_the_public_think_tank.Data;
using atlas_the_public_think_tank.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace atlas_the_public_think_tank.Controllers
{

    /// <summary>
    /// This controller handles all CRUD operations for 
    /// the Issue, Solutions, Comments, UserHistory, and Voting
    /// </summary>
    [Authorize]
    public class CRUDIssueController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public CRUDIssueController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// This method is used to return the create issue page.
        /// </summary>
        [Route("/create-issue")]
        public IActionResult CreateIssue()
        {

            Issue_CreateVM newIssue = new()
            {
                Categories = _context.Categories.ToList(),
                Scopes = _context.Scopes.ToList(),
            };

            return View(newIssue);
        }


        /// <summary>
        /// This method is used to create a new issue post.
        /// </summary>
        /// <param name="model"></param>
        [HttpPost]
        [Route("/create-issue")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateIssue(Issue_CreateVM model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);


                var issuePost = new Issue
                {
                    Title = model.Title,
                    Content = model.Content,
                    ScopeID = model.ScopeID,
                    ParentIssueID = model.ParentIssueID,
                    ContentStatus = model.ContentStatus,
                    AuthorID = user.Id,
                    CreatedAt = DateTime.UtcNow
                };

                var entry = _context.Issues.Add(issuePost);
                await _context.SaveChangesAsync(); // Save to generate the IssueID

                // Now add the category relationships
                if (model.SelectedCategoryIds != null && model.SelectedCategoryIds.Any())
                {
                    foreach (int categoryId in model.SelectedCategoryIds)
                    {
                        var issueCategory = new IssueCategory
                        {
                            IssueID = entry.Entity.IssueID,
                            CategoryID = categoryId
                        };
                        _context.IssueCategories.Add(issueCategory);
                    }


                    // Save the User History (todo)

                    await _context.SaveChangesAsync();
                }

                return RedirectToAction("Index", "Home");
            }

            // If we got this far, something failed, redisplay form
            // Repopulate the dropdown data
            model.Categories = _context.Categories.ToList();
            model.Scopes = _context.Scopes.ToList();
            return View(model);
        }


        /// <summary>
        /// This method is used to return all issue posts.
        /// </summary>
        [HttpGet]
        [Route("/api/posts")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllIssues()
        {
            var posts = await _context.Issues
                .Include(p => p.Scope)
                .Include(p => p.Author)
                .Include(p => p.IssueCategories)
                    .ThenInclude(fc => fc.Category)
                .Select(p => new Issue_ReadVM
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
                    Categories = p.IssueCategories.Select(fc => new Category_ReadVM
                    {
                        CategoryID = fc.Category.CategoryID,
                        CategoryName = fc.Category.CategoryName
                    })
                    .ToList()
                })
                .ToListAsync();

            return Ok(posts);
        }


        /// <summary>
        /// Returns a HTML page for a specific issue
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("/issue/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> ReadIssue(int id)
        {
            var issue = await _context.Issues
            .Include(f => f.Author)
            .Include(f => f.Scope)
            .Include(f => f.ParentIssue)
            .Include(f => f.ChildIssues)
            .Include(f => f.BlockedContent)
            .Include(f => f.Solutions)
            .Include(f => f.Comments)
            .Include(f => f.UserVotes)
            .Include(f => f.IssueCategories)
                .ThenInclude(fc => fc.Category)
            .FirstOrDefaultAsync(f => f.IssueID == id);

            if (issue == null)
            {
                return NotFound();
            }

            // Update the mapping of child issues to handle potential null values
            var childIssueVMs = issue.ChildIssues?
                .Select(child => new Issue_ReadVM
                {
                    IssueID = child.IssueID,
                    Title = child.Title,
                    Content = child.Content,
                    CreatedAt = child.CreatedAt,
                    ModifiedAt = child.ModifiedAt,
                    AuthorID = child.AuthorID,
                    ScopeID = child.ScopeID,
                    ParentIssueID = child.ParentIssueID,
                    // Map other properties as needed
                })
                .ToList() ?? new List<Issue_ReadVM>(); // Ensure a non-null list is assigned


            // Then map to the view model
            var issueVM = new Issue_ReadVM
            {
                IssueID = issue.IssueID,
                Title = issue.Title,
                Content = issue.Content,
                CreatedAt = issue.CreatedAt,

                // These navigation properties come directly from the query results
                Author = issue.Author,
                Scope = issue.Scope,
                ParentIssueVM = issue.ParentIssue == null ? null : new Issue_ReadVM
                {
                    IssueID = issue.ParentIssue.IssueID,
                    Title = issue.ParentIssue.Title,
                    Content = issue.ParentIssue.Content,
                    CreatedAt = issue.ParentIssue.CreatedAt,
                    ModifiedAt = issue.ParentIssue.ModifiedAt,
                    AuthorID = issue.ParentIssue.AuthorID,
                    ScopeID = issue.ParentIssue.ScopeID,
                    ParentIssueID = issue.ParentIssue.ParentIssueID,
                    // Map other properties as needed
                },

                SubIssues = childIssueVMs, 
                BlockedContent = issue.BlockedContent,
                Solutions = issue.Solutions,
                Comments = issue.Comments,
                UserVotes = issue.UserVotes,
                IssueCategories = issue.IssueCategories,

                // This is transformed from the many-to-many relationship
                Categories = issue.IssueCategories.Select(fc => new Category_ReadVM
                {
                    CategoryID = fc.Category.CategoryID,
                    CategoryName = fc.Category.CategoryName
                }).ToList()
            };

            return View(issueVM);
        }



        /// <summary>
        /// This method is used to return all categories.
        /// </summary>
        [HttpGet]
        [Route("/api/categories")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllCategories()
        {
            var posts = await _context.Categories
                    .Select(p => new Category_ReadVM
                    {
                        CategoryID = p.CategoryID,
                        CategoryName = p.CategoryName
                    })
                .ToListAsync();


            return Ok(posts);
        }


        /// <summary>
        /// This method is used to return the sidebar with all categories.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("/api/sidebar")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSidebar()
        {
            var categories = await _context.Categories
                .Select(p => new Category_ReadVM
                {
                    CategoryID = p.CategoryID,
                    CategoryName = p.CategoryName
                })
                .ToListAsync();

            return PartialView("~/Views/Issue/_left-sidebar-container.cshtml", categories);
        }

        /// <summary>
        /// This method is used to cast a vote on a issue post.
        /// </summary>
        /// <param name="model"></param>
        [HttpPost]
        [Route("/issue/vote")]
        public async Task<IActionResult> IssueVote(UserVote_Issue_CreateVM model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid vote data" });
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                {
                    return Json(new { success = false, message = "You must login in order to vote" });
                }

                // Check if the user has already voted on this issue
                var existingVote = await _context.UserVotes
                    .OfType<IssueVote>()
                    .FirstOrDefaultAsync(v => v.UserID == user.Id && v.IssueID == model.IssueID);

                if (existingVote != null)
                {
                    // Update existing vote
                    existingVote.VoteValue = (int)model.VoteValue;
                    existingVote.ModifiedAt = DateTime.UtcNow;
                    _context.UserVotes.Update(existingVote);
                }
                else
                {
                    // Create new vote
                    IssueVote vote = new IssueVote
                    {
                        User = user,
                        UserID = user.Id,
                        IssueID = model.IssueID,
                        VoteValue = (int)model.VoteValue,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.UserVotes.Add(vote);
                }

                await _context.SaveChangesAsync();

                // get updated stats
                double average = await _context.UserVotes
                     .OfType<IssueVote>()
                     .Where(v => v.IssueID == model.IssueID)
                     .AverageAsync(v => v.VoteValue);
                int count = await _context.UserVotes
                     .OfType<IssueVote>()
                     .Where(v => v.IssueID == model.IssueID)
                     .CountAsync();

                return Json(new
                {
                    success = true,
                    message = "Vote saved successfully",
                    average,
                    count
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        /// <summary>
        /// This method is used to get the vote dial for a specific issue.
        /// </summary>
        /// <param name="issueId"></param>
        /// <returns>HTML</returns>
        /// <throws>NotFound</throws> <--- Not sure if this is the right way to do this
        [AllowAnonymous]
        [Route("/Issue/GetVoteDial")]
        public async Task<IActionResult> GetVoteDial(int issueId)
        {

            int? userVote = null;
            // Check if the issueId exists in the database
            var issueExists = _context.Issues.Any(f => f.IssueID == issueId);
            if (!issueExists)
            {
                return NotFound(new { message = "Issue not found" });
            }

            var user = await _userManager.GetUserAsync(User);

            if (user != null)
            {
                // Get a possible vote value from the db
                var existingVote = await _context.UserVotes
                    .OfType<IssueVote>()
                    .FirstOrDefaultAsync(v => v.UserID == user.Id && v.IssueID == issueId);

                if (existingVote != null)
                {
                    userVote = existingVote.VoteValue;
                }
            }


            // Retrieve all user votes for the specified issue
            var userVotes = await _context.UserVotes
                .OfType<IssueVote>()
                   .Where(v => v.IssueID == issueId)
                .Select(v => new
                {
                    v.UserID,
                    v.VoteValue
                })
                .ToListAsync();


            // Get vote data for the issue
            UserVote_Issue_ReadVM m = new UserVote_Issue_ReadVM
            {
                IssueID = issueId,
                AverageVote = userVotes.Any() ? userVotes.Average(p => p.VoteValue) : 0,
                TotalVotes = userVotes.Count,
                UserVote = userVote,

            };

            // Return the partial view with the vote data model
            return PartialView("~/Views/Issue/_voteDial.cshtml", m);
        }


        [Route("/issue/create")]
        public async Task<IActionResult> CreateSolution() 
        {

            return View();
        }
        


    }

}