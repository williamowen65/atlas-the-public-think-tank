using AngleSharp.Dom;
using atlas_the_public_think_tank.Data;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;
using atlas_the_public_think_tank.Data.DatabaseEntities.Users;

using atlas_the_public_think_tank.Models.ViewModel.CRUD.ContentItem_Common;
using CloudTests.TestingSetup;
using CloudTests.TestingSetup.TestingData;
using System;
using System.Text.Json;


namespace CloudTests.SolutionTests
{
    [TestClass]
    public class VersionHistory_Solution_Tests
    {
        private static HttpClient _client;
        private static ApplicationDbContext _db;
        private static TestEnvironment _env;
        private static string editedSolutionId;

        [ClassInitialize]
        public static async Task ClassInit(TestContext context)
        {
            // Arrange environment
            _env = new TestEnvironment();
            _db = _env._db;
            _client = _env._client;

            editedSolutionId = await CreateDemoData();

        }

        public static async Task<string> CreateDemoData()
        {

            // Create and login user
            AppUser testUser = Users.CreateTestUser1(_db);
            string email = "testuser@example.com";
            string password = "Password123!";

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

            var (jsonDoc2, title2, content2) = await TestingCRUDHelpers.CreateSolution(_env,
               "This is just an example solution title (content creation)",
               "This is just an example solution content",
                new Scope()
                {
                    ScopeID = new Guid(scopeId!),
                    Scales = { Scale.Regional }
                },
                 parentIssueId
                );

            var rootElement2 = jsonDoc2.RootElement;
            string newSolutionId = rootElement2.GetProperty("contentId").ToString();

            var (jsonDoc3, title3, content3) = await TestingCRUDHelpers.EditSolution(_env,
            "This is just an example solution title (edit 1)",
            "This is just an example solution content",
            newSolutionId,
            parentIssueId,
            new Scope()
            {
                ScopeID = new Guid(scopeId!),
                Scales = { Scale.Regional }
            }
            );

            return newSolutionId;
        }


        [ClassCleanup]
        public static async Task ClassCleanup()
        {
            await TestingUtilityMethods.deleteDatabase(_client, _db);
        }


        [TestMethod]
        public async Task VersionHistoryIcon_DisplaysAfterUser1_CreatesAnIssue_CreatesASolution_AndEditsSolution()
        {
            // Go to the issues page and should see version control icon
            string issueUrl = $"/solution/{editedSolutionId}";
            var document = await _env.fetchHTML(issueUrl);
            var versionHistoryButton = document.QuerySelector(".show-version-history");
            Assert.IsNotNull(versionHistoryButton, "Version history button should be present");
        }

        [TestMethod]
        public async Task VersionHistoryModal_ForSolution_IsVisible()
        {
            // Go to the issues page and should see version control icon
            string solutionUrl = $"/solution-version-history?solutionId={editedSolutionId}";
            ContentCreationResponse_JsonVM response = await _env.fetchJson<ContentCreationResponse_JsonVM>(solutionUrl);
            var document = await _env.TextHtmlToDocument(response.Content);
            var modal = document.QuerySelector("#versionControlModal");
            Assert.IsNotNull(modal, "Version history modal should be visible");
        }

        [TestMethod]
        public async Task VersionHistoryModal_ForSolution_HasTitle_VersionHistory()
        {
            // Go to the issues page and should see version control icon
            string solutionUrl = $"/solution-version-history?solutionId={editedSolutionId}";
            ContentCreationResponse_JsonVM response = await _env.fetchJson<ContentCreationResponse_JsonVM>(solutionUrl);
            var document = await _env.TextHtmlToDocument(response.Content);
            var modal = document.QuerySelector("#versionControlModal");
            var title = modal.QuerySelector(".modal-title");
            Assert.IsTrue(title.InnerHtml.Contains("Version History"));
        }

        [TestMethod]
        public async Task VersionHistoryModal_ForSolution_Displays_2Entries()
        {
            // Go to the issues page and should see version control icon
            string solutionUrl = $"/solution-version-history?solutionId={editedSolutionId}";
            ContentCreationResponse_JsonVM response = await _env.fetchJson<ContentCreationResponse_JsonVM>(solutionUrl);
            var document = await _env.TextHtmlToDocument(response.Content);
            var modal = document.QuerySelector("#versionControlModal");
            int versionCount = modal.QuerySelectorAll(".solution-card").Count();
            Assert.AreEqual(2, versionCount);
        }


        [TestMethod]
        public async Task VersionHistoryModal_ForSolution_Displays_CompositeScopeInfo()
        {
            // Go to the issues page and should see version control icon
            string solutionUrl = $"/solution-version-history?solutionId={editedSolutionId}";
            ContentCreationResponse_JsonVM response = await _env.fetchJson<ContentCreationResponse_JsonVM>(solutionUrl);
            var document = await _env.TextHtmlToDocument(response.Content);
            var modal = document.QuerySelector("#versionControlModal");
            var versionedSolutionCards = modal.QuerySelectorAll(".solution-card");

            foreach (var card in versionedSolutionCards)
            {
                var compositeScopeContainer = card.QuerySelector(".composite-scope");
                Assert.IsNotNull(compositeScopeContainer, "Solution version card has composite scope");
            }
        }

    }
}