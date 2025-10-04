using atlas_the_public_think_tank.Data.CRUD;
using atlas_the_public_think_tank.Data.RepositoryPattern.Repository.Helpers;
using atlas_the_public_think_tank.Email.Infrastructure;
using atlas_the_public_think_tank.Email.Models;
using atlas_the_public_think_tank.Models.Enums;
using atlas_the_public_think_tank.Models.ViewModel.AjaxVM;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Issue;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Solution;
using atlas_the_public_think_tank.Models.ViewModel.CRUD_VM.ContentItem_Common;
using atlas_the_public_think_tank.Models.ViewModel.PageVM;
using atlas_the_public_think_tank.Models.ViewModel.UI_VM;
using atlas_the_public_think_tank.Utilities;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;


namespace atlas_the_public_think_tank.Controllers
{
    public class RnDController : Controller
    {

        private readonly Read _read;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;
        public RnDController(Read read, IEmailSender emailSender, IConfiguration configuration) 
        {
            _read = read;
            _emailSender = emailSender;
            _configuration = configuration;
        }

        public IActionResult RnDCreateIssue()
        {
            Issue_CreateOrEdit_AjaxVM model = new Issue_CreateOrEdit_AjaxVM() { 
                Issue = new Issue_CreateOrEditVM()
            };


            return View(model);
        }

        public async Task<IActionResult> RnDEditIssue(Guid issueId)
        {
           

            Issue_ReadVM? issue = await _read.Issue(issueId, new ContentFilter());


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

        [Route("test-real-email")]
        public async Task<IActionResult> TestRealEmail() {
            var emailSender = new STMPEmailSender(_configuration);
            await emailSender.SendEmailAsync("william.owen.career@gmail.com", "Test from the app", "hello world");

            return Ok();
        }


        [HttpPost]
        [Route("/test-email")]
        public async Task<IActionResult> TestEmail([FromBody] TestEmailRequest request, [FromQuery] string emailTemplate)
        {
            if (string.IsNullOrWhiteSpace(emailTemplate))
            {
                return BadRequest(new { success = false, message = "The 'emailTemplate' query parameter is required." });
            }

            if (!ModelState.IsValid)
            {
                // Return model validation errors as JSON
                return BadRequest(new
                {
                    success = false,
                    errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                      .ToDictionary(
                          kvp => kvp.Key,
                          kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                      )
                });
            }


            try
            {

                string modelTypeName = $"atlas_the_public_think_tank.Email.Models.{emailTemplate}Model";
                Type modelType = Type.GetType(modelTypeName);

                if (modelType == null)
                {
                    return BadRequest(new { success = false, message = $"Model type '{modelTypeName}' not found." });
                }

                var json = JsonSerializer.Serialize(request.BodyModel);
                var model = JsonSerializer.Deserialize(json, modelType);


                // Render the specified Razor email template with the provided model
                string emailTemplateRendered = await ControllerExtensions.RenderViewToStringAsync(
                    this,
                    $"~/Email/Templates/{emailTemplate}.cshtml",
                    model
                );

                // Send the rendered email
                await _emailSender.SendEmailAsync(request.To, request.Subject, emailTemplateRendered);
                return Ok(new { success = true, message = "Email sent successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = $"Failed to send email: {ex.Message}" });
            }
        }

        public class TestEmailRequest
        {
            public string To { get; set; }
            public string Subject { get; set; }
            public object BodyModel { get; set; }
        }
    }
}
