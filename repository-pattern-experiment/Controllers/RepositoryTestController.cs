using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using repository_pattern_experiment.Data.CRUD;
using repository_pattern_experiment.Data.RepositoryPattern.IRepository;
using repository_pattern_experiment.Data.RepositoryPattern.Repository.Helpers;
using repository_pattern_experiment.Models;
using repository_pattern_experiment.Models.ViewModel;
using System.Diagnostics;
namespace repository_pattern_experiment.Controllers
{
    public class RepositoryTestController : Controller
    {
        private readonly ILogger<RepositoryTestController> _logger;
        private readonly IFilterIdSetRepository filterIdSetRepository;

        public RepositoryTestController(
            ILogger<RepositoryTestController> logger, IFilterIdSetRepository filterIdSetRepository)
        {
            _logger = logger;
            this.filterIdSetRepository = filterIdSetRepository;
        }

        public IActionResult Index()
        {
            return View();
        }



        /*
         The entire test app has endpoints for testing of Repository Pattern setup

        Unlike the main app, these represent api end points
         */


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
        [Route("get-issue-by-id/{issueId}")]
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

        [Route("get-solution-by-id/{solutionId}")]
        public async Task<JsonResult> GetSolutionById(Guid solutionId, [FromQuery]  ContentFilter filter)
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


        [Route("get-content-feed")]
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
            The routes below are for testing, an not meant to be part of the public api.
         */

        [Route("get-content-feed-ids")]
        public async Task<JsonResult> GetContentFeedIds([FromQuery] ContentFilter filter, int pageNumber = 1)
        {
            try
            {
                var paginatedMainContentFeedIds = await filterIdSetRepository.GetPagedMainContentFeedIds(filter, pageNumber);
                return Json(paginatedMainContentFeedIds);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        [Route("get-sub-issue-feed-ids-of-issue/{issueId}")]
        public async Task<JsonResult> GetSubIssueFeedIdsOfIssue(Guid issueId, [FromQuery] ContentFilter filter, int pageNumber = 1)
        {
            try
            {

               var paginatedSubIssueFeedIds = await filterIdSetRepository.GetPagedSubIssueIdsOfIssueById(issueId, filter, pageNumber);

                return Json(paginatedSubIssueFeedIds);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [Route("get-sub-issue-feed-ids-of-solution/{solutionId}")]
        public async Task<JsonResult> GetSubIssueFeedIdsOfSolution(Guid solutionId, [FromQuery] ContentFilter filter, int pageNumber = 1)
        {
            try
            {

               var paginatedSubIssueFeedIds = await filterIdSetRepository.GetPagedSubIssueIdsOfSolutionById(solutionId, filter, pageNumber);

                return Json(paginatedSubIssueFeedIds);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [Route("get-solution-feed-ids-of-issue/{issueId}")]
        public async Task<JsonResult> GetSolutionFeedIdsOfIssue(Guid issueId, [FromQuery] ContentFilter filter, int pageNumber = 1)
        {
            try
            {

                var paginatedSolutionFeedIds = await filterIdSetRepository.GetPagedSolutionIdsOfIssueById(issueId, filter, pageNumber);

                return Json(paginatedSolutionFeedIds);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


    }
}
