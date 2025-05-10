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


        [Route("/create-forum")]
        public IActionResult CreateForum()
        {

            Forum_CreateVM newForum = new()
            {
                Categories = _context.Categories.ToList(),
                Scopes = _context.Scopes.ToList(),
            };

            return View(newForum);
        }

        [HttpPost]
        [Route("/create-forum")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateForum(Forum_CreateVM model)
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
                    ContentStatus = model.ContentStatus,
                    AuthorID = user.Id,
                    CreatedAt = DateTime.UtcNow
                };

                var entry = _context.Forums.Add(forumPost);
                await _context.SaveChangesAsync(); // Save to generate the ForumID

                // Now add the category relationships
                if (model.SelectedCategoryIds != null && model.SelectedCategoryIds.Any())
                {
                    foreach (int categoryId in model.SelectedCategoryIds)
                    {
                        var forumCategory = new ForumCategory
                        {
                            ForumID = entry.Entity.ForumID,
                            CategoryID = categoryId
                        };
                        _context.ForumCategories.Add(forumCategory);
                    }


                    // Save the User History (todo)

                    await _context.SaveChangesAsync();
                }

                return RedirectToAction("Index", "Home");
            }

            // If we got this far, something failed, redisplay form
            // Repopulate the dropdown data
            model.Categories = _context.Categories.ToList();
            model.Scopes = _context.Scopes.ToList();
            return View(model);
        }

        [HttpGet]
        [Route("/api/posts")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllForums()
        {
            var posts = await _context.Forums
                .Include(p => p.Scope)
                .Include(p => p.Author)
                .Include(p => p.ForumCategories)
                    .ThenInclude(fc => fc.Category)
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
                    Scope = p.Scope,
                    Categories = p.ForumCategories.Select(fc => new Category_ReadVM
                    {
                        CategoryID = fc.Category.CategoryID,
                        CategoryName = fc.Category.CategoryName
                    })
                    .ToList()
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

        /*
         
         TODO: Convert all of the Stored Procedures to C# code as routes.
        This will be more maintainable than splitting logic. 
        Stored procedures can be done in future if there are performance reasons. 
         
         */

        [HttpPost]
        [Route("/forum/vote")]
        public async Task<IActionResult> ForumVote(UserVote_Forum_CreateVM model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid vote data" });
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                {
                    return Json(new { success = false, message = "You must login in order to vote" });
                }

                // Check if the user has already voted on this forum
                var existingVote = await _context.UserVotes
                    .OfType<ForumVote>()
                    .FirstOrDefaultAsync(v => v.UserID == user.Id && v.ForumID == model.ForumID);

                if (existingVote != null)
                {
                    // Update existing vote
                    existingVote.VoteValue = (int)model.VoteValue;
                    existingVote.ModifiedAt = DateTime.UtcNow;
                    _context.UserVotes.Update(existingVote);
                }
                else
                {
                    // Create new vote
                    ForumVote vote = new ForumVote
                    {
                        User = user,
                        UserID = user.Id,
                        ForumID = model.ForumID,
                        VoteValue = (int)model.VoteValue,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.UserVotes.Add(vote);
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Vote saved successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        [AllowAnonymous]
        [Route("/Forum/GetVoteDial")]
        public async Task<IActionResult> GetVoteDial(int forumId)
        {

            int? userVote = 5; 
            // Check if the forumId exists in the database
            var forumExists = _context.Forums.Any(f => f.ForumID == forumId);
            if (!forumExists)
            {
                return NotFound(new { message = "Forum not found" });
            }

            var user = await _userManager.GetUserAsync(User);
           
            if (user != null)
            {
                // Get a possible vote value from the db
                var existingVote = await _context.UserVotes
                    .OfType<ForumVote>()
                    .FirstOrDefaultAsync(v => v.UserID == user.Id && v.ForumID == forumId);

                if (existingVote != null)
                {
                    userVote = existingVote.VoteValue;
                }
            }


            // Retrieve all user votes for the specified forum
            var userVotes = await _context.UserVotes
                .OfType<ForumVote>()
                .Select(v => new
                {
                    v.UserID,
                    v.VoteValue
                })
                .ToListAsync();


            // Get vote data for the forum
            UserVote_Forum_ReadVM m = new UserVote_Forum_ReadVM
            {
                ForumID = forumId,
                AverageVote = userVotes.Any() ? userVotes.Average(p => p.VoteValue) : 0,
                TotalVotes = userVotes.Count,
                UserVote = userVote,

            };

            // Return the partial view with the vote data model
            return PartialView("~/Views/Forum/_voteDial.cshtml", m);
        }
       



    }
}