
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
    public class User_DraftSolution_Workflow_Tests
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
            UserName = "draftSolution@example.com",
            NormalizedUserName = "DRAFTSOLUTION@EXAMPLE.COM",
            Email = "draftSolution@example.com",
            NormalizedEmail = "DRAFTSOLUTION@EXAMPLE.COM",
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
        public async Task User_CanCreateDraftSolution_AndThatDraftIsNotInMainFeed()
        {
            var (jsonDoc, parentIssueId, title, content, scope) = await _testingCRUDHelper.CreateTestIssue(ContentStatus.Draft);
            var (_jsonDoc, solutionId, _title, _content, _scope) = await _testingCRUDHelper.CreateTestSolution(parentIssueId, ContentStatus.Draft);
            ContentFilter contentFilter = new ContentFilter()
            {
                ContentType = "solution"
            };

            ContentItems_Paginated_ReadVM paginatedResponse = await Read.PaginatedMainContentFeed(contentFilter);
            Assert.IsTrue(paginatedResponse.TotalCount == 0, "There should be no main content items");
        }

        [TestMethod]
        public async Task User_CanCreateDraftSolution_ViewTheDraftOnDraftsPage()
        {
            var (jsonDoc, parentIssueId, title, content, scope) = await _testingCRUDHelper.CreateTestIssue(ContentStatus.Draft);
            var (_jsonDoc, solutionId, _title, _content, _scope) = await _testingCRUDHelper.CreateTestSolution(parentIssueId, ContentStatus.Draft);
            var draftsPage = await _env.fetchHTML("/drafts");
            var draftContent = draftsPage.QuerySelector($".card[id='{solutionId}']");
            Assert.IsNotNull(draftContent, "Draft content should exist on the draft page for this user");
        }

        [TestMethod]
        public async Task User_CanCreateDraftSolution_AnotherUserCanTryToViewItViaURL_ButWillFailBecause_NotAuthorizedToViewAnotherPersonsDraft()
        {
            var (jsonDoc, parentIssueId, title, content, scope) = await _testingCRUDHelper.CreateTestIssue(ContentStatus.Draft);
            var (_jsonDoc, solutionId, _title, _content, _scope) = await _testingCRUDHelper.CreateTestSolution(parentIssueId, ContentStatus.Draft);

            // Log a random user in
            AppUser testUser = Users.CreateTestUser(_db, TestRandomUser, TestRandomPassword);
            bool loginSuccess = await Users.LoginUserViaEndpoint(_env, TestRandomUser.Email!, TestRandomPassword);

            await Assert.ThrowsExceptionAsync<System.Net.Http.HttpRequestException>(async () =>
            {
                var solutionsPage = await _env.fetchHTML($"/solution/{solutionId}");
            });

        }


        [TestMethod]
        public async Task User_CannotPublish_Solution_WhichHasAParentIssue_InDraftMode()
        {
            // Create a root issue as a draft
            var (jsonDoc, parentIssueId, title, content, scope) = await _testingCRUDHelper.CreateTestIssue(ContentStatus.Draft);

            // Create a proper solution as a draft
            var (_jsonDoc, solutionId, _title, _content, solutionScope) = await _testingCRUDHelper.CreateTestSolution(parentIssueId, ContentStatus.Draft);

            // Create an invalid solution as published
            await Assert.ThrowsExceptionAsync<Microsoft.EntityFrameworkCore.DbUpdateException>(async () =>
            {
                await _testingCRUDHelper.CreateTestSolution(parentIssueId, ContentStatus.Published);
            }, "Directly trying to create a solution as published shouldn't work when parent issue is a draft");

            // The UI should have the publish button disabled.
            var editSolutionDocTemp = await _env.fetchJson<ContentCreationResponse_JsonVM>($"/edit-solution?solutionId={solutionId}");
            var editSolutionDoc = await _env.TextHtmlToDocument(editSolutionDocTemp.Content);
            var awaitingParentPublishBtn = editSolutionDoc.QuerySelector(".awaiting-parent-publish");
            Assert.IsNotNull(awaitingParentPublishBtn, "awaiting-parent-publish button should exist");
            var publishButton = editSolutionDoc.QuerySelector(".publish-issue");
            Assert.IsNull(publishButton, "publish button should not exist");

            // If the user directly pings the /edit-issue endpoint trying to publish it should still not work
            var ex = await Assert.ThrowsExceptionAsync<Exception>(async () =>
            {
                await _testingCRUDHelper.EditTestSolution(solutionId, solutionScope.ScopeID, parentIssueId, ContentStatus.Published);
            });
            StringAssert.Contains(ex.Message.ToLowerInvariant(), "awaiting parent publish", "Server should response with 'awaiting parent publish'");

        }

        [TestMethod]
        public async Task User_CanCreateDraftSolution_AndPublishIt_RelatedChangesShouldOccur()
        {
            // Create draft issue
            var (jsonDoc, parentIssueId, title, content, scope) = await _testingCRUDHelper.CreateTestIssue(ContentStatus.Draft);
            // Create a proper solution as a draft
            var (_jsonDoc, solutionId, _title, _content, solutionScope) = await _testingCRUDHelper.CreateTestSolution(parentIssueId, ContentStatus.Draft);
            ContentItems_Paginated_ReadVM paginatedResponse = await Read.PaginatedMainContentFeed(new ContentFilter());
            Assert.IsTrue(paginatedResponse.TotalCount == 0, "There should be no main content items");

            // Content Counts in drafts should be accurate
            var draftsPage1 = await _env.fetchHTML("/drafts");
            var draftCountEl1 = draftsPage1.QuerySelector(".solution-draft-count");
            int draftCount1 = int.Parse(draftCountEl1!.TextContent);
            Assert.IsTrue(draftCount1 == 1, "Draft Count should be 1");

            // User profile issues check
            var userProfilePage1 = await _env.fetchHTML($"/user-profile?userId={TestDraftUser.Id}");
            var userProfileSolutionCountEl1 = userProfilePage1.QuerySelector("#tab-solution .content-count");
            int userSolutionCount1 = int.Parse(userProfileSolutionCountEl1!.TextContent);
            Assert.IsTrue(userSolutionCount1 == 0, "User solution count should be 0");

            // publish issue (required for publishing solution)
            await _testingCRUDHelper.EditTestIssue(parentIssueId, scope.ScopeID, ContentStatus.Published);
            // publish solution
            await _testingCRUDHelper.EditTestSolution(solutionId, scope.ScopeID, parentIssueId, ContentStatus.Published);

            // Content Counts in drafts should be accurate
            var draftsPage2 = await _env.fetchHTML("/drafts");
            var draftCountEl2 = draftsPage2.QuerySelector(".solution-draft-count");
            int draftCount2 = int.Parse(draftCountEl2!.TextContent);
            Assert.IsTrue(draftCount2 == 0, "Draft Count should be 0");

            // User profile issues check
            var userProfilePage2 = await _env.fetchHTML($"/user-profile?userId={TestDraftUser.Id}");
            var userProfileIssueCountEl2 = userProfilePage2.QuerySelector("#tab-solution .content-count");
            int userIssueCount2 = int.Parse(userProfileIssueCountEl2!.TextContent);
            Assert.IsTrue(userIssueCount2 == 1, "User solution count should be 1");

        }

    }
}