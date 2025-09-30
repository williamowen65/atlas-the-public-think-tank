using atlas_the_public_think_tank.Data.CRUD;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;
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
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Solution;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Solution.SolutionVote;
using CloudTests.TestingSetup;
using CloudTests.TestingSetup.TestingData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static CloudTests.CacheTests.Cache_Issue_Tests;


namespace CloudTests.CacheTests
{

    [TestClass]
    public class Cache_Solution_Tests
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

            // Create and login user
            var (user, password) = Users.GetRandomAppUser();
            AppUser testUser = Users.CreateTestUser(_db, user, password);
            bool loginSuccess = await Users.LoginUserViaEndpoint(_env, user.Email!, password);
            Assert.IsTrue(loginSuccess, "Login should be successful");

        }





        [TestCleanup]
        public async Task TestCleanup()
        {
            await TestingUtilityMethods.deleteDatabase(_client, _db);
        }

        #region Solution Cache DTOs

        public class CacheSolutionEntry
        {
            public string Key { get; set; }
            public string Type { get; set; }
            public SolutionRepositoryViewModel Value { get; set; }
        }

        #endregion

        #region Testing creating solution

        [TestMethod]
        public async Task CacheTestingSolution_AddingNewSolution_CreatesCacheKeyForSolution() 
        {
            // Create Issue
            var (issueJsonDoc, parentIssueId, issuetitle, issueContent, issueScope) = await _testingCRUDHelper.CreateTestIssue(ContentStatus.Published);
            // Create Solution
            var (solutionJsonDoc, solutionId, solutionTitle, solutionContent, solutionScope) = await _testingCRUDHelper.CreateTestSolution(parentIssueId, ContentStatus.Published);
            // Create Cache Key
            string expectedCacheKey = $"{CacheKeyPrefix.Solution}:{solutionId}";
            // Fetch data
            string url = $"/api/cache-log/keys";
            var cacheLog = await _env.fetchJson<List<string>>(url);
            // Confirm data
            Assert.IsTrue(cacheLog.Contains(expectedCacheKey), $"cache log should contain key {expectedCacheKey}");
        }


        [TestMethod]
        public async Task CacheTestingSolution_AddingNewSolution_CreatesCacheEntryForSolution()
        {
            // Create Issue
            var (issueJsonDoc, parentIssueId, issuetitle, issueContent, issueScope) = await _testingCRUDHelper.CreateTestIssue(ContentStatus.Published);
           // Create solution
            var (solutionJsonDoc, solutionId, solutionTitle, solutionContent, solutionScope) = await _testingCRUDHelper.CreateTestSolution(parentIssueId, ContentStatus.Published);

            string url = $"/api/cache-log/entry?key=solution:{solutionId}";
            var cacheEntry = await _env.fetchJson<CacheSolutionEntry>(url);
            Assert.IsTrue(cacheEntry.Value.Content == solutionContent);
        }

        #endregion

        #region Testing editing a solution

        [TestMethod]
        public async Task CacheTestingSolution_UpdatingSolution_CreatesAndOverwritesCacheEntryForSolution()
        {
            // Create Issue
            var (issueJsonDoc, parentIssueId, issuetitle, issueContent, issueScope) = await _testingCRUDHelper.CreateTestIssue(ContentStatus.Published);
            // Create solution
            var (solutionJsonDoc, solutionId, solutionTitle, solutionContent, solutionScope) = await _testingCRUDHelper.CreateTestSolution(parentIssueId, ContentStatus.Published);
            // Edit Solution
            var (updatedSolutionJsonDoc, updatedSolutionId, updatedSolutionTitle, updatedSolutionContent, updatedSolutionScope) = await _testingCRUDHelper.EditTestSolution(solutionId, solutionScope.ScopeID, parentIssueId, ContentStatus.Published);

            string url = $"/api/cache-log/entry?key=solution:{solutionId}";
            var cacheEntry = await _env.fetchJson<CacheSolutionEntry>(url);
            Assert.IsTrue(cacheEntry.Value.Content == updatedSolutionContent); ;
        }

        [TestMethod]
        public async Task CacheTestingSolution_UpdatingSolution_UpdatesSolutionVersionHistoryCache()
        {

            var (jsonDoc, parentIssueId, title, content, scope) = await _testingCRUDHelper.CreateTestIssue(ContentStatus.Published);
            var (solutionJsonDoc, solutionId, solutionTitle, solutionContent, solutionScope) = await _testingCRUDHelper.CreateTestSolution(parentIssueId, ContentStatus.Published);

            // Get an Issue_ReadVM object which is used by Read.IssueVersionHistory
            Solution_ReadVM? solution = await _read.Solution(new Guid(solutionId), new ContentFilter());
            // Specifically populate the version history cache for the issue by reading
            List<ContentItem_ReadVM> contentItemVersions = await _read.SolutionVersionHistory(solution!);

            // Assemble Cache key and fetch data
            string cacheKey = $"solution-version-history:{solutionId}";
            string url = $"/api/cache-log/entry?key={cacheKey}";
            var cacheEntry1 = await _env.fetchJson<CacheEntryIssueVersionHistoryDTO>(url);

            // Confirm version history has one entry (The current solution)
            Assert.IsTrue(cacheEntry1.Value.Count() == 1);

            // Update the solution
            var (updatedSolutionJsonDoc, updatedSolutionId, updatedSolutionTitle, updatedSolutionContent, updatedSolutionScope) = await _testingCRUDHelper.EditTestSolution(solutionId, solutionScope.ScopeID, parentIssueId, ContentStatus.Published);

            // Repopulate the version history cache by reading
            List<ContentItem_ReadVM> contentItemVersions2 = await _read.SolutionVersionHistory(solution!);

            // Fetch Data
            var cacheEntry2 = await _env.fetchJson<CacheEntryIssueVersionHistoryDTO>(url);

            // Confirm that there are 2 entries in version history
            Assert.IsTrue(cacheEntry2.Value.Count() == 2);
        }

        [TestMethod]
        public async Task CacheTestingSolution_VotingOnSolution_UpdatesVoteStats()
        {
            // Create issue
            var (jsonDoc, parentIssueId, title, content, scope) = await _testingCRUDHelper.CreateTestIssue(ContentStatus.Published);
            var (solutionJsonDoc, solutionId, solutionTitle, solutionContent, solutionScope) = await _testingCRUDHelper.CreateTestSolution(parentIssueId, ContentStatus.Published);

            var cacheKey = $"{CacheKeyPrefix.VoteStats}:{solutionId}";

            string url = $"/api/cache-log/entry?key={cacheKey}";

            var cacheEntry0 = await _env.fetchJson<CacheEntry_SolutionVoteStatsDTO>(url);
            SolutionVotes_Cacheable_ReadVM voteStats0 = cacheEntry0.Value;

            Assert.IsTrue(voteStats0.TotalVotes == 0);
            Assert.IsTrue(voteStats0.SolutionVotes.Count == 0);

            // Updating a vote should update through the cache (No need to call Read.Issue to populate the cache)
            VoteResponse_AjaxVM voteResponse1 =  await _testingCRUDHelper.CreateTestVoteOnSolution(solutionId, 10);

            var cacheEntry1 = await _env.fetchJson<CacheEntry_SolutionVoteStatsDTO>(url);
            SolutionVotes_Cacheable_ReadVM voteStats1 = cacheEntry1.Value;

            Assert.IsTrue(voteStats1.TotalVotes == 1);
            Assert.IsTrue(voteStats1.SolutionVotes.Count == 1);
            Assert.IsTrue(voteStats1.SolutionVotes.Values.Average(v => v.VoteValue) == voteResponse1.Average);

            // Updating a vote should update through the cache (No need to call Read.Issue to populate the cache)
            VoteResponse_AjaxVM voteResponse2 =  await _testingCRUDHelper.CreateTestVoteOnSolution(solutionId, 6);

            var cacheEntry2 = await _env.fetchJson<CacheEntry_SolutionVoteStatsDTO>(url);
            SolutionVotes_Cacheable_ReadVM voteStats2 = cacheEntry2.Value;

            ///NOTE: The Total votes and count stay the same because the logged in user has not changed
            Assert.IsTrue(voteStats2.TotalVotes == 1);
            Assert.IsTrue(voteStats2.SolutionVotes.Count == 1);
            Assert.IsTrue(voteStats2.SolutionVotes.Values.Average(v => v.VoteValue) == voteResponse2.Average);

            //// Create and login user
            var (user, password) = Users.GetRandomAppUser();
            AppUser testUser = Users.CreateTestUser(_db, user, password);
            bool loginSuccess = await Users.LoginUserViaEndpoint(_env, user.Email!, password);

            // Updating a vote should update through the cache (No need to call Read.Issue to populate the cache)
            VoteResponse_AjaxVM voteResponse3 = await _testingCRUDHelper.CreateTestVoteOnSolution(solutionId, 7);

            var cacheEntry3 = await _env.fetchJson<CacheEntry_SolutionVoteStatsDTO>(url);
            SolutionVotes_Cacheable_ReadVM voteStats3 = cacheEntry3.Value;

            ///NOTE: The Total votes and count are updated because a new user has voted
            Assert.IsTrue(voteStats3.TotalVotes == 2);
            Assert.IsTrue(voteStats3.SolutionVotes.Count == 2);
            Assert.IsTrue(voteStats3.SolutionVotes.Values.Average(v => v.VoteValue) == voteResponse3.Average);

        }

        #endregion

        #region Testing solution sub-issues

        [TestMethod]
        public async Task CacheTestingSolution_PagedSubIssues_ShouldBeUpToDate()
        {
            // Create issue
            var (jsonDoc, parentIssueId, title, content, scope) = await _testingCRUDHelper.CreateTestIssue(ContentStatus.Published);
            // Create sub-issue
            var (solutionJsonDoc, parentSolutionId, solutionTitle, solutionContent, solutionScope) = await _testingCRUDHelper.CreateTestSolution(parentIssueId, ContentStatus.Published);
            // This is the solution being testing... 
            // Note it has no sub-issues at creation

            // Repopulate cache (this might not be necessary since creating a solution calls this method at the end)
            //await Read.Solution(new Guid(parentSolutionId!), new ContentFilter());


            // Assemble cache key for CacheHelper.CacheKeysPrefix.FeedIds.SubIssueOfIssue
            ContentFilter filter = new ContentFilter();
            string filterCacheString = filter.ToCacheString();
            int pageNumber = 1;
            var cacheKey = $"{CacheKeyPrefix.SubIssueOfIssueOrSolutionFeedIds}:{parentSolutionId}:{filterCacheString}:page({pageNumber})"; ;
            string encodedCacheKey = Uri.EscapeDataString(cacheKey);

            string url = $"/api/cache-log/entry?key={encodedCacheKey}";
            var cacheEntry = await _env.fetchJson<CacheEntryIdsDTO>(url);

            Assert.IsTrue(cacheEntry.Value.Count() == 0);

            //// Create a new sub-issue for the the solution
            var (subIssueJsonDoc1, subIssueId1, subIssueTitle1, subIssueContent1, subIssueScope1) = await _testingCRUDHelper.CreateTestSubIssue(ContentStatus.Published, null, new Guid(parentSolutionId));
            // Repopulate cache
            var solutionVM = await _read.Solution(new Guid(parentSolutionId!), new ContentFilter());

            var cacheEntry2 = await _env.fetchJson<CacheEntryIdsDTO>(url);
            //// Read this solution to repopulate cache
            Assert.IsTrue(cacheEntry2.Value.Count() == 1);
        }

        [TestMethod]
        public async Task CacheTestingSolution_SubIssues_ContentCounts_ShouldBeUpToDate()
        {
            // Create issue
            var (jsonDoc, parentIssueId, title, content, scope) = await _testingCRUDHelper.CreateTestIssue(ContentStatus.Published);
            // Create sub-issue
            var (solutionJsonDoc, parentSolutionId, solutionTitle, solutionContent, solutionScope) = await _testingCRUDHelper.CreateTestSolution(parentIssueId, ContentStatus.Published);
            // This is the solution being testing... 
            // Note it has no sub-issues at creation
            // Assemble cache key for CacheKeyPrefix.SubIssueForIssueContentCount
            ContentFilter filter = new ContentFilter();
            string filterCacheString = filter.ToCacheString();
            var cacheKey = $"{CacheKeyPrefix.SubIssueForIssueContentCount}:{parentSolutionId}:{filterCacheString}";
            string encodedCacheKey = Uri.EscapeDataString(cacheKey);

            string url = $"/api/cache-log/entry?key={encodedCacheKey}";
            var cacheEntry = await _env.fetchJson<CacheEntry_ContentCountDTO>(url);

            Assert.IsTrue(cacheEntry.Value.AbsoluteCount == 0);
            Assert.IsTrue(cacheEntry.Value.FilteredCount == 0);


            var (subIssueJsonDoc1, subIssueId1, subIssueTitle1, subIssueContent1, subIssueScope1) = await _testingCRUDHelper.CreateTestSubIssue(ContentStatus.Published, null, new Guid(parentSolutionId));
            await _read.Solution(new Guid(parentSolutionId!), new ContentFilter());

            var cacheEntry2 = await _env.fetchJson<CacheEntry_ContentCountDTO>(url);
            Assert.IsTrue(cacheEntry2.Value.AbsoluteCount == 1);
            Assert.IsTrue(cacheEntry2.Value.FilteredCount == 1);

            var (subIssueJsonDoc2, subIssueId2, subIssueTitle2, subIssueContent2, subIssueScope2) = await _testingCRUDHelper.CreateTestSubIssue(ContentStatus.Published, null, new Guid(parentSolutionId));
            await _read.Solution(new Guid(parentSolutionId!), new ContentFilter());

            var cacheEntry3 = await _env.fetchJson<CacheEntry_ContentCountDTO>(url);
            Assert.IsTrue(cacheEntry3.Value.AbsoluteCount == 2);
            Assert.IsTrue(cacheEntry3.Value.FilteredCount == 2);

            var (subIssueJsonDoc3, subIssueId3, subIssueTitle3, subIssueContent3, subIssueScope3) = await _testingCRUDHelper.CreateTestSubIssue(ContentStatus.Published, null, new Guid(parentSolutionId));
            await _read.Solution(new Guid(parentSolutionId!), new ContentFilter());

            var cacheEntry4 = await _env.fetchJson<CacheEntry_ContentCountDTO>(url);
            Assert.IsTrue(cacheEntry4.Value.AbsoluteCount == 3);
            Assert.IsTrue(cacheEntry4.Value.FilteredCount == 3);

            var (subIssueJsonDoc4, subIssueId4, subIssueTitle4, subIssueContent4, subIssueScope4) = await _testingCRUDHelper.CreateTestSubIssue(ContentStatus.Published, null, new Guid(parentSolutionId));
            await _read.Solution(new Guid(parentSolutionId!), new ContentFilter());

            var cacheEntry5 = await _env.fetchJson<CacheEntry_ContentCountDTO>(url);
            Assert.IsTrue(cacheEntry5.Value.AbsoluteCount == 4);
            Assert.IsTrue(cacheEntry5.Value.FilteredCount == 4); 

        }

        [TestMethod]
        public async Task CacheTestingSolution_SubIssues_UpdatingFilter_ContentCounts_ShouldBeUpToDate()
        {
            // Create issue
            var (jsonDoc, parentIssueId, title, content, scope) = await _testingCRUDHelper.CreateTestIssue(ContentStatus.Published);
            // Create sub-issue
            var (solutionJsonDoc, parentSolutionId, solutionTitle, solutionContent, solutionScope) = await _testingCRUDHelper.CreateTestSolution(parentIssueId, ContentStatus.Published);
            // This is the solution being tested... 

            var (subIssueJsonDoc1, subIssueId1, subIssueTitle1, subIssueContent1, subIssueScope1) = await _testingCRUDHelper.CreateTestSubIssue(ContentStatus.Published, null, new Guid(parentSolutionId));
            var (subIssueJsonDoc2, subIssueId2, subIssueTitle2, subIssueContent2, subIssueScope2) = await _testingCRUDHelper.CreateTestSubIssue(ContentStatus.Published, null, new Guid(parentSolutionId));
            var (subIssueJsonDoc3, subIssueId3, subIssueTitle3, subIssueContent3, subIssueScope3) = await _testingCRUDHelper.CreateTestSubIssue(ContentStatus.Published, null, new Guid(parentSolutionId));
            var (subIssueJsonDoc4, subIssueId4, subIssueTitle4, subIssueContent4, subIssueScope4) = await _testingCRUDHelper.CreateTestSubIssue(ContentStatus.Published, null, new Guid(parentSolutionId));

            await _testingCRUDHelper.CreateTestVoteOnIssue(subIssueId2, 8);

            

            // Default test with basically no filter
            ContentFilter filter = new ContentFilter()
            { 
                // This is the default
                AvgVoteRange = new RangeFilter<double> { Min = 0, Max = 10, },
                // Plus some other defaults
            };
            // populate cache
            await _read.Solution(new Guid(parentSolutionId!), filter);
            string filterCacheString = filter.ToCacheString();
            var cacheKey = $"{CacheKeyPrefix.SubIssueForIssueContentCount}:{parentSolutionId}:{filterCacheString}";
            string encodedCacheKey = Uri.EscapeDataString(cacheKey);

            string url = $"/api/cache-log/entry?key={encodedCacheKey}";
            var cacheEntry = await _env.fetchJson<CacheEntry_ContentCountDTO>(url);
            Assert.IsTrue(cacheEntry.Value.AbsoluteCount == 4);
            Assert.IsTrue(cacheEntry.Value.FilteredCount == 4);


            // Test with basically filtering out content
            ContentFilter filter2 = new ContentFilter()
            {
                AvgVoteRange = new RangeFilter<double> { Min = 5, Max = 10 },
                // Plus some other defaults
            };
            // populate cache
            await _read.Solution(new Guid(parentSolutionId!), filter2);
            string filterCacheString2 = filter2.ToCacheString();
            var cacheKey2 = $"{CacheKeyPrefix.SubIssueForIssueContentCount}:{parentSolutionId}:{filterCacheString2}";
            string encodedCacheKey2 = Uri.EscapeDataString(cacheKey2);
            string url2 = $"/api/cache-log/entry?key={encodedCacheKey2}";
            var cacheEntry2 = await _env.fetchJson<CacheEntry_ContentCountDTO>(url2);
            Assert.IsTrue(cacheEntry2.Value.AbsoluteCount == 4);
            Assert.IsTrue(cacheEntry2.Value.FilteredCount == 1); 




        }

        [TestMethod]
        public async Task CacheTestingSolutions_SubIssueVote_ReordersContent_PagedSubIssues_ShouldBeUpToDate()
        {
            // Create issue
            var (jsonDoc, parentIssueId, title, content, scope) = await _testingCRUDHelper.CreateTestIssue(ContentStatus.Published);
            // Create sub-issue
            var (solutionJsonDoc, parentSolutionId, solutionTitle, solutionContent, solutionScope) = await _testingCRUDHelper.CreateTestSolution(parentIssueId, ContentStatus.Published);

            ContentFilter filter = new ContentFilter();
            string filterCacheString = filter.ToCacheString();
            int pageNumber = 1;
            var cacheKey = $"{CacheKeyPrefix.SubIssueOfIssueOrSolutionFeedIds}:{parentSolutionId}:{filterCacheString}:page({pageNumber})";
            string encodedCacheKey = Uri.EscapeDataString(cacheKey);
            string url = $"/api/cache-log/entry?key={encodedCacheKey}";


            // Create sub issues for a solution (all with no votes)
            var (subIssueJsonDoc1, subIssueId1, subIssueTitle1, subIssueContent1, subIssueScope1) = await _testingCRUDHelper.CreateTestSubIssue(ContentStatus.Published, null, new Guid(parentSolutionId));
            var (subIssueJsonDoc2, subIssueId2, subIssueTitle2, subIssueContent2, subIssueScope2) = await _testingCRUDHelper.CreateTestSubIssue(ContentStatus.Published, null, new Guid(parentSolutionId));
            var (subIssueJsonDoc3, subIssueId3, subIssueTitle3, subIssueContent3, subIssueScope3) = await _testingCRUDHelper.CreateTestSubIssue(ContentStatus.Published, null, new Guid(parentSolutionId));

            // populate cache
            await _read.Solution(new Guid(parentSolutionId!), new ContentFilter());

            // VOTE 1
            // Cast a vote on sub-issue 2 (This should clear CacheHelper.CacheKeysPrefix.FeedIds.SubIssueOfIssue for this the parent solution of this sub-issue)
            await _testingCRUDHelper.CreateTestVoteOnIssue(subIssueId2, 8);
            await _read.Solution(new Guid(parentSolutionId!), new ContentFilter());
            var cacheEntry = await _env.fetchJson<CacheEntryIdsDTO>(url);
            Assert.IsTrue(cacheEntry.Value[0] == subIssueId2);

            // VOTE 2
            await _testingCRUDHelper.CreateTestVoteOnIssue(subIssueId3, 9);
            await _read.Solution(new Guid(parentSolutionId!), new ContentFilter());
            var cacheEntry2 = await _env.fetchJson<CacheEntryIdsDTO>(url);
            Assert.IsTrue(cacheEntry2.Value[0] == subIssueId3);
            Assert.IsTrue(cacheEntry2.Value[1] == subIssueId2);

            // VOTE 3
            await _testingCRUDHelper.CreateTestVoteOnIssue(subIssueId1, 10);
            await _read.Solution(new Guid(parentSolutionId!), new ContentFilter());
            var cacheEntry3 = await _env.fetchJson<CacheEntryIdsDTO>(url);
            Assert.IsTrue(cacheEntry3.Value[0] == subIssueId1);
            Assert.IsTrue(cacheEntry3.Value[1] == subIssueId3);
            Assert.IsTrue(cacheEntry3.Value[2] == subIssueId2);
        }

        #endregion



    }
}
