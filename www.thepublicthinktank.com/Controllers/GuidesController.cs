using Microsoft.AspNetCore.Mvc;

namespace atlas_the_public_think_tank.Controllers
{
    public class GuidesController : Controller
    {

        [Route("guides")]
        public IActionResult GuidesPage()
        {
            return View();
        }

        [Route("guides/testing")]
        public IActionResult TestingGuide()
        {
            return View();
        }

        [Route("guides/creating-issues")]
        public IActionResult CreatingIssuesGuide()
        {
            return View();
        }

        [Route("guides/creating-solutions")]
        public IActionResult CreatingSolutionsGuide()
        {
            return View();
        }
    }
}
