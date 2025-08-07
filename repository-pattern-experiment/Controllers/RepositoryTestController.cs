using Microsoft.AspNetCore.Mvc;
using repository_pattern_experiment.Data.RepositoryPattern.IRepository;
using repository_pattern_experiment.Models;
using repository_pattern_experiment.Models.ViewModel;
using System.Diagnostics;
namespace repository_pattern_experiment.Controllers
{
    public class RepositoryTestController : Controller
    {
        private readonly ILogger<RepositoryTestController> _logger;
        private readonly IIssueRepository _issueRepository;
        private readonly IBreadcrumbRepository _breadcrumbRepository;
        private readonly IVoteStatsRepository _voteStatsRepository;

        public RepositoryTestController(
            ILogger<RepositoryTestController> logger,
            IIssueRepository issueRepository,
            IBreadcrumbRepository breadcrumbRepository,
            IVoteStatsRepository voteStatsRepository)
        {
            _logger = logger;
            _issueRepository = issueRepository;
            _breadcrumbRepository = breadcrumbRepository;
            _voteStatsRepository = voteStatsRepository;
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
            var issueContent = await _issueRepository.GetIssueById(issueId);

            UserVote_Issue_ReadVM? issueVoteStats = await _voteStatsRepository.GetIssueVoteStats(issueId);
            // Assemble an issue from the IRepository
            // Feed data should be paginated (Solutions, Sub-Issues, Comments, Breadcrumb (Parent posts))
            // These will be database calls for list of IDs (Paginated/Filtered/Sorted)
            // ------ The related content of these IDs are then accessed via related repositories
            // The goal is to construct an Issue_ReadVM

            Issue_ReadVM issue = new Issue_ReadVM()
            {
                Content = issueContent.Content,
                ParentIssueID = issueContent.ParentIssueID,
                ParentSolutionID = issueContent.ParentSolutionID,
                Title = issueContent.Title,
                Author = issueContent.Author,
                ContentStatus = issueContent.ContentStatus,
                CreatedAt = issueContent.CreatedAt,
                ModifiedAt = issueContent.ModifiedAt,
                Scope = issueContent.Scope,
                IssueID = issueContent.Id,
                VoteStats = new UserVote_Issue_ReadVM()
                {
                    AverageVote = issueVoteStats.AverageVote,
                    ContentID = issueVoteStats.ContentID,
                    TotalVotes = issueVoteStats.TotalVotes,
                    IssueVotes = issueVoteStats.IssueVotes,
                    //UserVote = await _voteStatsRepository.GetActiveUserIssueVote(issueId),
                },
                BreadcrumbTags = await _breadcrumbRepository.GetBreadcrumbPagedAsync(issueContent.ParentIssueID ?? issueContent.ParentSolutionID ?? issueId)
            };

            return Json(issue);
        }

    }
}
