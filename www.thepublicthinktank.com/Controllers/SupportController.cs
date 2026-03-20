using Microsoft.AspNetCore.Mvc;

namespace atlas_the_public_think_tank.Controllers
{
    public class SupportController : Controller
    {
        [Route("support")]
        public IActionResult SupportPage()
        {
            return View();
        }

        [Route("support/donate")]
        public IActionResult DonatePage()
        {
            return View();
        }

        [Route("support/feedback")]
        public IActionResult FeedbackPage()
        {
            return View();
        }
    }
}
