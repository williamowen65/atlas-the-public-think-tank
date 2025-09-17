using atlas_the_public_think_tank.Data;
using atlas_the_public_think_tank.Data.CRUD;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Issue;
using atlas_the_public_think_tank.Data.DatabaseEntities.Users;
using atlas_the_public_think_tank.Data.RepositoryPattern.IRepository;
using atlas_the_public_think_tank.Data.RepositoryPattern.Repository.Helpers;
using atlas_the_public_think_tank.Data.SeedData.SeedIssues;
using atlas_the_public_think_tank.Data.SeedData.SeedIssues.Data;
using atlas_the_public_think_tank.Data.SeedData.SeedSolutions.Data;
using atlas_the_public_think_tank.Models;
using atlas_the_public_think_tank.Models.ViewModel.AjaxVM;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.ContentItem_Common;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Issue;
using atlas_the_public_think_tank.Models.ViewModel.UI_VM;
using CloudTests.TestingSetup;
using CloudTests.TestingSetup.TestingData;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static CloudTests.CacheTests.Cache_Issue_Tests;

namespace CloudTests.CacheTests
{

    [TestClass]
    public class Cache_HomePage_Tests
    {
        private static HttpClient _client;
        private static ApplicationDbContext _db;
        private static TestEnvironment _env;
        private TestingCRUDHelper _testingCRUDHelper;

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
            _testingCRUDHelper = new TestingCRUDHelper(_env);


            // Create and login user
            string email = Users.TestUser1.Email!;
            string password = Users.TestUser1Password;
            AppUser testUser = Users.CreateTestUser(_db, Users.TestUser1, password);

            bool loginSuccess = await Users.LoginUserViaEndpoint(_env, email, password);
            Assert.IsTrue(loginSuccess, "Login should be successful");

        }


        [TestCleanup]
        public async Task TestCleanup()
        {
            await TestingUtilityMethods.deleteDatabase(_client, _db);
        }

        [TestMethod]
        public async Task CacheTestingHomePage_AddingNew_Issues_Solutions_PagedContentShouldBeUpToDate()
        {
            var (jsonDoc, issueId1, title, content, scope) = await _testingCRUDHelper.CreateTestIssue();

            // Common filter
            ContentFilter filter = new ContentFilter();
            string filterCacheString = filter.ToCacheString();
            int pageNumber = 1;
            var cacheKey = $"main-content-feed-ids:{filterCacheString}:page({pageNumber})";
            string encodedCacheKey = Uri.EscapeDataString(cacheKey);
            string url = $"/api/cache-log/entry?key={encodedCacheKey}";

            // Visit the home page to populate the cache
            await _env.fetchHTML("/");

            // Fetch and eval
            var cacheEntry1 = await _env.fetchJson<CacheEntryIdsHomePageDTO>(url);
            Assert.IsTrue(cacheEntry1.Value.Any(entry => entry.Id.ToString() == issueId1));

            // Test issue 2

            var (jsonDoc2, issueId2, title2, content2, scope2) = await _testingCRUDHelper.CreateTestIssue();
            
            // Visit the home page to populate the cache
            await _env.fetchHTML("/");

            // Fetch and eval
            var cacheEntry2 = await _env.fetchJson<CacheEntryIdsHomePageDTO>(url);
            Assert.IsTrue(cacheEntry2.Value.Any(entry => entry.Id.ToString() == issueId1));
            Assert.IsTrue(cacheEntry2.Value.Any(entry => entry.Id.ToString() == issueId2));
        }

    }
}
