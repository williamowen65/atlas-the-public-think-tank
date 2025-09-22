using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;
using atlas_the_public_think_tank.Models.ViewModel.AjaxVM;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.ContentItem_Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CloudTests.TestingSetup
{
    /// <summary>
    /// Use this instead of <see cref="TestingCRUDHelpers"/> which is the old version.
    /// Migrate the other test over to this version
    /// </summary>
    public class TestingCRUDHelper
    {
        public readonly TestEnvironment _env;
        public TestingCRUDHelper(TestEnvironment env) { 
            _env = env;
        }

        #region Test Issue CRUD helpers

        public async Task<(JsonDocument JsonDoc, string issueId, string title, string content, Scope scope)> CreateTestIssue()
        {
            string title = "This is a title for a test issue";
            string content = "This is a content for a test issue";
            Scope scope = new Scope()
            {
                Scales = { Scale.Global }
            };
            var (jsonDoc, issueId, scopeId) = await CreateIssue(title, content, scope);
            scope.ScopeID = new Guid(scopeId);
            return (jsonDoc, issueId, title, content, scope);
        }

        public async Task<(JsonDocument JsonDoc, string issueId, string title, string content, Scope scope)> EditTestIssue(
            string issueId,
            Guid scopeId
         )
        {
            string title = "This is an updated title for a test issue";
            string content = "This is an updated content for a test issue";
            Scope scope = new Scope()
            {
                ScopeID = scopeId,
                Scales = { Scale.Global, Scale.National },
                Domains = { Domain.Social }
            };
            var (jsonDoc, _issueId, _scopeId) = await EditIssue(issueId, title, content, scope);
            return (jsonDoc, issueId, title, content, scope);
        }

        public async Task<(JsonDocument JsonDoc, string issueId, string title, string content, Scope scope)> CreateTestSubIssue(Guid? parentIssueId = null, Guid? parentSolutionId = null)
        {
            string title = "This is a title for a test sub-issue";
            string content = "This is a content for a test sub-issue";
            Scope scope = new Scope()
            {
                Scales = { Scale.Global }
            };
            var (jsonDoc, issueId, scopeId) = await CreateIssue(title, content, scope, parentIssueId, parentSolutionId);
            scope.ScopeID = new Guid(scopeId);
            return (jsonDoc, issueId, title, content, scope);
        }


        public async Task<(JsonDocument JsonDoc, string issueId, string scopeId)> CreateIssue(
            string title,
            string content,
            Scope scope,
            Guid? parentIssueId = null,
            Guid? parentSolutionId = null
         )
        {
            // 1. GET page to obtain antiforgery cookie + hidden token
            string tokenValue = await TestingCRUDHelpers.GetAntiForgeryToken(_env, "/create-issue");

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
            if (parentSolutionId != Guid.Empty)
            {
                formData.Add(new KeyValuePair<string, string>("ParentSolutionID", parentSolutionId.ToString()!));
            }

            // Update your PostFormAsync to accept List<KeyValuePair<string, string>>
            var postResponse = await _env.PostFormAsync("/create-issue", formData);
            var body = await postResponse.Content.ReadAsStringAsync();
            // Convert body to JSON
            var jsonDoc = JsonDocument.Parse(body);
            var rootElement = jsonDoc.RootElement;
            string issueId = rootElement.GetProperty("contentId").ToString();
            var issueDoc = await _env.TextHtmlToDocument(rootElement.GetProperty("content").ToString());
            var scopeRibbonEl = issueDoc.QuerySelector(".scope-ribbon");
            string? scopeId = scopeRibbonEl!.GetAttribute("data-scope-id");

            Assert.IsNotNull(scopeId, "ScopeId should exist");

            return (jsonDoc, issueId, scopeId);
        }

        public async Task<(JsonDocument JsonDoc, string issueId, string scopeId)> EditIssue(
            string issueId,
            string title,
            string content,
            Scope scope
       )
        {
            string url = $"/edit-issue?issueId={issueId}";
            // The edit issue GET endpoint returns JSON with an HTML partial in the "content" key.
            string tokenValue = await GetAntiForgeryTokenFromJson(url);

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

            var rootElement = jsonDoc.RootElement;
            var issueDoc = await _env.TextHtmlToDocument(rootElement.GetProperty("content").ToString());
            var scopeRibbonEl = issueDoc.QuerySelector(".scope-ribbon");
            string? scopeId = scopeRibbonEl!.GetAttribute("data-scope-id");
            Assert.IsNotNull(scopeId, "ScopeId should exist");
            return (jsonDoc, issueId, scopeId);
        }


        /// <summary>
        /// Calls This.CreateIssue with proper arguments.
        /// This method is created to readability of test, 
        /// making it clear that a sub issue is being created
        /// </summary>
        /// <param name="parentIssueId"></param>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="scope"></param>
        /// <returns></returns>
        public async Task<(string content, string issueId, string scopeId)> CreateSubIssue(
            string parentIssueId,
            string title,
            string content,
            Scope scope)
        {

            var (jsonDoc1, title1, content1) = await CreateIssue(
              title,
              content,
              new Scope()
              {
                  Scales = { Scale.Global, Scale.National }
              },
              new Guid(parentIssueId));

            var rootElement1 = jsonDoc1.RootElement;
            string issueId = rootElement1.GetProperty("contentId").ToString();
            var issueDoc = await _env.TextHtmlToDocument(rootElement1.GetProperty("content").ToString());
            var scopeRibbonEl = issueDoc.QuerySelector(".scope-ribbon");
            string? scopeId = scopeRibbonEl!.GetAttribute("data-scope-id");
            Assert.IsNotNull(scopeId, "ScopeId should exist");
            return (content, issueId, scopeId);
        }

        public async Task<VoteResponse_AjaxVM> CreateTestVoteOnIssue(string issueId, int voteValue)
        {
            string url = "/issue/vote";
            // Create a payload with vote data
            var votePayload = new
            {
                issueId,
                VoteValue = voteValue
            };

            return await _env.fetchPost<VoteResponse_AjaxVM, object>(url, votePayload);
        }
        public async Task<VoteResponse_AjaxVM> CreateTestVoteOnSolution(string solutionId, int voteValue)
        {
            string url = "/solution/vote";
            // Create a payload with vote data
            var votePayload = new
            {
                solutionId,
                VoteValue = voteValue
            };

            return await _env.fetchPost<VoteResponse_AjaxVM, object>(url, votePayload);
        }


        #endregion

        #region Test Solution CRUD helpers

        public async Task<(JsonDocument JsonDoc, string solutionId, string title, string content, Scope scope)> CreateTestSolution(string parentIssueId)
        {
            string title = "This is a title for a test solution";
            string content = "This is a content for a test solution";
            Scope scope = new Scope()
            {
                Scales = { Scale.Global }
            };
            var (jsonDoc, solutionId, scopeId) = await CreateSolution(title, content, scope, parentIssueId);
            scope.ScopeID = new Guid(scopeId);
            return (jsonDoc, solutionId, title, content, scope);
        }

        public async Task<(JsonDocument JsonDoc, string solutionId, string title, string content, Scope scope)> EditTestSolution(
           string solutionId,
           Guid scopeId,
           string parentIssueId
        )
        {
            string title = "This is an updated title for a test solution";
            string content = "This is an updated content for a test solution";
            Scope scope = new Scope()
            {
                ScopeID = scopeId,
                Scales = { Scale.Global, Scale.National },
                Domains = { Domain.Social }
            };
            var (jsonDoc, _solutionId, _scopeId) = await EditSolution(solutionId, title, content, parentIssueId, scope);
            return (jsonDoc, solutionId, title, content, scope);
        }

        public async Task<(JsonDocument JsonDoc, string solutionId, string scopeId)> CreateSolution(
          string title,
          string content,
          Scope scope,
          string parentIssueId
         )
        {
            // 1. GET page to obtain antiforgery cookie + hidden token
            string tokenValue = await GetAntiForgeryToken("/create-solution");
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
            var rootElement = jsonDoc.RootElement;
            string solutionId = rootElement.GetProperty("contentId").ToString();
            var issueDoc = await _env.TextHtmlToDocument(rootElement.GetProperty("content").ToString());
            var scopeRibbonEl = issueDoc.QuerySelector(".scope-ribbon");
            string? scopeId = scopeRibbonEl!.GetAttribute("data-scope-id");
            return (jsonDoc, solutionId, scopeId);
        }


      
        public async Task<(JsonDocument JsonDoc, string Title, string Content)> EditSolution(
          string solutionId,
          string title,
          string content,
          string parentIssueId,
          Scope scope
         )
        {
            string url = $"/edit-solution?solutionId={solutionId}";
            // The edit issue GET endpoint returns JSON with an HTML partial in the "content" key.
            string tokenValue = await GetAntiForgeryTokenFromJson(url);

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

        #endregion

        #region General Test CRUD helpers
        public void AttachScopeToFormData(Scope scope, List<KeyValuePair<string, string>> formData)
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


        public async Task<string> GetAntiForgeryToken(
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

        #endregion

    }
}
