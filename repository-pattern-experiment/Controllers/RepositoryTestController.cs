using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using repository_pattern_experiment.Data.CRUD;
using repository_pattern_experiment.Data.RepositoryPattern.IRepository;
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
         */


        /// <summary>
        /// Return Issue_ReadVM
        /// </summary>
        /// <remarks>
        /// An issue is composed of parts which are cached separately and then combined
        /// </remarks>
        [Route("get-issue-by-id/{issueId}")]
        public async Task<JsonResult> GetIssueById(Guid issueId)
        {
            try
            {
                var issue = await Read.Issue(issueId);
                return Json(issue);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [Route("get-solution-by-id/{solutionId}")]
        public async Task<JsonResult> GetSolutionById(Guid solutionId)
        {
            try
            {
                var solution = await Read.Solution(solutionId);
                return Json(solution);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [Route("get-sub-issue-feed-ids-of-issue/{issueId}")]
        public async Task<JsonResult> GetSubIssueFeedIdsOfIssue(Guid issueId, int pageNumber = 1)
        {
            try
            {

               var paginatedIssuesResponse = await filterIdSetRepository.GetPagedSubIssueIdsOfIssueById(issueId, pageNumber);

                return Json(paginatedIssuesResponse);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [Route("get-solution-feed-ids-of-issue/{issueId}")]
        public async Task<JsonResult> GetSolutionFeedIdsOfIssue(Guid issueId, int pageNumber = 1)
        {
            try
            {
                throw new NotImplementedException();

                var paginatedIssuesResponse = await filterIdSetRepository.GetPagedSubIssueIdsOfIssueById(issueId, pageNumber);

                return Json(paginatedIssuesResponse);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


    }
}
