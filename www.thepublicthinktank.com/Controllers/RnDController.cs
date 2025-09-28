using atlas_the_public_think_tank.Data.CRUD;
using atlas_the_public_think_tank.Data.RepositoryPattern.Repository.Helpers;
using atlas_the_public_think_tank.Models.Enums;
using atlas_the_public_think_tank.Models.ViewModel.AjaxVM;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Issue;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Solution;
using atlas_the_public_think_tank.Models.ViewModel.CRUD_VM.ContentItem_Common;
using atlas_the_public_think_tank.Models.ViewModel.PageVM;
using atlas_the_public_think_tank.Models.ViewModel.UI_VM;
using Microsoft.AspNetCore.Mvc;

namespace atlas_the_public_think_tank.Controllers
{
    public class RnDController : Controller
    {
    

        public IActionResult RnDCreateIssue()
        {
            Issue_CreateOrEdit_AjaxVM model = new Issue_CreateOrEdit_AjaxVM() { 
                Issue = new Issue_CreateOrEditVM()
            };


            return View(model);
        }

        public async Task<IActionResult> RnDEditIssue(Guid issueId)
        {
           

            Issue_ReadVM? issue = await Read.Issue(issueId, new ContentFilter());


            if (issue == null)
            {
                throw new Exception("Issue doesn't exist for GET EditIssuePartialView");
            }

            Issue_CreateOrEdit_AjaxVM model = new Issue_CreateOrEdit_AjaxVM() { 
                Issue = new Issue_CreateOrEditVM() {
                    Content = issue.Content,
                    ContentStatus = issue.ContentStatus,
                    ParentIssueID = issue.ParentIssueID,
                    ParentSolutionID = issue.ParentSolutionID,
                    Scope = new Scope_CreateOrEditVM()
                    {
                        ScopeID = issue.Scope.ScopeID,
                        Boundaries = issue.Scope.Boundaries,
                        Domains = issue.Scope.Domains,
                        EntityTypes = issue.Scope.EntityTypes,
                        Scales = issue.Scope.Scales,
                        Timeframes = issue.Scope.Timeframes
                    },
                    Title = issue.Title,
                    IssueID = issue.IssueID,
                }
            };


            return View(model);
        }

        /// <summary>
        /// This method is used to create a new issue post.
        /// </summary>
        /// <param name="model"></param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RnDCreateIssue(Issue_CreateOrEditVM model, ContentStatus contentStatus)
        {
            Console.WriteLine();
            return Ok();
        }

        [Route("/401")]
        public IActionResult Get401Page()
        {
            // this route is for observing this page, for making style updates.
            return Unauthorized();
        }
        [Route("/404")]
        public IActionResult Get404Page()
        {
            // this route is for observing this page, for making style updates.
            return NotFound();
        }
    }
}
