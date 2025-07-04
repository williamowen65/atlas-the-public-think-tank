using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using atlas_the_public_think_tank.Services;
using atlas_the_public_think_tank.Models;

/// This TestController contains routes that are only present 
/// when running the test project.
/// 
/// These routes are not present in production.
/// 
/// This allows for unit test to run in the end to end test environment. 
namespace CloudTests.TestControllers
{
    public class TestController : Controller
    {

        private readonly CRUD _crudService;
        public TestController(CRUD _crud) {
            _crudService = _crud;
        }

        [HttpGet]
        [Route("test-get-issues-paginated")]
        public async Task<IActionResult> Test()
        {
           PaginatedIssuesResponse res =  await _crudService.Issues.GetIssuesPagedAsync(1);

            return Ok(res);
        }
    }
}
