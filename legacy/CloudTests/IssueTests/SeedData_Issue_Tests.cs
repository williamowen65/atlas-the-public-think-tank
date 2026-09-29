using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Issue;
using atlas_the_public_think_tank.Data.DbContext;
using atlas_the_public_think_tank.Data.SeedData.SeedIssues;
using atlas_the_public_think_tank.Data.SeedData.SeedIssues.Data;

using atlas_the_public_think_tank.Models.ViewModel;
using CloudTests.TestingSetup;
using CloudTests.TestingSetup.TestingData;


namespace CloudTests.IssueTests
{
    [TestClass]
    public class SeedData_Issue_Tests
    {
        private HttpClient _client;
        private ApplicationDbContext _db;
        private TestEnvironment _env;


        [TestInitialize]
        public void Setup()
        {
            // Use the utility class to configure the test environment
            _env = new TestEnvironment();
            _db = _env._db;
            _client = _env._client;
        }

        [TestCleanup]
        public async Task ClassCleanup()
        {
            await TestingUtilityMethods.deleteDatabase(_client, _db);
        }



        [DataTestMethod]
        [DynamicData(nameof(TestingUtilityMethods.GetIssues), typeof(TestingUtilityMethods), DynamicDataSourceType.Method)]
        public async Task Issue_HasAPage(Issue issue)
        {
            string url = "/issue/" + issue.IssueID;
            var document = await _env.fetchHTML(url);
            var issueCard = document.GetElementById(issue.IssueID.ToString());
            Assert.IsNotNull(issueCard, $"The {issue.Title} issue card should be present");
        }

        [DataTestMethod]
        [DynamicData(nameof(TestingUtilityMethods.GetIssues), typeof(TestingUtilityMethods), DynamicDataSourceType.Method)]
        public async Task IssuePage_HasCorrectIssueTextContent(Issue issue)
        {
            string url = "/issue/" + issue.IssueID;
            var document = await _env.fetchHTML(url);
            var issueCard = document.QuerySelector($".issue-card[id='{issue.IssueID}']");
            var contentContainer = issueCard.QuerySelector(".issue-content");
            Assert.IsTrue(contentContainer!.TextContent == issue.Content);
        }

    }    
}