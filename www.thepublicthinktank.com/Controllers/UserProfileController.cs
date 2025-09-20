using atlas_the_public_think_tank.Data.CRUD;
using atlas_the_public_think_tank.Data.DatabaseEntities.Users;
using atlas_the_public_think_tank.Data.RepositoryPattern.Repository.Helpers;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Issue;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Solution;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.User;
using atlas_the_public_think_tank.Models.ViewModel.PageVM;
using atlas_the_public_think_tank.Models.ViewModel.UI_VM;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace atlas_the_public_think_tank.Controllers
{
    public class UserProfileController : Controller
    {

        [Route("/user-profile")]
        public async Task<IActionResult> UserProfile([FromQuery][BindRequired] Guid userId)
        {

       
            if (userId == Guid.Empty)
            {
                return BadRequest("User ID is required.");
            }

            ContentFilter filter = new ContentFilter();
            if (Request.Cookies.TryGetValue("contentFilter", out string? cookieValue) && cookieValue != null)
            {
                filter = ContentFilter.FromJson(cookieValue);
            }


            List<Issue_ReadVM> usersIssues = (await Read.PaginatedUsersIssues(userId, filter))!;
            ContentCount_VM usersIssueCounts = (await Read.UsersIssueCounts(userId, filter))!;
            List<Solution_ReadVM> usersSolutions = (await Read.PaginatedUsersSolutions(userId, filter))!;
            ContentCount_VM usersSolutionCounts = (await Read.UsersSolutionCounts(userId, filter))!;

            UserProfile_PageVM userProfile_PageVM = new UserProfile_PageVM()
            {
                appUserVM = (await Read.AppUser(userId))!,
                userHistory = (await Read.UserHistory(userId))!,
                usersIssues = usersIssues,
                userStats = new UserStats() { 
                    Issues = usersIssueCounts.AbsoluteCount,
                    Solutions = usersSolutionCounts.AbsoluteCount
                }
            };

            return View(userProfile_PageVM);
        }

    }
}
