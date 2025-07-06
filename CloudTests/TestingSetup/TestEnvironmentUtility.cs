using atlas_the_public_think_tank.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net.Http;

namespace CloudTests.TestingSetup
{
    public static class TestEnvironmentUtility
    {
        /// <summary>
        /// Configures the test environment with a SQLite database for integration testing
        /// </summary>
        /// <param name="sqliteFixture">The SQLite test fixture containing the test database</param>
        /// <param name="baseUrl">The base URL used for the test server</param>
        /// <returns>A tuple containing the WebApplicationFactory and HttpClient configured for testing</returns>
        public static (WebApplicationFactory<atlas_the_public_think_tank.Program> factory, HttpClient client, string baseUrl)
            ConfigureTestEnvironment(SqliteTestFixture sqliteFixture)
        {
            string baseUrl = "https://localhost:5501";

            // Setup test server with SQLite database and explicitly configure the host
            var factory = new WebApplicationFactory<atlas_the_public_think_tank.Program>()
                .WithWebHostBuilder(builder =>
                {
                    // Explicitly set server URLs to use a fixed port for testing
                    builder.UseUrls(baseUrl);

                    // This environment toggles DB connection for the project build
                    builder.UseEnvironment("Testing");

                    builder.ConfigureLogging(logging =>
                    {
                        logging.ClearProviders();
                        logging.SetMinimumLevel(LogLevel.Error);
                    });

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
                        var sqliteServiceScope = sqliteFixture.ServiceProvider.CreateScope();
                        var sqliteOptions = sqliteServiceScope.ServiceProvider
                            .GetRequiredService<DbContextOptions<ApplicationDbContext>>();

                        // Add the SQLite DbContext options to the test server
                        services.AddDbContext<ApplicationDbContext>(options =>
                        {
                            // Copy options from the SQLite fixture
                            options.UseSqlite(
                                sqliteOptions
                                .FindExtension<Microsoft.EntityFrameworkCore.Sqlite.Infrastructure.Internal.SqliteOptionsExtension>()
                                .Connection);
                        });

                        // Register controllers from the test assembly
                        services.AddControllers()
                            .AddApplicationPart(typeof(TestControllers.UnitTestController).Assembly); 
                    });
              
                });

            // Create client with specific options
            var client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = true,
                HandleCookies = true,
                BaseAddress = new Uri(baseUrl)
            });

            baseUrl = client.BaseAddress.ToString().TrimEnd('/');
            Console.WriteLine($"Test server URL: {baseUrl}");

            return (factory, client, baseUrl);
        }
    }
}