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

        var response = await client.GetAsync("/api/posts");
        if (response.IsSuccessStatusCode)
        {
            var jsonString = await response.Content.ReadAsStringAsync();
            var posts = JsonSerializer.Deserialize<List<ForumPost_ReadVM>>(jsonString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // Handles case-insensitive property names
            });

            return View(posts); // Pass the posts to the view
        }

        // Handle error (e.g., log it or show an error message)
        _logger.LogError($"Failed to fetch posts. Status Code: {response.StatusCode}");
        return View(new List<ForumPost_ReadVM>()); // Return an empty list if the API call fails
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
