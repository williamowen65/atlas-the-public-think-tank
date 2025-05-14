using atlas_the_public_think_tank.Data;
using atlas_the_public_think_tank.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
        public IActionResult CreateIssue(int? parentIssueID)
        {

            Issue_CreateVM newIssue = new()
            {
                Categories = _context.Categories.ToList(),
                Scopes = _context.Scopes.ToList(),
                ParentIssueID = parentIssueID
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
                .Include(f => f.ParentIssue)
                .Include(f => f.ChildIssues)
                .Include(f => f.BlockedContent)
                .Include(f => f.Solutions)
                .Include(f => f.Comments)
                .Include(f => f.UserVotes)
                .Include(p => p.IssueCategories)
                    .ThenInclude(fc => fc.Category)
                .ToListAsync();

            // Map the results to view models after retrieving from the database
            var postsViewModel = posts.Select(p => new Issue_ReadVM
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
                    .ToList(),
                SubIssueCount = _context.Issues.Count(i => i.ParentIssueID == p.IssueID),
                SubIssues = p.ChildIssues == null
                        ? new List<Issue_ReadVM>()
                        : p.ChildIssues.Select(child => new Issue_ReadVM
                        {
                            IssueID = child.IssueID,
                            Title = child.Title,
                            Content = child.Content,
                            CreatedAt = child.CreatedAt,
                            ModifiedAt = child.ModifiedAt,
                            AuthorID = child.AuthorID,
                            ScopeID = child.ScopeID,
                            ParentIssueID = child.ParentIssueID,
                            SubIssueCount = _context.Issues.Count(i => i.ParentIssueID == child.IssueID),
                        })
                        .ToList(),
                Solutions = p.Solutions
            }).ToList();

            return Ok(postsViewModel);
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

            Console.WriteLine("hi");

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
                    Scope = child.Scope,
                    SubIssueCount = _context.Issues.Count(i => i.ParentIssueID == child.IssueID)

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
                    Scope = issue.Scope,
                    SubIssueCount = _context.Issues.Count(i => i.ParentIssueID == issue.ParentIssue.IssueID)
                },

                SubIssues = childIssueVMs,
                SubIssueCount = _context.Issues.Count(i => i.ParentIssueID == issue.IssueID),
                BlockedContent = issue.BlockedContent,
                //Solutions = issue.Solutions,
                SolutionVM = issue.Solutions.Select(s => new Solution_ReadVM
                {
                    SolutionID = s.SolutionID,
                    Title = s.Title,
                    Content = s.Content,
                    CreatedAt = s.CreatedAt,
                    ModifiedAt = s.ModifiedAt,
                    AuthorID = s.AuthorID,
                    IssueID = s.IssueID,
                    ContentStatus = s.ContentStatus,
                    BlockedContentID = s.BlockedContentID
                }).ToList() ?? new List<Solution_ReadVM>(),
                Comments = issue.Comments,
                UserVotes = issue.UserVotes,
                IssueCategories = issue.IssueCategories,

                // This is transformed from the many-to-many relationship
                Categories = issue.IssueCategories?.Select(fc => new Category_ReadVM
                {
                    CategoryID = fc.Category.CategoryID,
                    CategoryName = fc.Category.CategoryName
                }).ToList() ?? new List<Category_ReadVM>()

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
/// This method is used to get the vote dial for issues, solutions, or comments.
/// At least one ID parameter must be provided.
/// </summary>
/// <param name="issueId">Optional issue ID</param>
/// <param name="solutionId">Optional solution ID</param>
/// <param name="commentId">Optional comment ID</param>
/// <returns>HTML partial view of vote dial</returns>
[AllowAnonymous]
[Route("/api/GetVoteDial")]
public async Task<IActionResult> GetVoteDial(int? issueId = null, int? solutionId = null, int? commentId = null)
{
    // Ensure at least one ID is provided
    if (!issueId.HasValue && !solutionId.HasValue && !commentId.HasValue)
    {
        return BadRequest(new { message = "At least one ID must be provided" });
    }

    int? userVote = null;
    int? contentId = null;
    string contentType = null;
    bool contentExists = false;
    
    // Determine which content type we're working with
    if (issueId.HasValue)
    {
        contentId = issueId;
        contentType = "Issue";
        contentExists = _context.Issues.Any(i => i.IssueID == issueId);
    }
    else if (solutionId.HasValue)
    {
        contentId = solutionId;
        contentType = "Solution";
        contentExists = _context.Solutions.Any(s => s.SolutionID == solutionId);
    }
    else if (commentId.HasValue)
    {
        contentId = commentId;
        contentType = "Comment";
        contentExists = _context.Comments.Any(c => c.CommentID == commentId);
    }

    if (!contentExists)
    {
        return NotFound(new { message = $"{contentType} not found" });
    }

    var user = await _userManager.GetUserAsync(User);

    if (user != null)
    {
        // Get a possible vote value from the db based on content type
        if (contentType == "Issue")
        {
            var existingVote = await _context.UserVotes
                .OfType<IssueVote>()
                .FirstOrDefaultAsync(v => v.UserID == user.Id && v.IssueID == contentId);

            if (existingVote != null)
            {
                userVote = existingVote.VoteValue;
            }
        }
        else if (contentType == "Solution")
        {
            var existingVote = await _context.UserVotes
                .OfType<SolutionVote>()
                .FirstOrDefaultAsync(v => v.UserID == user.Id && v.IssueSolutionID == contentId);

            if (existingVote != null)
            {
                userVote = existingVote.VoteValue;
            }
        }
        else if (contentType == "Comment")
        {
            var existingVote = await _context.UserVotes
                .OfType<CommentVote>()
                .FirstOrDefaultAsync(v => v.UserID == user.Id && v.CommentID == contentId);

            if (existingVote != null)
            {
                userVote = existingVote.VoteValue;
            }
        }
    }

    // Retrieve vote data based on content type
    double averageVote = 0;
    int totalVotes = 0;

    if (contentType == "Issue")
    {
        var votes = await _context.UserVotes
            .OfType<IssueVote>()
            .Where(v => v.IssueID == contentId)
            .ToListAsync();

        averageVote = votes.Any() ? votes.Average(v => v.VoteValue) : 0;
        totalVotes = votes.Count;
    }
    else if (contentType == "Solution")
    {
        var votes = await _context.UserVotes
            .OfType<SolutionVote>()
            .Where(v => v.IssueSolutionID == contentId)
            .ToListAsync();

        averageVote = votes.Any() ? votes.Average(v => v.VoteValue) : 0;
        totalVotes = votes.Count;
    }
    else if (contentType == "Comment")
    {
        var votes = await _context.UserVotes
            .OfType<CommentVote>()
            .Where(v => v.CommentID == contentId)
            .ToListAsync();

        averageVote = votes.Any() ? votes.Average(v => v.VoteValue) : 0;
        totalVotes = votes.Count;
    }

    // Create the appropriate view model based on content type
    var model = new UserVote_Generic_ReadVM
    {
        ContentType = contentType,
        ContentID = contentId.Value,
        AverageVote = averageVote,
        TotalVotes = totalVotes,
        UserVote = userVote
    };

    // Return the partial view with the vote data model
    return PartialView("~/Views/Issue/_voteDial.cshtml", model);
}

        [Route("/solution/create")]
        public async Task<IActionResult> CreateSolution(int? issueId = null)
        {
            // Initialize the ViewModel
            var viewModel = new Solution_CreateVM
            {
                IssueID = issueId,
                ContentStatus = ContentStatus.Draft,
                Categories = await _context.Categories.ToListAsync(),
            };

            return View(viewModel);
        }

        [HttpPost]
        [Route("/solution/create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSolution(Solution_CreateVM model)
        {
            if (ModelState.IsValid)
            {
                // Get the current user ID
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Create the Solution entity
                var solution = new Solution
                {
                    Title = model.Title,
                    Content = model.Content,
                    IssueID = model.IssueID.Value,
                    ContentStatus = model.ContentStatus,
                    AuthorID = userId,
                    CreatedAt = DateTime.Now
                };

                // Add the solution to the context
                _context.Solutions.Add(solution);
                await _context.SaveChangesAsync();

                // Process categories if any were selected
                if (model.SelectedCategoryIds != null && model.SelectedCategoryIds.Any())
                {
                    // Add logic here to associate categories with the solution
                    // This would depend on your data model for solution categories
                    // If you have a SolutionCategory entity, you would create those relationships here
                }

                // Redirect to the details view of the issue this solution is for
                return RedirectToAction("ReadIssue", new { id = model.IssueID });
            }

            // If we got to here, something failed, redisplay form
            model.Categories = await _context.Categories.ToListAsync();
            return View(model);
        }



    }
}