using AngleSharp.Dom;
using atlas_the_public_think_tank.Data;
using atlas_the_public_think_tank.Data.DatabaseEntities.Users;
 
using atlas_the_public_think_tank.Models.ViewModel.CRUD.ContentItem_Common;
using CloudTests.TestingSetup;
using CloudTests.TestingSetup.TestingData;
using System;
using System.Text.Json;


namespace CloudTests.UserStories
{
    [TestClass]
    public class User_IssueVersionHistory_Tests
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
            AppUser testUser = Users.CreateTestUser1(_db);
            string email = "testuser@example.com";
            string password = "Password123!";

            bool loginSuccess = await Users.LoginUserViaEndpoint(_env, email, password);
            Assert.IsTrue(loginSuccess, "Login should be successful");
        }


        [ClassCleanup]
        public static async Task ClassCleanup()
        {
            await TestingUtilityMethods.deleteDatabase(_client, _db);
        }




        [TestMethod]
        public async Task User1_CanCreateAnIssue_AndEditIssue_AndSeeVersionHistoryIcon()
        {
            var (jsonDoc1, title1, content1) = await CreateIssue(
                "This is just an example issue title (content creation)",
                "This is just an example issue content");

            var rootElement1 = jsonDoc1.RootElement;
            string newContentId1 = rootElement1.GetProperty("contentId").ToString();

            var (jsonDoc2, title2, content2) = await EditIssue(
              "This is just an example issue title (edit 1)",
              "This is just an example issue content",
              newContentId1);


            // Go to the issues page and should see version control icon
            string issueUrl = $"/issue/{newContentId1}";
            var document = await _env.fetchHTML(issueUrl);
            var versionHistoryButton = document.QuerySelector(".show-version-history");
            Assert.IsNotNull(versionHistoryButton, "Version history button should be present");

        }




        public async Task<(JsonDocument JsonDoc, string Title, string Content)> CreateIssue(
          string title,
          string content,
          Guid? parentIssueId = null
         )
        {
            // 1. GET page to obtain antiforgery cookie + hidden token
            string tokenValue = await GetAntiForgeryToken("/create-issue");
            string scopeId = await GetScopeIDFromPage("/create-issue");


            // 3. Prepare form data INCLUDING the antiforgery token
            var formData = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = tokenValue!,
                ["Title"] = title,
                ["Content"] = content,
                ["ScopeID"] = scopeId,
            };

            if (parentIssueId != Guid.Empty)
            {
                formData.Add("ParentIssueID", parentIssueId.ToString()!);
            }
           
            // 4. POST (cookie with antiforgery token should already be in HttpClient handler)
            var postResponse = await _env.PostFormAsync("/create-issue", formData);
            var body = await postResponse.Content.ReadAsStringAsync();
            // Convert body to JSON
            var jsonDoc = JsonDocument.Parse(body);
            return (jsonDoc, title, content);
        }


        public async Task<(JsonDocument JsonDoc, string Title, string Content)> EditIssue(
          string title,
          string content,
          string issueId
         )
        {
            string url = $"/edit-issue?issueId={issueId}";
            // The edit issue GET endpoint returns JSON with an HTML partial in the "content" key.
            string tokenValue = await GetAntiForgeryTokenFromJson(url);
            string scopeId = await GetScopeIDFromJsonPage(url);

            // 3. Prepare form data INCLUDING the antiforgery token
            var formData = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = tokenValue!,
                ["Title"] = title,
                ["Content"] = content,
                ["ScopeID"] = scopeId,
                ["IssueID"] = issueId
            };

           
            // 4. POST (cookie with antiforgery token should already be in HttpClient handler)
            var postResponse = await _env.PostFormAsync("/edit-issue", formData);
            var body = await postResponse.Content.ReadAsStringAsync();
            // Convert body to JSON
            var jsonDoc = JsonDocument.Parse(body);
            return (jsonDoc, title, content);
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

        // New helper for endpoints returning JSON with an embedded HTML partial in a "content" key
        public async Task<string> GetAntiForgeryTokenFromJson(string url)
        {
            ContentCreationResponse_JsonVM json = await _env.fetchJson<ContentCreationResponse_JsonVM>(url);
            
            string? html = json.Content;

            var document = await _env.TextHtmlToDocument(html!);
            var tokenValue = document
                .QuerySelector("input[name=__RequestVerificationToken]")
                ?.GetAttribute("value");

            Assert.IsFalse(string.IsNullOrWhiteSpace(tokenValue), "Antiforgery token not found in JSON provided HTML partial.");
            return tokenValue!;
        }

        public async Task<string> GetScopeIDFromPage(string url)
        {
            var document = await _env.fetchHTML(url);

            // Try to find a select element for scope - assuming the name is either "Scope" or "ScopeID"
            var selectElement =  document.QuerySelector("select[name=ScopeID]");

            Assert.IsNotNull(selectElement, "Scope select element not found on the page.");

            // Attempt to find a valid option with a non-empty value.
            var optionElements = selectElement.QuerySelectorAll("option");
            foreach (var option in optionElements)
            {
                string? value = option.GetAttribute("value");
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }

            Assert.Fail("No valid ScopeID option found.");
            return string.Empty;
        }

        // New helper for endpoints returning JSON with an embedded HTML partial in a "content" key
        public async Task<string> GetScopeIDFromJsonPage(string url, string htmlJsonKey = "content")
        {
            ContentCreationResponse_JsonVM json = await _env.fetchJson<ContentCreationResponse_JsonVM>(url);
            
            string? html = json.Content;

            var document = await _env.TextHtmlToDocument(html!);
            var selectElement = document.QuerySelector("select[name=ScopeID]");
            Assert.IsNotNull(selectElement, "Scope select element not found in JSON provided HTML partial.");

            var optionElements = selectElement.QuerySelectorAll("option");
            foreach (var option in optionElements)
            {
                string? value = option.GetAttribute("value");
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }

            Assert.Fail("No valid ScopeID option found in JSON provided HTML partial.");
            return string.Empty;
        }
    }
}