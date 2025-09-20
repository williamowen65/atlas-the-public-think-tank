using Microsoft.AspNetCore.Mvc;

namespace atlas_the_public_think_tank.Controllers
{
    public class UserProfileController : Controller
    {

        [Route("/user-profile")]
        public IActionResult UserProfile()
        {
            return View();
        }
    }
}
