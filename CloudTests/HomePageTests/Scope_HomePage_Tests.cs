using AngleSharp;
using AngleSharp.Dom;
using atlas_the_public_think_tank.Data.CRUD;
using atlas_the_public_think_tank.Data.DbContext;
using atlas_the_public_think_tank.Data.RepositoryPattern.Repository.Helpers;
using atlas_the_public_think_tank.Models.ViewModel;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.ContentItem_Common;
using CloudTests.TestingSetup;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CloudTests.HomePageTests
{
    [TestClass]
    public class Scope_HomePage_Tests

    {
        private static string _baseUrl;
        private static HttpClient _client;
        private static TestEnvironment _env;
        private static ApplicationDbContext _db;

        [TestInitialize]
        public async Task Setup()
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
        public async Task ContentFeedItems_ShouldContainCorrectCompositeScopes()
        {
            ContentItems_Paginated_ReadVM paginatedResponse = await Read.PaginatedMainContentFeed(new ContentFilter());
            var document = await _env.fetchHTML("/");
            var mainContent = document.QuerySelector("#main-content");


            foreach (var item in paginatedResponse.ContentItems) 
            {
                string itemId = item.ContentID.ToString();
                var contentCard = mainContent.QuerySelector($".card[id='{itemId}']");
                var contentCompositeScope = contentCard.QuerySelector(".composite-scope");

                foreach (var scale in item.Scope.Scales)
                {
                    Assert.IsTrue(
                        contentCompositeScope.TextContent.Contains(scale.ToString(), StringComparison.OrdinalIgnoreCase),
                        $"Scale '{scale}' not found in composite scope for content item {item.ContentID}"
                    );
                }
                foreach (var domain in item.Scope.Domains)
                {
                    Assert.IsTrue(
                        contentCompositeScope.TextContent.Contains(domain.ToString(), StringComparison.OrdinalIgnoreCase),
                        $"Domain '{domain}' not found in composite scope for content item {item.ContentID}"
                    );
                }
                foreach (var entityType in item.Scope.EntityTypes)
                {
                    Assert.IsTrue(
                        contentCompositeScope.TextContent.Contains(entityType.ToString(), StringComparison.OrdinalIgnoreCase),
                        $"EntityType '{entityType}' not found in composite scope for content item {item.ContentID}"
                    );
                }
                foreach (var timeframe in item.Scope.Timeframes)
                {
                    Assert.IsTrue(
                        contentCompositeScope.TextContent.Contains(timeframe.ToString(), StringComparison.OrdinalIgnoreCase),
                        $"Timeframe '{timeframe}' not found in composite scope for content item {item.ContentID}"
                    );
                }
                foreach (var boundary in item.Scope.Boundaries)
                {
                    Assert.IsTrue(
                        contentCompositeScope.TextContent.Contains(boundary.ToString(), StringComparison.OrdinalIgnoreCase),
                        $"Boundary '{boundary}' not found in composite scope for content item {item.ContentID}"
                    );
                }
            }


        }


    }
}
