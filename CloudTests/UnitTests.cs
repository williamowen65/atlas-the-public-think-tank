using atlas_the_public_think_tank.Models;
using CloudTests.TestingSetup;
using Microsoft.AspNetCore.Mvc.Testing;
using AngleSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CloudTests
{
    [TestClass]
    public class UnitTests
    {


        private WebApplicationFactory<atlas_the_public_think_tank.Program> _factory;
        private SqliteTestFixture _sqliteFixture;
        private static string _baseUrl;
        private static HttpClient _client;

        [TestInitialize]
        public async Task Setup()
        {
            // Create SQLite test fixture
            _sqliteFixture = new SqliteTestFixture();

            // Use the utility class to configure the test environment
            (_factory, _client, _baseUrl) = TestEnvironmentUtility.ConfigureTestEnvironment(_sqliteFixture);
        }

        [TestMethod]
        public async Task GetIssuesPagedAsync_ShouldReturn_OnlyThreePosts()
        {
            // Get the response
            var response = await _client.GetAsync("test-GetIssuesPagedAsync");
            response.EnsureSuccessStatusCode();

            // Deserialize into PaginatedIssuesResponse
            var jsonResponse = await response.Content.ReadAsStringAsync();

            // Use System.Text.Json to deserialize the response
            var paginatedResponse = System.Text.Json.JsonSerializer.Deserialize<PaginatedIssuesResponse>(
                jsonResponse,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            // Assert that the response is not null
            Assert.IsNotNull(paginatedResponse, "Paginated response should not be null");

            if (paginatedResponse.TotalCount >= 3)
            {
                // Assert that the response contains exactly 3 issues
                Assert.AreEqual(3, paginatedResponse.Issues.Count, "Should contain exactly 3 issues");
            }
            else
            {
                Assert.AreEqual(paginatedResponse.TotalCount, paginatedResponse.Issues.Count, "Should contain exactly " + paginatedResponse.TotalCount + " issues");
            }

            Assert.AreEqual(1, paginatedResponse.CurrentPage, "Current page should be 1");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
