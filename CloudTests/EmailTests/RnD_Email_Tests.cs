
using AngleSharp.Dom;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;
using atlas_the_public_think_tank.Data.DatabaseEntities.Users;
using atlas_the_public_think_tank.Data.DbContext;
using atlas_the_public_think_tank.Data.SeedData.SeedIssues;
using atlas_the_public_think_tank.Data.SeedData.SeedSolutions;

using atlas_the_public_think_tank.Models.ViewModel;
using CloudTests.TestingSetup;
using CloudTests.TestingSetup.TestingData;
using System;
using System.Text.Json;


namespace CloudTests.EmailTests
{
    [TestClass]
    public class RnD_Email_Tests
    {
        private HttpClient _client;
        private ApplicationDbContext _db;
        private TestEnvironment _env;


        [TestInitialize]
        public async Task Setup()
        {
            // Arrange environment
            _env = new TestEnvironment();
            _db = _env._db;
            _client = _env._client;

        }

        [TestCleanup]
        public async Task ClassCleanup()
        {
            await TestingUtilityMethods.deleteDatabase(_client, _db);
        }

        class SuccessObject { 
            public bool Success { get; set; }
        }


        [TestMethod]
        public async Task RnD_TestEmailFromGitHubActions()
        {
            // Create and login user
            var (user, password) = Users.GetRandomAppUser();
            AppUser testUser = Users.CreateTestUser_ViaDbDirectly(_db, user, password);
            bool loginSuccess = await Users.LoginUserViaEndpoint(_env, user.Email!, password);
            Assert.IsTrue(loginSuccess, "Login should be successful");

            var document = await _env.fetchJson<SuccessObject>("/github-actions-test-email");

           Assert.IsTrue(document.Success, "expected email to be successfully sent");

            // Should be able to look up the email log in the db

            // Should be able to look at the html markup of the email in email hog
            /// Email can be read via an api... http://localhost:8025/api/v2/messages
            /// but the emails have to base 64 decoded.

        }
    }
}