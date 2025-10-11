using atlas_the_public_think_tank.Data.CRUD;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Issue;
using atlas_the_public_think_tank.Data.DatabaseEntities.Users;
using atlas_the_public_think_tank.Data.DbContext;
using atlas_the_public_think_tank.Data.RawSQL;
using atlas_the_public_think_tank.Data.RepositoryPattern.IRepository;
using atlas_the_public_think_tank.Data.RepositoryPattern.Repository.Helpers;
using atlas_the_public_think_tank.Models.Cacheable;
using atlas_the_public_think_tank.Models.Enums;
using atlas_the_public_think_tank.Models.ViewModel;
using atlas_the_public_think_tank.Models.ViewModel.AjaxVM;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.ContentItem_Common;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Issue;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Issue.IssueVote;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Solution;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Solution.SolutionVote;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.User;
using atlas_the_public_think_tank.Models.ViewModel.CRUD_VM.ContentItem_Common;
using atlas_the_public_think_tank.Models.ViewModel.CRUD_VM.Issue.IssueVote;
using atlas_the_public_think_tank.Models.ViewModel.PageVM;
using atlas_the_public_think_tank.Models.ViewModel.UI_VM;
using atlas_the_public_think_tank.Utilities;
using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol;
using System;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Web;
using static atlas_the_public_think_tank.Data.SeedData.SeedIds;
using ContentType = atlas_the_public_think_tank.Models.Enums.ContentType;

namespace atlas_the_public_think_tank.Controllers;

