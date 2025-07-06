using AngleSharp;
using AngleSharp.Dom;
using atlas_the_public_think_tank.Data;
using atlas_the_public_think_tank.Models;
using CloudTests.TestingSetup;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

        [DataTestMethod]
        [DataRow("/")]
        [DataRow("/privacy")]
        [DataRow("/issue/44444444-4444-4444-4444-444444444444")]
        public async Task PageShould_ContainCommonHeaderWithAtlasString(string url)
        {
            // Get the response
            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode(); // Will throw if not 2xx
            var html = await response.Content.ReadAsStringAsync();

            Console.WriteLine(html);

            // Parse the HTML using AngleSharp
            var context = BrowsingContext.New(Configuration.Default);
            var document = await context.OpenAsync(req => req.Content(html));

            // Now you can use DOM navigation and CSS selectors
            var header = document.QuerySelector("header");

            // More specific assertions
            Assert.IsNotNull(header, "header should be present");
            if (header is not null)
            {
                // Fix: Use the TextContent property to check if the header contains the text "Atlas"
                Assert.IsTrue(header.TextContent.Contains("Atlas"), "header should contain the text Atlas");
            }
        }

        [DataTestMethod]
        [DataRow("/")]
        [DataRow("/privacy")]
        [DataRow("/issue/44444444-4444-4444-4444-444444444444")]
        public async Task PageShould_ContainCommonFooterWithAtlasString(string url)
        {
            // Get the response
            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode(); // Will throw if not 2xx
            var html = await response.Content.ReadAsStringAsync();

            Console.WriteLine(html);

            // Parse the HTML using AngleSharp
            var context = BrowsingContext.New(Configuration.Default);
            var document = await context.OpenAsync(req => req.Content(html));

            // Now you can use DOM navigation and CSS selectors
            var footer = document.QuerySelector("footer");

            // More specific assertions
            Assert.IsNotNull(footer, "footer should be present");
            if (footer is not null)
            {
                Assert.IsTrue(footer.TextContent.Contains("Atlas"), "footer should contain the text Atlas");
            }
        }


        [DataTestMethod]
        [DataRow("/issue/44444444-4444-4444-4444-444444444444", "This is a test issue for testing solutions")]
        [DataRow("/issue/55555555-5555-5555-5555-555555555555", "This is a another test issue for testing solutions")]
        public async Task PageShould_ShowTextContentOfGivenIssue(string url, string expectedContent)
        {
            // Get the response
            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode(); // Will throw if not 2xx
            var html = await response.Content.ReadAsStringAsync();

            Console.WriteLine(html);

            // Parse the HTML using AngleSharp
            var context = BrowsingContext.New(Configuration.Default);
            var document = await context.OpenAsync(req => req.Content(html));

            string id = url.Split('/').Last();

            var issueCard = document.GetElementById(id);
            Assert.IsNotNull(issueCard, "The main issue card should be present");
            if (issueCard is not null)
            {
                Assert.IsTrue(issueCard.TextContent.Contains(expectedContent), "The issue card should contain the issue content");
            }

        }

       

    }
}