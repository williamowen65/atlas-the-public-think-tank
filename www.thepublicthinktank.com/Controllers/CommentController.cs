using Microsoft.AspNetCore.Mvc;

/// <summary>
/// This is the C# Controller for managing comments for issues and solutions.
/// </summary>
namespace atlas_the_public_think_tank.Controllers
{
    public class CommentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
