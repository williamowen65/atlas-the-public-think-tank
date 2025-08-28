using atlas_the_public_think_tank.Data;
using atlas_the_public_think_tank.Data.CRUD;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Issue;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Solution;
using atlas_the_public_think_tank.Data.DatabaseEntities.Users;
using atlas_the_public_think_tank.Data.RepositoryPattern.Repository.Helpers;
using atlas_the_public_think_tank.Models;
using atlas_the_public_think_tank.Models.Enums;
using atlas_the_public_think_tank.Models.ViewModel;
using atlas_the_public_think_tank.Models.ViewModel.AjaxVM;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.ContentItem_Common;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Issue;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Solution;
using atlas_the_public_think_tank.Models.ViewModel.CRUD_VM.ContentItem_Common;
using atlas_the_public_think_tank.Models.ViewModel.CRUD_VM.Solution.SolutionVote;
using atlas_the_public_think_tank.Models.ViewModel.PageVM;
using atlas_the_public_think_tank.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        /// This method is used to return solution create form for the create issue page.
        /// </summary>
        /// <remarks>
        /// This is similar to the process that happens on the create solution page, but it occurs in this controller action instead
        /// </remarks>
        [Route("/create-solution-form")]
        public async Task<IActionResult> CreateSolutionPartialView(Guid issueId)
        {
            Issue_ReadVM? issue = await Read.Issue(issueId, new ContentFilter());

            // issue must already exist

            Solution_CreateOrEdit_AjaxVM solutionWrapper = new Solution_CreateOrEdit_AjaxVM()
            {
                Solution = new Solution_CreateOrEditVM() { 
                    ParentIssueID = issueId,
                    ParentIssue = issue
                },
                Scopes = await _context.Scopes.ToListAsync()
            };

            // render Partial view and return json
            string html = await ControllerExtensions.RenderViewToStringAsync(this, "~/Views/Solution/_create-or-edit-solution.cshtml", solutionWrapper);

            ContentCreationResponse_JsonVM contentCreationResponse = new ContentCreationResponse_JsonVM();

            contentCreationResponse.Success = true;
            contentCreationResponse.Content = html;

            return Json(contentCreationResponse);
        }

        /// <summary>
        /// This method is used to create a new solution post.
        /// After submitting a post, the form is replaced with a rendered solution card (ready for voting)
        /// </summary>
        /// <param name="model"></param>
        [HttpPost]
        [Route("/create-solution")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSolutionPost(Solution_CreateOrEditVM model, ContentStatus contentStatus)
        {
            ContentCreationResponse_JsonVM contentCreationResponse = new ContentCreationResponse_JsonVM();

            try
            {
                // Create new solution via repository pattern (cache)
                if (!ModelState.IsValid)
                {
                    contentCreationResponse.Success = false;

                    // Add validation errors to response
                    foreach (var state in ModelState)
                    {
                        foreach (var error in state.Value.Errors)
                        {
                            var errorEntry = new List<string>();
                            errorEntry.Add(state.Key);
                            errorEntry.Add(error.ErrorMessage);
                            contentCreationResponse.Errors.Add(errorEntry);
                        }
                    }

                    return Json(contentCreationResponse);
                }

                // Get author
                var user = await _userManager.GetUserAsync(User);

                // Create new solution via repository pattern (cache)
                Solution_ReadVM solution = await Create.Solution(new Solution()
                { 
                    ParentIssueID = (Guid)model.ParentIssueID!,
                    AuthorID = user.Id,
                    Content = model.Content,
                    ContentStatus = contentStatus,
                    CreatedAt = DateTime.UtcNow,
                    ScopeID = (Guid)model.ScopeID!,
                    Title = model.Title,
                });



                string partialViewHtml = await ControllerExtensions.RenderViewToStringAsync(this, "~/Views/Solution/_solution-card.cshtml", solution);

                //contentCreationResponse.Content = partialViewHtml;
                contentCreationResponse.Content = partialViewHtml;
                contentCreationResponse.ContentId = solution.SolutionID;
                contentCreationResponse.Success = true;
            }
            catch (Exception ex)
            {
                contentCreationResponse.Success = false;
            }

            return Json(contentCreationResponse);
        }

        
        /// <summary>
        /// This method serves the "create solution" page
        /// </summary>
        [Route("/create-solution")]
        public IActionResult CreateSolutionPage(Guid? parentIssueID = null)
        {

            CreateSolution_PageVM model = new CreateSolution_PageVM
            {
                // Load Scopes from the database
                Scopes = _context.Scopes.ToList()
            };
            // Set parent IDs if provided
            if (parentIssueID.HasValue)
            {
                model.Solution.ParentIssueID = parentIssueID;
            }


            return View(model);
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

        /// <summary>
        /// This method is used to return the partial for editing a solution
        /// </summary>
        [Route("/edit-solution")]
        public async Task<IActionResult> EditSolutionPartialView(Guid solutionId)
        {

            ContentCreationResponse_JsonVM contentCreationResponse = new ContentCreationResponse_JsonVM();

            Solution_ReadVM? solution = await Read.Solution(solutionId, new ContentFilter());

            var user = await _userManager.GetUserAsync(User);

            if (solution == null)
            {
                throw new Exception("Solution doesn't exist for GET EditIssuePartialView");
            }
            // Confirm this user owns this content
            if (user.Id != solution.Author.Id)
            {
                contentCreationResponse.Success = false;
                var errorEntry = new List<string>();
                errorEntry.Add("Error Message");
                errorEntry.Add($"You are not the author of the solution with the id {solution.SolutionID}");
                contentCreationResponse.Errors.Add(errorEntry);
                return Json(contentCreationResponse);
            }



            Solution_CreateOrEdit_AjaxVM solutionWrapper = new Solution_CreateOrEdit_AjaxVM()
            {
                Solution = new Solution_CreateOrEditVM()
                { 
                    SolutionID = solution.SolutionID,
                    Content = solution.Content,
                    ContentStatus = solution.ContentStatus,
                    ScopeID = solution.Scope.ScopeID,
                    Title = solution.Title,
                    ParentIssue = await Read.Issue(solution.ParentIssueID, new ContentFilter()),
                    ParentIssueID = solution.ParentIssueID,
                },
                Scopes = await _context.Scopes.ToListAsync()
            };

            // render Partial view and return json
            string html = await ControllerExtensions.RenderViewToStringAsync(this, "~/Views/Solution/_create-or-edit-solution.cshtml", solutionWrapper);

            contentCreationResponse.Success = true;
            contentCreationResponse.Content = html;

            return Json(contentCreationResponse);
        }


        /// <summary>
        /// This method is used to edit an issue post.
        /// </summary>
        /// <param name="model"></param>
        [HttpPost]
        [Route("/edit-solution")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSolution(Solution_CreateOrEditVM model, ContentStatus contentStatus)
        {
            ContentCreationResponse_JsonVM contentCreationResponse = new ContentCreationResponse_JsonVM();

            if (!ModelState.IsValid)
            {
                contentCreationResponse.Success = false;

                // Add validation errors to response
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        var errorEntry = new List<string>();
                        errorEntry.Add(state.Key);
                        errorEntry.Add(error.ErrorMessage);
                        contentCreationResponse.Errors.Add(errorEntry);
                    }
                }
                return Json(contentCreationResponse);
            }



            var user = await _userManager.GetUserAsync(User);

            // pull issue from DAL
            Solution_ReadVM? solutionRef = await Read.Solution((Guid)model.SolutionID!, new ContentFilter());
            // Confirm this user owns this content
            if (user.Id != solutionRef.Author.Id)
            {
                contentCreationResponse.Success = false;
                var errorEntry = new List<string>();
                errorEntry.Add("Error Message");
                errorEntry.Add($"You are not the author of the solution with the id {solutionRef.SolutionID}");
                contentCreationResponse.Errors.Add(errorEntry);
                return Json(contentCreationResponse);
            }

            // Update solution
            Solution_ReadVM? solution = await Update.Solution(new Solution()
            {
                SolutionID = (Guid)model.SolutionID!,
                ParentIssueID = (Guid)model.ParentIssueID!,
                AuthorID = user.Id,
                Content = model.Content,
                ContentStatus = contentStatus,
                CreatedAt = solutionRef.CreatedAt,
                ModifiedAt = DateTime.UtcNow, // Set ModifiedAt
                ScopeID = (Guid)model.ScopeID!,  // Use ScopeID instead of Scope.ScopeID
                Title = model.Title
            });


            // Render issue
            // render Partial view and return json
            string html = await ControllerExtensions.RenderViewToStringAsync(this, "~/Views/Solution/_solution-card.cshtml", solution);

            contentCreationResponse.Content = html;
            contentCreationResponse.Success = true;

            return Json(contentCreationResponse);
        }

        #region Vote on a solution


        /// <summary>
        /// This method is used to cast a vote on a solution post.
        /// </summary>
        /// <param name="model"></param>
        [AllowAnonymous] // There will be an error sent if user is not logged in
        [HttpPost]
        [Route("/solution/vote")]
        public async Task<IActionResult> SolutionVote([FromBody] SolutionVote_UpsertVM model)
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
                VoteResponse_AjaxVM? voteResponse = await Upsert.SolutionVote(model, user);
                // Successful path
                return Json(voteResponse);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        #endregion

        #region Solution Version History Feed

        /// <summary>
        /// This method is used to return the version history of an issue
        /// </summary>
        [AllowAnonymous]
        [Route("/solution-version-history")]
        public async Task<IActionResult> SolutionVersionHistory(Guid solutionId)
        {
            ContentCreationResponse_JsonVM contentCreationResponse = new ContentCreationResponse_JsonVM();

            // check if issue exists
            Solution_ReadVM? solution = await Read.Solution(solutionId, new ContentFilter());
            if (solution == null)
            {
                contentCreationResponse.Success = false;
                List<string> errorEntry = new List<string> { $"The solution {solutionId} does not exist" };
                contentCreationResponse.Errors.Add(errorEntry);
                return Json(contentCreationResponse);
            }

            List<ContentItem_ReadVM> contentItemVersions = await Read.SolutionVersionHistory(solution);

            string html = await ControllerExtensions.RenderViewToStringAsync(
                this,
                "~/Views/Shared/_VersionHistoryModal.cshtml",
                new VersionHistoryModal_VM
                {
                    contentItemVersions = contentItemVersions
                });

            contentCreationResponse.Success = true;
            contentCreationResponse.Content = html;

            return Json(contentCreationResponse);
        }

        #endregion
    }
}
