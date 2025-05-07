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
        public async Task<IActionResult> Create(Forum_CreateVM model) // Updated type
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);

                var forumPost = new Forum
                {
                    Title = model.Title,
                    Content = model.Content,
                    ScopeID = model.ScopeID,
                    ParentForumID = model.ParentForumID,
                    BlockedContentID = model.BlockedContentID,
                    AuthorID = user.Id,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Forums.Add(forumPost);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Categories = _context.Categories.ToList();
            return View(model);
        }

        [HttpGet]
        [Route("/api/posts")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllPosts()
        {
            var posts = await _context.Forums
                .Include(p => p.Scope)
                .Include(p => p.Author)
                .Select(p => new Forum_ReadVM
                {
                    ForumID = p.ForumID,
                    Title = p.Title,
                    Content = p.Content,
                    CreatedAt = p.CreatedAt,
                    ModifiedAt = p.ModifiedAt,
                    AuthorID = p.AuthorID,
                    ScopeID = p.ScopeID,
                    ParentForumID = p.ParentForumID,
                    BlockedContentID = p.BlockedContentID,
                    Author = p.Author,
                    Scope = p.Scope
                })
                .ToListAsync();

            return Ok(posts);
        }


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

        // Add this to CRUDForumController.cs
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

            return PartialView("~/Views/Forum/_left-sidebar-container.cshtml", categories);
        }


    }
}