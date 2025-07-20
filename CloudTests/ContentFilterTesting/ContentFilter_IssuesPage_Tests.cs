using atlas_the_public_think_tank.Data;
using atlas_the_public_think_tank.Data.SeedData.SeedIssues;
using atlas_the_public_think_tank.Data.SeedData.SeedIssues.Data;
using atlas_the_public_think_tank.Data.SeedData.SeedSolutions;
using atlas_the_public_think_tank.Models.Database;
using atlas_the_public_think_tank.Models.ViewModel;
using atlas_the_public_think_tank.Services;
using CloudTests.TestingSetup;
using CloudTests.TestingSetup.TestingData;
using System.Text.Json;

namespace CloudTests.ContentFilterTesting
{
    [TestClass]
    public class ContentFilter_IssuesPage_Tests
    {
        private static HttpClient _client;
        private static ApplicationDbContext _db;
        private static TestEnvironment _env;

        [TestInitialize]
        public async Task Setup()
        {

        }

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            // Use the utility class to configure the test environment
            _env = new TestEnvironment();
            _db = _env._db;
            _client = _env._client;
        }

  

        [DataTestMethod]
        [DataRow(0, 10)]
        [DataRow(5, 10)]
        [DataRow(1, 4.5)]
        [DataRow(6, 7.8)]
        [DataRow(4.4, 8)]
        [DataRow(2, 9)]
        [DataRow(3, 6.7)]
        public async Task ContentFilter_IssuePage_AverageVote_TotalSolutions(double min, double max)
        {
            var filterSettings = new ContentFilter
            {
                AvgVoteRange = new RangeFilter<double> { Min = min, Max = max }
                // All other properties will use their defaults
            };

            _env.SetCookie("contentFilter", filterSettings);


            string url = "/issue/" + CanSocialMediaPlatformsBeBetter.ContentId;
            var document = await _env.fetchHTML(url);
            var paginationButton = document.QuerySelector("#fetchPaginatedSolutions");
            var buttonText = paginationButton.TextContent;

            SeedSolutionContainer[] AllSolutionsOfIssue = TestingUtilityMethods.GetSeedSolutionDataContainersOf(new CanSocialMediaPlatformsBeBetter().issue);

            int expectedCount = TestingUtilityMethods.filterByAvgVoteRange(AllSolutionsOfIssue, min, max).Count();

            string expectedCountDisplay = $"({Math.Min(3, expectedCount)}/{expectedCount})";
            Console.WriteLine($"AverageVote Filter Of {min} to {max}: {expectedCountDisplay}");
            bool passingTest = buttonText.Contains(expectedCountDisplay);
            Assert.IsTrue(passingTest);
        }


    }    
}