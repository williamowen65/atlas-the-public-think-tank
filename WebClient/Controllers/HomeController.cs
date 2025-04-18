using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using atlas_the_public_think_tank.Models;

namespace atlas_the_public_think_tank.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        //_logger.LogInformation("Loading home page");
        //_logger.LogDebug("Testing a log with debug level");
        //_logger.LogWarning("Testing a log with warning level");
        //_logger.LogError("Testing a log with error level");

        //Console.WriteLine("Testing regular write line");
        return View();
    }

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
