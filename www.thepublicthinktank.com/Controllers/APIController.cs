using atlas_the_public_think_tank.Data;
using atlas_the_public_think_tank.Models.ViewModel;
using atlas_the_public_think_tank.Models.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using atlas_the_public_think_tank.Data.RepositoryPattern.Repository.Helpers;
using atlas_the_public_think_tank.Data.CRUD;


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

        [AllowAnonymous]
        public JsonResult Index()
        {
            var apiDocs = new Dictionary<string, object>
            {
                ["/api"] = new Dictionary<string, object>
                {
                    ["/content-feed"] = "Endpoint for the home page content feed",
                    ["/issue/{issueId}"] = "Endpoint for an issue page",
                    ["/solution/{solutionId}"] = "Endpoint for a solution page",
                    ["/cache-log"] = new Dictionary<string, object>
                    {
                        ["/keys"] = "Endpoint for inspecting cache keys",
                        ["/entries"] = "Endpoint for inspecting cache entries",
                    }
                },
                ["Filter Query Params Examples"] = new List<string>
                {
                    "{endpoint}?AvgVoteRange.Min=2.5&AvgVoteRange.Max=9.5&TotalVoteCount.Min=10&TotalVoteCount.Max=&DateRange.Start=2025-01-01&DateRange.End=2025-05-01&Tags=bug&Tags=urgent"
                }
            };

            return Json(apiDocs);
        }


        /// <summary>
        /// Api route which returns an Issue_ReadVM
        /// </summary>
        /// <remarks>
        /// An issue is composed of parts which are cached separately and then combined
        /// <para>
        /// Apply filter queries <br/>
        /// Ex:    {endpoint}?AvgVoteRange.Min=2.5&AvgVoteRange.Max=9.5&TotalVoteCount.Min=10&TotalVoteCount.Max=&DateRange.Start=2025-01-01&DateRange.End=2025-05-01&Tags=bug&Tags=urgent
        /// </para>
        /// </remarks>
        [Route("api/issue/{issueId}")]
        [AllowAnonymous]
        public async Task<JsonResult> GetIssueById(Guid issueId, [FromQuery] ContentFilter filter)
        {
            try
            {
                var issue = await Read.Issue(issueId, filter);
                return Json(issue);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [Route("api/solution/{solutionId}")]
        [AllowAnonymous]
        public async Task<JsonResult> GetSolutionById(Guid solutionId, [FromQuery] ContentFilter filter)
        {
            try
            {
                var solution = await Read.Solution(solutionId, filter);
                return Json(solution);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        [Route("api/content-feed")]
        [AllowAnonymous]
        public async Task<JsonResult> GetContentFeed([FromQuery] ContentFilter filter)
        {
            try
            {
                var contentItems = await Read.ContentItems(filter);
                return Json(contentItems);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /*

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
        */

    }
}
