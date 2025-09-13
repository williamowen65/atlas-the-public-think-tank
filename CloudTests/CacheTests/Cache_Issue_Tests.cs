using atlas_the_public_think_tank.Data;
using atlas_the_public_think_tank.Data.CRUD;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Issue;
using atlas_the_public_think_tank.Data.DatabaseEntities.Users;
using atlas_the_public_think_tank.Data.RepositoryPattern.IRepository;
using atlas_the_public_think_tank.Data.RepositoryPattern.Repository.Helpers;
using atlas_the_public_think_tank.Data.SeedData.SeedIssues;
using atlas_the_public_think_tank.Data.SeedData.SeedIssues.Data;
using atlas_the_public_think_tank.Data.SeedData.SeedSolutions.Data;
using atlas_the_public_think_tank.Models;
using atlas_the_public_think_tank.Models.ViewModel.AjaxVM;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.ContentItem_Common;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Issue;
using atlas_the_public_think_tank.Models.ViewModel.UI_VM;
using CloudTests.TestingSetup;
using CloudTests.TestingSetup.TestingData;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static CloudTests.CacheTests.Cache_Solution_Tests;

namespace CloudTests.CacheTests
{

    [TestClass]
    public class Cache_Issue_Tests
    {
        private static HttpClient _client;
        private static ApplicationDbContext _db;
        private static TestEnvironment _env;


        [TestInitialize]
        public async Task Setup()
        {
            // Use the utility class to configure the test environment
            bool applySeedData = false;
            _env = new TestEnvironment(applySeedData);
            _db = _env._db;
            _client = _env._client;


            // Create and login user
            AppUser testUser = Users.CreateTestUser1(_db);
            string email = "testuser@example.com";
            string password = "Password123!";

            bool loginSuccess = await Users.LoginUserViaEndpoint(_env, email, password);
            Assert.IsTrue(loginSuccess, "Login should be successful");

        }


        [TestCleanup]
        public async Task TestCleanup()
        {
            await TestingUtilityMethods.deleteDatabase(_client, _db);
        }


        public class CacheIssueEntry
        {
            public string Key { get; set; }
            public string Type { get; set; }
            public IssueRepositoryViewModel Value { get; set; }
        }

        public class CacheEntryIdsDTO
        {
            public string Key { get; set; }
            public List<string> Value { get; set; }
        }

        public class CacheEntrySubIssueContentCountDTO
        { 
            public string Key { get; set; }
            public ContentCount_VM Value { get; set; }
        }

        public class CacheEntryIssueVersionHistoryDTO
        { 
            public string Key { get; set; }
            public List<ContentItem_ReadVM> Value { get; set; }
        }

        public async Task<(string content, string issueId, string scopeId)> CreateTestIssue()
        {
            string content = "This is just an example issue content";

            var (jsonDoc1, title1, content1) = await TestingCRUDHelpers.CreateIssue(_env,
              "This is just an example issue title (content creation)",
              content,
              new Scope()
              {
                  Scales = { Scale.Global, Scale.National }
              });

            var rootElement1 = jsonDoc1.RootElement;
            string issueId = rootElement1.GetProperty("contentId").ToString();
            var issueDoc = await _env.TextHtmlToDocument(rootElement1.GetProperty("content").ToString());
            var scopeRibbonEl = issueDoc.QuerySelector(".scope-ribbon");
            string? scopeId = scopeRibbonEl!.GetAttribute("data-scope-id");

            return (content, issueId, scopeId);
        }

        public async Task<(string content, string issueId, string scopeId)> EditTestIssue(string issueId, string editText)
        {
            string content = "This is just an example issue content";

            var (jsonDoc1, title1, content1) = await TestingCRUDHelpers.EditIssue(_env,
              "This is just an example issue title (content creation) " + editText,
              content,
              issueId,
              new Scope()
              {
                  Scales = { Scale.Global, Scale.National }
              });

            var rootElement1 = jsonDoc1.RootElement;
            var issueDoc = await _env.TextHtmlToDocument(rootElement1.GetProperty("content").ToString());
            var scopeRibbonEl = issueDoc.QuerySelector(".scope-ribbon");
            string? scopeId = scopeRibbonEl!.GetAttribute("data-scope-id");

            return (content, issueId, scopeId);
        }

        public async Task<(string content, string issueId, string scopeId)> CreateTestSubIssue(string parentIssueId, string? extraContent)
        {
            string content = $"This is just an example sub-issue content {(extraContent != null ? extraContent : "")}";

            var (jsonDoc1, title1, content1) = await TestingCRUDHelpers.CreateIssue(_env,
              "This is just an example issue title (content creation)",
              content,
              new Scope()
              {
                  Scales = { Scale.Global, Scale.National }
              },
              new Guid(parentIssueId!));

            var rootElement1 = jsonDoc1.RootElement;
            string issueId = rootElement1.GetProperty("contentId").ToString();
            var issueDoc = await _env.TextHtmlToDocument(rootElement1.GetProperty("content").ToString());
            var scopeRibbonEl = issueDoc.QuerySelector(".scope-ribbon");
            string? scopeId = scopeRibbonEl!.GetAttribute("data-scope-id");

            return (content, issueId, scopeId);
        }

