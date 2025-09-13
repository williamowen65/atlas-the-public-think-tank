using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.ContentItem_Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CloudTests.TestingSetup
{
    public static class TestingCRUDHelpers
    {

        public static void AttachScopeToFormData(Scope scope, List<KeyValuePair<string, string>> formData)
        {
            if (scope != null)
            {
                if (scope.ScopeID != Guid.Empty) 
                {
                    // Add ScopeID
                    formData.Add(new KeyValuePair<string, string>("Scope.ScopeID", scope.ScopeID.ToString()));
                }

                if (scope.Scales != null && scope.Scales.Any())
                {
                    foreach (var scale in scope.Scales)
                        formData.Add(new KeyValuePair<string, string>("Scope.Scales", scale.ToString()));
                }
                if (scope.Domains != null && scope.Domains.Any())
                {
                    foreach (var domain in scope.Domains)
                        formData.Add(new KeyValuePair<string, string>("Scope.Domains", domain.ToString()));
                }
                if (scope.EntityTypes != null && scope.EntityTypes.Any())
                {
                    foreach (var entityType in scope.EntityTypes)
                        formData.Add(new KeyValuePair<string, string>("Scope.EntityTypes", entityType.ToString()));
                }
                if (scope.Timeframes != null && scope.Timeframes.Any())
                {
                    foreach (var timeframe in scope.Timeframes)
                        formData.Add(new KeyValuePair<string, string>("Scope.Timeframes", timeframe.ToString()));
                }
                if (scope.Boundaries != null && scope.Boundaries.Any())
                {
                    foreach (var boundary in scope.Boundaries)
                        formData.Add(new KeyValuePair<string, string>("Scope.Boundaries", boundary.ToString()));
                }
            }

        }


        public static async Task<(JsonDocument JsonDoc, string Title, string Content)> CreateIssue(
          TestEnvironment _env,
          string title,
          string content,
          Scope scope,
          Guid? parentIssueId = null
         )
        {
            // 1. GET page to obtain antiforgery cookie + hidden token
            string tokenValue = await TestingCRUDHelpers.GetAntiForgeryToken(_env, "/create-issue");
            //string scopeId = await GetScopeIDFromPage(_env, "/create-issue");


            var formData = new List<KeyValuePair<string, string>>
            {
                new("__RequestVerificationToken", tokenValue!),
                new("Title", title),
                new("Content", content),
            };

            AttachScopeToFormData(scope, formData);

            if (parentIssueId != Guid.Empty)
            {
                formData.Add(new KeyValuePair<string, string>("ParentIssueID", parentIssueId.ToString()!));
            }

            // Update your PostFormAsync to accept List<KeyValuePair<string, string>>
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
          Scope scope,
          string parentIssueId
         )
        {
            // 1. GET page to obtain antiforgery cookie + hidden token
            string tokenValue = await GetAntiForgeryToken(_env, "/create-solution");
            //string scopeId = await GetScopeIDFromPage(_env, "/create-solution");


            // 3. Prepare form data INCLUDING the antiforgery token
            var formData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("__RequestVerificationToken", tokenValue!),
                new KeyValuePair<string, string>("Title", title),
                new KeyValuePair<string, string>("Content", content),
                new KeyValuePair<string, string>("ParentIssueID", parentIssueId)
            };

            AttachScopeToFormData(scope, formData);

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
          string issueId,
          Scope scope
         )
        {
            string url = $"/edit-issue?issueId={issueId}";
            // The edit issue GET endpoint returns JSON with an HTML partial in the "content" key.
            string tokenValue = await GetAntiForgeryTokenFromJson(_env, url);

            // 3. Prepare form data INCLUDING the antiforgery token
            var formData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("__RequestVerificationToken", tokenValue!),
                new KeyValuePair<string, string>("Title", title),
                new KeyValuePair<string, string>("Content", content),
                new KeyValuePair<string, string>("IssueID", issueId),

            };

            AttachScopeToFormData(scope, formData);

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
          string parentIssueId,
          Scope scope
         )
        {
            string url = $"/edit-solution?solutionId={solutionId}";
            // The edit issue GET endpoint returns JSON with an HTML partial in the "content" key.
            string tokenValue = await GetAntiForgeryTokenFromJson(_env, url);

            // 3. Prepare form data INCLUDING the antiforgery token
            var formData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("__RequestVerificationToken", tokenValue!),
                new KeyValuePair<string, string>("Title", title),
                new KeyValuePair<string, string>("Content", content),
                new KeyValuePair<string, string>("SolutionID", solutionId),
                new KeyValuePair<string, string>("ParentIssueID", parentIssueId)
            };

            AttachScopeToFormData(scope, formData);


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

       
    }
}
