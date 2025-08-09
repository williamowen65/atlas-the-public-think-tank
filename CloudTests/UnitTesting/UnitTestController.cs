using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using atlas_the_public_think_tank.Services;
using atlas_the_public_think_tank.Models.ViewModel;
using atlas_the_public_think_tank.Data.CRUD;
using atlas_the_public_think_tank.Data.RepositoryPattern.Repository.Helpers;

/// This TestController contains routes that are only present 
/// when running the test project.
/// 
/// These routes are not present in production.
/// 
/// This allows for unit test to run in the end to end test environment. 
namespace CloudTests.UnitTesting
{
    public class UnitTestController : Controller
    {

        public UnitTestController()
        {
        }

        [HttpGet]
        [Route("test-ReadContentItems")]
        public async Task<IActionResult> Test()
        {

            PaginatedContentItemsResponse res = await Read.ContentItems(new ContentFilter());

            return Ok(res);
        }

        [HttpGet("/api/debug/cookies")]
        public IActionResult DebugCookies()
        {
            var cookies = Request.Cookies.ToDictionary(c => c.Key, c => c.Value);
            return Json(cookies);
        }
    }
}
