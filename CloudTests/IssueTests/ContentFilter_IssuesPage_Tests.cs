using atlas_the_public_think_tank.Data.DbContext;
using atlas_the_public_think_tank.Data.RepositoryPattern.Repository.Helpers;
using atlas_the_public_think_tank.Data.SeedData.SeedIssues;
using atlas_the_public_think_tank.Data.SeedData.SeedIssues.Data;
using atlas_the_public_think_tank.Data.SeedData.SeedSolutions;
using CloudTests.TestingSetup;

namespace CloudTests.IssueTests
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

        [ClassCleanup]
        public static async Task ClassCleanup()
        {
            await TestingUtilityMethods.deleteDatabase(_client, _db);
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


            string url = "/issue/" + SeedIssues.SeedIssuesDataContainers[0].issue.IssueID;
            var document = await _env.fetchHTML(url);
            var paginationButton = document.QuerySelector("#fetchPaginatedSolutions");
            var buttonText = paginationButton.TextContent;

            SeedSolutionContainer[] AllSolutionsOfIssue = TestingUtilityMethods.GetSeedSolutionDataContainersOf(SeedIssues.SeedIssuesDataContainers[0].issue);

            int expectedCount = TestingUtilityMethods.filterByAvgVoteRange(AllSolutionsOfIssue, min, max).Count();

            string expectedCountDisplay = $"({Math.Min(3, expectedCount)}/{expectedCount})";
            Console.WriteLine($"AverageVote Filter Of {min} to {max}: {expectedCountDisplay}");
            bool passingTest = buttonText.Contains(expectedCountDisplay);
            Assert.IsTrue(passingTest);
        }


        [DataTestMethod]
        [DataRow(5, 10)]
        [DataRow(6, 9)]
        [DataRow(1, 8)]
        [DataRow(2, 5)]
        [DataRow(5.2, 8.3)]
        [DataRow(1, 1.8)]
        public async Task ContentFilter_IssuePage_WhenContentIsFilteredOut_TotalAndAbsoluteTotalContentNumber_AreVisible(double min, double max)
        {
            var filterSettings = new ContentFilter
            {
                AvgVoteRange = new RangeFilter<double> { Min = min, Max = max }
                // All other properties will use their defaults
            };

            _env.SetCookie("contentFilter", filterSettings);


            string url = "/issue/" + SeedIssues.SeedIssuesDataContainers[0].issue.IssueID;
            var document = await _env.fetchHTML(url);
            var paginationButton = document.QuerySelector("#fetchPaginatedSolutions");
            var buttonText = paginationButton.TextContent;

            SeedSolutionContainer[] AllSolutionsOfIssue = TestingUtilityMethods.GetSeedSolutionDataContainersOf(SeedIssues.SeedIssuesDataContainers[0].issue);
            int allSolutionsCount = AllSolutionsOfIssue.Length;
            int expectedSolutionFilteredCount = TestingUtilityMethods.filterByAvgVoteRange(AllSolutionsOfIssue, min, max).Count();

            SeedIssueContainer[] AllSubIssuesOfIssue = TestingUtilityMethods.GetSubIssuesOf(SeedIssues.SeedIssuesDataContainers[0].issue);
            int allSubIssuesCount = AllSubIssuesOfIssue.Length;
            int expectedSubIssuesFilteredCount = TestingUtilityMethods.filterByAvgVoteRange(AllSubIssuesOfIssue, min, max).Count();

            var contextSection = document.QuerySelector("#page-info .page-context");
            //Assert.IsNotNull(contextSection);
            string contextSectionText = contextSection.TextContent;


            if (allSolutionsCount != expectedSolutionFilteredCount)
            {
                Assert.IsTrue(contextSectionText.Contains($"{expectedSolutionFilteredCount} of {AllSolutionsOfIssue.Count()} solutions"));
            }


            if (allSubIssuesCount != expectedSubIssuesFilteredCount)
            {
                bool test = contextSectionText.Contains($"{expectedSubIssuesFilteredCount} of {AllSubIssuesOfIssue.Count()} sub-issues");
                //if (!test) {
                //    Console.WriteLine();
                //}
                Assert.IsTrue(test);
            }



        }


    }    
}