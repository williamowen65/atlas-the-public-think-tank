using atlas_the_public_think_tank.Data;
using atlas_the_public_think_tank.Data.CRUD;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;
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
using atlas_the_public_think_tank.Models.ViewModel.UI_VM;
using atlas_the_public_think_tank.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Threading.Tasks;
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


        #region Solution page


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

            Solution_PageVM solution_PageVM = new Solution_PageVM();

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

            solution_PageVM.Solution = solution;
            solution_PageVM.Sidebar.PageInfo = GetPageInfo(filter, solution);

            // Map to the view model (adjust as needed for your project)

            return View(solution_PageVM);
        }

        #endregion


        #region Paginated Solution PageFeeds

        /// <summary>
        /// This method is used to return paginated issue posts.
        /// </summary>
        /// <returns></returns>
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

            Solution_ReadVM? solution = await Read.Solution(solutionId, filter);

            Issues_Paginated_ReadVM paginatedIssues = await Read.PaginatedSubIssueFeedForSolution(solutionId, filter, currentPage);

            string partialViewHtml = await ControllerExtensions.RenderViewToStringAsync(this, "~/Views/Issue/_issue-cards.cshtml", paginatedIssues.Issues);

            var response = new ContentItems_Paginated_AjaxVM
            {
                html = partialViewHtml,
                pagination = new PaginationStats_VM
                {
                    TotalCount = paginatedIssues.ContentCount.TotalCount,
                    PageSize = paginatedIssues.PageSize,
                    CurrentPage = paginatedIssues.CurrentPage,
                    TotalPages = (int)Math.Ceiling(paginatedIssues.ContentCount.TotalCount / (double)paginatedIssues.PageSize)
                }
            };

            response.Sidebar.PageInfo = GetPageInfo(filter, solution!);

            return Json(response);
        }


        #endregion

        #region Create new solution 

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


                Scope_ReadVM scopeVM = await Create.Scope(new Scope()
                {
                    Boundaries = model.Scope.Boundaries,
                    Domains = model.Scope.Domains,
                    EntityTypes = model.Scope.EntityTypes,
                    Scales = model.Scope.Scales,
                    Timeframes = model.Scope.Timeframes
                });


                // Create new solution via repository pattern (cache)
                Solution_ReadVM solution = await Create.Solution(new Solution()
                { 
                    ParentIssueID = (Guid)model.ParentIssueID!,
                    AuthorID = user.Id,
                    Content = model.Content,
                    ContentStatus = contentStatus,
                    CreatedAt = DateTime.UtcNow,
                    ScopeID = scopeVM.ScopeID,
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
        public async Task<IActionResult> CreateSolutionPage(Guid? parentIssueID = null)
        {

            CreateSolution_PageVM model = new CreateSolution_PageVM();
           
            // Set parent IDs if provided
            if (parentIssueID.HasValue)
            {
                model.Solution.ParentIssueID = parentIssueID;
                Issue_ReadVM? parentIssue = await Read.Issue((Guid)model.Solution.ParentIssueID!, new ContentFilter());
                model.Solution.ParentIssue = parentIssue;
            }

            model.Sidebar = new SideBar_VM()
            {
                ShowPageDisplayOptions = false,
                PageInfo = new PageInfo()
                {
                    PageContext = $"""
                    <strong>Create a New Solution</strong><br/>
                    
                    """
                }
            };


            return View(model);
        }


        #endregion

        #region Edit Solution

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
                    Scope = new Scope_CreateOrEditVM()
                    {
                        ScopeID = solution.Scope.ScopeID,
                        Boundaries = solution.Scope.Boundaries,
                        Domains = solution.Scope.Domains,
                        EntityTypes = solution.Scope.EntityTypes,
                        Scales = solution.Scope.Scales,
                        Timeframes = solution.Scope.Timeframes
                    },
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
            // Also to get the created at value
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


            Scope incomingScope = new Scope()
            {
                ScopeID = (Guid)model.Scope.ScopeID!,
                Boundaries = model.Scope.Boundaries,
                Domains = model.Scope.Domains,
                EntityTypes = model.Scope.EntityTypes,
                Scales = model.Scope.Scales,
                Timeframes = model.Scope.Timeframes
            };

            bool scopeDiff = DiffCheckers.AreScopesDifferent(incomingScope, solutionRef.Scope);

            if (scopeDiff)
            {
                // Update the Scope
                Scope_ReadVM? scopeVM = await Update.Scope(incomingScope);
            }

            Solution incomingSolution = new Solution()
            {
                SolutionID = (Guid)model.SolutionID!,
                ParentIssueID = (Guid)model.ParentIssueID!,
                AuthorID = user.Id,
                Content = model.Content,
                ContentStatus = contentStatus,
                CreatedAt = solutionRef.CreatedAt,
                ModifiedAt = DateTime.UtcNow, // Set ModifiedAt
                ScopeID = (Guid)model.Scope.ScopeID!,
                Title = model.Title,
                Scope = incomingScope,
            };

            bool solutionDiff = DiffCheckers.AreSolutionsDifferent(Converter.ConvertSolution_ReadVMToSolution(solutionRef), incomingSolution);
            // Update solution
            Solution_ReadVM? solution = solutionRef;
            if (solutionDiff) { 
                
                solution = await Update.Solution(incomingSolution);
            }
            else if (scopeDiff == true && solutionDiff != true)
            {
                // Update solution so that the scope it versioned as a new issue
                solution = await Update.Solution(incomingSolution);
            }


            // Render issue
            // render Partial view and return json
            string html = await ControllerExtensions.RenderViewToStringAsync(this, "~/Views/Solution/_solution-card.cshtml", solution);

            contentCreationResponse.Content = html;
            contentCreationResponse.Success = true;

            return Json(contentCreationResponse);
        }

        #endregion

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



        public PageInfo GetPageInfo(ContentFilter filter, Solution_ReadVM solution)
        {

            PageInfo pageInfo = new PageInfo();

            string filterHash = filter.ToJson().GetHashCode().ToString();
            ContentFilter defaultFilter = new ContentFilter();
            string defaultFilterHash = defaultFilter.ToJson().GetHashCode().ToString();

            bool isFilterApplied = filterHash != defaultFilterHash;

            // Build filter difference details
            StringBuilder filterDetails = new StringBuilder();
            if (isFilterApplied)
            {
                // Check Content Type
                if (!string.IsNullOrEmpty(filter.ContentType) && filter.ContentType != defaultFilter.ContentType)
                {
                    filterDetails.Append($"• Content Type: {filter.ContentType}<br>");
                }

                // Check Vote Range
                if (filter.AvgVoteRange?.Min != defaultFilter.AvgVoteRange?.Min ||
                    filter.AvgVoteRange?.Max != defaultFilter.AvgVoteRange?.Max)
                {
                    filterDetails.Append($"• Vote Range: {filter.AvgVoteRange?.Min ?? 0} to {filter.AvgVoteRange?.Max ?? 5}<br>");
                }

                // Check Vote Count
                if (filter.TotalVoteCount?.Min != defaultFilter.TotalVoteCount?.Min ||
                   filter.TotalVoteCount?.Max != defaultFilter.TotalVoteCount?.Max)
                {
                    int min = filter.TotalVoteCount?.Min ?? 0;
                    int? max = filter.TotalVoteCount?.Max;
                    string rangeText = max.HasValue ? $"{min} to {max.Value}" : $"{min} and up";
                    filterDetails.Append($"• Vote Count: {rangeText}<br>");
                }

                // Check Date Range
                if (filter.DateRange?.From != defaultFilter.DateRange?.From ||
                    filter.DateRange?.To != defaultFilter.DateRange?.To)
                {
                    string dateRangeText = "";
                    if (filter.DateRange?.From != null)
                        dateRangeText += $"from {filter.DateRange.From.Value.ToShortDateString()} ";
                    if (filter.DateRange?.To != null)
                        dateRangeText += $"to {filter.DateRange.To.Value.ToShortDateString()}";

                    filterDetails.Append($"• Date Range: {dateRangeText.Trim()}<br>");
                }

                // Check Tags
                if (filter.Tags?.Count > 0)
                {
                    filterDetails.Append($"• Tags: {string.Join(", ", filter.Tags)}<br>");
                }
            }

            // Build PageInfo HTML
            pageInfo.FilterAlert = isFilterApplied
                 ? $"""
          <div style='color: #b94a48; font-weight: bold;'>
              <div style='margin-top: 5px; font-weight: normal;'>
                  <div style='padding-left: 10px; font-size: 0.9em;'>
                      {filterDetails}
                  </div>
              </div>
          </div>
          """
                 : null;

            pageInfo.PageContext = $"""
            <span style="font-size:13px;">
                <strong>You are visiting a Solution page.</strong>
                <br/>
                <span class="title" data-content-id="{solution.SolutionID}">{solution.Title}</span>
                <br />
                <strong>Stats:</strong> 
                <br/><span class="sub-issue-content-count">{solution.PaginatedSubIssues?.ContentCount?.TotalCount ?? 0}</span> {(isFilterApplied ? $" of {solution.PaginatedSubIssues?.ContentCount?.AbsoluteCount}" : "")} sub-issues 
            </span>
            """;


            return pageInfo;
        }


    }
}
