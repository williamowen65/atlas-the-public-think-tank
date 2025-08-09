using AngleSharp;
using AngleSharp.Dom;
using atlas_the_public_think_tank.Models.ViewModel;
using CloudTests.TestingSetup;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CloudTests.UnitTesting
{
    [TestClass]
    public class UnitTests
    {
        private static string _baseUrl;
        private static HttpClient _client;
        private static TestEnvironment _env;

        [TestInitialize]
        public async Task Setup()
        {
            // Use the utility class to configure the test environment
            _env = new TestEnvironment();
            //_db = testEnv._db;
            _client = _env._client;
        }

        [TestMethod]
        public async Task ReadContentItems_ShouldReturn_OnlyThreePosts()
        {
            // Get the response
            var url = "test-ReadContentItems";

            var paginatedResponse = await _env.fetchJson<PaginatedContentItemsResponse>(url);
            // Assert that the response is not null
            Assert.IsNotNull(paginatedResponse, "Paginated response should not be null");

            if (paginatedResponse.TotalCount >= 3)
            {
                // Assert that the response contains exactly 3 issues
                Assert.AreEqual(3, paginatedResponse.ContentItems.Count, "Should contain exactly 3 issues");
            }
            else
            {
                // Or less issues if total is less than 3
                Assert.AreEqual(paginatedResponse.TotalCount, paginatedResponse.ContentItems.Count, "Should contain exactly " + paginatedResponse.TotalCount + " issues");
            }

            Assert.AreEqual(1, paginatedResponse.CurrentPage, "Current page should be 1");
        }
    }
}
