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
    public class ContentFilter_HomePage_Tests
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

  
        [TestMethod]
        public async Task ContentFilter_MinVote1000_FiltersOutAllContent()
        {
            // Arrange - Create filter with default settings but customize just the vote count
            var filterSettings = new ContentFilter
            {
                TotalVoteCount = new NullableMaxRangeFilter<int> { Min = 1000, Max = null }
                // All other properties will use their defaults
            };

            // Act - Set the cookie
            _env.SetCookie("contentFilter", filterSettings);

            string url = "/";
            var document = await _env.fetchHTML(url);
            var contentContainer = document.GetElementById("main-content");
            Assert.IsTrue(contentContainer.TextContent.Contains("No posts available. Be the first to create a post"));

        }

        [DataTestMethod]
        [DataRow(5, 10)]
        [DataRow(1, 4.5)]
        [DataRow(6, 7.8)]
        [DataRow(4.4, 8)]
        [DataRow(2, 9)]
        [DataRow(3, 6.7)]
        public async Task ContentFilter_AverageVote_TotalContentItems(double min, double max)
        {
            var filterSettings = new ContentFilter
            {
                AvgVoteRange = new RangeFilter<double> { Min = min, Max = max }
                // All other properties will use their defaults
            };

            _env.SetCookie("contentFilter", filterSettings);


            string url = "/";
            var document = await _env.fetchHTML(url);
            var paginationButton = document.QuerySelector("#fetchPaginatedContent");
            var buttonText = paginationButton.TextContent;

            int expectedCount = TestingUtilityMethods.filterByAvgVoteRange(SeedIssues.SeedIssuesDataContainers, min, max).Count() + TestingUtilityMethods.filterByAvgVoteRange(SeedSolutions.SeedSolutionDataContainers, min, max).Count();

            string expectedCountDisplay = $"({Math.Min(3, expectedCount)}/{expectedCount})";
            Console.WriteLine($"AverageVote Filter Of {min} to {max}: {expectedCountDisplay}");
            Assert.IsTrue(buttonText.Contains(expectedCountDisplay));
        }


        // TODO: create testing for content filter on issue pages and solution pages.


    }    
}