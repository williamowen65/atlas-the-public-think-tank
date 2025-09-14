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

namespace CloudTests.CacheTests
{

    [TestClass]
    public class Cache_Issue_Tests
    {
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


        #region Cache Issue DTOs

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

        #endregion

        [TestMethod]
        public async Task CacheTesting_AddingNewIssue_CreatesCacheEntryForIssue() 
        {
            var (jsonDoc, issueId, title, content, scope) = await _testingCRUDHelper.CreateTestIssue();

            string url = $"/api/cache-log/keys";
            string expectedCacheKey = $"issue:{issueId}";
            var cacheLog = await _env.fetchJson<List<string>>(url);
            Assert.IsTrue(cacheLog.Contains(expectedCacheKey), $"cache log should contain key {expectedCacheKey}");
        }


        [TestMethod]
        public async Task CacheTesting_UpdatingAnIssue_OverwritesCacheEntryForIssue()
        {
            var (jsonDoc, issueId, title, content, scope) = await _testingCRUDHelper.CreateTestIssue();

            // Reading populates the cache with initial data
            await Read.Issue(new Guid(issueId), new ContentFilter());

            // Updating an issue should update the cache
            var (updatedJsonDoc, _issueId, updatedTitle, updatedContent, updatedScope) = await _testingCRUDHelper.EditTestIssue(issueId, scope.ScopeID);

            // Shouldn't even need to re-read the issue
            await Read.Issue(new Guid(issueId), new ContentFilter());

            string cacheKey = $"issue:{issueId}";
            string url = $"/api/cache-log/entry?key={cacheKey}";

            var cacheEntry = await _env.fetchJson<CacheIssueEntry>(url);

            Assert.IsTrue(cacheEntry.Value.Content == updatedContent);
        }

        [TestMethod]
        public async Task CacheTesting_UpdatingAnIssue_UpdatesIssueVersionHistoryCache()
        {

            var (jsonDoc, issueId, title, content, scope) = await _testingCRUDHelper.CreateTestIssue();

            // Get an Issue_ReadVM object which is used by Read.IssueVersionHistory
            Issue_ReadVM? issue = await Read.Issue(new Guid(issueId), new ContentFilter());
            // Specifically populate the version history cache for the issue by reading
            List<ContentItem_ReadVM> contentItemVersions = await Read.IssueVersionHistory(issue!);

            // Assemble Cache key and fetch data
            string cacheKey = $"issue-version-history:{issueId}";
            string url = $"/api/cache-log/entry?key={cacheKey}";
            var cacheEntry1 = await _env.fetchJson<CacheEntryIssueVersionHistoryDTO>(url);

            // Confirm version history has one entry (The current issue)
            Assert.IsTrue(cacheEntry1.Value.Count() == 1);

            // Update the issue
            var (updatedJsonDoc, _issueId, updatedTitle, updatedContent, updatedScope) = await _testingCRUDHelper.EditTestIssue(issueId, scope.ScopeID);
            
            // Repopulate the version history cache by reading
            List<ContentItem_ReadVM> contentItemVersions2 = await Read.IssueVersionHistory(issue!);

            // Fetch Data
            var cacheEntry2 = await _env.fetchJson<CacheEntryIssueVersionHistoryDTO>(url);
            
            // Confirm that there are 2 entries in version history
            Assert.IsTrue(cacheEntry2.Value.Count() == 2);
        }

        [TestMethod]
        public async Task CacheTesting_PagedSubIssues_ShouldBeUpToDate()
        {
            // Create issue
            var (jsonDoc, parentIssueId, title, content, scope) = await _testingCRUDHelper.CreateTestIssue();
            // Create sub-issue
            var (subIssueJsonDoc, subIssueId, subIssueTitle, subIssueContent, subIssueScope) = await _testingCRUDHelper.CreateTestSubIssue(new Guid(parentIssueId));
           
            // Read this issue to repopulate cache
            Issue_ReadVM? issueVM = await Read.Issue(new Guid(parentIssueId!), new ContentFilter());

            // Assemble cache key for sub-issue-feed-ids
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
            // Create Issue
            var (jsonDoc, parentIssueId, title, content, scope) = await _testingCRUDHelper.CreateTestIssue();
            // Create sub-issue 1
            var (subIssueJsonDoc1, subIssueId1, subIssueTitle1, subIssueContent1, subIssueScope1) = await _testingCRUDHelper.CreateTestSubIssue(new Guid(parentIssueId));

            // Read this issue to repopulate cache
            await Read.Issue(new Guid(parentIssueId!), new ContentFilter());

            // Assemble cache key for sub-issue-content-counts
            ContentFilter filter = new ContentFilter();
            string filterHash = filter.ToJson().GetHashCode().ToString();
            var cacheKey = $"sub-issue-content-counts:{parentIssueId}:{filterHash}";

            // Fetch data
            string url = $"/api/cache-log/entry?key={cacheKey}";
            var cacheEntry = await _env.fetchJson<CacheEntrySubIssueContentCountDTO>(url);

            // Confirm data
            Assert.IsTrue(cacheEntry.Value.AbsoluteCount == 1);
            Assert.IsTrue(cacheEntry.Value.TotalCount == 1);

            // Add another sub-issue
            var (subIssueJsonDoc2, subIssueId2, subIssueTitle2, subIssueContent2, subIssueScope2) = await _testingCRUDHelper.CreateTestSubIssue(new Guid(parentIssueId));
          
            // Read this issue to repopulate cache
            await Read.Issue(new Guid(parentIssueId!), new ContentFilter());

            // Fetch data
            var cacheEntry2 = await _env.fetchJson<CacheEntrySubIssueContentCountDTO>(url);

            // Confirm data
            Assert.IsTrue(cacheEntry2.Value.AbsoluteCount == 2);
            Assert.IsTrue(cacheEntry2.Value.TotalCount == 2);
        }


