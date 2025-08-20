using atlas_the_public_think_tank.Data;
using atlas_the_public_think_tank.Data.CRUD;
using atlas_the_public_think_tank.Data.RepositoryPattern.Repository.Helpers;
using atlas_the_public_think_tank.Models;
using atlas_the_public_think_tank.Models.Database;
using atlas_the_public_think_tank.Models.ViewModel;
using atlas_the_public_think_tank.Utilities;
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

        public SolutionController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        /// <summary>
        /// This method is used to create a new issue post.
        /// </summary>
        /// <param name="model"></param>
        [HttpPost]
        [Route("/create-solution")]
        [ValidateAntiForgeryToken]
        public IActionResult CreateSolution(CreateSolutionViewModel model, ContentStatus contentStatus)
        {


            return StatusCode(200);
        }

        /*
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
            /// Retrieves a paginated list of sub-issues for a specified solution.
            /// </summary>
            /// <param name="solutionId">The unique identifier of the solution for which sub-issues are requested.</param>
            /// <param name="currentPage">The current page number of the paginated results. Defaults to 1.</param>
            /// <returns>An <see cref="IActionResult"/> containing the paginated sub-issues in JSON format.</returns>
            [HttpGet]
            [AllowAnonymous]
            [Route("/solution/getPaginatedSubIssues/{solutionId}")]
            public async Task<IActionResult> GetPaginatedSubIssues(Guid solutionId, int currentPage = 1)
            {

                ContentFilter filter = new ContentFilter();
                if (Request.Cookies.TryGetValue("contentFilter", out string? cookieValue) && cookieValue != null)
                {
                    filter = ContentFilter.FromJson(cookieValue);
                }


                PaginatedIssuesResponse paginatedSubIssues = await _crud.Solutions.GetSubIssuesPagedAsync(solutionId, currentPage, filter);

                string partialViewHtml = await ControllerExtensions.RenderViewToStringAsync(this, "~/Views/Issue/_issue-cards.cshtml", paginatedSubIssues.Issues);


                var response = new
                {
                    html = partialViewHtml,
                    pagination = new PaginationStats
                    {
                        TotalCount = paginatedSubIssues.TotalCount,
                        PageSize = paginatedSubIssues.PageSize,
                        CurrentPage = paginatedSubIssues.CurrentPage,
                        TotalPages = (int)Math.Ceiling(paginatedSubIssues.TotalCount / (double)paginatedSubIssues.PageSize)
                    }
                };

                return Json(response);
            }


            */

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

            ViewData["FilterPanelMode"] = "ContentItem";

            ContentFilter filter = new ContentFilter();
            if (Request.Cookies.TryGetValue("contentFilter", out string? cookieValue) && cookieValue != null)
            {
                filter = ContentFilter.FromJson(cookieValue);
            }

            bool fetchParent = true;

            var solution = await Read.Solution(id, filter, fetchParent);
           

            if (solution == null)
            {
                return NotFound();
            }

            // Map to the view model (adjust as needed for your project)

            return View(solution);
        }

        #region Vote on a solution


        /// <summary>
        /// This method is used to cast a vote on a solution post.
        /// </summary>
        /// <param name="model"></param>
        [AllowAnonymous] // There will be an error sent if user is not logged in
        [HttpPost]
        [Route("/solution/vote")]
        public async Task<IActionResult> IssueVote([FromBody] UserVote_Solution_UpsertVM model)
        {

            if (ModelState.IsValid)
            {
                // Add specific validation checks
                if (model.SolutionID == Guid.Empty)
                {
                    ModelState.AddModelError("SolutionID", "SolutionID: cannot be empty");
                }

                if (model.VoteValue < 0 || model.VoteValue > 10) // Adjust range as needed
                {
                    ModelState.AddModelError("VoteValue", "VoteValue: must be between 0 and 10 (inclusive)");
                }
            }

            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid vote data", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized(new { success = false, message = "You must login in order to vote" });
            }

            try
            {
                JsonVoteResponse? voteResponse = await Upsert.SolutionVote(model, user);
                // Successful path
                return Json(voteResponse);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        #endregion
    }
}
