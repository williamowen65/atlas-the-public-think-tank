
using atlas_the_public_think_tank.Data;
using atlas_the_public_think_tank.Data.SeedData.SeedIssues.Data;
using CloudTests.TestingSetup;


namespace CloudTests.UserInterface
{
    [TestClass]
    public class Layout_Tests
    {
        private static string _baseUrl;
        private static HttpClient _client;
        private static TestEnvironment _env;
        private static ApplicationDbContext _db;

        [TestInitialize]
        public async Task Setup()
        {
            _env = new TestEnvironment();
            _db = _env._db;
            _client = _env._client;
        }

        [DataTestMethod]
        [DataRow("/")]
        [DataRow("/privacy")]
        [DynamicData(nameof(GetIssueUrls), DynamicDataSourceType.Method)]
        public async Task PageShould_ContainCommonHeaderWithAtlasString(string url)
        {
            var document = await _env.fetchHTML(url);
            var header = document.QuerySelector("header");
            Assert.IsNotNull(header, "header should be present");
            if (header is not null)
            {
                Assert.IsTrue(header.TextContent.Contains("Atlas"), "header should contain the text Atlas");
            }
        }

        [DataTestMethod]
        [DataRow("/")]
        [DataRow("/privacy")]
        [DynamicData(nameof(GetIssueUrls), DynamicDataSourceType.Method)]
        public async Task PageShould_ContainCommonFooterWithAtlasString(string url)
        {
            var document = await _env.fetchHTML(url);
            var footer = document.QuerySelector("footer");
            Assert.IsNotNull(footer, "footer should be present");
            if (footer is not null)
            {
                Assert.IsTrue(footer.TextContent.Contains("Atlas"), "Footer should contain the text Atlas");
            }
        }

        public static IEnumerable<object[]> GetIssueUrls()
        {
            yield return new object[] { "/issue/" + ClimateChange.ContentId.ToString() };
            yield return new object[] { "/issue/" + CriticalDeclineOfEndangeredSpecies.ContentId.ToString() };
            yield return new object[] { "/issue/" + Homelessness.ContentId.ToString() };
        }
    }
}