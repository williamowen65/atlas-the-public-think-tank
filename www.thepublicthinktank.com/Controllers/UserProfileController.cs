using atlas_the_public_think_tank.Data.CRUD;
using atlas_the_public_think_tank.Data.DatabaseEntities.Users;
using atlas_the_public_think_tank.Data.RepositoryPattern.Repository.Helpers;
using atlas_the_public_think_tank.Models.Enums;
using atlas_the_public_think_tank.Models.ViewModel.AjaxVM;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Issue;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Solution;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.User;
using atlas_the_public_think_tank.Models.ViewModel.CRUD_VM.ContentItem_Common;
using atlas_the_public_think_tank.Models.ViewModel.CRUD_VM.Solution;
using atlas_the_public_think_tank.Models.ViewModel.PageVM;
using atlas_the_public_think_tank.Models.ViewModel.UI_VM;
using atlas_the_public_think_tank.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace atlas_the_public_think_tank.Controllers
{
    public class UserProfileController : Controller
    {

        public readonly UserManager<AppUser> _userManager;
        public readonly Read _read;

        public UserProfileController(UserManager<AppUser> userManager, Read read)
        {
            _userManager = userManager;
            _read = read;
        }

        [Route("/user-profile")]
        public async Task<IActionResult> UserProfile([FromQuery][BindRequired] Guid userId)
        {

            ViewData["FilterPanelMode"] = "UserProfile";


            if (userId == Guid.Empty)
            {
                return BadRequest("User ID is required.");
            }

            ContentFilter filter = new ContentFilter();
            if (Request.Cookies.TryGetValue("contentFilter", out string? cookieValue) && cookieValue != null)
            {
                filter = ContentFilter.FromJson(cookieValue);
            }


            Issues_Paginated_ReadVM usersIssues = (await _read.PaginatedUsersIssues(userId, filter))!;
            Solutions_Paginated_ReadVM usersSolutions = (await _read.PaginatedUsersSolutions(userId, filter))!;

            UserProfile_PageVM userProfile_PageVM = new UserProfile_PageVM()
            {
                appUserVM = (await _read.AppUser(userId))!,
                userHistory = (await _read.UserHistory(userId))!,
                paginatedUserIssues = usersIssues,
                paginatedUserSolutions = usersSolutions,
                userStats = new UserStats() { 
                    Followers = 0,
                    Following = 0,
                    Projects = 0
                }
            };

            return View(userProfile_PageVM);
        }

        [Authorize]
        [Route("/drafts")]
        public async Task<IActionResult> Drafts()
        {
            ViewData["FilterPanelMode"] = "Drafts";

            // The user must be logged in to view this page
            // No url params needed. Will load user content. 

            // Fetch user Drafts
            ContentFilter filter = new ContentFilter();
            if (Request.Cookies.TryGetValue("contentFilter", out string? cookieValue) && cookieValue != null)
            {
                filter = ContentFilter.FromJson(cookieValue);
            }

            // get userId from User
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out Guid userGuid))
            {
                return Unauthorized();
            }

            var userDraft_pageVM = await _read.PaginatedUserDrafts(userGuid, filter);

            return View(userDraft_pageVM);
        }


        /// <summary>
        /// This method is used to return paginated issue posts.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        [Route("/user-profile/getPaginatedUserIssues/{userId}")]
        public async Task<IActionResult> GetPaginatedUserIssues(Guid userId, int currentPage = 1)
        {
            ContentFilter filter = new ContentFilter();
            if (Request.Cookies.TryGetValue("contentFilter", out string? cookieValue) && cookieValue != null)
            {
                filter = ContentFilter.FromJson(cookieValue);
            }


            Issues_Paginated_ReadVM paginatedIssues = await _read.PaginatedUsersIssues(userId, filter, currentPage);

            string partialViewHtml = await ControllerExtensions.RenderViewToStringAsync(this, "~/Views/Issue/_issue-cards.cshtml", paginatedIssues.Issues);

            var response = new ContentItems_Paginated_AjaxVM
            {
                html = partialViewHtml,
                pagination = new PaginationStats_VM
                {
                    TotalCount = paginatedIssues.ContentCount.FilteredCount,
                    PageSize = paginatedIssues.PageSize,
                    CurrentPage = paginatedIssues.CurrentPage,
                    TotalPages = (int)Math.Ceiling(paginatedIssues.ContentCount.FilteredCount / (double)paginatedIssues.PageSize)
                }
            };

            return Json(response);
        }

        /// <summary>
        /// This method is used to return paginated issue posts.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        [Route("/user-profile/getPaginatedUserSolutions/{userId}")]
        public async Task<IActionResult> GetPaginatedUserSolutions(Guid userId, int currentPage = 1)
        {
            ContentFilter filter = new ContentFilter();
            if (Request.Cookies.TryGetValue("contentFilter", out string? cookieValue) && cookieValue != null)
            {
                filter = ContentFilter.FromJson(cookieValue);
            }


            Solutions_Paginated_ReadVM paginatedSolutions = await _read.PaginatedUsersSolutions(userId, filter, currentPage);

            string partialViewHtml = await ControllerExtensions.RenderViewToStringAsync(this, "~/Views/Solution/_solution-cards.cshtml", paginatedSolutions.Solutions);

            var response = new ContentItems_Paginated_AjaxVM
            {
                html = partialViewHtml,
                pagination = new PaginationStats_VM
                {
                    TotalCount = paginatedSolutions.ContentCount.FilteredCount,
                    PageSize = paginatedSolutions.PageSize,
                    CurrentPage = paginatedSolutions.CurrentPage,
                    TotalPages = (int)Math.Ceiling(paginatedSolutions.ContentCount.FilteredCount / (double)paginatedSolutions.PageSize)
                }
            };

            return Json(response);
        }



    }
}