        [TestMethod]
        public async Task CacheTesting_SubIssues_UpdatingFilter_ContentCounts_ShouldBeUpToDate()
        {
            // Create issue
            var (jsonDoc, parentIssueId, title, content, scope) = await _testingCRUDHelper.CreateTestIssue();
            // Create sub-issue (No votes cast on sub-issue)
            var (subIssueJsonDoc1, subIssueId1, subIssueTitle1, subIssueContent1, subIssueScope1) = await _testingCRUDHelper.CreateTestSubIssue(new Guid(parentIssueId));

            // Read this issue to repopulate cache
            await Read.Issue(new Guid(parentIssueId!), new ContentFilter());

            // Assemble key
            ContentFilter filter = new ContentFilter();
            string filterHash = filter.ToJson().GetHashCode().ToString();
            var cacheKey = $"sub-issue-content-counts:{parentIssueId}:{filterHash}";
            
            // Fetch data
            string url = $"/api/cache-log/entry?key={cacheKey}";
            var cacheEntry = await _env.fetchJson<CacheEntrySubIssueContentCountDTO>(url);

            // Test response
            Assert.IsTrue(cacheEntry.Value.AbsoluteCount == 1);
            Assert.IsTrue(cacheEntry.Value.TotalCount == 1);

            // Update filter  to filter out sub issue 1 (which has no votes)
            ContentFilter updatedFilter = new ContentFilter()
            {
                AvgVoteRange = new RangeFilter<double> { Min = 5, Max = 10 }
            };

            // Read this issue to repopulate cache
            await Read.Issue(new Guid(parentIssueId!), updatedFilter);

            // Assemble new cache key with new filter
            string filterHash2 = updatedFilter.ToJson().GetHashCode().ToString();
            var cacheKey2 = $"sub-issue-content-counts:{parentIssueId}:{filterHash2}";

            // Fetch data
            string url2 = $"/api/cache-log/entry?key={cacheKey2}";
            var cacheEntry2 = await _env.fetchJson<CacheEntrySubIssueContentCountDTO>(url2);

            // Test response
            Assert.IsTrue(cacheEntry2.Value.AbsoluteCount == 1);
            Assert.IsTrue(cacheEntry2.Value.TotalCount == 0);

        }


        [TestMethod]
        public async Task CacheTesting_SubIssueVote_ReordersContent_PagedSubIssues_ShouldBeUpToDate()
        {
            var (jsonDoc, parentIssueId, scopeId, title, content) = await _testingCRUDHelper.CreateTestIssue();

            // Sub issues created with average votes of 0
            var (subIssueJsonDoc1, subIssueId1, subIssueTitle1, subIssueContent1, subIssueScope1) = await _testingCRUDHelper.CreateTestSubIssue(new Guid(parentIssueId));
            var (subIssueJsonDoc2, subIssueId2, subIssueTitle2, subIssueContent2, subIssueScope2) = await _testingCRUDHelper.CreateTestSubIssue(new Guid(parentIssueId));
            var (subIssueJsonDoc3, subIssueId3, subIssueTitle3, subIssueContent3, subIssueScope3) = await _testingCRUDHelper.CreateTestSubIssue(new Guid(parentIssueId));

            ContentFilter filter = new ContentFilter();
            // Read this issue to populate cache
            await Read.Issue(new Guid(parentIssueId!), filter);

            // Cast a vote on sub-issue 2 (This should clear sub-issue-feed-ids for this the parent issue of this sub-issue)
            await _testingCRUDHelper.CreateTestVoteOnIssue(subIssueId2, 10);

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
