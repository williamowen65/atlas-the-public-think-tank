using atlas_the_public_think_tank.Data;
using atlas_the_public_think_tank.Data.CRUD;
using atlas_the_public_think_tank.Data.RepositoryPattern.Repository.Helpers;
using atlas_the_public_think_tank.Data.SeedData.SeedUsers.Data;
using atlas_the_public_think_tank.Models;
using atlas_the_public_think_tank.Models.Database;
using atlas_the_public_think_tank.Models.ViewModel;
using atlas_the_public_think_tank.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace atlas_the_public_think_tank.Controllers
{

    /// <summary>
    /// This controller handles all CRUD operations for 
    /// the Issue, Solutions, Comments, UserHistory, and Voting
    /// </summary>
    [Authorize]
    public class IssueController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public IssueController(
            ApplicationDbContext context, 
            UserManager<AppUser> userManager, 
            IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _environment = env;
        }

        #region Issue Page
        /// <summary>
        /// Returns a HTML page for a specific issue
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("/issue/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> ReadIssue(Guid id)
        {
            ViewData["FilterPanelMode"] = "ContentItem";

            ContentFilter filter = new ContentFilter();
            if (Request.Cookies.TryGetValue("contentFilter", out string? cookieValue) && cookieValue != null)
            {
                filter = ContentFilter.FromJson(cookieValue);
            }

            bool fetchParent = true;

            var issue = await Read.Issue(id, filter, fetchParent);

            if (issue == null)
            {
                return NotFound();
            }


            return View(issue);
        }
        #endregion

        #region Paginated Issue Page Feeds

        /// <summary>
        /// This method is used to return paginated issue posts.
        /// </summary>
        /// <returns></returns>
        [HttpGet]

        [AllowAnonymous]
        [Route("/issue/getPaginatedSubIssues/{issueId}")]
        public async Task<IActionResult> GetPaginatedSubIssues(Guid issueId, int currentPage = 1)
        {
            ContentFilter filter = new ContentFilter();
            if (Request.Cookies.TryGetValue("contentFilter", out string? cookieValue) && cookieValue != null)
            {
                filter = ContentFilter.FromJson(cookieValue);
            }


            PaginatedIssuesResponse paginatedIssues = await Read.PaginatedSubIssueFeedForIssue(issueId, filter, currentPage);

            string partialViewHtml = await ControllerExtensions.RenderViewToStringAsync(this, "~/Views/Issue/_issue-cards.cshtml", paginatedIssues.Issues);

            var response = new PaginatedContentItemsJsonResponse
            {
                html = partialViewHtml,
                pagination = new PaginationStats
                {
                    TotalCount = paginatedIssues.ContentCount.TotalCount,
                    PageSize = paginatedIssues.PageSize,
                    CurrentPage = paginatedIssues.CurrentPage,
                    TotalPages = (int)Math.Ceiling(paginatedIssues.ContentCount.TotalCount / (double)paginatedIssues.PageSize)
                }
            };

            return Json(response);
        }



        /// <summary>
        /// This method is used to return paginated solution posts for a specific issue.
        /// </summary>
        /// <remarks>
        /// NOTE: This is for Issues fetching child solutions
        /// </remarks>
        /// <param name="issueId">The ID of the parent issue</param>
        /// <param name="currentPage">The page number to retrieve</param>
        /// <returns>A partial view with the solutions for the specified page</returns>
        [HttpGet]
        [AllowAnonymous]
        [Route("/issue/getPaginatedSolutions/{issueId}")]
        public async Task<IActionResult> GetPaginatedSolutions(Guid issueId, int currentPage = 1)
        {
            ContentFilter filter = new ContentFilter();
            if (Request.Cookies.TryGetValue("contentFilter", out string? cookieValue) && cookieValue != null)
            {
                filter = ContentFilter.FromJson(cookieValue);
            }


            PaginatedSolutionsResponse paginatedSolutions = await Read.PaginatedSolutionFeedForIssue(issueId, filter, currentPage);

            string partialViewHtml = await ControllerExtensions.RenderViewToStringAsync(this, "~/Views/Solution/_solution-cards.cshtml", paginatedSolutions.Solutions);

            // Create a response object with both the HTML and the pagination metadata
            var response = new PaginatedContentItemsJsonResponse
            {
                html = partialViewHtml,
                pagination = new PaginationStats
                {
                    TotalCount = paginatedSolutions.ContentCount.TotalCount,
                    PageSize = paginatedSolutions.PageSize,
                    CurrentPage = paginatedSolutions.CurrentPage,
                    TotalPages = (int)Math.Ceiling(paginatedSolutions.ContentCount.TotalCount / (double)paginatedSolutions.PageSize)
                }
            };

            return Json(response);
        }
        #endregion

        #region Create New Issue Page

        /// <summary>
        /// This method is used to return the create issue page.
        /// </summary>
        [Route("/create-issue")]
        public IActionResult CreateIssuePage(Guid? parentIssueID, Guid? parentSolutionID)
        {
            CreateIssuePageViewModel model = new CreateIssuePageViewModel
            {
                // Load Scopes from the database
                Scopes = _context.Scopes.ToList()
            };

            // Set parent IDs if provided
            if (parentIssueID.HasValue)
            {
                model.MainIssue.ParentIssueID = parentIssueID;
                
            }

            if (parentSolutionID.HasValue)
            {
                model.MainIssue.ParentSolutionID = parentSolutionID;
            }

            return View(model);
        }


        /// <summary>
        /// This method is used to create a new issue post.
        /// </summary>
        /// <param name="model"></param>
        [HttpPost]
        [Route("/create-issue")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateIssue(CreateIssueViewModel model, ContentStatus contentStatus)
        {
            ContentCreationResponseBase contentCreationResponse = new ContentCreationResponseBase();

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
                Issue_ReadVM issue = await Create.Issue(new Issue()
                {
                    ParentIssueID = model.ParentIssueID,
                    ParentSolutionID = model.ParentSolutionID,
                    AuthorID = user.Id,
                    Content = model.Content,
                    ContentStatus = contentStatus,
                    CreatedAt = DateTime.UtcNow,
                    ScopeID = (Guid)model.ScopeID!,  // Use ScopeID instead of Scope.ScopeID
                    Title = model.Title
                });

                string partialViewHtml = await ControllerExtensions.RenderViewToStringAsync(this, "~/Views/Issue/_issue-card.cshtml", issue);

                contentCreationResponse.Content = partialViewHtml;
                contentCreationResponse.Success = true;
            }
            catch (Exception ex)
            {
                contentCreationResponse.Success = false;
            }

            return Json(contentCreationResponse);
        }

        /// <summary>
        /// This method is used to return the create issue page.
        /// </summary>
        [Route("/edit-issue")]
        public async  Task<IActionResult> EditIssuePartialView(Guid issueId)
        {
            Issue_ReadVM? issue = await Read.Issue(issueId, new ContentFilter());


            EditIssueWrapper issueWrapper = new EditIssueWrapper()
            {
                Issue = issue,
                Scopes = await _context.Scopes.ToListAsync()
            };

            // render Partial view and return json
            string html = await ControllerExtensions.RenderViewToStringAsync(this,"~/Views/Issue/_edit-issue.cshtml", issueWrapper);


            return Json(new {
                content= html
            });
        }

        /// <summary>
        /// This method is used to edit an issue post.
        /// </summary>
        /// <param name="model"></param>
        [HttpPost]
        [Route("/edit-issue")]
        [ValidateAntiForgeryToken]
        //public async Task<IActionResult> EditIssue(CreateIssueViewModel model, ContentStatus contentStatus)
        public async  Task<IActionResult> EditIssue(UpdateIssueViewModel model, ContentStatus contentStatus)
        {
            ContentCreationResponseBase contentCreationResponse = new ContentCreationResponseBase();

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

            // Update Issue
            Issue_ReadVM? issue = await Update.Issue(new Issue()
            {
                IssueID = (Guid)model.IssueID!,
                ParentIssueID = model.ParentIssueID,
                ParentSolutionID = model.ParentSolutionID,
                AuthorID = user.Id,
                Content = model.Content,
                ContentStatus = contentStatus,
                CreatedAt = DateTime.UtcNow,
                ScopeID = (Guid)model.ScopeID!,  // Use ScopeID instead of Scope.ScopeID
                Title = model.Title
            });


            // Render issue
            // render Partial view and return json
            string html = await ControllerExtensions.RenderViewToStringAsync(this, "~/Views/Issue/_issue-card.cshtml", issue);

            contentCreationResponse.Content = html;
            contentCreationResponse.Success = true;

            return Json(contentCreationResponse);
        }


            /*




                /// <summary>
                /// This method is used to create a new issue post.
                /// </summary>
                /// <param name="model"></param>
                [HttpPost]
                [Route("/create-issue")]
                [ValidateAntiForgeryToken]
                public async Task<IActionResult> CreateIssue(Issue_CreateVM model)
                {

                    // Custom validation: Only one of ParentIssueID or ParentSolutionID can be set
                    bool bothParentIdsSet = model.ParentIssueID.HasValue && model.ParentSolutionID.HasValue;

                    if (bothParentIdsSet)
                    {
                        ModelState.AddModelError(string.Empty, "You must specify either a parent issue or a parent solution, but not both.");
                    }


                    if (ModelState.IsValid)
                    {
                        var user = await _userManager.GetUserAsync(User);


                        var issuePost = new Issue
                        {
                            Title = model.Title,
                            Content = model.Content,
                            ScopeID = model.ScopeID,
                            ParentIssueID = model.ParentIssueID,
                            ParentSolutionID = model.ParentSolutionID,
                            ContentStatus = model.ContentStatus,
                            AuthorID = user.Id,
                            CreatedAt = DateTime.UtcNow
                        };

                        var entry = _context.Issues.Add(issuePost);
                        await _context.SaveChangesAsync(); // Save to generate the IssueID

                        // Now add the category relationships
                        if (model.SelectedCategoryIds != null && model.SelectedCategoryIds.Any())
                        {
                            foreach (Guid categoryId in model.SelectedCategoryIds)
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
            */

            #endregion

            #region Vote on an issue


            /// <summary>
            /// This method is used to cast a vote on a issue post.
            /// </summary>
            /// <param name="model"></param>
            [AllowAnonymous] // There will be an error sent if user is not logged in
        [HttpPost]
        [Route("/issue/vote")]
        public async Task<IActionResult> IssueVote([FromBody] UserVote_Issue_UpsertVM model)
        {

            if (ModelState.IsValid) { 
                // Add specific validation checks
                if (model.IssueID == Guid.Empty)
                {
                    ModelState.AddModelError("IssueID", "IssueID: cannot be empty");
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
                JsonVoteResponse? voteResponse = await Upsert.IssueVote(model, user);   
                // Successful path
                return Json(voteResponse);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message});
            }
        }
        
        
        #endregion
    }
}