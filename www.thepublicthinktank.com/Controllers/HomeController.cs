using atlas_the_public_think_tank.Models;
using atlas_the_public_think_tank.Models.ViewModel;
using atlas_the_public_think_tank.Services;
using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly Services.CRUD _crudService; 

    public HomeController(ILogger<HomeController> logger, Services.CRUD crudService)
    {
        _logger = logger;
        _crudService = crudService;
    }


    public async Task<IActionResult> Index()
    {

        // Create a view model to hold both issues and categories
        var viewModel = new HomeIndexViewModel();

        viewModel.PaginatedContent = await _crudService.GetContentItemsPagedAsync(1);

        //viewModel.Categories = new List<Category_ReadVM>();
        return View(viewModel);
    }


    /// <summary>
    /// This method is used to return paginated issue posts.
    /// </summary>
    /// <returns></returns>
    [HttpGet]

    [AllowAnonymous]
    [Route("/home/getPaginatedContent")]
    public async Task<IActionResult> GetPaginatedContentItems(int currentPage = 1)
    {
        PaginatedContentItemsResponse paginatedContentItems = await _crudService.GetContentItemsPagedAsync(currentPage, 3);

        return PartialView("~/Views/Home/_content-item-cards.cshtml", paginatedContentItems.ContentItems);
    }

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
        var alert = new Alert_ReadVM
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



}