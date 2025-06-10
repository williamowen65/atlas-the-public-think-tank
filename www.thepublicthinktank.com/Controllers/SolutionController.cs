using atlas_the_public_think_tank.Data;
using atlas_the_public_think_tank.Models;
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
                IssueID = parentIssueID,
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
                    IssueID = model.IssueID.Value,
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
                return RedirectToAction("ReadIssue", "Issue", new { id = model.IssueID });
            }

            // If we got to here, something failed, redisplay form
            model.Categories = await _context.Categories.ToListAsync();
            return View(model);
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
                .Include(s => s.Issue) // ParentIssue for a solution
                    .ThenInclude(i => i.Scope)
                .Include(s=> s.Issue) // ParentIssue for a solution
                    .ThenInclude(i => i.Solutions)
                .Include(s => s.Issue) // ParentIssue for a solution
                    .ThenInclude(i => i.Author)
                .Include(s => s.Issue)
                .Include(s => s.BlockedContent)
                .Include(s => s.Comments)
                 .Include(s => s.SolutionCategories)
                     .ThenInclude(sc => sc.Category)
                .FirstOrDefaultAsync(s => s.SolutionID == id);

            if (solution == null)
            {
                return NotFound();
            }

            // Map to the view model (adjust as needed for your project)            var solutionVM = _crud.Solutions.ConvertSolutionEntityToVM(solution);
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
