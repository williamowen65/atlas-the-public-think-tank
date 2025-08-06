using Microsoft.AspNetCore.Mvc;
using repository_pattern_experiment.Data.RepositoryPattern.IRepository;
using repository_pattern_experiment.Models;
using System.Diagnostics;

namespace repository_pattern_experiment.Controllers
{
    public class RepositoryTestController : Controller
    {
        private readonly ILogger<RepositoryTestController> _logger;
        private readonly IIssueRepository _issueRepository;

        public RepositoryTestController(
            ILogger<RepositoryTestController> logger, 
            IIssueRepository issueRepository)
        {
            _logger = logger;
            _issueRepository = issueRepository;
        }

        public IActionResult Index()
        {
            return View();
        }

        /*
         The entire test app has endpoints for testing of Repository Pattern setup
         */

        [Route("get-issue-by-id/{issueId}")]
        public async Task<JsonResult> GetIssueById(Guid issueId)
        {
            var issue = await _issueRepository.GetIssueById(issueId);
            return Json(issue);
        }

    }
}
