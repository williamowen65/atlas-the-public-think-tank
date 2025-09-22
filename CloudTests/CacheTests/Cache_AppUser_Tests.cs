using atlas_the_public_think_tank.Data.CRUD;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;
using atlas_the_public_think_tank.Data.DatabaseEntities.History;
using atlas_the_public_think_tank.Data.DatabaseEntities.Users;
using atlas_the_public_think_tank.Data.DbContext;
using atlas_the_public_think_tank.Data.RepositoryPattern.Cache.Helpers;
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
using static CloudTests.CacheTests.Cache_Issue_Tests;

namespace CloudTests.CacheTests
{

    [TestClass]
    public class Cache_AppUser_Tests
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

            // Create and login user
            AppUser testUser = Users.CreateTestUser(_db, TestAppUser, TestAppUserPassword);

            bool loginSuccess = await Users.LoginUserViaEndpoint(_env, TestAppUser.Email!, TestAppUserPassword);
        }


        [TestCleanup]
        public async Task TestCleanup()
        {
            await TestingUtilityMethods.deleteDatabase(_client, _db);
        }

        public AppUser TestAppUser { get; } = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = "appusertest@example.com",
            NormalizedUserName = "APPUSERTEST@EXAMPLE.COM",
            Email = "appusertest@example.com",
            NormalizedEmail = "APPUSERTEST@EXAMPLE.COM",
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString(),
            ConcurrencyStamp = Guid.NewGuid().ToString(),
            LockoutEnabled = false
        };
        public string TestAppUserPassword = "Password1234!";


        public class CacheUserHistoryEntry {
            public string Key { get; set; }
            public string Type { get; set; }
            public List<UserHistory> Value { get; set; }
        }


        [TestMethod]
        public async Task CacheTesting_AppUser_UserHistory()
        {

            var userId = TestAppUser.Id.ToString();
            string profileUrl = $"/user-profile?userId={userId}";
            // fetch user profile to populate cache
            await _env.fetchHTML(profileUrl);    
            string cacheKey = $"{CacheKeyPrefix.UserHistory}:{userId}";
            string cacheUrl = $"/api/cache-log/entry?key={cacheKey}";
            var cacheEntry = await _env.fetchJson<CacheUserHistoryEntry>(cacheUrl);
            Assert.IsTrue(cacheEntry.Value[0].Action == "Account Created", "Cache should have an initial value");

            // Create issue
            var testHelper = new TestingCRUDHelper(_env);
            var (jsonDoc, issueId, title, content, scope) = await testHelper.CreateTestIssue();
            await _env.fetchHTML(profileUrl);

            var cacheEntry2 = await _env.fetchJson<CacheUserHistoryEntry>(cacheUrl);
            Assert.IsTrue(cacheEntry2.Value.Count() == 2, "Cache should have been updated with another entry");

        }




    }
}
