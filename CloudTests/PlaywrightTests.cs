using atlas_the_public_think_tank.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace CloudTests
{
    [TestClass]
    public class PlaywrightTests
    {
        private WebApplicationFactory<atlas_the_public_think_tank.Program> _factory;
        private SqliteTestFixture _sqliteFixture;
        private IPlaywright _playwright;
        private IBrowser _browser;
        private IPage _page;
        private string _baseUrl;

        [TestInitialize]
        public async Task Setup()
        {
            Console.WriteLine("Setting up Playwright test with SQLite");

            // Create SQLite test fixture
            _sqliteFixture = new SqliteTestFixture();

            // Setup test server with SQLite database
            _factory = new WebApplicationFactory<atlas_the_public_think_tank.Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.UseEnvironment("Testing");

                    builder.ConfigureServices(services =>
                    {
                        // Remove existing DbContext registration
                        var descriptor = services.SingleOrDefault(
                            d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                        if (descriptor != null)
                        {
                            services.Remove(descriptor);
                        }

                        // Get the SQLite DbContext options from the fixture's service provider
                        var sqliteServiceScope = _sqliteFixture.ServiceProvider.CreateScope();
                        var sqliteOptions = sqliteServiceScope.ServiceProvider
                            .GetRequiredService<DbContextOptions<ApplicationDbContext>>();

                        // Add the SQLite DbContext options to the test server
                        services.AddDbContext<ApplicationDbContext>(options =>
                        {
                            // Copy options from the SQLite fixture
                            options.UseSqlite(
                                ((DbContextOptions<ApplicationDbContext>)sqliteOptions)
                                .FindExtension<Microsoft.EntityFrameworkCore.Sqlite.Infrastructure.Internal.SqliteOptionsExtension>()
                                .Connection);
                        });
                    });
                });

            var client = _factory.CreateClient();
            _baseUrl = client.BaseAddress.ToString().TrimEnd('/');

            Console.WriteLine($"Test server URL: {_baseUrl}");

            // Setup Playwright browser
            _playwright = await Playwright.CreateAsync();
            _browser = await PlaywrightFixture.InitializeBrowserAsync(headless: false);
            _page = await _browser.NewPageAsync();

            // Add console logging from browser
            _page.Console += (_, msg) => Console.WriteLine($"Browser console: {msg.Text}");

            // Navigate to home page to verify server is running
            await _page.GotoAsync(_baseUrl);
            Console.WriteLine($"Initial page title: {await _page.TitleAsync()}");
        }

        [TestMethod]
        public async Task Should_CreateSolution_When_FormIsValid()
        {
            // Login with test user
            await _page.GotoAsync($"{_baseUrl}/Identity/Account/Login");
            Console.WriteLine("Navigated to login page");

            await _page.FillAsync("#Input_Email", "testuser@example.com");
            await _page.FillAsync("#Input_Password", "Password123!");
            await _page.ClickAsync("#login-submit");

            // Wait for navigation after login
            await _page.WaitForURLAsync($"{_baseUrl}/**");
            Console.WriteLine($"After login, URL: {_page.Url}");

            // Go to solution creation page with the test issue
            await _page.GotoAsync($"{_baseUrl}/solution/create?parentIssueID=44444444-4444-4444-4444-444444444444");
            Console.WriteLine("Navigated to solution creation page");

            // Fill the form
            await _page.FillAsync("#Title", "Test Solution");
            await _page.FillAsync("#Content", "This is a test solution");
            await _page.SelectOptionAsync("#ScopeID", new[] { "22222222-2222-2222-2222-222222222222" });

            // Submit the form
            await _page.ClickAsync("button[type='submit']");

            // Verify redirect to issue page
            await _page.WaitForURLAsync($"{_baseUrl}/issue/44444444-4444-4444-4444-444444444444");
            Console.WriteLine($"After form submission, URL: {_page.Url}");

            // Verify solution appears in the list
            var solutionExists = await _page.Locator("text=Test Solution").CountAsync() > 0;
            Assert.IsTrue(solutionExists, "Solution was not created successfully");
        }

        [TestCleanup]
        public async Task Cleanup()
        {
            await _browser?.CloseAsync();
            _playwright?.Dispose();
            _factory?.Dispose();
            _sqliteFixture?.Dispose();
        }
    }
}