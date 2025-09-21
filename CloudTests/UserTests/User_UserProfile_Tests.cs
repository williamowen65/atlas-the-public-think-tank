
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


namespace CloudTests.UserTests
{
    [TestClass]
    public class User_UserProfile_Tests
    {
        private static HttpClient _client;
        private static ApplicationDbContext _db;
        private static TestEnvironment _env;

        [ClassInitialize]
        public static async Task ClassInit(TestContext context)
        {
            // Arrange environment
            _env = new TestEnvironment();
            _db = _env._db;
            _client = _env._client;

            // Create and login user
            string email = Users.TestUser1.Email!;
            string password = Users.TestUser1Password;
            AppUser testUser = Users.CreateTestUser(_db, Users.TestUser1, password);

            bool loginSuccess = await Users.LoginUserViaEndpoint(_env, email, password);
            Assert.IsTrue(loginSuccess, "Login should be successful");
        }

        [ClassCleanup]
        public static async Task ClassCleanup()
        {
            await TestingUtilityMethods.deleteDatabase(_client, _db);
        }

        [TestMethod]
        public async Task UserProfile_GetsUpdatedWhenIssueIs_Created()
        {
            // Content counts
            // Content in the feed
            Assert.Fail();
        }

        [TestMethod]
        public async Task UserProfile_GetsUpdatedWhenIssue_Updated()
        {
            // Content updates are reflects
            Assert.Fail();
        }

        [TestMethod]
        public async Task UserProfile_GetsUpdatedWhenSolutionIs_Created()
        {
            // Content counts
            // Content in the feed
            Assert.Fail();
        }

        [TestMethod]
        public async Task UserProfile_GetsUpdatedWhenSolution_Updated()
        {
            // Content updates are reflects
            Assert.Fail();
        }


        [TestMethod]
        public async Task UserProfile_FilterLogic_CanUpdate_UserIssueFeed()
        {
            // Updating vote range should filter out content
            Assert.Fail();
        }

        [TestMethod]
        public async Task UserProfile_FilterLogic_CanUpdate_UserSolutionFeed()
        {
            // Updating vote range should filter out content
            Assert.Fail();
        }


        [TestMethod]
        public async Task UserProfile_UserHistory_IsUpdated_WithEveryAction()
        {
            Assert.Fail();
        }



    }
}