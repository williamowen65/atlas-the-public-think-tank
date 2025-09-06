using atlas_the_public_think_tank.Data;
using atlas_the_public_think_tank.Data.CRUD;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Issue;
using atlas_the_public_think_tank.Data.DatabaseEntities.Users;
using atlas_the_public_think_tank.Data.RepositoryPattern.IRepository;
using atlas_the_public_think_tank.Data.RepositoryPattern.Repository;
using atlas_the_public_think_tank.Data.RepositoryPattern.Repository.Helpers;
using atlas_the_public_think_tank.Models.Enums;
using atlas_the_public_think_tank.Models.ViewModel.AjaxVM;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.ContentItem_Common;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.ContentItem_Common.ContentItemVote;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Issue;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Solution;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.User;
using atlas_the_public_think_tank.Models.ViewModel.CRUD_VM.ContentItem_Common;
using atlas_the_public_think_tank.Models.ViewModel.CRUD_VM.Issue.IssueVote;
using atlas_the_public_think_tank.Models.ViewModel.CRUD_VM.Solution;
using atlas_the_public_think_tank.Models.ViewModel.PageVM;
using atlas_the_public_think_tank.Models.ViewModel.UI_VM;
using atlas_the_public_think_tank.Utilities;
using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Text;

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
        private readonly IBreadcrumbRepository _breadcrumbRepository;
        private readonly IAppUserRepository _appUserRepository;

        public IssueController(
            ApplicationDbContext context,
            UserManager<AppUser> userManager,
            IWebHostEnvironment env,
            IBreadcrumbRepository breadcrumbRepository,
            IAppUserRepository appUserRepository
            )
        {
            _context = context;
            _userManager = userManager;
            _environment = env;
            _breadcrumbRepository = breadcrumbRepository;
            _appUserRepository = appUserRepository;
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
            Issue_PageVM issue_PageVM = new Issue_PageVM();

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

            issue_PageVM.Issue = issue;
            issue_PageVM.Sidebar.PageInfo = GetPageInfo(filter, issue);

            return View(issue_PageVM);
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


            Issues_Paginated_ReadVM paginatedIssues = await Read.PaginatedSubIssueFeedForIssue(issueId, filter, currentPage);

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

            Issue_ReadVM? issue = await Read.Issue(issueId, filter);

            Solutions_Paginated_ReadVM paginatedSolutions = await Read.PaginatedSolutionFeedForIssue(issueId, filter, currentPage);

            string partialViewHtml = await ControllerExtensions.RenderViewToStringAsync(this, "~/Views/Solution/_solution-cards.cshtml", paginatedSolutions.Solutions);

            // Create a response object with both the HTML and the pagination metadata
            var response = new ContentItems_Paginated_AjaxVM
            {
                html = partialViewHtml,
                pagination = new PaginationStats_VM
                {
                    TotalCount = paginatedSolutions.ContentCount.TotalCount,
                    PageSize = paginatedSolutions.PageSize,
                    CurrentPage = paginatedSolutions.CurrentPage,
                    TotalPages = (int)Math.Ceiling(paginatedSolutions.ContentCount.TotalCount / (double)paginatedSolutions.PageSize)
                }
            };

            response.Sidebar.PageInfo = GetPageInfo(filter, issue!);

            return Json(response);
        }
        #endregion

        #region Create New Issue Page

        /// <summary>
        /// This method is used to return the create issue page.
        /// </summary>
        [Route("/create-issue")]
        public async Task<IActionResult> CreateIssuePage(Guid? parentIssueID, Guid? parentSolutionID)
        {
            CreateIssue_PageVM model = new CreateIssue_PageVM();

            // Set parent IDs if provided
            if (parentIssueID.HasValue)
            {
                model.MainIssue.ParentIssueID = parentIssueID;
                Issue_ReadVM? parentIssue = await Read.Issue((Guid)model.MainIssue.ParentIssueID!, new ContentFilter());
                model.MainIssue.ParentIssue = parentIssue;
            }

            if (parentSolutionID.HasValue)
            {
                model.MainIssue.ParentSolutionID = parentSolutionID;
                Solution_ReadVM? parentSolution = await Read.Solution((Guid)model.MainIssue.ParentSolutionID!, new ContentFilter());
                model.MainIssue.ParentSolution = parentSolution;
            }

            model.Sidebar =  new SideBar_VM()
            {
                ShowPageDisplayOptions = false,
                PageInfo = new PageInfo()
                {
                    PageContext = $"""
                    <strong>Create a New Issue</strong><br/>
                    <p>
                      Use this page to describe a problem clearly and concisely. A good issue 
                      explains <em>what the problem is</em>, <em>who it affects</em>, and 
                      <em>why it matters</em>.
                    </p>

                    <p>
                      <strong>Scope:</strong> Select the scope that best describes the level of 
                      impact — for example, individual, local, national, or global. Scopes can be 
                      composite, so think about all the groups or systems your issue touches.
                    </p>

                    <p>
                      <strong>Parent Issues or Solutions:</strong> If your issue relates to a 
                      larger problem or emerges as a challenge within a proposed solution, link it 
                      as a sub-issue. This helps others see how problems connect in the bigger picture.
                    </p>
                    """
                }
            };


            return View(model);
        }


        /// <summary>
        /// This method is used to create a new issue post.
        /// </summary>
        /// <param name="model"></param>
        [HttpPost]
        [Route("/create-issue")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateIssue(Issue_CreateOrEditVM model, ContentStatus contentStatus)
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


                /// Convert Scope_CreateVM to Scope
                /// You must save the scope first to get an ID
                Scope_ReadVM scopeVM = await Create.Scope(new Scope()
                {
                    Boundaries = model.Scope.Boundaries,
                    Domains = model.Scope.Domains,
                    EntityTypes = model.Scope.EntityTypes,
                    Scales = model.Scope.Scales,
                    Timeframes = model.Scope.Timeframes
                });

                // Create new solution via repository pattern (cache)
                Issue_ReadVM issue = await Create.Issue(new Issue()
                {
                    ParentIssueID = model.ParentIssueID,
                    ParentSolutionID = model.ParentSolutionID,
                    AuthorID = user.Id,
                    Content = model.Content,
                    ContentStatus = contentStatus,
                    CreatedAt = DateTime.UtcNow,
                    ScopeID = scopeVM.ScopeID,
                    Title = model.Title
                });

                string partialViewHtml = await ControllerExtensions.RenderViewToStringAsync(this, "~/Views/Issue/_issue-card.cshtml", issue);

                contentCreationResponse.Content = partialViewHtml;
                contentCreationResponse.ContentId = issue.IssueID;
                contentCreationResponse.Success = true;
            }
            catch (Exception ex)
            {
                contentCreationResponse.Success = false;
                List<string> errorEntry = new List<string>();
                errorEntry.Add(ex.Message);
                contentCreationResponse.Errors.Add(errorEntry);
            }

            return Json(contentCreationResponse);
        }

        #endregion

        #region edit issue

        /// <summary>
        /// This method is used to return the create issue page.
        /// </summary>
        [Route("/edit-issue")]
        public async Task<IActionResult> EditIssuePartialView(Guid issueId)
        {
            ContentCreationResponse_JsonVM contentCreationResponse = new ContentCreationResponse_JsonVM();
            Issue_ReadVM? issue = await Read.Issue(issueId, new ContentFilter());

            var user = await _userManager.GetUserAsync(User);

            if (issue == null)
            {
                throw new Exception("Issue doesn't exist for GET EditIssuePartialView");
            }
            // Confirm this user owns this content
            if (user.Id != issue.Author.Id)
            {
                contentCreationResponse.Success = false;
                var errorEntry = new List<string>();
                errorEntry.Add("Error Message");
                errorEntry.Add($"You are not the author of the issue with the id {issue.IssueID}");
                contentCreationResponse.Errors.Add(errorEntry);
                return Json(contentCreationResponse);
            }

            Issue_CreateOrEdit_AjaxVM issueWrapper = new Issue_CreateOrEdit_AjaxVM()
            {
                Issue = new Issue_CreateOrEditVM()
                {
                    Content = issue.Content,
                    ContentStatus = issue.ContentStatus,
                    ParentIssueID = issue.ParentIssueID,
                    ParentSolutionID = issue.ParentSolutionID,
                    Scope = new Scope_CreateOrEditVM()
                    {
                        ScopeID = issue.Scope.ScopeID,
                        Boundaries = issue.Scope.Boundaries,
                        Domains = issue.Scope.Domains,
                        EntityTypes = issue.Scope.EntityTypes,
                        Scales = issue.Scope.Scales,
                        Timeframes = issue.Scope.Timeframes
                    },
                    Title = issue.Title,
                    IssueID = issue.IssueID,
                }
            };

            if (issue.ParentIssueID != null)
            {
                issueWrapper.Issue.ParentIssue = await Read.Issue((Guid)issue.ParentIssueID!, new ContentFilter());
            }
            if (issue.ParentSolutionID != null)
            {
                issueWrapper.Issue.ParentSolution = await Read.Solution((Guid)issue.ParentSolutionID!, new ContentFilter());
            }

            // render Partial view and return json
            string html = await ControllerExtensions.RenderViewToStringAsync(this, "~/Views/Issue/_create-or-edit-issue.cshtml", issueWrapper);

            contentCreationResponse.Success = true;
            contentCreationResponse.Content = html;

            return Json(contentCreationResponse);
        }

        /// <summary>
        /// This method is used to edit an issue post.
        /// </summary>
        /// <param name="model"></param>
        [HttpPost]
        [Route("/edit-issue")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditIssue(Issue_CreateOrEditVM model, ContentStatus contentStatus)
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
            // Also get the createdAt value
            Issue_ReadVM? issueRef = await Read.Issue((Guid)model.IssueID!, new ContentFilter());
            // Confirm this user owns this content
            if (user.Id != issueRef.Author.Id)
            {
                contentCreationResponse.Success = false;
                var errorEntry = new List<string>();
                errorEntry.Add("Error Message");
                errorEntry.Add($"You are not the author of the issue with the id {issueRef.IssueID}");
                contentCreationResponse.Errors.Add(errorEntry);
                return Json(contentCreationResponse);
            }

            // Update the Scope
            Scope_ReadVM? scopeVM = await Update.Scope(new Scope()
            {
                ScopeID = (Guid)model.Scope.ScopeID!,
                Boundaries = model.Scope.Boundaries,
                Domains = model.Scope.Domains,
                EntityTypes = model.Scope.EntityTypes,
                Scales = model.Scope.Scales,
                Timeframes = model.Scope.Timeframes
            });



            // Update Issue
            Issue_ReadVM? issue = await Update.Issue(new Issue()
            {
                IssueID = (Guid)model.IssueID!,
                ParentIssueID = model.ParentIssueID,
                ParentSolutionID = model.ParentSolutionID,
                AuthorID = user.Id,
                Content = model.Content,
                ContentStatus = contentStatus,
                CreatedAt = issueRef.CreatedAt,
                ModifiedAt = DateTime.UtcNow, // Set ModifiedAt
                ScopeID = (Guid)model.Scope.ScopeID!,
                Title = model.Title
            });


            // Render issue
            // render Partial view and return json
            string html = await ControllerExtensions.RenderViewToStringAsync(this, "~/Views/Issue/_issue-card.cshtml", issue);

            contentCreationResponse.Content = html;
            contentCreationResponse.Success = true;

            return Json(contentCreationResponse);
        }

        #endregion

        #region Vote on an issue


        /// <summary>
        /// This method is used to cast a vote on a issue post.
        /// </summary>
        /// <param name="model"></param>
        [AllowAnonymous] // There will be an error sent if user is not logged in
        [HttpPost]
        [Route("/issue/vote")]
        public async Task<IActionResult> IssueVote([FromBody] IssueVote_UpsertVM model)
        {

            if (ModelState.IsValid)
            {
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
                VoteResponse_AjaxVM? voteResponse = await Upsert.IssueVote(model, user);
                // Successful path
                return Json(voteResponse);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        #endregion

        #region Issue Version History Feed

        /// <summary>
        /// This method is used to return the version history of an issue
        /// </summary>
        [AllowAnonymous]
        [Route("/issue-version-history")]
        public async Task<IActionResult> IssueVersionHistory(Guid issueId)
        {
            ContentCreationResponse_JsonVM contentCreationResponse = new ContentCreationResponse_JsonVM();

            // check if issue exists
            Issue_ReadVM? issue = await Read.Issue(issueId, new ContentFilter());
            if (issue == null)
            {
                contentCreationResponse.Success = false;
                List<string> errorEntry = new List<string> { $"The issue {issueId} does not exist" };
                contentCreationResponse.Errors.Add(errorEntry);
                return Json(contentCreationResponse);
            }

            List<ContentItem_ReadVM> contentItemVersions = await Read.IssueVersionHistory(issue);

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




        public PageInfo GetPageInfo(ContentFilter filter, Issue_ReadVM issue)
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
                <strong>You are visiting an Issue page.</strong>
                <br/>
                <span class="title" data-content-id="{issue.IssueID}">{issue.Title}</span>
                <br />
                <strong>Stats{(isFilterApplied ? " with filter" : "")}:</strong> 
                <br/><span class="solution-content-count">{issue.PaginatedSolutions?.ContentCount?.TotalCount ?? 0}</span>{(isFilterApplied ? $" of {issue.PaginatedSolutions?.ContentCount?.AbsoluteCount}" : "")} solutions
                <br/><span class="sub-issue-content-count">{issue.PaginatedSubIssues?.ContentCount?.TotalCount ?? 0}</span>{(isFilterApplied ? $" of {issue.PaginatedSubIssues?.ContentCount?.AbsoluteCount}" : "")} sub-issues 
            </span>
            """;



            return pageInfo;
        }


    }
}