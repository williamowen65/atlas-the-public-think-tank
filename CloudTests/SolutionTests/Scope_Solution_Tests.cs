using AngleSharp;
using AngleSharp.Dom;
using atlas_the_public_think_tank.Data.CRUD;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Issue;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Solution;
using atlas_the_public_think_tank.Data.DbContext;
using atlas_the_public_think_tank.Data.RepositoryPattern.Repository.Helpers;
using atlas_the_public_think_tank.Models.ViewModel;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.ContentItem_Common;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Issue;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Solution;
using CloudTests.TestingSetup;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CloudTests.SolutionTests
{
    [TestClass]
    public class Scope_Solution_Tests

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
        [DynamicData(nameof(TestingUtilityMethods.GetSolutions), typeof(TestingUtilityMethods), DynamicDataSourceType.Method)]

        public async Task MainSolution_ShouldContainCorrectCompositeScopes(Solution solution)
        {
            string url = "/solution/" + solution.SolutionID;
            Solution_ReadVM? solutionResponse = await Read.Solution(solution.SolutionID, new ContentFilter());
            var document = await _env.fetchHTML(url);

            var contentCard = document.QuerySelector($".card[id='{solution.SolutionID}']");
            var contentCompositeScope = contentCard.QuerySelector(".composite-scope");

            foreach (var scale in solutionResponse.Scope.Scales)
            {
                Assert.IsTrue(
                    contentCompositeScope.TextContent.Contains(scale.ToString(), StringComparison.OrdinalIgnoreCase),
                    $"Scale '{scale}' not found in composite scope for content item {solution.SolutionID}"
                );
            }
            foreach (var domain in solutionResponse.Scope.Domains)
            {
                Assert.IsTrue(
                    contentCompositeScope.TextContent.Contains(domain.ToString(), StringComparison.OrdinalIgnoreCase),
                    $"Domain '{domain}' not found in composite scope for content item {solution.SolutionID}"
                );
            }
            foreach (var entityType in solutionResponse.Scope.EntityTypes)
            {
                Assert.IsTrue(
                    contentCompositeScope.TextContent.Contains(entityType.ToString(), StringComparison.OrdinalIgnoreCase),
                    $"EntityType '{entityType}' not found in composite scope for content item {solution.SolutionID}"
                );
            }
            foreach (var timeframe in solutionResponse.Scope.Timeframes)
            {
                Assert.IsTrue(
                    contentCompositeScope.TextContent.Contains(timeframe.ToString(), StringComparison.OrdinalIgnoreCase),
                    $"Timeframe '{timeframe}' not found in composite scope for content item {solution.SolutionID}"
                );
            }
            foreach (var boundary in solutionResponse.Scope.Boundaries)
            {
                Assert.IsTrue(
                    contentCompositeScope.TextContent.Contains(boundary.ToString(), StringComparison.OrdinalIgnoreCase),
                    $"Boundary '{boundary}' not found in composite scope for content item {solution.SolutionID}"
                );
            }
        }
    }
}
