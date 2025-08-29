using atlas_the_public_think_tank.Models.ViewModel.CRUD.ContentItem_Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CloudTests.TestingSetup
{
    public static class TestingVersionHistoryHelpers
    {



        public static async Task<(JsonDocument JsonDoc, string Title, string Content)> CreateIssue(
          TestEnvironment _env,
          string title,
          string content,
          Guid? parentIssueId = null
         )
        {
            // 1. GET page to obtain antiforgery cookie + hidden token
            string tokenValue = await GetAntiForgeryToken(_env, "/create-issue");
            string scopeId = await GetScopeIDFromPage(_env, "/create-issue");


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
        public static async Task<(JsonDocument JsonDoc, string Title, string Content)> CreateSolution(
          TestEnvironment _env,
          string title,
          string content,
          string parentIssueId
         )
        {
            // 1. GET page to obtain antiforgery cookie + hidden token
            string tokenValue = await GetAntiForgeryToken(_env, "/create-solution");
            string scopeId = await GetScopeIDFromPage(_env, "/create-solution");


            // 3. Prepare form data INCLUDING the antiforgery token
            var formData = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = tokenValue!,
                ["Title"] = title,
                ["Content"] = content,
                ["ScopeID"] = scopeId,
                ["ParentIssueID"] = parentIssueId
            };

         
            // 4. POST (cookie with antiforgery token should already be in HttpClient handler)
            var postResponse = await _env.PostFormAsync("/create-solution", formData);
            var body = await postResponse.Content.ReadAsStringAsync();
            // Convert body to JSON
            var jsonDoc = JsonDocument.Parse(body);
            return (jsonDoc, title, content);
        }


        public static async Task<(JsonDocument JsonDoc, string Title, string Content)> EditIssue(
            TestEnvironment _env,
          string title,
          string content,
          string issueId
         )
        {
            string url = $"/edit-issue?issueId={issueId}";
            // The edit issue GET endpoint returns JSON with an HTML partial in the "content" key.
            string tokenValue = await GetAntiForgeryTokenFromJson(_env, url);
            string scopeId = await GetScopeIDFromJsonPage(_env, url);

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

        public static async Task<(JsonDocument JsonDoc, string Title, string Content)> EditSolution(
            TestEnvironment _env,
          string title,
          string content,
          string solutionId,
          string parentIssueId
         )
        {
            string url = $"/edit-solution?solutionId={solutionId}";
            // The edit issue GET endpoint returns JSON with an HTML partial in the "content" key.
            string tokenValue = await GetAntiForgeryTokenFromJson(_env, url);
            string scopeId = await GetScopeIDFromJsonPage(_env, url);

            // 3. Prepare form data INCLUDING the antiforgery token
            var formData = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = tokenValue!,
                ["Title"] = title,
                ["Content"] = content,
                ["ScopeID"] = scopeId,
                ["SolutionID"] = solutionId,
                ["ParentIssueID"] = parentIssueId
            };


            // 4. POST (cookie with antiforgery token should already be in HttpClient handler)
            var postResponse = await _env.PostFormAsync("/edit-solution", formData);
            var body = await postResponse.Content.ReadAsStringAsync();
            // Convert body to JSON
            var jsonDoc = JsonDocument.Parse(body);
            return (jsonDoc, title, content);
        }


        public static async Task<string> GetAntiForgeryToken(
            TestEnvironment _env,
            string url)
        {
            var document = await _env.fetchHTML(url);

            var tokenValue = document
                .QuerySelector("input[name=__RequestVerificationToken]")
                ?.GetAttribute("value");

            Assert.IsFalse(string.IsNullOrWhiteSpace(tokenValue), "Antiforgery token not found in form.");

            return tokenValue;
        }

        // New helper for endpoints returning JSON with an embedded HTML partial in a "content" key
        public static async Task<string> GetAntiForgeryTokenFromJson(TestEnvironment _env, string url)
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

        public static async Task<string> GetScopeIDFromPage(
            TestEnvironment _env,
            string url)
        {
            var document = await _env.fetchHTML(url);

            // Try to find a select element for scope - assuming the name is either "Scope" or "ScopeID"
            var selectElement = document.QuerySelector("select[name=ScopeID]");

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
        public static async Task<string> GetScopeIDFromJsonPage(TestEnvironment _env, string url, string htmlJsonKey = "content")
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
