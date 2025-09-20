
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
    public class User_ContentCreation_Tests
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

        [DataTestMethod]
        [DataRow("/create-issue")]
        [DataRow("/create-solution")]
        public async Task User1_CanVisit_CreateContentPage_AndSeeForm(string url)
        {
            var document = await _env.fetchHTML(url);

            // Look for either issue or solution editor
            var editorElement = document.QuerySelector(".issue-editor, .solution-editor");

            Assert.IsNotNull(
                editorElement,
                $"Editor form (.issue-editor or .solution-editor) should exist on the create content page '{url}'."
            );
        }


        [TestMethod]
        public async Task User1_CanSubmit_IssueForm_WithError_AndGetErrorFeedback()
        {
            // 1. GET page to obtain antiforgery cookie + hidden token
            string tokenValue = await  TestingCRUDHelpers.GetAntiForgeryToken(_env, "/create-issue");

            // 3. Prepare form data INCLUDING the antiforgery token
            var formData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("__RequestVerificationToken", tokenValue!),
                new KeyValuePair<string, string>("Title", "Test Issue"),
                new KeyValuePair<string, string>("Description", "Repro steps ..."),
            };

            // 4. POST (cookie with antiforgery token should already be in HttpClient handler)
            var postResponse = await _env.PostFormAsync("/create-issue", formData);
            var body = await postResponse.Content.ReadAsStringAsync();
            // Convert body to JSON
            var jsonDoc = JsonDocument.Parse(body);
            var rootElement = jsonDoc.RootElement;
            bool success = rootElement.GetProperty("success").GetBoolean();
            Assert.IsFalse(success);
            string errorsArrayString = rootElement.GetProperty("errors").ToString();
            Assert.IsTrue(errorsArrayString.Contains("[\"Title\",\"Title must be between 15 and 150 characters\"]"));
            Assert.IsTrue(errorsArrayString.Contains("[\"Content\",\"Content is required\"]"));
            Assert.IsTrue(errorsArrayString.Contains("[\"Scope.Scales\",\"At least one scale must be selected.\"]"));
        }

        [TestMethod]
        public async Task User1_CanSubmit_CorrectIssueForm_AndViewTheNewIssue()
        {
            // 1. GET page to obtain antiforgery cookie + hidden token
            var (jsonDoc, title, content) = await CreateValidIssue();

            var rootElement = jsonDoc.RootElement;
            bool success = rootElement.GetProperty("success").GetBoolean();
            Assert.IsTrue(success);

            string newContentId = rootElement.GetProperty("contentId").ToString();
            string url = $"/issue/{newContentId}";
            var document = await _env.fetchHTML(url);
            var container = document.QuerySelector("body");
            Assert.IsTrue(container.TextContent.Contains(title));
            Assert.IsTrue(container.TextContent.Contains(content));
        }

        [TestMethod]
        public async Task User1_VisitsAuthoredContent_AndViews_AuthorContentTag()
        {
            // 1. GET page to obtain antiforgery cookie + hidden token
            var (jsonDoc, title, content) = await CreateValidIssue();

            var rootElement = jsonDoc.RootElement;

            string newContentId = rootElement.GetProperty("contentId").ToString();
            string url = $"/issue/{newContentId}";
            var document = await _env.fetchHTML(url);
            var authorContentTag = document.QuerySelector($".author-content-alert[data-id='{newContentId}']");
            Assert.IsNotNull(authorContentTag, "Author content tag should exist.");
        }

        [TestMethod]
        public async Task User1_CanCreateAnIssue_AndThenCreateSolutionsForThatIssue()
        {
            var (issueJsonDoc, issueTitle, content) = await CreateValidIssue();
            var rootElement = issueJsonDoc.RootElement;
            string newContentId = rootElement.GetProperty("contentId").GetString();

            // call CreateValidSolution and test the result
            var (solutionJsonDoc, solutionTitle, solutionContent) = await CreateValidSolution(newContentId);
            var solutionRootElement = solutionJsonDoc.RootElement;
            bool success = solutionRootElement.GetProperty("success").GetBoolean();
            Assert.IsTrue(success);
        }

        [TestMethod]
        public async Task User1_CanCreateAnIssue_AndThenCreateSolutionsForThatIssue_AndVisitContent_AndSeeAuthoredTag()
        {
            var (issueJsonDoc, issueTitle, content) = await CreateValidIssue();
            var rootElement = issueJsonDoc.RootElement;
            string newContentId = rootElement.GetProperty("contentId").GetString();

            // call CreateValidSolution and test the result
            var (solutionJsonDoc, solutionTitle, solutionContent) = await CreateValidSolution(newContentId);
            var solutionRootElement = solutionJsonDoc.RootElement;
            bool success = solutionRootElement.GetProperty("success").GetBoolean();
            Assert.IsTrue(success);

            string newSolutionContentId = solutionRootElement.GetProperty("contentId").ToString();
            string url = $"/solution/{newSolutionContentId}";
            var document = await _env.fetchHTML(url);
            var authorContentTag = document.QuerySelector($".author-content-alert[data-id='{newSolutionContentId}']");
            Assert.IsNotNull(authorContentTag, "Author content tag should exist.");
        }

        [TestMethod]
        public async Task User1_CanCreateAnIssue_AndThenCreateSolutionsForThatIssue_AndWhenCreatingAnIssueForThatSolution_TheParentSolutionSelect_shouldBeSetToCorrectSolution()
        {
            var (issueJsonDoc, issueTitle, content) = await CreateValidIssue();
            var rootElement = issueJsonDoc.RootElement;
            string newContentId = rootElement.GetProperty("contentId").GetString();

            // call CreateValidSolution and test the result
            var (solutionJsonDoc, solutionTitle, solutionContent) = await CreateValidSolution(newContentId);
            var solutionRootElement = solutionJsonDoc.RootElement;
            string newSolutionContentId = solutionRootElement.GetProperty("contentId").ToString();
            string url = $"/create-issue?parentSolutionID={newSolutionContentId}";
            var document = await _env.fetchHTML(url);
            var ParentSolutionSelect = document.QuerySelector($"select#ParentSolutionID");
            // Select should be disabled and set to solution
            Assert.IsTrue(ParentSolutionSelect.HasAttribute("disabled"), "Parent Solution select should be disabled.");
            var selectedOption = ParentSolutionSelect.QuerySelector("option[selected]");
            Assert.AreEqual(newSolutionContentId, selectedOption.GetAttribute("value"), "Parent Solution select should be preset to the solution id.");
        }




        public async Task<(JsonDocument JsonDoc, string Title, string Content)> CreateValidIssue()
        {
            return await TestingCRUDHelpers.CreateIssue(_env,
               "This is just an example issue title (content creation)",
               "This is just an example issue content",
               new Scope()
               {
                   Scales = { Scale.Global }
               });
        }

        public async Task<(JsonDocument JsonDoc, string Title, string Content)> CreateValidSolution(string parentIssueID)
        {
            return await TestingCRUDHelpers.CreateSolution(_env,
              "This is just an example solution title (content creation)",
              "This is just an example solution content",
              new Scope()
              {
                  Scales = { Scale.Global }
              }, parentIssueID);
        }


        public async Task<string> GetAntiForgeryToken(string url)
        {
            var document = await _env.fetchHTML(url);

            var tokenValue = document
                .QuerySelector("input[name=__RequestVerificationToken]")
                ?.GetAttribute("value");

            Assert.IsFalse(string.IsNullOrWhiteSpace(tokenValue), "Antiforgery token not found in form.");

            return tokenValue;
        }

    }
}