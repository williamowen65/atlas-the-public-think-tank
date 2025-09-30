using atlas_the_public_think_tank.Data.CRUD;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Issue;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Solution;
using atlas_the_public_think_tank.Data.DatabaseEntities.Users;
using atlas_the_public_think_tank.Data.DbContext;
using atlas_the_public_think_tank.Data.RepositoryPattern.Cache.Helpers;
using atlas_the_public_think_tank.Data.RepositoryPattern.IRepository;
using atlas_the_public_think_tank.Data.RepositoryPattern.Repository.Helpers;
using atlas_the_public_think_tank.Data.SeedData.SeedIssues;
using atlas_the_public_think_tank.Data.SeedData.SeedIssues.Data;
using atlas_the_public_think_tank.Data.SeedData.SeedSolutions.Data;
using atlas_the_public_think_tank.Models;
using atlas_the_public_think_tank.Models.Enums;
using atlas_the_public_think_tank.Models.ViewModel.AjaxVM;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.ContentItem_Common;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Issue;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Issue.IssueVote;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Solution.SolutionVote;
using atlas_the_public_think_tank.Models.ViewModel.UI_VM;
using CloudTests.TestingSetup;
using CloudTests.TestingSetup.TestingData;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
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
            


            var (user, password) = Users.GetRandomAppUser();
            AppUser testUser = Users.CreateTestUser(_db, user, password);
            bool loginSuccess = await Users.LoginUserViaEndpoint(_env, user.Email!, password);
            Assert.IsTrue(loginSuccess, "Login should be successful");

            Console.WriteLine($"\nusing Db {_env._connectionString}\n");

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
        public class CacheEntryIdsHomePageDTO
        {
            public string Key { get; set; }
            public List<ContentIdentifier> Value { get; set; }
        }
        public class CacheEntry_IssueVoteStatsDTO
        {
            public string Key { get; set; }
            public IssueVotes_Cacheable_ReadVM Value { get; set; }
        }
        public class CacheEntry_SolutionVoteStatsDTO
        {
            public string Key { get; set; }
            public SolutionVotes_Cacheable_ReadVM Value { get; set; }
        }

        public class CacheEntry_ContentCountDTO
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

        #region Testing creating a root issue

        [TestMethod]
        public async Task CacheTestingIssue_AddingNewIssue_CreatesCacheEntryForIssue() 
        {
            var (jsonDoc, issueId, title, content, scope) = await _testingCRUDHelper.CreateTestIssue(ContentStatus.Published);

            string url = $"/api/cache-log/keys";
            string expectedCacheKey = $"{CacheKeyPrefix.Issue}:{issueId}";
            var cacheLog = await _env.fetchJson<List<string>>(url);
            Assert.IsTrue(cacheLog.Contains(expectedCacheKey), $"cache log should contain key {expectedCacheKey}");
        }

        #endregion

        #region Testing editing an issue


        [TestMethod]
        public async Task CacheTestingIssue_UpdatingAnIssue_OverwritesCacheEntryForIssue()
        {
            var (jsonDoc, issueId, title, content, scope) = await _testingCRUDHelper.CreateTestIssue(ContentStatus.Published);

            // Reading populates the cache with initial data
            await _read.Issue(new Guid(issueId), new ContentFilter());

            // Updating an issue should update the cache
            var (updatedJsonDoc, _issueId, updatedTitle, updatedContent, updatedScope) = await _testingCRUDHelper.EditTestIssue(issueId, scope.ScopeID, ContentStatus.Published);

            // Shouldn't even need to re-read the issue
            await _read.Issue(new Guid(issueId), new ContentFilter());

            string cacheKey = $"{CacheKeyPrefix.Issue}:{issueId}";
            string url = $"/api/cache-log/entry?key={cacheKey}";

            var cacheEntry = await _env.fetchJson<CacheIssueEntry>(url);

            Assert.IsTrue(cacheEntry.Value.Content == updatedContent);
        }

        [TestMethod]
        public async Task CacheTestingIssue_UpdatingAnIssue_UpdatesIssueVersionHistoryCache()
        {

            var (jsonDoc, issueId, title, content, scope) = await _testingCRUDHelper.CreateTestIssue(ContentStatus.Published);

            // Get an Issue_ReadVM object which is used by Read.IssueVersionHistory
            Issue_ReadVM? issue = await _read.Issue(new Guid(issueId), new ContentFilter());
            // Specifically populate the version history cache for the issue by reading
            List<ContentItem_ReadVM> contentItemVersions = await _read.IssueVersionHistory(issue!);

            // Assemble Cache key and fetch data
            string cacheKey = $"{CacheKeyPrefix.IssueVersionHistory}:{issueId}";
            string url = $"/api/cache-log/entry?key={cacheKey}";
            var cacheEntry1 = await _env.fetchJson<CacheEntryIssueVersionHistoryDTO>(url);

            // Confirm version history has one entry (The current issue)
            Assert.IsTrue(cacheEntry1.Value.Count() == 1);

            // Update the issue
            var (updatedJsonDoc, _issueId, updatedTitle, updatedContent, updatedScope) = await _testingCRUDHelper.EditTestIssue(issueId, scope.ScopeID, ContentStatus.Published);
            
            // Repopulate the version history cache by reading
            List<ContentItem_ReadVM> contentItemVersions2 = await _read.IssueVersionHistory(issue!);

            // Fetch Data
            var cacheEntry2 = await _env.fetchJson<CacheEntryIssueVersionHistoryDTO>(url);
            
            // Confirm that there are 2 entries in version history
            Assert.IsTrue(cacheEntry2.Value.Count() == 2);
        }

        #endregion

        #region Testing vote on issue

        [TestMethod]
        public async Task CacheTestingIssue_VotingOnIssue_UpdatesVoteStats()
        {
            // Create issue
            var (jsonDoc, issueId, title, content, scope) = await _testingCRUDHelper.CreateTestIssue(ContentStatus.Published);
            var cacheKey = $"{CacheKeyPrefix.VoteStats}:{issueId}";

            string url = $"/api/cache-log/entry?key={cacheKey}";
            
            var cacheEntry0 = await _env.fetchJson<CacheEntry_IssueVoteStatsDTO>(url);
            IssueVotes_Cacheable_ReadVM voteStats0 = cacheEntry0.Value;

            Assert.IsTrue(voteStats0.TotalVotes == 0);
            Assert.IsTrue(voteStats0.IssueVotes.Count == 0);

            // Updating a vote should update through the cache (No need to call Read.Issue to populate the cache)
            VoteResponse_AjaxVM voteResponse1 = await _testingCRUDHelper.CreateTestVoteOnIssue(issueId, 10);

            var cacheEntry1 = await _env.fetchJson<CacheEntry_IssueVoteStatsDTO>(url);
            IssueVotes_Cacheable_ReadVM voteStats1 = cacheEntry1.Value;

            Assert.IsTrue(voteStats1.TotalVotes == 1);
            Assert.IsTrue(voteStats1.IssueVotes.Count == 1);
            Assert.IsTrue(voteStats1.IssueVotes.Values.Average(v => v.VoteValue) == voteResponse1.Average);

            // Updating a vote should update through the cache (No need to call Read.Issue to populate the cache)
            VoteResponse_AjaxVM voteResponse2 = await _testingCRUDHelper.CreateTestVoteOnIssue(issueId, 6);

            var cacheEntry2 = await _env.fetchJson<CacheEntry_IssueVoteStatsDTO>(url);
            IssueVotes_Cacheable_ReadVM voteStats2 = cacheEntry2.Value;

            ///NOTE: The Total votes and count stay the same because the logged in user has not changed
            Assert.IsTrue(voteStats2.TotalVotes == 1);
            Assert.IsTrue(voteStats2.IssueVotes.Count == 1);
            Assert.IsTrue(voteStats2.IssueVotes.Values.Average(v => v.VoteValue) == voteResponse2.Average);

            //// Create and login user
            var (user, password) = Users.GetRandomAppUser();
            AppUser testUser = Users.CreateTestUser(_db, user, password);

            bool loginSuccess = await Users.LoginUserViaEndpoint(_env, user.Email!, password);
            Assert.IsTrue(loginSuccess, "Login should be successful");

            // Updating a vote should update through the cache (No need to call Read.Issue to populate the cache)
            VoteResponse_AjaxVM voteResponse3 = await _testingCRUDHelper.CreateTestVoteOnIssue(issueId, 7);

            var cacheEntry3 = await _env.fetchJson<CacheEntry_IssueVoteStatsDTO>(url);
            IssueVotes_Cacheable_ReadVM voteStats3 = cacheEntry3.Value;

            ///NOTE: The Total votes and count are updated because a new user has voted
            Assert.IsTrue(voteStats3.TotalVotes == 2);
            Assert.IsTrue(voteStats3.IssueVotes.Count == 2);
            Assert.IsTrue(voteStats3.IssueVotes.Values.Average(v => v.VoteValue) == voteResponse3.Average);

        }

        #endregion

        #region Testing Issue sub-issues

        [TestMethod]
        public async Task CacheTestingIssue_PagedSubIssues_ShouldBeUpToDate()
        {
            // Create issue
            var (jsonDoc, parentIssueId, title, content, scope) = await _testingCRUDHelper.CreateTestIssue(ContentStatus.Published);

            // Create sub-issue
            var (subIssueJsonDoc, subIssueId, subIssueTitle, subIssueContent, subIssueScope) = await _testingCRUDHelper.CreateTestSubIssue(ContentStatus.Published, new Guid(parentIssueId));
           
            // Read this issue to repopulate cache
            Issue_ReadVM? issueVM = await _read.Issue(new Guid(parentIssueId!), new ContentFilter());

            // Assemble cache key for CacheHelper.CacheKeysPrefix.FeedIds.SubIssueOfIssue
            ContentFilter filter = new ContentFilter();
            string filterCacheString = filter.ToCacheString();
            int pageNumber = 1;
            var cacheKey = $"{CacheKeyPrefix.SubIssueOfIssueOrSolutionFeedIds}:{parentIssueId}:{filterCacheString}:page({pageNumber})"; ;
            string encodedCacheKey = Uri.EscapeDataString(cacheKey);
            string url = $"/api/cache-log/entry?key={encodedCacheKey}";
            var cacheEntry = await _env.fetchJson<CacheEntryIdsDTO>(url);

            Assert.IsTrue(cacheEntry.Value.Count() == 1);
        }

        [TestMethod]
        public async Task CacheTestingIssue_SubIssues_ContentCounts_ShouldBeUpToDate()
        {
            // Create Issue
            var (jsonDoc, parentIssueId, title, content, scope) = await _testingCRUDHelper.CreateTestIssue(ContentStatus.Published);
            // Create sub-issue 1
            var (subIssueJsonDoc1, subIssueId1, subIssueTitle1, subIssueContent1, subIssueScope1) = await _testingCRUDHelper.CreateTestSubIssue(ContentStatus.Published, new Guid(parentIssueId));

            // Read this issue to repopulate cache
            await _read.Issue(new Guid(parentIssueId!), new ContentFilter());

            // Assemble cache key for CacheKeyPrefix.SubIssueForIssueContentCount
            ContentFilter filter = new ContentFilter();
            string filterCacheString = filter.ToCacheString();
            var cacheKey = $"{CacheKeyPrefix.SubIssueForIssueContentCount}:{parentIssueId}:{filterCacheString}";
            string encodedCacheKey = Uri.EscapeDataString(cacheKey);

            // Fetch data
            string url = $"/api/cache-log/entry?key={encodedCacheKey}";
            var cacheEntry = await _env.fetchJson<CacheEntry_ContentCountDTO>(url);

            // Confirm data
            Assert.IsTrue(cacheEntry.Value.AbsoluteCount == 1);
            Assert.IsTrue(cacheEntry.Value.FilteredCount == 1);

            // Add another sub-issue
            var (subIssueJsonDoc2, subIssueId2, subIssueTitle2, subIssueContent2, subIssueScope2) = await _testingCRUDHelper.CreateTestSubIssue(ContentStatus.Published, new Guid(parentIssueId));
          
            // Read this issue to repopulate cache
            await _read.Issue(new Guid(parentIssueId!), new ContentFilter());

            // Fetch data
            var cacheEntry2 = await _env.fetchJson<CacheEntry_ContentCountDTO>(url);

            // Confirm data
            Assert.IsTrue(cacheEntry2.Value.AbsoluteCount == 2);
            Assert.IsTrue(cacheEntry2.Value.FilteredCount == 2);
        }


        [TestMethod]
        public async Task CacheTestingIssue_SubIssues_UpdatingFilter_ContentCounts_ShouldBeUpToDate()
        {
            // Create issue
            var (jsonDoc, parentIssueId, title, content, scope) = await _testingCRUDHelper.CreateTestIssue(ContentStatus.Published);
            // Create sub-issue (No votes cast on sub-issue)
            var (subIssueJsonDoc1, subIssueId1, subIssueTitle1, subIssueContent1, subIssueScope1) = await _testingCRUDHelper.CreateTestSubIssue(ContentStatus.Published, new Guid(parentIssueId));

            // Read this issue to repopulate cache
            await _read.Issue(new Guid(parentIssueId!), new ContentFilter());

            // Assemble key
            ContentFilter filter = new ContentFilter();
            string filterCacheString = filter.ToCacheString();
            var cacheKey = $"{CacheKeyPrefix.SubIssueForIssueContentCount}:{parentIssueId}:{filterCacheString}";
            string encodedCacheKey = Uri.EscapeDataString(cacheKey);

            // Fetch data
            string url = $"/api/cache-log/entry?key={encodedCacheKey}";
            var cacheEntry = await _env.fetchJson<CacheEntry_ContentCountDTO>(url);

            // Test response
            Assert.IsTrue(cacheEntry.Value.AbsoluteCount == 1);
            Assert.IsTrue(cacheEntry.Value.FilteredCount == 1);

            // Update filter  to filter out sub issue 1 (which has no votes)
            ContentFilter updatedFilter = new ContentFilter()
            {
                AvgVoteRange = new RangeFilter<double> { Min = 5, Max = 10 }
            };

            // Read this issue to repopulate cache
            await _read.Issue(new Guid(parentIssueId!), updatedFilter);

            // Assemble new cache key with new filter
            string filterCacheString2 = updatedFilter.ToCacheString();
            var cacheKey2 = $"{CacheKeyPrefix.SubIssueForIssueContentCount}:{parentIssueId}:{filterCacheString2}";
            string encodedCacheKey2 = Uri.EscapeDataString(cacheKey2);
            // Fetch data
            string url2 = $"/api/cache-log/entry?key={encodedCacheKey2}";
            var cacheEntry2 = await _env.fetchJson<CacheEntry_ContentCountDTO>(url2);

            // Test response
            Assert.IsTrue(cacheEntry2.Value.AbsoluteCount == 1);
            Assert.IsTrue(cacheEntry2.Value.FilteredCount == 0); // <-- this is the filtered count

        }


        [TestMethod]
        public async Task CacheTestingIssue_SubIssueVote_ReordersContent_PagedSubIssues_ShouldBeUpToDate()
        {
            var (jsonDoc, parentIssueId, scopeId, title, content) = await _testingCRUDHelper.CreateTestIssue(ContentStatus.Published);

            // Sub issues created with average votes of 0
            var (subIssueJsonDoc1, subIssueId1, subIssueTitle1, subIssueContent1, subIssueScope1) = await _testingCRUDHelper.CreateTestSubIssue(ContentStatus.Published, new Guid(parentIssueId));
            var (subIssueJsonDoc2, subIssueId2, subIssueTitle2, subIssueContent2, subIssueScope2) = await _testingCRUDHelper.CreateTestSubIssue(ContentStatus.Published, new Guid(parentIssueId));
            var (subIssueJsonDoc3, subIssueId3, subIssueTitle3, subIssueContent3, subIssueScope3) = await _testingCRUDHelper.CreateTestSubIssue(ContentStatus.Published, new Guid(parentIssueId));

            ContentFilter filter = new ContentFilter();
            // Read this issue to populate cache
            await _read.Issue(new Guid(parentIssueId!), filter);

            // Cast a vote on sub-issue 2 (This should clear CacheHelper.CacheKeysPrefix.FeedIds.SubIssueOfIssue for this the parent issue of this sub-issue)
            await _testingCRUDHelper.CreateTestVoteOnIssue(subIssueId2, 10);

            // Repopulate cache
            await _read.Issue(new Guid(parentIssueId!), filter);

            string filterCacheString = filter.ToCacheString();
            int pageNumber = 1;
            var cacheKey = $"{CacheKeyPrefix.SubIssueOfIssueOrSolutionFeedIds}:{parentIssueId}:{filterCacheString}:page({pageNumber})";
            string encodedCacheKey = Uri.EscapeDataString(cacheKey);
            string url = $"/api/cache-log/entry?key={encodedCacheKey}";
            var cacheEntry = await _env.fetchJson<CacheEntryIdsDTO>(url);

            Assert.IsTrue(cacheEntry.Value[0] == subIssueId2);

        }

        #endregion

        #region Testing Issue solutions

        [TestMethod]
        public async Task CacheTestingIssue_PagedSolutions_ShouldBeUpToDate()
        {
            // Create issue
            var (jsonDoc, parentIssueId, title, content, scope) = await _testingCRUDHelper.CreateTestIssue(ContentStatus.Published);
            // Create solution
            var (solutionJsonDoc, solutionId, solutionTitle, solutionContent, solutionScope) = await _testingCRUDHelper.CreateTestSolution(parentIssueId, ContentStatus.Published);

            // Read this issue to repopulate cache
            Issue_ReadVM? issueVM = await _read.Issue(new Guid(parentIssueId!), new ContentFilter());

            // Assemble cache key for CacheKeyPrefix.SolutionsOfIssueFeedIds
            ContentFilter filter = new ContentFilter();
            string filterCacheString = filter.ToCacheString();
            int pageNumber = 1;
            var cacheKey = $"{CacheKeyPrefix.SolutionsOfIssueFeedIds}:{parentIssueId}:{filterCacheString}:page({pageNumber})";
            string encodedCacheKey = Uri.EscapeDataString(cacheKey);
            string url = $"/api/cache-log/entry?key={encodedCacheKey}";
            var cacheEntry = await _env.fetchJson<CacheEntryIdsDTO>(url);

            Assert.IsTrue(cacheEntry.Value.Count() == 1);
        }

        [TestMethod]
        public async Task CacheTestingIssue_SolutionFeed_ContentCounts_ShouldBeUpToDate()
        {
            // Create Issue
            var (jsonDoc, parentIssueId, title, content, scope) = await _testingCRUDHelper.CreateTestIssue(ContentStatus.Published);
            // Create solution 1
            var (solutionJsonDoc1, solutionId1, solutionTitle1, solutionContent1, solutionScope1) = await _testingCRUDHelper.CreateTestSolution(parentIssueId, ContentStatus.Published);

            // Read this issue to repopulate cache
            await _read.Issue(new Guid(parentIssueId!), new ContentFilter());

            // Assemble cache key for CacheKeyPrefix.SolutionForIssueContentCount
            ContentFilter filter = new ContentFilter();
            string filterCacheString = filter.ToCacheString();
            var cacheKey = $"{CacheKeyPrefix.SolutionForIssueContentCount}:{parentIssueId}:{filterCacheString}";
            string encodedCacheKey = Uri.EscapeDataString(cacheKey);

            // Fetch data
            string url = $"/api/cache-log/entry?key={encodedCacheKey}";
            var cacheEntry = await _env.fetchJson<CacheEntry_ContentCountDTO>(url);

            // Confirm data
            Assert.IsTrue(cacheEntry.Value.AbsoluteCount == 1);
            Assert.IsTrue(cacheEntry.Value.FilteredCount == 1);

            // Add another solution
            var (solutionJsonDoc2, solutionId2, solutionTitle2, solutionContent2, solutionScope2) = await _testingCRUDHelper.CreateTestSolution(parentIssueId, ContentStatus.Published);

            // Read this issue to repopulate cache
            await _read.Issue(new Guid(parentIssueId!), new ContentFilter());

            // Fetch data
            var cacheEntry2 = await _env.fetchJson<CacheEntry_ContentCountDTO>(url);

            // Confirm data
            Assert.IsTrue(cacheEntry2.Value.AbsoluteCount == 2);
            Assert.IsTrue(cacheEntry2.Value.FilteredCount == 2);
        }

        [TestMethod]
        public async Task CacheTestingIssue_SolutionFeed_UpdatingFilter_ContentCounts_ShouldBeUpToDate()
        {
            // Create issue
            var (jsonDoc, parentIssueId, title, content, scope) = await _testingCRUDHelper.CreateTestIssue(ContentStatus.Published);
            // Create sub-issue (No votes cast on sub-issue)
            var (subIssueJsonDoc1, subIssueId1, subIssueTitle1, subIssueContent1, subIssueScope1) = await _testingCRUDHelper.CreateTestSolution(parentIssueId, ContentStatus.Published);

            // Read this issue to repopulate cache
            await _read.Issue(new Guid(parentIssueId!), new ContentFilter());

            // Assemble key
            ContentFilter filter = new ContentFilter();
            string filterCacheString = filter.ToCacheString();
            var cacheKey = $"{CacheKeyPrefix.SolutionForIssueContentCount}:{parentIssueId}:{filterCacheString}";
            string encodedCacheKey = Uri.EscapeDataString(cacheKey);
            // Fetch data
            string url = $"/api/cache-log/entry?key={encodedCacheKey}";
            var cacheEntry = await _env.fetchJson<CacheEntry_ContentCountDTO>(url);

            // Test response
            Assert.IsTrue(cacheEntry.Value.AbsoluteCount == 1);
            Assert.IsTrue(cacheEntry.Value.FilteredCount == 1);

            // Update filter  to filter out sub issue 1 (which has no votes)
            ContentFilter updatedFilter = new ContentFilter()
            {
                AvgVoteRange = new RangeFilter<double> { Min = 5, Max = 10 }
            };

            // Read this issue to repopulate cache
            await _read.Issue(new Guid(parentIssueId!), updatedFilter);

            // Assemble new cache key with new filter
            string filterCacheString2 = updatedFilter.ToCacheString();
            var cacheKey2 = $"{CacheKeyPrefix.SolutionForIssueContentCount}:{parentIssueId}:{filterCacheString2}";
            string encodedCacheKey2 = Uri.EscapeDataString(cacheKey2);
            // Fetch data
            string url2 = $"/api/cache-log/entry?key={encodedCacheKey2}";
            var cacheEntry2 = await _env.fetchJson<CacheEntry_ContentCountDTO>(url2);

            // Test response
            Assert.IsTrue(cacheEntry2.Value.AbsoluteCount == 1);
            Assert.IsTrue(cacheEntry2.Value.FilteredCount == 0);
        }

        [TestMethod]
        public async Task CacheTestingIssue_SolutionFeed_SolutionVote_ReordersContent_PagedSolutions_ShouldBeUpToDate()
        {
            var (jsonDoc, parentIssueId, scopeId, title, content) = await _testingCRUDHelper.CreateTestIssue(ContentStatus.Published);

            // solution created with average votes of 0
            var (solutionJsonDoc1, solutionId1, solutionTitle1, solutionContent1, solutionScope1) = await _testingCRUDHelper.CreateTestSolution(parentIssueId, ContentStatus.Published);
            var (solutionJsonDoc2, solutionId2, solutionTitle2, solutionContent2, solutionScope2) = await _testingCRUDHelper.CreateTestSolution(parentIssueId, ContentStatus.Published);
            var (solutionJsonDoc3, solutionId3, solutionTitle3, solutionContent3, solutionScope3) = await _testingCRUDHelper.CreateTestSolution(parentIssueId, ContentStatus.Published);

            ContentFilter filter = new ContentFilter();
            // Read this issue to populate cache
            await _read.Issue(new Guid(parentIssueId!), filter);

            // Cast a vote on sub-issue 2 (This should clear CacheHelper.CacheKeysPrefix.FeedIds.SubIssueOfIssue for this the parent issue of this sub-issue)
            await _testingCRUDHelper.CreateTestVoteOnSolution(solutionId2, 10);

            // Repopulate cache
            await _read.Issue(new Guid(parentIssueId!), filter);

            string filterCacheString = filter.ToCacheString();
            int pageNumber = 1;

            var cacheKey = $"{CacheKeyPrefix.SolutionsOfIssueFeedIds}:{parentIssueId}:{filterCacheString}:page({pageNumber})"; ;
            string encodedCacheKey = Uri.EscapeDataString(cacheKey);
            string url = $"/api/cache-log/entry?key={encodedCacheKey}";
            var cacheEntry = await _env.fetchJson<CacheEntryIdsDTO>(url);

            Assert.IsTrue(cacheEntry.Value[0] == solutionId2);
        }



        #endregion
    }
}
