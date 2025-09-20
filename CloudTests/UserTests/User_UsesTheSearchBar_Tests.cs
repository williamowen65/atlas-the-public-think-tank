
using AngleSharp.Dom;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;
using atlas_the_public_think_tank.Data.DatabaseEntities.Users;
using atlas_the_public_think_tank.Data.DbContext;
using atlas_the_public_think_tank.Data.RawSQL;
using atlas_the_public_think_tank.Data.SeedData.SeedIssues;
using atlas_the_public_think_tank.Data.SeedData.SeedIssues.Data;
using atlas_the_public_think_tank.Data.SeedData.SeedSolutions;

using atlas_the_public_think_tank.Models.ViewModel;
using CloudTests.TestingSetup;
using CloudTests.TestingSetup.TestingData;
using Microsoft.EntityFrameworkCore;
using System;
using System.Text.Json;


namespace CloudTests.UserTests
{
    [TestClass]
    public class User_UsesTheSearchBar_Tests
    {
        private static HttpClient _client;
        private static ApplicationDbContext _db;
        private static TestEnvironment _env;

        [ClassInitialize]
        public static async Task ClassInit(TestContext context)
        {

            string appSettings = @"
            {
                ""ApplySeedData"": true,
                ""Caching"": {
                    ""enabled"": true
                }
            }";
            // Arrange environment
            _env = new TestEnvironment(appSettings);
            _db = _env._db;
            _client = _env._client;

        }

        [ClassCleanup]
        public static async Task ClassCleanup()
        {
            await TestingUtilityMethods.deleteDatabase(_client, _db);
        }

        [TestMethod]
        public async Task User_CanSeeA_SearchBar()
        {
            var document = await _env.fetchHTML("/");

            // Look for either issue or solution editor
            var headerSearchBar = document.QuerySelector("#header-search-bar");

            Assert.IsNotNull(
                headerSearchBar,
                $"SearchBar should exists"
            );
        }

        // Define a type with an 'html' property for the result
        public class SearchResult
        {
            public string html { get; set; }
        }
        private async Task<bool> IsFullTextIndexReadyAsync(ApplicationDbContext db)
        {
            // Checks if the full-text index for the Issues table is ready (PopulateStatus = 0)
            var sql = @"
                SELECT TOP 1 c.is_importing
                FROM sys.fulltext_catalogs c
            ";
            // Execute the SQL and get the status value
            var status = await db.Database.ExecuteSqlRawAsync(sql);
            // PopulateStatus: 0 = Idle (ready), 1 = Full population in progress, 2 = Incremental population in progress, 3 = Throttled, 4 = Stopped
            return status == 0;
        }

        private async Task WaitForFullTextIndexAsync(ApplicationDbContext db, int timeoutSeconds = 60, int pollIntervalMs = 1000)
        {
            var start = DateTime.UtcNow;
            while ((DateTime.UtcNow - start).TotalSeconds < timeoutSeconds)
            {
                if (await IsFullTextIndexReadyAsync(db))
                    return;
                await Task.Delay(pollIntervalMs);
            }
            throw new TimeoutException("Full-text index was not ready within the timeout period.");
        }

        // Usage in the test method
        [TestMethod]
        public async Task User_Searching_ReturnsDropdown()
        {

            //await WaitForFullTextIndexAsync(_db);
            await Task.Delay(10000);

            var payload = new { searchString = "Homelessness " };
            var result = await _env.fetchPost<SearchResult, object>("/search", payload);
            Assert.IsNotNull(result);

            var resultHTMLDoc = await _env.TextHtmlToDocument(result.html);
            Assert.IsNotNull(resultHTMLDoc);
            Assert.IsNotNull(resultHTMLDoc.QuerySelector(".search-results-container"));
            var issue = new Homelessness();
            string selector = $".card[id='{issue.issue.IssueID.ToString()}']";
            var homelessnessResult = resultHTMLDoc.QuerySelector(selector);
            Assert.IsNotNull(homelessnessResult, "Homelessness result should be present in this search result");
        }


    }
}