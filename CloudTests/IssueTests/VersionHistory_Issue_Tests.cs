using AngleSharp.Dom;
using atlas_the_public_think_tank.Data;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;
using atlas_the_public_think_tank.Data.DatabaseEntities.Users;

using atlas_the_public_think_tank.Models.ViewModel.CRUD.ContentItem_Common;
using CloudTests.TestingSetup;
using CloudTests.TestingSetup.TestingData;
using System;
using System.Text.Json;


namespace CloudTests.IssueTests
{
    [TestClass]
    public class VersionHistory_Issue_Tests
    {
        private static HttpClient _client;
        private static ApplicationDbContext _db;
        private static TestEnvironment _env;
        private static string editedContentId;

        [ClassInitialize]
        public static async Task ClassInit(TestContext context)
        {
            // Arrange environment
            _env = new TestEnvironment();
            _db = _env._db;
            _client = _env._client;

            editedContentId = await CreateDemoData();

        }

        /// <summary>
        /// Create user, Create Issue, Edit that issue
        /// </summary>
        /// <remarks>
        /// All of the content is created and edited before the page is actually viewed.
        /// Viewing the app is what sets the cache.
        /// So these test cases work because they represent an initial page load.
        /// </remarks>
        public static async Task<string> CreateDemoData()
        {

            // Create and login user
            string email = Users.TestUser1.Email!;
            string password = Users.TestUser1Password;
            AppUser testUser = Users.CreateTestUser(_db, Users.TestUser1, password);

            bool loginSuccess = await Users.LoginUserViaEndpoint(_env, email, password);
            Assert.IsTrue(loginSuccess, "Login should be successful");

            var (jsonDoc1, title1, content1) = await TestingCRUDHelpers.CreateIssue(_env,
                "This is just an example issue title (content creation)",
                "This is just an example issue content",
                new Scope()
                {
                    Scales = { Scale.Household }
                });

            var rootElement1 = jsonDoc1.RootElement;
            string parentIssueId = rootElement1.GetProperty("contentId").ToString();
            var issueDoc = await _env.TextHtmlToDocument(rootElement1.GetProperty("content").ToString());
            var scopeRibbonEl = issueDoc.QuerySelector(".scope-ribbon");
            Assert.IsNotNull(scopeRibbonEl, "Scope ribbon el should exists");
            string? scopeId = scopeRibbonEl.GetAttribute("data-scope-id");
            Assert.IsNotNull(scopeId, "Scope Id should exists");

            var (jsonDoc2, title2, content2) = await TestingCRUDHelpers.EditIssue(_env,
              "This is just an example issue title (edit 1)",
              "This is just an example issue content",
              parentIssueId,
              new Scope()
              {
                ScopeID = new Guid(scopeId!),
                Scales = { Scale.Regional }
              });

            return parentIssueId;
        }


        [ClassCleanup]
        public static async Task ClassCleanup()
        {
            await TestingUtilityMethods.deleteDatabase(_client, _db);
        }


        [TestMethod]
        public async Task VersionHistoryIcon_DisplaysAfterUser1_CreateAnIssue_AndEditsIssue()
        {
            // Go to the issues page and should see version control icon
            string issueUrl = $"/issue/{editedContentId}";
            var document = await _env.fetchHTML(issueUrl);
            var versionHistoryButton = document.QuerySelector(".show-version-history");
            Assert.IsNotNull(versionHistoryButton, "Version history button should be present");
        }

        [TestMethod]
        public async Task VersionHistoryModal_ForIssue_IsVisible()
        {
            // Go to the issues page and should see version control icon
            string issueUrl = $"/issue-version-history?issueId={editedContentId}";
            ContentCreationResponse_JsonVM response = await _env.fetchJson<ContentCreationResponse_JsonVM>(issueUrl);
            var document = await _env.TextHtmlToDocument(response.Content);
            var modal = document.QuerySelector("#versionControlModal");
            Assert.IsNotNull(modal, "Version history modal should be visible");
        }

        [TestMethod]
        public async Task VersionHistoryModal_ForIssue_HasTitle_VersionHistory()
        {
            // Go to the issues page and should see version control icon
            string issueUrl = $"/issue-version-history?issueId={editedContentId}";
            ContentCreationResponse_JsonVM response = await _env.fetchJson<ContentCreationResponse_JsonVM>(issueUrl);
            var document = await _env.TextHtmlToDocument(response.Content);
            var modal = document.QuerySelector("#versionControlModal");
            var title = modal.QuerySelector(".modal-title");
            Assert.IsTrue(title.InnerHtml.Contains("Version History"));
        }

        [TestMethod]
        public async Task VersionHistoryModal_ForIssue_Displays_2Entries()
        {
            // Go to the issues page and should see version control icon
            string issueUrl = $"/issue-version-history?issueId={editedContentId}";
            ContentCreationResponse_JsonVM response = await _env.fetchJson<ContentCreationResponse_JsonVM>(issueUrl);
            var document = await _env.TextHtmlToDocument(response.Content);
            var modal = document.QuerySelector("#versionControlModal");
            int versionCount = modal.QuerySelectorAll(".issue-card").Count();
            Assert.AreEqual(2, versionCount);
        }


        [TestMethod]
        public async Task VersionHistoryModal_ForIssue_Displays_CompositeScopeInfo()
        {
            // Go to the issues page and should see version control icon
            string issueUrl = $"/issue-version-history?issueId={editedContentId}";
            ContentCreationResponse_JsonVM response = await _env.fetchJson<ContentCreationResponse_JsonVM>(issueUrl);
            var document = await _env.TextHtmlToDocument(response.Content);
            var modal = document.QuerySelector("#versionControlModal");
            var versionedIssueCards = modal.QuerySelectorAll(".issue-card");

            foreach (var card in versionedIssueCards)
            {
                var compositeScopeContainer = card.QuerySelector(".composite-scope");
                Assert.IsNotNull(compositeScopeContainer,"Issue version card has composite scope");
            }
        }

    }
}