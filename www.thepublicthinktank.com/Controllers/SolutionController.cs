using atlas_the_public_think_tank.Data;
using atlas_the_public_think_tank.Models.Database;
using atlas_the_public_think_tank.Models.ViewModel;
using atlas_the_public_think_tank.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using static atlas_the_public_think_tank.Data.SeedData.SeedIds;

namespace atlas_the_public_think_tank.Controllers
{
    /// <summary>
    /// This C# Controller manages the solutions
    /// </summary>

    [Authorize]
    public class SolutionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly CRUD _crud;

        public SolutionController(ApplicationDbContext context, UserManager<AppUser> userManager, CRUD crud)
        {
            _context = context;
            _userManager = userManager;
            _crud = crud;
        }

        [Route("/solution/create")]
        public async Task<IActionResult> CreateSolution(Guid? parentIssueID = null)
        {
            // Initialize the ViewModel
            var viewModel = new Solution_CreateVM
            {
                ParentIssueID = parentIssueID,
                Scopes = _context.Scopes.ToList(),
                Categories = await _context.Categories.ToListAsync(),
                //BreadcrumbTags = await _crud.BreadcrumbAccessor.GetContentBreadcrumb(parentIssueID ?? Guid.Empty)
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
                string userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                Guid userId;
                if (!Guid.TryParse(userIdStr, out userId))
                {
                    // Handle the error (e.g., return an error response or throw)
                    ModelState.AddModelError("", "Invalid user ID.");
                }

                // Create the Solution entity
                var solution = new Solution
                {
                    Title = model.Title,
                    Content = model.Content,
                    ParentIssueID = model.ParentIssueID.Value,
                    ContentStatus = model.ContentStatus,
                    AuthorID = userId,
                    CreatedAt = DateTime.Now,
                    ScopeID = model.ScopeID,
                };

                // Add the solution to the context
                _context.Solutions.Add(solution);
                await _context.SaveChangesAsync();

                // Now add the category relationships
                if (model.SelectedCategoryIds != null && model.SelectedCategoryIds.Any())
                {
                    foreach (Guid categoryId in model.SelectedCategoryIds)
                    {
                        var solutionCategory = new SolutionCategory
                        {
                            SolutionID = solution.SolutionID,
                            CategoryID = categoryId
                        };
                        _context.SolutionCategories.Add(solutionCategory);
                    }

                    // Save the User History (todo)

                    await _context.SaveChangesAsync();
                }

                // Redirect to the details view of the issue this solution is for
                return RedirectToAction("ReadIssue", "Issue", new { id = model.ParentIssueID });
            }

            // If we got to here, something failed, redisplay form
            model.Categories = await _context.Categories.ToListAsync();
            return View(model);
        }

        /// <summary>
        /// This method is used to return paginated solution posts for a specific issue.
        /// </summary>
        /// <param name="issueId">The ID of the parent issue</param>
        /// <param name="currentPage">The page number to retrieve</param>
        /// <returns>A partial view with the solutions for the specified page</returns>
        [HttpGet]
        [AllowAnonymous]
        [Route("/solution/getPaginatedSolutions/{issueId}")]
        public async Task<IActionResult> GetPaginatedSolutions(Guid issueId, int currentPage = 1)
        {
            PaginatedSolutionsResponse paginatedSolutions = await _crud.Solutions.GetSolutionsPagedAsync(issueId, currentPage, 3);
            return PartialView("~/Views/Solution/_solution-cards.cshtml", paginatedSolutions.Solutions);
        }

        /// <summary>
        /// Returns the vote dial for a specific solution.
        /// </summary>
        /// <remarks>
        /// This method is no longer used in the app b/c dial is now rendered with the main page load
        /// </remarks>
        /// <param name="solutionId">The ID of the solution</param>
        /// <returns>HTML partial view of vote dial</returns>
        [AllowAnonymous]
        [Route("/solution/GetVoteDial")]
        public async Task<IActionResult> GetVoteDial(Guid? solutionId = null)
        {
            var model = await _crud.Solutions.GetSolutionVoteStats(solutionId);
            return PartialView("~/Views/Shared/Components/_voteDial.cshtml", model);
        }

        /// <summary>
        /// Returns a HTML page for a specific solution
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("/solution/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> ReadSolution(Guid id)
        {
            var solution = await _context.Solutions
                .Include(s => s.Author)
                .Include(s => s.Scope)
                .Include(f => f.ChildIssues)
                .Include(s => s.ParentIssue) // ParentIssue for a solution
                    .ThenInclude(i => i.Scope)
                .Include(s => s.ParentIssue) // ParentIssue for a solution
                    .ThenInclude(i => i.Solutions)
                .Include(s => s.ParentIssue) // ParentIssue for a solution
                    .ThenInclude(i => i.Author)
                .Include(s => s.ParentIssue)
                .Include(s => s.BlockedContent)
                .Include(s => s.Comments)
                .Include(s => s.SolutionCategories)
                    .ThenInclude(sc => sc.Category)
                .FirstOrDefaultAsync(s => s.SolutionID == id);

            if (solution == null)
            {
                return NotFound();
            }

            // Map to the view model (adjust as needed for your project)
            var solutionVM = await _crud.Solutions.ConvertSolutionEntityToVM(solution);

            return View(solutionVM);
        }

        /// <summary>
        /// This method is used to cast a vote on a solution post.
        /// </summary>
        /// <param name="model"></param>
        [HttpPost]
        [Route("/solution/vote")]
        public async Task<IActionResult> SolutionVote(UserVote_Solution_CreateVM model)
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

                // Check if the user has already voted on this solution
                var existingVote = await _context.SolutionVotes
                    .OfType<SolutionVote>()
                    .FirstOrDefaultAsync(v => v.UserID == user.Id && v.SolutionID == model.SolutionID);

                if (existingVote != null)
                {
                    // Update existing vote
                    existingVote.VoteValue = (int)model.VoteValue;
                    existingVote.ModifiedAt = DateTime.UtcNow;
                    _context.SolutionVotes.Update(existingVote);
                }
                else
                {
                    // Create new vote
                    SolutionVote vote = new SolutionVote
                    {
                        User = user,
                        UserID = user.Id,
                        SolutionID = model.SolutionID,
                        VoteValue = (int)model.VoteValue,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.SolutionVotes.Add(vote);
                }

                await _context.SaveChangesAsync();

                // get updated stats
                double average = await _context.SolutionVotes
                     .OfType<SolutionVote>()
                     .Where(v => v.SolutionID == model.SolutionID)
                     .AverageAsync(v => v.VoteValue);
                int count = await _context.SolutionVotes
                     .OfType<SolutionVote>()
                     .Where(v => v.SolutionID == model.SolutionID)
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
    }
}
