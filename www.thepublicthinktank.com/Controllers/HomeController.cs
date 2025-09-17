using atlas_the_public_think_tank.Data.CRUD;
using atlas_the_public_think_tank.Data.RepositoryPattern.Repository.Helpers;
using atlas_the_public_think_tank.Models.Enums;
using atlas_the_public_think_tank.Models.ViewModel;
using atlas_the_public_think_tank.Models.ViewModel.AjaxVM;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.ContentItem_Common;
using atlas_the_public_think_tank.Models.ViewModel.CRUD_VM.ContentItem_Common;
using atlas_the_public_think_tank.Models.ViewModel.PageVM;
using atlas_the_public_think_tank.Models.ViewModel.UI_VM;
using atlas_the_public_think_tank.Utilities;
using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol;
using System;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Web;

namespace atlas_the_public_think_tank.Controllers;

/// <summary>
/// This controller handles ALL the main pages   
/// and alert functionalities of the application.
/// </summary>
/// 
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    #region Serve the home page
    public async Task<IActionResult> Index()
    {
        ContentFilter filter = new ContentFilter();
        if (Request.Cookies.TryGetValue("contentFilter", out string? cookieValue) && cookieValue != null)
        {
            filter = ContentFilter.FromJson(cookieValue);
        }

   
        // Create a view model to hold both issues and categories
        var viewModel = new Home_PageVM();
        viewModel.Sidebar.PageInfo = GetPageInfo(filter);
        viewModel.PaginatedContent = await Read.PaginatedMainContentFeed(filter);

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

        ContentItems_Paginated_ReadVM paginatedContentItems = await Read.PaginatedMainContentFeed(filter, currentPage);

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
