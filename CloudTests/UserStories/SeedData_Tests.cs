
using atlas_the_public_think_tank.Data;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Issue;
using atlas_the_public_think_tank.Data.SeedData.SeedIssues.Data;
 
using atlas_the_public_think_tank.Models.ViewModel;
using CloudTests.TestingSetup;
using CloudTests.TestingSetup.TestingData;


namespace CloudTests.UserStories
{
    [TestClass]
    public class SeedData_Tests
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
        [DynamicData(nameof(GetIssues), DynamicDataSourceType.Method)]
        public async Task Issue_HasAPage(Issue issue)
        {
            string url = "/issue/" + issue.IssueID;
            var document = await _env.fetchHTML(url);
            var issueCard = document.GetElementById(issue.IssueID.ToString());
            Assert.IsNotNull(issueCard, $"The {issue.Title} issue card should be present");
        }

        [DataTestMethod]
        [DynamicData(nameof(GetIssues), DynamicDataSourceType.Method)]
        public async Task IssuePage_HasCorrectIssueTextContent(Issue issue)
        {
            string url = "/issue/" + issue.IssueID;
            var document = await _env.fetchHTML(url);
            var issueCard = document.QuerySelector($".issue-card[id='{issue.IssueID}']");
            var contentContainer = issueCard.QuerySelector(".issue-content");
            Assert.IsTrue(contentContainer!.TextContent == issue.Content);
        }

        public static IEnumerable<object[]> GetIssues()
        {
            yield return new object[] { new ClimateChange().issue };
            yield return new object[] { new CriticalDeclineOfEndangeredSpecies().issue };
            yield return new object[] { new Homelessness().issue };
        }

    }    
}