using atlas_the_public_think_tank.Data;
using atlas_the_public_think_tank.Models;
using atlas_the_public_think_tank.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace atlas_the_public_think_tank.Controllers
{


    /// <summary>
    /// This controller handles all CRUD operations for 
    /// the Issue, Solutions, Comments, UserHistory, and Voting
    /// </summary>
    [Authorize]
    public class IssueController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly CRUD _crud;

        public IssueController(ApplicationDbContext context, UserManager<AppUser> userManager, CRUD crud)
        {
            _context = context;
            _userManager = userManager;
            _crud = crud;
        }

        /// <summary>
        /// This method is used to return the create issue page.
        /// </summary>
        [Route("/create-issue")]
        public IActionResult CreateIssue(Guid? parentIssueID, Guid? parentSolutionID)
        {

            Issue_CreateVM newIssue = new()
            {
                Categories = _context.Categories.ToList(),
                Scopes = _context.Scopes.ToList(),
                ParentIssueID = parentIssueID,
                ParentSolutionID = parentSolutionID
            };

            return View(newIssue);
        }




        /// <summary>
        /// This method is used to create a new issue post.
        /// </summary>
        /// <param name="model"></param>
        [HttpPost]
        [Route("/create-issue")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateIssue(Issue_CreateVM model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);


                var issuePost = new Issue
                {
                    Title = model.Title,
                    Content = model.Content,
                    ScopeID = model.ScopeID,
                    ParentIssueID = model.ParentIssueID,
                    ParentSolutionID = model.ParentSolutionID,
                    ContentStatus = model.ContentStatus,
                    AuthorID = user.Id,
                    CreatedAt = DateTime.UtcNow
                };

                var entry = _context.Issues.Add(issuePost);
                await _context.SaveChangesAsync(); // Save to generate the IssueID

                // Now add the category relationships
                if (model.SelectedCategoryIds != null && model.SelectedCategoryIds.Any())
                {
                    foreach (Guid categoryId in model.SelectedCategoryIds)
                    {
                        var issueCategory = new IssueCategory
                        {
                            IssueID = entry.Entity.IssueID,
                            CategoryID = categoryId
                        };
                        _context.IssueCategories.Add(issueCategory);
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


        /// <summary>
        /// This method is used to return all issue posts.
        /// </summary>
        [HttpGet]
        [Route("/api/posts")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllIssues()
        {
            List<Issue_ReadVM> postsViewModel = await _crud.Issues.GetEveryIssue();

            return Ok(postsViewModel);
        }


        /// <summary>
        /// Returns a HTML page for a specific issue
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("/issue/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> ReadIssue(Guid id)
        {
            var issue = await _context.Issues
            .Include(f => f.Author)
            .Include(f => f.Scope)
            .Include(f => f.ParentIssue)
                .ThenInclude(p => p.Scope) // include the parent issue's scope
            .Include(f => f.ParentIssue)
                .ThenInclude(p => p.Solutions)
            .Include(f => f.ParentSolution)
                .ThenInclude(p => p.Scope) // include the parent issue's scope
            //.Include(f => f.ParentSolution)
                //.ThenInclude(p => p.Solutions)
            .Include(f => f.ChildIssues)
                 .ThenInclude(c => c.Scope) // include the Child issue's scope
            .Include(f => f.BlockedContent)
            .Include(f => f.Solutions)
                .ThenInclude(s => s.Scope)
            .Include(f => f.Solutions)
                .ThenInclude(s => s.ChildIssues)
            .Include(f => f.Solutions)
                .ThenInclude(s => s.SolutionCategories)
                .ThenInclude(sc => sc.Category)
            .Include(f => f.Comments)
            .Include(f => f.IssueCategories)
                .ThenInclude(fc => fc.Category)
            .FirstOrDefaultAsync(f => f.IssueID == id);


            if (issue == null)
            {
                return NotFound();
            }



            // Then map to the view model
            var issueVM = _crud.Issues.ConvertIssueEntityToVM(issue);


            return View(issueVM);
        }




        /// <summary>
        /// This method is used to cast a vote on a issue post.
        /// </summary>
        /// <param name="model"></param>
        [HttpPost]
        [Route("/issue/vote")]
        public async Task<IActionResult> IssueVote(UserVote_Issue_CreateVM model)
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

                // Check if the user has already voted on this issue
                var existingVote = await _context.IssueVotes
                    .OfType<IssueVote>()
                    .FirstOrDefaultAsync(v => v.UserID == user.Id && v.IssueID == model.IssueID);

                if (existingVote != null)
                {
                    // Update existing vote
                    existingVote.VoteValue = (int)model.VoteValue;
                    existingVote.ModifiedAt = DateTime.UtcNow;
                    _context.IssueVotes.Update(existingVote);
                }
                else
                {
                    // Create new vote
                    IssueVote vote = new IssueVote
                    {
                        User = user,
                        UserID = user.Id,
                        IssueID = model.IssueID,
                        VoteValue = (int)model.VoteValue,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.IssueVotes.Add(vote);
                }

                await _context.SaveChangesAsync();

                // get updated stats
                double average = await _context.IssueVotes
                     .OfType<IssueVote>()
                     .Where(v => v.IssueID == model.IssueID)
                     .AverageAsync(v => v.VoteValue);
                int count = await _context.IssueVotes
                     .OfType<IssueVote>()
                     .Where(v => v.IssueID == model.IssueID)
                     .CountAsync();

                return Json(new
                {
                    success = true,
                    message = "Vote saved successfully",
                    average,
                    count
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// This method is used to get the vote dial for issues only.
        /// </summary>
        /// <param name="issueId">The ID of the issue</param>
        /// <returns>HTML partial view of vote dial</returns>
        [AllowAnonymous]
        [Route("/issue/GetVoteDial")]
        public async Task<IActionResult> GetVoteDial(Guid? issueId = null)
        {
            if (!issueId.HasValue)
            {
                return BadRequest(new { message = "Issue ID must be provided" });
            }

            int? userVote = null;

            // Check if the issue exists
            bool contentExists = _context.Issues.Any(i => i.IssueID == issueId);
            if (!contentExists)
            {
                return NotFound(new { message = "Issue not found" });
            }

            var user = await _userManager.GetUserAsync(User);

            if (user != null)
            {
                var existingVote = await _context.IssueVotes
                    .OfType<IssueVote>()
                    .FirstOrDefaultAsync(v => v.UserID == user.Id && v.IssueID == issueId);

                if (existingVote != null)
                {
                    userVote = existingVote.VoteValue;
                }
            }

            // Retrieve vote data for the issue
            var votes = await _context.IssueVotes
                .OfType<IssueVote>()
                .Where(v => v.IssueID == issueId)
                .ToListAsync();

            double averageVote = votes.Any() ? votes.Average(v => v.VoteValue) : 0;
            int totalVotes = votes.Count;

            var model = new UserVote_Generic_ReadVM
            {
                ContentType = "Issue",
                ContentID = issueId.Value,
                AverageVote = averageVote,
                TotalVotes = totalVotes,
                UserVote = userVote
            };

            return PartialView("~/Views/Shared/Components/_voteDial.cshtml", model);
        }

    }
}