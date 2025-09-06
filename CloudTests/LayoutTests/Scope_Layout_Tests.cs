
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
    public class Scope_Layout_Tests
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
        [DynamicData(nameof(TestingUtilityMethods.GetIssueUrls), typeof(TestingUtilityMethods), DynamicDataSourceType.Method)]
        public async Task PageShould_Contain_GlobalCompositeScopesToggle(string url)
        {
            var document = await _env.fetchHTML(url);
            var globalCompositeScopesToggle = document.QuerySelector("#globalCompositeScopesToggle");
            Assert.IsNotNull(globalCompositeScopesToggle, "globalCompositeScopesToggle should be present");
            if (globalCompositeScopesToggle is not null)
            {
                Assert.IsTrue(globalCompositeScopesToggle.GetAttribute("type") == "checkbox", "globalCompositeScopesToggle should be a checkbox input");
            }
        }



    }
}