
using AngleSharp.Dom;
using atlas_the_public_think_tank.Data;
using atlas_the_public_think_tank.Data.DatabaseEntities.Users;
using atlas_the_public_think_tank.Data.SeedData.SeedIssues.Data;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.ContentItem_Common;
using CloudTests.TestingSetup;
using CloudTests.TestingSetup.TestingData;


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

        [TestCleanup]
        public async Task ClassCleanup()
        {
            await TestingUtilityMethods.deleteDatabase(_client, _db);
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

        [TestMethod]
        public async Task PageShould_ContainSidebar_OnLoad()
        {
            var document = await _env.fetchHTML("/");
            var sidebar = document.QuerySelector("#sidebar-content-container");
            Assert.IsNotNull(sidebar, "Sidebar should exist on page load");
            var averageScoreFilter = document.QuerySelector("#average-score-filter");
            Assert.IsNotNull(averageScoreFilter, "Content filter field should exist on page load");
        }

        [DataTestMethod]
        [DataRow("/create-issue")]
        [DataRow("/create-solution")]
        public async Task CreateForm_ShouldHave_SpecificAttributes(string url)
        {
            // Create and login user
            AppUser testUser = Users.CreateTestUser1(_db);
            string email = "testuser@example.com";
            string password = "Password123!";

            bool loginSuccess = await Users.LoginUserViaEndpoint(_env, email, password);
            Assert.IsTrue(loginSuccess, "Login should be successful");

          
            var document = await _env.fetchHTML(url);
            var form = document.QuerySelector("form.issue-editor, form.solution-editor");
            Assert.IsNotNull(form, "Edit form should exist");
            string? contentType = form.GetAttribute("data-content-type");
            Assert.IsNotNull(contentType, "Content Type attribute should exists");
            string? formUrl = form.GetAttribute("data-form-url");
            Assert.IsNotNull(formUrl, "Form Url attribute should exists");
        }

        [TestMethod]
        public async Task EditIssueForm_ShouldHave_SpecificAttributes()
        {
            // Create and login user
            AppUser testUser = Users.CreateTestUser1(_db);
            string email = "testuser@example.com";
            string password = "Password123!";

            bool loginSuccess = await Users.LoginUserViaEndpoint(_env, email, password);
            Assert.IsTrue(loginSuccess, "Login should be successful");

            var (jsonDoc1, title1, content1) = await TestingVersionHistoryHelpers.CreateIssue(_env,
                "This is just an example issue title (content creation)",
                "This is just an example issue content");

            var rootElement1 = jsonDoc1.RootElement;
            string newContentId1 = rootElement1.GetProperty("contentId").ToString();

            string url = $"/edit-issue?issueId={newContentId1}";

            var contentCreationResponse = await _env.fetchJson<ContentCreationResponse_JsonVM>(url);
            var document = await _env.TextHtmlToDocument(contentCreationResponse.Content);
            var form = document.QuerySelector("form.issue-editor");
            Assert.IsNotNull(form, "Edit form should exist");
            string? contentType = form.GetAttribute("data-content-type");
            Assert.IsNotNull(contentType, "Content Type attribute should exists");
            string? formUrl = form.GetAttribute("data-form-url");
            Assert.IsNotNull(formUrl, "Form Url attribute should exists");
        }
        [TestMethod]
        public async Task EditSolutionForm_ShouldHave_SpecificAttributes()
        {
            // Create and login user
            AppUser testUser = Users.CreateTestUser1(_db);
            string email = "testuser@example.com";
            string password = "Password123!";

            bool loginSuccess = await Users.LoginUserViaEndpoint(_env, email, password);
            Assert.IsTrue(loginSuccess, "Login should be successful");


            var (jsonDoc1, title1, content1) = await TestingVersionHistoryHelpers.CreateIssue(_env,
                "This is just an example issue title (content creation)",
                "This is just an example issue content");

            var rootElement1 = jsonDoc1.RootElement;
            string parentIssueId = rootElement1.GetProperty("contentId").ToString();

            var (jsonDoc2, title2, content2) = await TestingVersionHistoryHelpers.CreateSolution(_env,
               "This is just an example solution title (content creation)",
               "This is just an example solution content",
               parentIssueId);

            var rootElement2 = jsonDoc2.RootElement;
            string newContentId2 = rootElement2.GetProperty("contentId").ToString();


            string url = $"/edit-solution?solutionId={newContentId2}";

            var contentCreationResponse = await _env.fetchJson<ContentCreationResponse_JsonVM>(url);
            var document = await _env.TextHtmlToDocument(contentCreationResponse.Content);
            var form = document.QuerySelector("form.solution-editor");
            Assert.IsNotNull(form, "Edit form should exist");
            string? contentType = form.GetAttribute("data-content-type");
            Assert.IsNotNull(contentType, "Content Type attribute should exists");
            string? formUrl = form.GetAttribute("data-form-url");
            Assert.IsNotNull(formUrl, "Form Url attribute should exists");
        }


        public static IEnumerable<object[]> GetIssueUrls()
        {
            yield return new object[] { "/issue/" + ClimateChange.ContentId.ToString() };
            yield return new object[] { "/issue/" + CriticalDeclineOfEndangeredSpecies.ContentId.ToString() };
            yield return new object[] { "/issue/" + Homelessness.ContentId.ToString() };
        }
        public static IEnumerable<object[]> GetEditContentUrls()
        {
            yield return new object[] { "/create-issue"};
            yield return new object[] { "/create-solution"};
            //yield return new object[] { "/issue/" + CriticalDeclineOfEndangeredSpecies.ContentId.ToString() };
            //yield return new object[] { "/issue/" + Homelessness.ContentId.ToString() };
        }



    }
}