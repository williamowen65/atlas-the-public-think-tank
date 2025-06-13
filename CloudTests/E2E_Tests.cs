using atlas_the_public_think_tank.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using AngleSharp;
using AngleSharp.Dom;

namespace CloudTests
{
    [TestClass]
    public class E2E_Tests
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
        public async Task Should_LoadMockAtlasApplicationForTesting()
        {
            // Get the response
            var response = await _client.GetAsync("/");
            response.EnsureSuccessStatusCode(); // Will throw if not 2xx
            var html = await response.Content.ReadAsStringAsync();

            // Parse the HTML using AngleSharp
            var context = BrowsingContext.New(Configuration.Default);
            var document = await context.OpenAsync(req => req.Content(html));

            // Now you can use DOM navigation and CSS selectors
            var title = document.Title;
            var header = document.QuerySelector("header");

            // More specific assertions
            Assert.IsNotNull(header, "header should be present");
            if (header is not null)
            {
                // Fix: Use the TextContent property to check if the header contains the text "Atlas"
                Assert.IsTrue(header.TextContent.Contains("Atlas"), "header should contain the text Atlas");
            }

            Console.WriteLine($"Page title: {title}");
        }
    }
}