        public async Task CreateTestVoteOnIssue(string issueId, int voteValue)
        {
            string url = "/issue/vote";
            // Create a payload with vote data
            var votePayload = new
            {
                issueId,
                VoteValue = voteValue
            };

            // Send the unauthorized request
            var response = await _env.fetchPost<VoteResponse_AjaxVM, object>(url, votePayload);

        }



        [DataTestMethod]
        [DataRow("keys")]
        [DataRow("entries")]
        public async Task CacheTesting_AddingNewIssue_CreatesCacheEntryForIssue(string path) 
        {

            var (content, issueId, scopeId) = await CreateTestIssue();

            string url = $"/api/cache-log/{path}";
            string expectedCacheKey = $"issue:{issueId}";
            if (path == "keys") { 
                var cacheLog = await _env.fetchJson<List<string>>(url);
                Assert.IsTrue(cacheLog.Contains(expectedCacheKey), $"cache log should contain key {expectedCacheKey}");
            }
            if (path == "entries") {
           
                var cacheLog = await _env.fetchJson<List<CacheIssueEntry>>(url);
                var cacheEntryExist = cacheLog.Any(e => e.Key == expectedCacheKey);
                Assert.IsTrue(cacheEntryExist, $"cache log should contain key {expectedCacheKey}");
                var cacheEntry = cacheLog.FirstOrDefault(e => e.Key == expectedCacheKey);

                Assert.IsTrue(cacheEntry.Value.Content == content);
            }
        }


        [TestMethod]
        public async Task CacheTesting_UpdatingAnIssue_OverwritesCacheEntryForIssue()
        {
            var (content, issueId, scopeId) = await CreateTestIssue();
            
            string contentUpdate = "This is just an example issue content with updated content";
            var (_jsonDoc2, _title2, _content2) = await TestingCRUDHelpers.EditIssue(_env,
             "This is just an example issue title (content creation)",
             contentUpdate,
             issueId,
             new Scope()
             {
                 ScopeID = new Guid(scopeId!),
                 Scales = { Scale.Global, Scale.National }
             });


            string url = $"/api/cache-log/entries";
            string expectedCacheKey = $"issue:{issueId}";

            var cacheLog = await _env.fetchJson<List<CacheIssueEntry>>(url);
            var cacheEntryExist = cacheLog.Any(e => e.Key == expectedCacheKey);
            Assert.IsTrue(cacheEntryExist, $"cache log should contain key {expectedCacheKey}");
            var cacheEntry = cacheLog.FirstOrDefault(e => e.Key == expectedCacheKey);

            Assert.IsTrue(cacheEntry.Value.Content == contentUpdate);
        }

        [TestMethod]
        public async Task CacheTesting_UpdatingAnIssue_UpdatesIssueVersionHistoryCache()
        {
            var (content, issueId, scopeId) = await CreateTestIssue();

            Issue_ReadVM? issue = await Read.Issue(new Guid(issueId), new ContentFilter());
            List<ContentItem_ReadVM> contentItemVersions = await Read.IssueVersionHistory(issue!);

            string cacheKey = $"issue-version-history:{issueId}";
            string url = $"/api/cache-log/entry?key={cacheKey}";

            var cacheEntry1 = await _env.fetchJson<CacheEntryIssueVersionHistoryDTO>(url);

            Assert.IsTrue(cacheEntry1.Value.Count() == 1);

            string contentUpdate = "This is just an example issue content with updated content";
            var (_jsonDoc2, _title2, _content2) = await TestingCRUDHelpers.EditIssue(_env,
             "This is just an example issue title (content creation)",
             contentUpdate,
             issueId,
             new Scope()
             {
                 ScopeID = new Guid(scopeId!),
                 Scales = { Scale.Global, Scale.National }
             });

            List<ContentItem_ReadVM> contentItemVersions2 = await Read.IssueVersionHistory(issue!);

            var cacheEntry2 = await _env.fetchJson<CacheEntryIssueVersionHistoryDTO>(url);

            Assert.IsTrue(cacheEntry2.Value.Count() == 2);
        }

        [TestMethod]
        public async Task CacheTesting_PagedSubIssues_ShouldBeUpToDate()
        {
            var (issueContent, parentIssueId, issueScopeId) = await CreateTestIssue();

            var(content, subIssueId, scopeId) =  await CreateTestSubIssue(parentIssueId, "sub-issue");
            
            // Read this issue to repopulate cache
            Issue_ReadVM issueVM = await Read.Issue(new Guid(parentIssueId!), new ContentFilter());

            ContentFilter filter = new ContentFilter();
            string filterHash = filter.ToJson().GetHashCode().ToString();
            int pageNumber = 1;

            var cacheKey = $"sub-issue-feed-ids:{parentIssueId}:{filterHash}:{pageNumber}"; ;
            string url = $"/api/cache-log/entry?key={cacheKey}";
            var cacheEntry = await _env.fetchJson<CacheEntryIdsDTO>(url);


            Assert.IsTrue(cacheEntry.Value.Count() == 1);
        }

