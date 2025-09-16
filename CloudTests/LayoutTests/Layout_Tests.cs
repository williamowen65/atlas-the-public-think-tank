
using AngleSharp.Dom;
using atlas_the_public_think_tank.Data;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;
using atlas_the_public_think_tank.Data.DatabaseEntities.Users;
using atlas_the_public_think_tank.Data.RepositoryPattern.Repository.Helpers;
using atlas_the_public_think_tank.Data.SeedData.SeedIssues;
using atlas_the_public_think_tank.Data.SeedData.SeedIssues.Data;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.ContentItem_Common;
using CloudTests.TestingSetup;
using CloudTests.TestingSetup.TestingData;
using Humanizer;
using System;


namespace CloudTests.LayoutTests
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


        #region general layout
   

        [DataTestMethod]
        [DataRow("/")]
        [DataRow("/privacy")]
        [DynamicData(nameof(TestingUtilityMethods.GetIssueUrls), typeof(TestingUtilityMethods), DynamicDataSourceType.Method)]
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
        [DynamicData(nameof(TestingUtilityMethods.GetIssueUrls), typeof(TestingUtilityMethods), DynamicDataSourceType.Method)]
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

        #endregion

        #region Content Editing UI

        [DataTestMethod]
        [DataRow("/create-issue")]
        [DataRow("/create-solution")]
        public async Task CreateForm_ShouldHave_SpecificAttributes(string url)
        {
            // Create and login user
            string email = Users.TestUser1.Email!;
            string password = Users.TestUser1Password;
            AppUser testUser = Users.CreateTestUser(_db, Users.TestUser1, password);

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
            string email = Users.TestUser1.Email!;
            string password = Users.TestUser1Password;
            AppUser testUser = Users.CreateTestUser(_db, Users.TestUser1, password);

            bool loginSuccess = await Users.LoginUserViaEndpoint(_env, email, password);
            Assert.IsTrue(loginSuccess, "Login should be successful");

            var (jsonDoc1, title1, content1) = await TestingCRUDHelpers.CreateIssue(_env,
                "This is just an example issue title (content creation)",
                "This is just an example issue content",
                new Scope() {
                    Scales = { Scale.Global, Scale.National }
                });

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
            string email = Users.TestUser1.Email!;
            string password = Users.TestUser1Password;
            AppUser testUser = Users.CreateTestUser(_db, Users.TestUser1, password);

            bool loginSuccess = await Users.LoginUserViaEndpoint(_env, email, password);
            Assert.IsTrue(loginSuccess, "Login should be successful");


            var (jsonDoc1, title1, content1) = await TestingCRUDHelpers.CreateIssue(_env,
                "This is just an example issue title (content creation)",
                "This is just an example issue content",
                new Scope() { 
                    Scales = { Scale.Global }   
                });

            var rootElement1 = jsonDoc1.RootElement;
            string parentIssueId = rootElement1.GetProperty("contentId").ToString();

            var (jsonDoc2, title2, content2) = await TestingCRUDHelpers.CreateSolution(_env,
               "This is just an example solution title (content creation)",
               "This is just an example solution content",
               new Scope() { 
                    Scales = { Scale.Global }
               },
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

        #endregion

        #region Sidebar UI

        /// Every page should have a Context section

        [DataTestMethod]
        [DataRow("/")]
        [DynamicData(nameof(TestingUtilityMethods.GetIssueUrls), typeof(TestingUtilityMethods), DynamicDataSourceType.Method)]
        public async Task EveryPage_ShouldHave_PageContextSection(string url)
        {
            var document = await _env.fetchHTML(url);
            var contextSection = document.QuerySelector("#page-info .page-context");
            Assert.IsNotNull(contextSection, "Context section should exist");
        }

        /// If a filter is applied, there should be an alert

        [DataTestMethod]
        [DataRow("/")]
        [DynamicData(nameof(TestingUtilityMethods.GetIssueUrls), typeof(TestingUtilityMethods), DynamicDataSourceType.Method)]
        public async Task IfFilterApplied_ShouldHave_FilterAlert(string url) 
        {

            var filterSettings = new ContentFilter
            {
                AvgVoteRange = new RangeFilter<double> { Min = 1 }
                // All other properties will use their defaults
            };

            _env.SetCookie("contentFilter", filterSettings);

            var document = await _env.fetchHTML(url);
            var filterAlterSection = document.QuerySelector("#page-info .page-active-filters");
            Assert.IsNotNull(filterAlterSection, "Context section should exist");
            Assert.IsFalse(filterAlterSection.ClassList.Contains("d-none"), "Filter alert should be visible (not have d-none class)");
        }

      

        [DataTestMethod]
        [DynamicData(nameof(TestingUtilityMethods.GetFilterDataRows), typeof(TestingUtilityMethods), DynamicDataSourceType.Method)]
        public async Task IfFilterApplied_ShouldHave_FilterAlert_WithSpecificFilterInfo(ContentFilter filter, string[] expectedAlerts) 
        {


            _env.SetCookie("contentFilter", filter);

            var document = await _env.fetchHTML("/");
            var filterAlterSection = document.QuerySelector("#page-info .page-active-filters");
            Assert.IsNotNull(filterAlterSection, "Filter alert section should exist");
            foreach (string alert in expectedAlerts) { 
                Assert.IsTrue(filterAlterSection.InnerHtml.Contains(alert));
            }
        }

        [DataTestMethod]
        [DataRow("/")]
        [DynamicData(nameof(TestingUtilityMethods.GetIssueUrls), typeof(TestingUtilityMethods), DynamicDataSourceType.Method)]
        public async Task IfNoFilterApplied_ShouldNotHave_FilterAlert(string url) 
        {

            var document = await _env.fetchHTML(url);
            var filterAlterSection = document.QuerySelector("#page-info .page-active-filters");
            Assert.IsNotNull(filterAlterSection, "Filter alert section should exist");
            Assert.IsTrue(filterAlterSection.ClassList.Contains("d-none"), "Filter alert should not be visible (should have d-none class)");
        }


        // When a filter is applied the total count and absolute count are visible


        #endregion




    }
}