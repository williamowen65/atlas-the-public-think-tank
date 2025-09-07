using atlas_the_public_think_tank.Data.CRUD;
using atlas_the_public_think_tank.Data.RepositoryPattern.Repository.Helpers;
//using atlas_the_public_think_tank.Services;
using atlas_the_public_think_tank.Models.ViewModel;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.ContentItem_Common;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// This TestController contains routes that are only present 
/// when running the test project.
/// 
/// These routes are not present in production.
namespace CloudTests.TestingSetup
{
    public class TestController : Controller
    {

        public TestController()
        {
        }

        [HttpGet("/api/debug/cookies")]
        public IActionResult DebugCookies()
        {
            var cookies = Request.Cookies.ToDictionary(c => c.Key, c => c.Value);
            return Json(cookies);
        }
    }
}
