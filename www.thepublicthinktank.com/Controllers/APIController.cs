using atlas_the_public_think_tank.Data;
using atlas_the_public_think_tank.Models.ViewModel;
using atlas_the_public_think_tank.Models.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace atlas_the_public_think_tank.Controllers
{
    /// <summary>
    /// This C# controller handles the "/api/{...}" endpoints.<br/>
    /// The app will consume its own API to access data, making this data publicly available and consumable.
    /// </summary>
    [Authorize]
    public class APIController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public APIController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        /// <summary>
        /// This method is used to return all categories.
        /// </summary>
        [HttpGet]
        [Route("/api/categories")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllCategories()
        {
            var posts = await _context.Categories
                    .Select(p => new Category_ReadVM
                    {
                        CategoryID = p.CategoryID,
                        CategoryName = p.CategoryName
                    })
                .ToListAsync();


            return Ok(posts);
        }



        /// <summary>
        /// This method is used to return the sidebar with all categories.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("/api/sidebar")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSidebar()
        {
            var categories = await _context.Categories
                .Select(p => new Category_ReadVM
                {
                    CategoryID = p.CategoryID,
                    CategoryName = p.CategoryName
                })
                .ToListAsync();

            return PartialView("~/Views/Shared/Components/_left-sidebar-container.cshtml", categories);
        }


    }
}
