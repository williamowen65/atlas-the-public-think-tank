
using atlas_the_public_think_tank.Data;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;
using atlas_the_public_think_tank.Data.DatabaseEntities.Users;
using atlas_the_public_think_tank.Data.SeedData.SeedIssues;
using atlas_the_public_think_tank.Data.SeedData.SeedSolutions;
using atlas_the_public_think_tank.Models.ViewModel;
using atlas_the_public_think_tank.Models.ViewModel.AjaxVM;
using CloudTests.TestingSetup;
using CloudTests.TestingSetup.TestingData;


namespace CloudTests.UserTests
{
    [TestClass]
    public class UserCreatesIssues_invalidTestingPattern_Tests
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
            
            UserStory_Setup();
        }

        [ClassCleanup]
        public static async Task ClassCleanup()
        {
            await TestingUtilityMethods.deleteDatabase(_client, _db);
        }

        public static void UserStory_Setup()
        {

            string email = Users.TestUser1.Email!;
            string password = Users.TestUser1Password;
            AppUser testUser = Users.CreateTestUser(_db, Users.TestUser1, password);
            // Add test scopes
            Scope globalScope = Scopes.CreateGlobalScope(_db);
            // Add test issues
            Issues.CreateTestIssue(_db, Issues.TestIssue1);
            // Add a sub issue (first sub issue)
            Issues.CreateTestIssue(_db, Issues.TestIssue2);
            // Add another sub-issue (second sub issue)
            Issues.CreateTestIssue(_db, Issues.TestIssue3);
            // Add another sub-issue (third sub issue)
            Issues.CreateTestIssue(_db, Issues.TestIssue4);
            // Add another sub-issue (fourth sub issue)
            Issues.CreateTestIssue(_db, Issues.TestIssue5);

            _db.SaveChanges();
        }



        [TestMethod]
        public async Task HomePage_CanBeVisited_AndKnowAboutAllContentItems()
        {
            string url = "/";
            var document = await _env.fetchHTML(url);
            var paginationButton = document.QuerySelector("#fetchPaginatedContent");
            var buttonText = paginationButton.TextContent;
            // This count includes the seed data from the main app
            int seedIssuesDataCount = SeedIssues.SeedIssuesData.Length;
            int seedSolutionsDataCount = SeedSolutions.SeedSolutionData.Length;
            int testIssuesDataCount = 5;
            int totalIssues = seedIssuesDataCount + testIssuesDataCount + seedSolutionsDataCount;

            Assert.IsTrue(buttonText.Contains($"(3/{totalIssues})"));
        }

        [TestMethod]
        public async Task TestIssue1Page_HasAPage()
        {
            // Go to the issues page and expect to see some issue text
            string url = "/issue/" + Issues.TestIssue1.IssueID;

            var document = await _env.fetchHTML(url);

            var issueCard = document.GetElementById(Issues.TestIssue1.IssueID.ToString());
            Assert.IsNotNull(issueCard, "The main issue card should be present");
        }

        [TestMethod]
        public async Task TestIssue1Page_PaginatedSubIssues_Only3SubIssuesShow()
        {
            // Go to the issues page and expect to see some issue text
            string url = "/issue/" + Issues.TestIssue1.IssueID;
            var document = await _env.fetchHTML(url);

            var subIssuePane = document.GetElementById("sub-issue-content");
            Assert.IsTrue(subIssuePane.ChildElementCount == 3);
        }

        [TestMethod]
        public async Task TestIssue1Page_PaginatedSubIssues_PaginationButtonShows4TotalSubIssues()
        {
            // Go to the issues page and expect to see some issue text
            string url = "/issue/" + Issues.TestIssue1.IssueID;
            var document = await _env.fetchHTML(url);

            var subIssuePaginationButton = document.GetElementById("fetchPaginatedSubIssues");
            var buttonText = subIssuePaginationButton.TextContent;

            Assert.IsNotNull(subIssuePaginationButton);
            Assert.IsTrue(buttonText.Contains("(3/4)"));
        }

        [TestMethod]
        public async Task TestIssue1Page_PaginationAPI_Page2_ShouldReturnJsonWithOneMorePost()
        {
            string url = "/issue/getPaginatedSubIssues/" + Issues.TestIssue1.IssueID.ToString() + "?currentPage=2";
            var JsonResponse = await _env.fetchJson<ContentItems_Paginated_AjaxVM>(url);
            var document = await _env.TextHtmlToDocument(JsonResponse.html);
            var container = document.QuerySelector("body");
            
            Assert.IsTrue(container.ChildElementCount == 1);

        }



        [TestMethod]
        public async Task TestIssue2Page_HasIssue1AsParentIssue()
        {
            string url = "/issue/" + Issues.TestIssue2.IssueID;
            var document = await _env.fetchHTML(url);

            var parentPostContainer = document.GetElementById("parent-post");

            Assert.IsNotNull(parentPostContainer, "Parent post container should be present");
            var parentIssueElement = parentPostContainer.QuerySelector($"[id='{Issues.TestIssue1.IssueID}']");
            Assert.IsNotNull(parentIssueElement, $"Parent issue with ID {Issues.TestIssue1.IssueID} should be present");
        }


        [TestMethod]
        public async Task TestIssue2Page_ParentIssue1_HasSubIssueIconWith4SubIssues()
        {
            string url = "/issue/" + Issues.TestIssue2.IssueID;
            var document = await _env.fetchHTML(url);
            var parentIssueElement = document.QuerySelector($"[id='{Issues.TestIssue1.IssueID}']");
            var parentSubIssuesElement = parentIssueElement!.QuerySelector(".sub-issue-count");
            Assert.IsTrue(parentSubIssuesElement!.TextContent.Trim() == "4");
        }
    }    
}