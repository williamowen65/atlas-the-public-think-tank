using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using atlas_the_public_think_tank.Models;
using Azure.Core;
using System.Text.Json;

namespace atlas_the_public_think_tank.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        using var client = new HttpClient();
        client.BaseAddress = new Uri($"{Request.Scheme}://{Request.Host}");

        // Create a view model to hold both forums and categories
        var viewModel = new HomeIndexViewModel();
        
        // Fetch forums
        var forumResponse = await client.GetAsync("/api/posts");
        if (forumResponse.IsSuccessStatusCode)
        {
            var jsonString = await forumResponse.Content.ReadAsStringAsync();
            viewModel.Forums = JsonSerializer.Deserialize<List<Forum_ReadVM>>(jsonString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        else
        {
            _logger.LogError($"Failed to fetch posts. Status Code: {forumResponse.StatusCode}");
            viewModel.Forums = new List<Forum_ReadVM>();
        }
        
        // Fetch categories
        var categoryResponse = await client.GetAsync("/api/categories");
        if (categoryResponse.IsSuccessStatusCode)
        {
            var jsonString = await categoryResponse.Content.ReadAsStringAsync();
            viewModel.Categories = JsonSerializer.Deserialize<List<Category_ReadVM>>(jsonString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        else
        {
            _logger.LogError($"Failed to fetch categories. Status Code: {categoryResponse.StatusCode}");
            viewModel.Categories = new List<Category_ReadVM>();
        }

        return View(viewModel);
    }


    [Route("test")]
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