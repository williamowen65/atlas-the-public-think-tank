using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace atlas_the_public_think_tank.Controllers
{
    [Authorize]
    public class CRUDForumController : Controller
    {
        [Route("/create")]
        public IActionResult Create()
        {
            return View();
        }
    }
}
