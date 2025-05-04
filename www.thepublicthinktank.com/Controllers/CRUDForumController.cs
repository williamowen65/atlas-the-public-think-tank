using atlas_the_public_think_tank.Data;
using atlas_the_public_think_tank.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace atlas_the_public_think_tank.Controllers
{
    [Authorize]
    public class CRUDForumController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public CRUDForumController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Route("/create")]
        public IActionResult Create()
        {
            ViewBag.Categories = _context.Categories.ToList();
            return View();
        }

        [HttpPost]
        [Route("/create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ForumPost_CreateVM model)
        {
           if (ModelState.IsValid)
           {
               var user = await _userManager.GetUserAsync(User);

               var forumPost = new ForumPost
               {
                   Title = model.Title,
                   Content = model.Content,
                   CategoryID = model.CategoryID,
                   ParentPostID = model.ParentPostID,
                   Status = model.Status,
                   UserID = user.Id,
                   CreatedAt = DateTime.UtcNow
               };

               _context.ForumPosts.Add(forumPost);
               await _context.SaveChangesAsync();

               return RedirectToAction("Index", "Home");
           }

           ViewBag.Categories = _context.Categories.ToList();
           return View(model);
        }

    }
}
