using AngleSharp;
using AngleSharp.Dom;
using atlas_the_public_think_tank.Data;
using atlas_the_public_think_tank.Data.CRUD;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Issue;
using atlas_the_public_think_tank.Data.RepositoryPattern.Repository.Helpers;
using atlas_the_public_think_tank.Models.ViewModel;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.ContentItem_Common;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Issue;
using CloudTests.TestingSetup;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CloudTests.IssueTests
{
    [TestClass]
    public class Scope_Issue_Tests

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
        [DynamicData(nameof(TestingUtilityMethods.GetIssues), typeof(TestingUtilityMethods), DynamicDataSourceType.Method)]

        public async Task MainIssue_ShouldContainCorrectCompositeScopes(Issue issue)
        {
            string url = "/issue/" + issue.IssueID;
            Issue_ReadVM? issueResponse = await Read.Issue(issue.IssueID, new ContentFilter());
            var document = await _env.fetchHTML(url);

            var contentCard = document.QuerySelector($".card[id='{issue.IssueID}']");
            var contentCompositeScope = contentCard.QuerySelector(".composite-scope");

            foreach (var scale in issueResponse.Scope.Scales)
            {
                Assert.IsTrue(
                    contentCompositeScope.TextContent.Contains(scale.ToString(), StringComparison.OrdinalIgnoreCase),
                    $"Scale '{scale}' not found in composite scope for content item {issue.IssueID}"
                );
            }
            foreach (var domain in issueResponse.Scope.Domains)
            {
                Assert.IsTrue(
                    contentCompositeScope.TextContent.Contains(domain.ToString(), StringComparison.OrdinalIgnoreCase),
                    $"Domain '{domain}' not found in composite scope for content item {issue.IssueID}"
                );
            }
            foreach (var entityType in issueResponse.Scope.EntityTypes)
            {
                Assert.IsTrue(
                    contentCompositeScope.TextContent.Contains(entityType.ToString(), StringComparison.OrdinalIgnoreCase),
                    $"EntityType '{entityType}' not found in composite scope for content item {issue.IssueID}"
                );
            }
            foreach (var timeframe in issueResponse.Scope.Timeframes)
            {
                Assert.IsTrue(
                    contentCompositeScope.TextContent.Contains(timeframe.ToString(), StringComparison.OrdinalIgnoreCase),
                    $"Timeframe '{timeframe}' not found in composite scope for content item {issue.IssueID}"
                );
            }
            foreach (var boundary in issueResponse.Scope.Boundaries)
            {
                Assert.IsTrue(
                    contentCompositeScope.TextContent.Contains(boundary.ToString(), StringComparison.OrdinalIgnoreCase),
                    $"Boundary '{boundary}' not found in composite scope for content item {issue.IssueID}"
                );
            }
        }
    }
}
