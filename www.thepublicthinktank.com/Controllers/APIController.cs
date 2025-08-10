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

        #region Api Docs

        [AllowAnonymous]
        public JsonResult Index()
        {
            var apiDocs = new Dictionary<string, object>
            {
                ["/api"] = new Dictionary<string, object>
                {
                    ["/content-feed"] = "Endpoint for the home page content feed",
                    ["/content-feed?pageNumber=1"] = "Endpoint for the home page content feed with pagination",
                    ["/issue/{issueId}"] = "Endpoint for an issue page",
                    ["/issue-comments/{issueId}?pageNumber=1"] = "Endpoint for an issue page comment pagination",
                    ["/issue-sub-issues/{issueId}?pageNumber=1"] = "Endpoint for an issue page sub-issues pagination",
                    ["/issue-solutions/{issueId}?pageNumber=1"] = "Endpoint for an issue page solutions pagination",
                    ["/solution/{solutionId}"] = "Endpoint for a solution page",
                    ["/solution-comments/{solutionId}?pageNumber=1"] = "Endpoint for a solution page comments pagination",
                    ["/solution-sub-issues/{solutionId}?pageNumber=1"] = "Endpoint for a solution page sub-issue pagination",
                    ["/cache-log"] = new Dictionary<string, object>
                    {
                        ["/keys"] = "Endpoint for inspecting cache keys",
                        ["/entries"] = "Endpoint for inspecting cache entries",
                    }
                },
                ["Filter Query Params Examples"] = new List<string>
                {
                    "{endpoint}?AvgVoteRange.Min=2.5&AvgVoteRange.Max=9.5&TotalVoteCount.Min=10&TotalVoteCount.Max=&DateRange.Start=2025-01-01&DateRange.End=2025-05-01&Tags=bug&Tags=urgent"
                },
                
            };

            return Json(apiDocs);
        }
        #endregion

        #region Api Routes

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

                bool fetchParent = true;

                var issue = await Read.Issue(issueId, filter, fetchParent);
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
                bool fetchParent = true;
                var solution = await Read.Solution(solutionId, filter, fetchParent);
                return Json(solution);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        [Route("api/content-feed")]
        [AllowAnonymous]
        public async Task<JsonResult> GetContentFeed([FromQuery] ContentFilter filter, int pageNumber = 1)
        {
            try
            {
                var contentItems = await Read.ContentItems(filter, pageNumber);
                return Json(contentItems);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        #endregion


    }
}
