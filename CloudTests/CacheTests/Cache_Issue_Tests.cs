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
    public class Cache_Issue_Tests
    {
        private static HttpClient _client;
        private static ApplicationDbContext _db;
        private static TestEnvironment _env;


        [TestInitialize]
        public async Task Setup()
        {
            // Use the utility class to configure the test environment
            bool applySeedData = false;
            _env = new TestEnvironment(applySeedData);
            _db = _env._db;
            _client = _env._client;
        }


        [TestCleanup]
        public async Task TestCleanup()
        {
            await TestingUtilityMethods.deleteDatabase(_client, _db);
        }


        public class CacheIssueEntry
        {
            public string Key { get; set; }
            public string Type { get; set; }
            public IssueRepositoryViewModel Value { get; set; }
        }

        [DataTestMethod]
        [DataRow("keys")]
        [DataRow("entries")]
        public async Task CacheTesting_AddingNewIssue_CreatesCacheEntryForIssue(string path) 
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
            string issueId = rootElement1.GetProperty("contentId").ToString();


            string url = $"/api/cache-log/{path}";
            string expectedCacheKey = $"issue:{issueId}";
            if (path == "keys") { 
                var cacheLog = await _env.fetchJson<List<string>>(url);
                Assert.IsTrue(cacheLog.Contains(expectedCacheKey), $"cache log should contain key {expectedCacheKey}");
            }
            if (path == "entries") {
           
                var cacheLog = await _env.fetchJson<List<CacheIssueEntry>>(url);
                var cacheEntryExist = cacheLog.Any(e => e.Key == expectedCacheKey);
                Assert.IsTrue(cacheEntryExist, $"cache log should contain key {expectedCacheKey}");
                var cacheEntry = cacheLog.FirstOrDefault(e => e.Key == expectedCacheKey);

                Assert.IsTrue(cacheEntry.Value.Content == content);
            }
        }


        [TestMethod]
        public async Task CacheTesting_AddingNewIssue_AndUpdatingIt_CreatesAndOverwritesCacheEntryForIssue()
        {

            // Create and login user
            AppUser testUser = Users.CreateTestUser1(_db);
            string email = "testuser@example.com";
            string password = "Password123!";

            bool loginSuccess = await Users.LoginUserViaEndpoint(_env, email, password);
            Assert.IsTrue(loginSuccess, "Login should be successful");

            string content = "This is just an example issue content";
            string contentUpdate = "This is just an example issue content with updated content";

            var (jsonDoc1, _title1, _content1) = await TestingCRUDHelpers.CreateIssue(_env,
              "This is just an example issue title (content creation)",
              content,
              new Scope()
              {
                  Scales = { Scale.Global, Scale.National }
              });

            var rootElement1 = jsonDoc1.RootElement;
            string issueId = rootElement1.GetProperty("contentId").ToString();
            var issueDoc = await _env.TextHtmlToDocument(rootElement1.GetProperty("content").ToString());
            var scopeRibbonEl = issueDoc.QuerySelector(".scope-ribbon");
            string? scopeId = scopeRibbonEl!.GetAttribute("data-scope-id");


            var (_jsonDoc2, _title2, _content2) = await TestingCRUDHelpers.EditIssue(_env,
             "This is just an example issue title (content creation)",
             contentUpdate,
             issueId,
             new Scope()
             {
                 ScopeID = new Guid(scopeId!),
                 Scales = { Scale.Global, Scale.National }
             });



            string url = $"/api/cache-log/entries";
            string expectedCacheKey = $"issue:{issueId}";

            var cacheLog = await _env.fetchJson<List<CacheIssueEntry>>(url);
            var cacheEntryExist = cacheLog.Any(e => e.Key == expectedCacheKey);
            Assert.IsTrue(cacheEntryExist, $"cache log should contain key {expectedCacheKey}");
            var cacheEntry = cacheLog.FirstOrDefault(e => e.Key == expectedCacheKey);

            Assert.IsTrue(cacheEntry.Value.Content == contentUpdate);
        }




    }
}
