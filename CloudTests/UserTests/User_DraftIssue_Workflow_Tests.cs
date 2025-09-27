
using AngleSharp.Dom;
using atlas_the_public_think_tank.Data.CRUD;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;
using atlas_the_public_think_tank.Data.DatabaseEntities.Users;
using atlas_the_public_think_tank.Data.DbContext;
using atlas_the_public_think_tank.Data.RepositoryPattern.Repository.Helpers;
using atlas_the_public_think_tank.Data.SeedData.SeedIssues;
using atlas_the_public_think_tank.Data.SeedData.SeedSolutions;
using atlas_the_public_think_tank.Models.Enums;
using atlas_the_public_think_tank.Models.ViewModel;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.ContentItem_Common;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Issue;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Solution;
using CloudTests.TestingSetup;
using CloudTests.TestingSetup.TestingData;
using System;
using System.Text.Json;


namespace CloudTests.UserTests
{
    [TestClass]
    public class User_DraftIssue_Workflow_Tests
    {
        #region Test setup

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
            AppUser testUser = Users.CreateTestUser(_db, TestDraftUser, TestDraftPassword);

            bool loginSuccess = await Users.LoginUserViaEndpoint(_env, TestDraftUser.Email!, TestDraftPassword);
        }

        [TestCleanup]
        public async Task TestCleanup()
        {
            await TestingUtilityMethods.deleteDatabase(_client, _db);
        }

        #endregion

        #region testUsers

        public AppUser TestDraftUser { get; } = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = "draft@example.com",
            NormalizedUserName = "DRAFT@EXAMPLE.COM",
            Email = "draft@example.com",
            NormalizedEmail = "DRAFT@EXAMPLE.COM",
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString(),
            ConcurrencyStamp = Guid.NewGuid().ToString(),
            LockoutEnabled = false
        };
        public string TestDraftPassword = "Password1234!";
        public AppUser TestRandomUser { get; } = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = "random@example.com",
            NormalizedUserName = "RANDOM@EXAMPLE.COM",
            Email = "random@example.com",
            NormalizedEmail = "RANDOM@EXAMPLE.COM",
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString(),
            ConcurrencyStamp = Guid.NewGuid().ToString(),
            LockoutEnabled = false
        };
        public string TestRandomPassword = "Password1234!";

        #endregion


        [TestMethod]
        public async Task User_CanCreateDraftIssue_AndThatDraftIsNotInMainFeed()
        {
            var (jsonDoc, parentIssueId, title, content, scope) = await _testingCRUDHelper.CreateTestIssue(ContentStatus.Draft);
            ContentItems_Paginated_ReadVM paginatedResponse = await Read.PaginatedMainContentFeed(new ContentFilter());
            Assert.IsTrue(paginatedResponse.TotalCount == 0, "There should be no main content items");

        }

        [TestMethod]
        public async Task User_CanCreateDraftIssue_ViewTheDraftOnDraftsPage()
        {
            var (jsonDoc, parentIssueId, title, content, scope) = await _testingCRUDHelper.CreateTestIssue(ContentStatus.Draft);
            var draftsPage = await _env.fetchHTML("/drafts");
            var draftContent = draftsPage.QuerySelector($".card[id='{parentIssueId}']");
            Assert.IsNotNull(draftContent, "Draft content should exist on the draft page for this user");
        }

        [TestMethod]
        public async Task User_CanCreateDraftIssue_AnotherUserCanTryToViewItViaURL_ButWillFailBecause_NotAuthorizedToViewAnotherPersonsDraft()
        {
            var (jsonDoc, parentIssueId, title, content, scope) = await _testingCRUDHelper.CreateTestIssue(ContentStatus.Draft);

            // Log a random user in
            AppUser testUser = Users.CreateTestUser(_db, TestRandomUser, TestRandomPassword);
            bool loginSuccess = await Users.LoginUserViaEndpoint(_env, TestRandomUser.Email!, TestRandomPassword);

            try
            {
                var issuesPage = await _env.fetchHTML($"/issue/{parentIssueId}");
            }
            catch (Exception ex) {
                Assert.IsTrue(ex.Message.Contains("401 (Unauthorized)"), "The random user shouldn't be able to view another persons draft");
            }
        }
        
        [TestMethod]
        public async Task User_CanCreateDraftIssue_AndPublishIt_RelatedChangesShouldOccur()
        {
            // Create draft issue
            var (jsonDoc, parentIssueId, title, content, scope) = await _testingCRUDHelper.CreateTestIssue(ContentStatus.Draft);
            ContentItems_Paginated_ReadVM paginatedResponse = await Read.PaginatedMainContentFeed(new ContentFilter());
            Assert.IsTrue(paginatedResponse.TotalCount == 0, "There should be no main content items");

            // Content Counts in drafts should be accurate
            var draftsPage1 = await _env.fetchHTML("/drafts");
            var draftCountEl1 = draftsPage1.QuerySelector(".issue-draft-count");
            int draftCount1 = int.Parse(draftCountEl1!.TextContent);
            Assert.IsTrue(draftCount1 == 1, "Draft Count should be 1");

            // User profile issues check
            var userProfilePage1 = await _env.fetchHTML($"/user-profile?userId={TestDraftUser.Id}");
            var userProfileIssueCountEl1 = userProfilePage1.QuerySelector("#tab-issue .content-count");
            int userIssueCount1 = int.Parse(userProfileIssueCountEl1!.TextContent);
            Assert.IsTrue(userIssueCount1 == 0, "User issue count should be 0");

            // publish issue
            await _testingCRUDHelper.EditTestIssue(parentIssueId, scope.ScopeID, ContentStatus.Published);
            
            
            // Content can be viewed in main content
            ContentItems_Paginated_ReadVM paginatedResponse2 = await Read.PaginatedMainContentFeed(new ContentFilter());
            Assert.IsTrue(paginatedResponse2.TotalCount == 1, "There should be one main content items");

            // Content Counts in drafts should be accurate
            var draftsPage2 = await _env.fetchHTML("/drafts");
            var draftCountEl2 = draftsPage2.QuerySelector(".issue-draft-count");
            int draftCount2 = int.Parse(draftCountEl2!.TextContent);
            Assert.IsTrue(draftCount2 == 0, "Draft Count should be 0");

            // User profile issues check
            var userProfilePage2 = await _env.fetchHTML($"/user-profile?userId={TestDraftUser.Id}");
            var userProfileIssueCountEl2 = userProfilePage2.QuerySelector("#tab-issue .content-count");
            int userIssueCount2 = int.Parse(userProfileIssueCountEl2!.TextContent);
            Assert.IsTrue(userIssueCount2 == 1, "User issue count should be 1");

        }



    }
}