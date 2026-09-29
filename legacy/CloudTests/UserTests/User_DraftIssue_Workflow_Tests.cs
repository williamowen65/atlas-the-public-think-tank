
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
using Microsoft.CodeAnalysis;
using NuGet.ContentModel;
using System;
using System.Text.Json;


namespace CloudTests.UserTests
{
    [TestClass]
    public class User_DraftIssue_Workflow_Tests
    {
        #region Test setup

        private HttpClient _client;
        private ApplicationDbContext _db;
        private TestEnvironment _env;
        private TestingCRUDHelper _testingCRUDHelper;
        private Read _read;


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
            _read = _env._read;

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
            UserName = "draftIssue@example.com",
            NormalizedUserName = "DRAFTISSUE@EXAMPLE.COM",
            Email = "draftIssue@example.com",
            NormalizedEmail = "DRAFTISSUE@EXAMPLE.COM",
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
            var (jsonDoc, parentIssueId, title, content, scope) = await _testingCRUDHelper.CreateTestIssue_ViaWebApp(ContentStatus.Draft);
            ContentItems_Paginated_ReadVM paginatedResponse = await _read.PaginatedMainContentFeed(new ContentFilter());
            Assert.IsTrue(paginatedResponse.TotalCount == 0, "There should be no main content items");

        }

        [TestMethod]
        public async Task User_CanCreateDraftIssue_ViewTheDraftOnDraftsPage()
        {
            var (jsonDoc, parentIssueId, title, content, scope) = await _testingCRUDHelper.CreateTestIssue_ViaWebApp(ContentStatus.Draft);
            var draftsPage = await _env.fetchHTML("/drafts");
            var draftContent = draftsPage.QuerySelector($".card[id='{parentIssueId}']");
            Assert.IsNotNull(draftContent, "Draft content should exist on the draft page for this user");
        }

        [TestMethod]
        public async Task User_CanCreateDraftIssue_AnotherUserCanTryToViewItViaURL_ButWillFailBecause_NotAuthorizedToViewAnotherPersonsDraft()
        {
            var (jsonDoc, parentIssueId, title, content, scope) = await _testingCRUDHelper.CreateTestIssue_ViaWebApp(ContentStatus.Draft);

            // Log a random user in
            AppUser testUser = Users.CreateTestUser(_db, TestRandomUser, TestRandomPassword);
            bool loginSuccess = await Users.LoginUserViaEndpoint(_env, TestRandomUser.Email!, TestRandomPassword);

            await Assert.ThrowsExceptionAsync<System.Net.Http.HttpRequestException>(async () =>
            {
                var issuesPage = await _env.fetchHTML($"/issue/{parentIssueId}");
            }, "The random user shouldn't be able to view another persons draft");

        }



        [TestMethod]
        public async Task User_CannotPublish_SubIssue_WhichHasAParentIssue_InDraftMode()
        {
            // Create a root issue as a draft
            var (jsonDoc, parentIssueId, title, content, scope) = await _testingCRUDHelper.CreateTestIssue_ViaWebApp(ContentStatus.Draft);
          
            // Create a proper subissue as a draft
            var (_jsonDoc, subIssueId, _title, _content, subIssueScope) = await _testingCRUDHelper.CreateTestSubIssue_ViaWebApp(ContentStatus.Draft, new Guid(parentIssueId));


            // The UI should have the publish button disabled.
            var editIssueDocTemp = await _env.fetchJson<ContentCreationResponse_JsonVM>($"/edit-issue?issueId={subIssueId}");
            var editIssueDoc = await _env.TextHtmlToDocument(editIssueDocTemp.Content);
            var awaitingParentPublishBtn = editIssueDoc.QuerySelector(".awaiting-parent-publish");
            Assert.IsNotNull(awaitingParentPublishBtn, "awaiting-parent-publish button should exist");
            var publishButton = editIssueDoc.QuerySelector(".publish-issue");
            Assert.IsNull(publishButton, "publish button should not exist");


            // This test would now send back the error page
            // If the user directly pings the /edit-issue endpoint trying to publish it should still not work
            //var ex = await Assert.ThrowsExceptionAsync<Exception>(async () =>
            //{
            //    await _testingCRUDHelper.EditTestIssue_ViaWebApp(subIssueId, subIssueScope.ScopeID, ContentStatus.Published);
            //});
            //StringAssert.Contains(ex.Message.ToLowerInvariant(), "awaiting parent publish", "Server should response with 'awaiting parent publish'");


            // If someone with DB access tries to directly update to Published, the DB should stop them.
            var subIssueGuid = new Guid(subIssueId);
            var subIssueEntity = _db.Issues.First(i => i.IssueID == subIssueGuid);
            // Set the status directly on the tracked entity.
            // Using Property("ContentStatus") avoids relying on the concrete property name in the Issue class.
            _db.Entry(subIssueEntity).Property("ContentStatus").CurrentValue = ContentStatus.Published;
            await Assert.ThrowsExceptionAsync<Microsoft.EntityFrameworkCore.DbUpdateException>(async () =>
            {
                await _db.SaveChangesAsync();
            });
        }
        
        [TestMethod]
        public async Task User_CanCreateDraftIssue_AndPublishIt_RelatedChangesShouldOccur()
        {
            // Create draft issue
            var (jsonDoc, parentIssueId, title, content, scope) = await _testingCRUDHelper.CreateTestIssue_ViaWebApp(ContentStatus.Draft);
            ContentItems_Paginated_ReadVM paginatedResponse = await _read.PaginatedMainContentFeed(new ContentFilter());
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
            await _testingCRUDHelper.EditTestIssue_ViaWebApp(parentIssueId, scope.ScopeID, ContentStatus.Published);
            
            
            // Content can be viewed in main content
            ContentItems_Paginated_ReadVM paginatedResponse2 = await _read.PaginatedMainContentFeed(new ContentFilter());
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