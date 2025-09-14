using atlas_the_public_think_tank.Data;
using atlas_the_public_think_tank.Data.CRUD;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;
using atlas_the_public_think_tank.Data.DatabaseEntities.Users;
using atlas_the_public_think_tank.Data.RepositoryPattern.IRepository;
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
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;


namespace CloudTests.CacheTests
{

    [TestClass]
    public class Cache_Solution_Tests
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


        public class CacheSolutionEntry
        {
            public string Key { get; set; }
            public string Type { get; set; }
            public SolutionRepositoryViewModel Value { get; set; }
        }

        [TestMethod]
        public async Task CacheTesting_AddingNewSolution_CreatesCacheKeyForSolution() 
        {

            // Create and login user
            AppUser testUser = Users.CreateTestUser1(_db);
            string email = "testuser@example.com";
            string password = "Password123!";

            bool loginSuccess = await Users.LoginUserViaEndpoint(_env, email, password);
            Assert.IsTrue(loginSuccess, "Login should be successful");

            string content = "This is just an example issue content";

            var (jsonDoc1, title1, content1) = await TestingCRUDHelpers.CreateIssue(_env,
              "This is just an example issue title (content creation)",
              content,
              new Scope()
              {
                  Scales = { Scale.Global, Scale.National }
              });

            var rootElement1 = jsonDoc1.RootElement;
            string parentIssueId = rootElement1.GetProperty("contentId").ToString();

            var (jsonDoc2, title2, content2) = await TestingCRUDHelpers.CreateSolution(_env,
            "This is just an example issue title (content creation)",
            content,
            new Scope()
            {
                Scales = { Scale.Global, Scale.National }
            },
            parentIssueId);


            var rootElement2 = jsonDoc2.RootElement;
            string solutionId = rootElement2.GetProperty("contentId").ToString();


            string url = $"/api/cache-log/keys";
            string expectedCacheKey = $"solution:{solutionId}";
            var cacheLog = await _env.fetchJson<List<string>>(url);
            Assert.IsTrue(cacheLog.Contains(expectedCacheKey), $"cache log should contain key {expectedCacheKey}");
        }


        [TestMethod]
        public async Task CacheTesting_AddingNewSolution_CreatesCacheEntryForSolution()
        {

            // Create and login user
            AppUser testUser = Users.CreateTestUser1(_db);
            string email = "testuser@example.com";
            string password = "Password123!";

            bool loginSuccess = await Users.LoginUserViaEndpoint(_env, email, password);
            Assert.IsTrue(loginSuccess, "Login should be successful");

            string issueContent = "This is just an example issue content";
            string solutionContent = "This is just an example solution content";

            var (jsonDoc1, title1, content1) = await TestingCRUDHelpers.CreateIssue(_env,
              "This is just an example issue title (content creation)",
              issueContent,
              new Scope()
              {
                  Scales = { Scale.Global, Scale.National }
              });

            var rootElement1 = jsonDoc1.RootElement;
            string parentIssueId = rootElement1.GetProperty("contentId").ToString();

            var (jsonDoc2, title2, content2) = await TestingCRUDHelpers.CreateSolution(_env,
            "This is just an example issue title (content creation)",
            solutionContent,
            new Scope()
            {
                Scales = { Scale.Global, Scale.National }
            },
            parentIssueId);


            var rootElement2 = jsonDoc2.RootElement;
            string solutionId = rootElement2.GetProperty("contentId").ToString();

            string url = $"/api/cache-log/entry?key=solution:{solutionId}";
            var cacheEntry = await _env.fetchJson<CacheSolutionEntry>(url);
            Assert.IsTrue(cacheEntry.Value.Content == solutionContent);
        }

        [TestMethod]
        public async Task CacheTesting_UpdatingSolution_CreatesAndOverwritesCacheEntryForSolution()
        {

            // Create and login user
            AppUser testUser = Users.CreateTestUser1(_db);
            string email = "testuser@example.com";
            string password = "Password123!";

            bool loginSuccess = await Users.LoginUserViaEndpoint(_env, email, password);
            Assert.IsTrue(loginSuccess, "Login should be successful");

            string issueContent = "This is just an example issue content";
            string solutionContent = "This is just an example solution content";
            string solutionContentUpdate = "This is just an example solution content with an update";

            var (jsonDoc1, title1, content1) = await TestingCRUDHelpers.CreateIssue(_env,
              "This is just an example issue title (content creation)",
              issueContent,
              new Scope()
              {
                  Scales = { Scale.Global, Scale.National }
              });

            var rootElement1 = jsonDoc1.RootElement;
            string parentIssueId = rootElement1.GetProperty("contentId").ToString();

            var (jsonDoc2, title2, content2) = await TestingCRUDHelpers.CreateSolution(_env,
            "This is just an example solution title",
            solutionContent,
            new Scope()
            {
                Scales = { Scale.Global, Scale.National }
            },
            parentIssueId);


            var rootElement2 = jsonDoc2.RootElement;
            string solutionId = rootElement2.GetProperty("contentId").ToString();

            var solutionDoc = await _env.TextHtmlToDocument(rootElement2.GetProperty("content").ToString());
            var scopeRibbonEl = solutionDoc.QuerySelector(".scope-ribbon");
            string? scopeId = scopeRibbonEl!.GetAttribute("data-scope-id");


            var (_jsonDoc3, _title3, _content3) = await TestingCRUDHelpers.EditSolution(_env,
             "This is just an example solution title",
             solutionContentUpdate,
             solutionId,
             parentIssueId,
             new Scope()
             {
                 ScopeID = new Guid(scopeId!),
                 Scales = { Scale.Global, Scale.National }
             });


            string url = $"/api/cache-log/entry?key=solution:{solutionId}";
            var cacheEntry = await _env.fetchJson<CacheSolutionEntry>(url);
            Assert.IsTrue(cacheEntry.Value.Content == solutionContentUpdate); ;
        }




    }
}
