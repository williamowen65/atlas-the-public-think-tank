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
            AppUser testUser = Users.CreateTestUser1(_db);
            string email = "testuser@example.com";
            string password = "Password123!";

            bool loginSuccess = await Users.LoginUserViaEndpoint(_env, email, password);
            Assert.IsTrue(loginSuccess, "Login should be successful");

        }


        [TestCleanup]
        public async Task TestCleanup()
        {
            await TestingUtilityMethods.deleteDatabase(_client, _db);
        }



    }
}
