using atlas_the_public_think_tank.Data;
using atlas_the_public_think_tank.Data.CRUD;
using atlas_the_public_think_tank.Data.DatabaseEntities.Users;
using atlas_the_public_think_tank.Data.RepositoryPattern.Repository.Helpers;
using atlas_the_public_think_tank.Data.SeedData.SeedIssues;
using atlas_the_public_think_tank.Data.SeedData.SeedIssues.Data;
using atlas_the_public_think_tank.Data.SeedData.SeedSolutions.Data;
using atlas_the_public_think_tank.Models;
using atlas_the_public_think_tank.Models.ViewModel.AjaxVM;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.ContentItem_Common;
using CloudTests.TestingSetup;
using CloudTests.TestingSetup.TestingData;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CloudTests.CacheTests
{

    [TestClass]
    public class CacheTestEnvironmentTests
    {
        private static HttpClient _client;
        private static ApplicationDbContext _db;
        private static TestEnvironment _env;


        [TestInitialize]
        public async Task Setup()
        {
            // Use the utility class to configure the test environment
            string appSettings = @"
            {
                ""ApplySeedData"": false,
                ""Caching"": {
                    ""enabled"": true
                }
            }";
            _env = new TestEnvironment(appSettings);
            _db = _env._db;
            _client = _env._client;
        }


        [TestCleanup]
        public async Task TestCleanup()
        {
            await TestingUtilityMethods.deleteDatabase(_client, _db);
        }

        [TestMethod]
        public async Task CacheTesting_Environment_ShouldHaveNoSeedData() 
        {
            // Read main page content and count should be 0
            ContentItems_Paginated_ReadVM paginatedResponse = await Read.PaginatedMainContentFeed(new ContentFilter());
            Assert.IsTrue(paginatedResponse.TotalCount == 0, "Total Count should be 0");
        }

        [DataTestMethod]
        [DataRow("keys")]
        [DataRow("entries")]
        public async Task CacheTesting_Environment_CacheLog_Keys_ShouldHaveNoData(string path)
        {
            var cacheLogKeys = await _env.fetchJson<List<object>>($"/api/cache-log/{path}");
            Assert.IsTrue(cacheLogKeys.Count() == 0, $"cache log {path} should be empty");
        }
    }
}