/// <summary>
/// This controller handles ALL the main pages   
/// and alert functionalities of the application.
/// </summary>
/// 
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<AppUser> _userManager;
    private readonly IVoteStatsRepository _voteStatsRepository;
    private readonly IAppUserRepository _appUserRepository;
    private readonly IIssueRepository _issueRepository;
    private readonly Read _read;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<AppUser> userManager, IVoteStatsRepository voteStatsRepository, IAppUserRepository appUserRepository, IIssueRepository issueRepository, Read read)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _voteStatsRepository = voteStatsRepository;
        _appUserRepository = appUserRepository;
        _issueRepository = issueRepository;
        _read = read;
    }

    #region Serve the home page
    public async Task<IActionResult> HomePage()
    {
        ContentFilter filter = new ContentFilter();
        if (Request.Cookies.TryGetValue("contentFilter", out string? cookieValue) && cookieValue != null)
        {
            filter = ContentFilter.FromJson(cookieValue);
        }

        filter.ContentStatus = ContentStatus.Published;


        // Create a view model to hold both issues and categories
        var viewModel = new Home_PageVM();
        viewModel.Sidebar.PageInfo = GetPageInfo(filter);
        viewModel.PaginatedContent = await _read.PaginatedMainContentFeed(filter);

        //viewModel.Categories = new List<Category_ReadVM>();
        return View(viewModel);
    }

    #endregion

    #region Get paginated home page content

    /// <summary>
    /// This method is used to return paginated issue posts.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [AllowAnonymous]
    [Route("/getPaginatedMainContentFeed")]
    public async Task<JsonResult> GetPaginatedContentItems(int currentPage = 1)
    {
        ContentFilter filter = new ContentFilter();
        if (Request.Cookies.TryGetValue("contentFilter", out string? cookieValue) && cookieValue != null)
        {
            filter = ContentFilter.FromJson(cookieValue);
        }

        ContentItems_Paginated_ReadVM paginatedContentItems = await _read.PaginatedMainContentFeed(filter, currentPage);

        // Render the partial view to a string
        string partialViewHtml = await ControllerExtensions.RenderViewToStringAsync(this, "~/Views/Home/_content-item-cards.cshtml", paginatedContentItems.ContentItems);

        // Create a response object with both the HTML and the pagination metadata
        var response = new ContentItems_Paginated_AjaxVM
        {
            html = partialViewHtml,
            pagination = new PaginationStats_VM
            {
                TotalCount = paginatedContentItems.TotalCount,
                PageSize = paginatedContentItems.PageSize,
                CurrentPage = paginatedContentItems.CurrentPage,
                TotalPages = (int)Math.Ceiling(paginatedContentItems.TotalCount / (double)paginatedContentItems.PageSize)
            }
        };

        response.Sidebar.PageInfo = GetPageInfo(filter);

        return Json(response);
    }

    #endregion

    public class SearchRequest
    {
        public string SearchString { get; set; }
    }

    [HttpPost]
    [Route("/search")]
    public async Task<IActionResult> Search([FromBody] SearchRequest searchRequest, [FromQuery] bool showRanks, [FromQuery] double rankCutOffPercent, [FromQuery] bool getSelect2Item, [FromQuery] string contentType = "both")
    {


        List<SearchResult> searchResult = await SearchContentItems.SearchAsync(searchRequest.SearchString, _context, rankCutOffPercent, contentType);

        if (getSelect2Item) {
            // add a rendering of the content item to each entry
            // loop over searchResult
            // for each type solution, 
            foreach (SearchResult result in searchResult)
            {
                if (result.Type == "Solution")
                {
                    Solution_ReadVM solution = await _read.Solution(result.Id, new ContentFilter());
                    result.Select2Item = await ControllerExtensions.RenderViewToStringAsync(this, "~/Views/Solution/_solution-select2-item.cshtml", solution);
                }
                if (result.Type == "Issue")
                {
                    Issue_ReadVM? issue = await _read.Issue(result.Id, new ContentFilter());
                    result.Select2Item = await ControllerExtensions.RenderViewToStringAsync(this, "~/Views/Issue/_issue-select2-item.cshtml", issue);
                }
            };


        }

        if (showRanks)
        {
            return Json(searchResult);
        }

        string html = await ControllerExtensions.RenderViewToStringAsync(this, "~/Views/Shared/_search-result.cshtml", searchResult);


        return Json(new {
            html = html
        });
    }


    /// <summary>
    /// This method is for manual http testing of Contains Search vs Free Text Search
    /// </summary>
    /// <param name="searchRequest"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("/search-contains")]
    public async Task<IActionResult> SearchContains([FromBody] SearchRequest searchRequest)
    {
        List<SearchResult> searchResult = await SearchContentItems.SearchContainsTableAsync(searchRequest.SearchString, _context);

        return Json(searchResult);
    }




    /// <summary>
    /// This method is used to request to vote
    /// It's purpose is to prevent accidental votes when just swiping
    /// </summary>
    /// <param name="model"></param>
    [AllowAnonymous] // There will be an error sent if user is not logged in
    [HttpPost]
    [Route("/vote-request")]
    public async Task<IActionResult> VoteRequest([FromBody] ConfirmVoteRequestDTO confirmVoteRequestDTO)
    {
        if (ModelState.IsValid) {
            // catch programming errors
            // Add specific validation checks
            if (confirmVoteRequestDTO.ContentID == Guid.Empty)
            {
                ModelState.AddModelError("ContentID", "ContentID: cannot be empty");
            }
            if (confirmVoteRequestDTO.ContentType == null)
            {
                ModelState.AddModelError("ContentType", "ContentType: cannot be empty");
            }

            if (confirmVoteRequestDTO.VoteValue < 0 || confirmVoteRequestDTO.VoteValue > 10) // Adjust range as needed
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

        var confirmVoteVM = new ConfirmVoteViewModel()
        {
            ContentID = confirmVoteRequestDTO.ContentID,
            ContentType = confirmVoteRequestDTO.ContentType,
            VoteValue = confirmVoteRequestDTO.VoteValue,
        };

        if (confirmVoteRequestDTO.ContentType == "issue")
        {
            Issue_ReadVM?  issue =  await _read.Issue(confirmVoteRequestDTO.ContentID, new ContentFilter());
            confirmVoteVM.ContentItem = Converter.ConvertIssue_ReadVMToContentItem_ReadVM(issue!);
        }
        else if (confirmVoteRequestDTO.ContentType == "solution")
        { 
            Solution_ReadVM? solution = await _read.Solution(confirmVoteRequestDTO.ContentID, new ContentFilter());
            confirmVoteVM.ContentItem = Converter.ConvertSolution_ReadVMToContentItem_ReadVM(solution!);
        }

        

        return Json(new {
            html = await ControllerExtensions.RenderViewToStringAsync(this, "~/Views/Shared/_ConfirmVote.cshtml", confirmVoteVM)
        });
    }


 

   

    [AllowAnonymous]
    [Route("/user-vote-modal")]
    public async Task<IActionResult> UserVoteModal([FromQuery] UserVoteModalRequest userVoteModalRequest)
    {
        if (userVoteModalRequest.ContentId == Guid.Empty)
        {
            ModelState.AddModelError("ContentId", "ContentID: cannot be empty");
        }
        if (userVoteModalRequest.ContentType == null || (userVoteModalRequest.ContentType != ContentType.Solution && userVoteModalRequest.ContentType != ContentType.Issue))
        {
            ModelState.AddModelError("ContentType", "ContentType: must not be null, and must equal issue or solution");
        }

        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Invalid vote data", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        var userVoteModalVM = new UserVoteModalVM
        {
            ContentVotesWithUserKey = new Dictionary<AppUser_ReadVM, Vote_Cacheable>()
        };

        if (userVoteModalRequest.ContentType == ContentType.Issue)
        {

            // Get info about the content
            Issue_ReadVM? issue_ReadVM = await _read.Issue(userVoteModalRequest.ContentId, new ContentFilter());
            if (issue_ReadVM != null)
            {
                userVoteModalVM.content = Converter.ConvertIssue_ReadVMToContentItem_ReadVM(issue_ReadVM);
                // Get user info from the vote stats
                var ContentVotes = issue_ReadVM.VoteStats.IssueVotes;
                foreach (var kvp in ContentVotes)
                {
                    var userId = kvp.Key;
                    var vote = kvp.Value;
                    var userVM = await _read.AppUser(userId);
                    if (userVM != null)
                    {
                        userVoteModalVM.ContentVotesWithUserKey[userVM] = vote;
                    }
                }
            }

          
        }
        else if (userVoteModalRequest.ContentType == ContentType.Solution)
        {
            Solution_ReadVM? solution_ReadVM = await _read.Solution(userVoteModalRequest.ContentId, new ContentFilter());
            if (solution_ReadVM != null)
            {
                userVoteModalVM.content = Converter.ConvertSolution_ReadVMToContentItem_ReadVM(solution_ReadVM);
                var ContentVotes = solution_ReadVM.VoteStats.SolutionVotes;
                foreach (var kvp in ContentVotes)
                {
                    var userId = kvp.Key;
                    var vote = kvp.Value;
                    var userVM = await _read.AppUser(userId);
                    if (userVM != null)
                    {
                        userVoteModalVM.ContentVotesWithUserKey[userVM] = vote;
                    }
                }
            }
        }

        return Json(new
        {
            success = true,
            html = await ControllerExtensions.RenderViewToStringAsync(this, "~/Views/Shared/Components/_userVotesModal.cshtml", userVoteModalVM)
        });
    }




    /// <summary>
    /// If no step is passed, it will just check if orientation is needed
    /// If a step is passed, it'll return html with a check on orientation status
    /// </summary>
    /// <param name="step"></param>
    /// <returns></returns>
    [Route("/orientation")]
    public async Task<IActionResult> Orientation([FromQuery] string? step, [FromQuery] bool? complete, [FromQuery] bool? restart)
    {
        // Check if user is signed in with sign in manager
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            // User is signed in
            // You can get the user object if needed:
            AppUser? user = await _userManager.GetUserAsync(User);


            if (complete == true)
            {
                try
                {
                    // Attach user entity to current context (in case it's not tracked)
                    _context.Attach(user!);
                    user!.OrientationCompletedAt = DateTime.UtcNow;
                    _context.Entry(user).Property(u => u.OrientationCompletedAt).IsModified = true;
                    await _context.SaveChangesAsync();
                    return Ok();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to set OrientationCompletedAt for user {UserId}", user.Id);
                    return Json(new { todoOrientation = true, error = "Failed to complete orientation. Please try again." });
                }
            }

            if (restart == true)
            {
                try
                {
                    // Attach user entity to current context (in case it's not tracked)
                    _context.Attach(user!);
                    user!.OrientationCompletedAt = null;
                    _context.Entry(user).Property(u => u.OrientationCompletedAt).IsModified = true;
                    await _context.SaveChangesAsync();
                    return Ok();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to set OrientationCompletedAt for user {UserId}", user.Id);
                    return Json(new { todoOrientation = true, error = "Failed to restart orientation. Please try again." });
                }
            }



            if (user.OrientationCompletedAt != null)
            {
                return (Json(new
                {
                    todoOrientation = false
                }));
            }


            if (step != null)
            {
                return (Json(new
                {
                    todoOrientation = true,
                    html = await ControllerExtensions.RenderViewToStringAsync(this, $"~/Views/Orientation/Steps/{step}.cshtml", new { })
                }));
            }

            return Json(new
            {
                todoOrientation = true
            });


        }

        // User is not signed in
        // Redirect or show a message as needed
        return (Json(new
        {
            todoOrientation = false
        }));

    }






    #region Routes that could be in a MiscellenousController

    /// <summary>
    /// This method is used to return a partial view for displaying alerts.
    /// </summary>
    /// <param name="alertType"></param>
    /// <param name="message"></param>

    [HttpGet]
    [Route("Shared/_Alert")]
    public IActionResult Alert(string type, string message = null, bool? dismissible = null, int? timeout = null)
    {

        AlertType alertType;
        if (!Enum.TryParse(type, ignoreCase: true, out alertType))
        {
            alertType = AlertType.success; // fallback
        }
        // Check if the message is in the header
        if (string.IsNullOrEmpty(message) && Request.Headers.TryGetValue("X-Alert-Message", out var headerMessage))
        {
            // Decode the Base64 message
            byte[] data = Convert.FromBase64String(headerMessage);
            message = System.Web.HttpUtility.UrlDecode(System.Text.Encoding.UTF8.GetString(data));
        }

        // Check for dismissible in headers
        bool isDismissible = true; // Default value
        if (dismissible.HasValue)
        {
            isDismissible = dismissible.Value;
        }
        else if (Request.Headers.TryGetValue("X-Alert-Dismissible", out var dismissibleHeader))
        {
            if (bool.TryParse(dismissibleHeader, out var parsedValue))
            {
                isDismissible = parsedValue;
            }
        }

        // Check for timeout in headers
        int alertTimeout = 5000; // Default value
        if (timeout.HasValue && timeout.Value >= 0)
        {
            alertTimeout = timeout.Value;
        }
        else if (Request.Headers.TryGetValue("X-Alert-Timeout", out var timeoutHeader))
        {
            if (int.TryParse(timeoutHeader, out var parsedValue) && parsedValue >= 0)
            {
                alertTimeout = parsedValue;
            }
        }

        // Create an alert view model
        var alert = new Alert_VM
        {
            Message = message,
            Type = alertType,
            Dismissible = isDismissible,
            Timeout = alertTimeout
        };

        // Return the alert partial view with the model
        return PartialView("~/Views/Shared/_Alert.cshtml", alert);
    }


    

    [Route("privacy")]
    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    #endregion



    public PageInfo GetPageInfo(ContentFilter filter)
    {

        PageInfo pageInfo = new PageInfo();

        string filterCacheString = filter.ToCacheString();
        ContentFilter defaultFilter = new ContentFilter();
        string defaultFilterCacheString = defaultFilter.ToCacheString();

        bool isFilterApplied = filterCacheString != defaultFilterCacheString;

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

        pageInfo.PageContext = """
            <span style="font-size:13px;">
                <strong>You are visiting the home page.</strong>
                <p>This page shows a combined feed of issues and solutions from across the platform. 
            Content is ordered by your selected sorting option and may be filtered based on your settings.</p>
            </span>
            """;


        return pageInfo;
    }

}
