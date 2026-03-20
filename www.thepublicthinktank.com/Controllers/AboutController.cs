using Microsoft.AspNetCore.Mvc;

namespace atlas_the_public_think_tank.Controllers
{
    public class AboutController : Controller
    {
        [Route("about")]
        public IActionResult AboutPage()
        {
            return View();
        }

        [Route("about/how-it-works")]
        public IActionResult HowItWorks()
        {
            return View();
        }

        [Route("about/mission")]
        public IActionResult Mission()
        {
            return View();
        }
    }
}