        [TestMethod]
        public async Task CacheTesting_SubIssues_ContentCounts_ShouldBeUpToDate()
        {
            var (issueContent, parentIssueId, issueScopeId) = await CreateTestIssue();

            var(content, subIssueId, scopeId) =  await CreateTestSubIssue(parentIssueId, "Sub-Issue 1");
            
            // Read this issue to repopulate cache
            await Read.Issue(new Guid(parentIssueId!), new ContentFilter());

            ContentFilter filter = new ContentFilter();
            string filterHash = filter.ToJson().GetHashCode().ToString();
            int pageNumber = 1;

            var cacheKey = $"sub-issue-content-counts:{parentIssueId}:{filterHash}"; 
            string url = $"/api/cache-log/entry?key={cacheKey}";
            var cacheEntry = await _env.fetchJson<CacheEntrySubIssueContentCountDTO>(url);


            Assert.IsTrue(cacheEntry.Value.AbsoluteCount == 1);
            Assert.IsTrue(cacheEntry.Value.TotalCount == 1);

            var (content2, subIssueId2, scopeId2) = await CreateTestSubIssue(parentIssueId, "Sub-Issue 2");
            // Read this issue to repopulate cache
            await Read.Issue(new Guid(parentIssueId!), new ContentFilter());

            var cacheEntry2 = await _env.fetchJson<CacheEntrySubIssueContentCountDTO>(url);

            Assert.IsTrue(cacheEntry2.Value.AbsoluteCount == 2);
            Assert.IsTrue(cacheEntry2.Value.TotalCount == 2);
        }


        [TestMethod]
        public async Task CacheTesting_SubIssues_UpdatingFilter_ContentCounts_ShouldBeUpToDate()
        {
            var (issueContent, parentIssueId, issueScopeId) = await CreateTestIssue();

            var (content, subIssueId, scopeId) = await CreateTestSubIssue(parentIssueId, "Sub-Issue 1 - This issue has no votes - average of 0");

            // Read this issue to repopulate cache
            await Read.Issue(new Guid(parentIssueId!), new ContentFilter());

            ContentFilter filter = new ContentFilter();
            string filterHash = filter.ToJson().GetHashCode().ToString();

            var cacheKey = $"sub-issue-content-counts:{parentIssueId}:{filterHash}";
            string url = $"/api/cache-log/entry?key={cacheKey}";
            var cacheEntry = await _env.fetchJson<CacheEntrySubIssueContentCountDTO>(url);


            Assert.IsTrue(cacheEntry.Value.AbsoluteCount == 1);
            Assert.IsTrue(cacheEntry.Value.TotalCount == 1);

            // Filter out sub issue 1
            ContentFilter updatedFilter = new ContentFilter()
            {
                AvgVoteRange = new RangeFilter<double> { Min = 5, Max = 10 }
            };

            // Read this issue to repopulate cache
            await Read.Issue(new Guid(parentIssueId!), updatedFilter);

            string filterHash2 = updatedFilter.ToJson().GetHashCode().ToString();
            var cacheKey2 = $"sub-issue-content-counts:{parentIssueId}:{filterHash2}";
            string url2 = $"/api/cache-log/entry?key={cacheKey2}";
            var cacheEntry2 = await _env.fetchJson<CacheEntrySubIssueContentCountDTO>(url2);


            Assert.IsTrue(cacheEntry2.Value.AbsoluteCount == 1);
            Assert.IsTrue(cacheEntry2.Value.TotalCount == 0);

        }


        [TestMethod]
        public async Task CacheTesting_SubIssueVote_ReordersContent_PagedSubIssues_ShouldBeUpToDate()
        {
            var (issueContent, parentIssueId, issueScopeId) = await CreateTestIssue();

            // Sub issues created with average votes of 0
            var (content1, subIssueId1, scopeId1) = await CreateTestSubIssue(parentIssueId, "Sub-Issue 1");
            var (content2, subIssueId2, scopeId2) = await CreateTestSubIssue(parentIssueId, "Sub-Issue 2");
            var (content3, subIssueId3, scopeId3) = await CreateTestSubIssue(parentIssueId, "Sub-Issue 3");

            ContentFilter filter = new ContentFilter();
            // Read this issue to populate cache
            await Read.Issue(new Guid(parentIssueId!), filter);

            // Cast a vote on sub-issue 2
            await CreateTestVoteOnIssue(subIssueId2, 10);

            // Repopulate cache
            await Read.Issue(new Guid(parentIssueId!), filter);

            string filterHash = filter.ToJson().GetHashCode().ToString();
            int pageNumber = 1;

            var cacheKey = $"sub-issue-feed-ids:{parentIssueId}:{filterHash}:{pageNumber}"; ;
            string url = $"/api/cache-log/entry?key={cacheKey}";
            var cacheEntry = await _env.fetchJson<CacheEntryIdsDTO>(url);

            Assert.IsTrue(cacheEntry.Value[0] == subIssueId2);

        }


    }
}
