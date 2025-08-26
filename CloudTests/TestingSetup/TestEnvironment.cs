using AngleSharp;
using AngleSharp.Dom;
using atlas_the_public_think_tank.Data;
using atlas_the_public_think_tank.Data.DatabaseEntities.Users;
 
using CloudTests.UnitTesting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace CloudTests.TestingSetup
{
    public class TestEnvironment
    {
        private WebApplicationFactory<atlas_the_public_think_tank.Program> _factory;
        private SqliteTestFixture _sqliteFixture;
        private string _baseUrl;
        public HttpClient _client;
        public ApplicationDbContext _db;
        public CookieContainer _cookieContainer;

        public TestEnvironment() {
            // Create SQLite test fixture
            _sqliteFixture = new SqliteTestFixture();
            
            // Initialize cookie container
            _cookieContainer = new CookieContainer();
            
            // Use the utility class to configure the test environment
            (_factory, _client, _baseUrl) = TestEnvironmentUtility.ConfigureTestEnvironment(_sqliteFixture, _cookieContainer);
            _db = _sqliteFixture.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        }

        /// <summary>
        /// Sends a POST request to the specified URL with JSON payload and returns the deserialized response
        /// </summary>
        /// <typeparam name="TResult">The type to deserialize the response to</typeparam>
        /// <typeparam name="TPayload">The type of the payload to send</typeparam>
        /// <param name="url">The URL to post to</param>
        /// <param name="payload">The payload to send as JSON</param>
        /// <returns>The deserialized response</returns>
        public async Task<TResult> fetchPost<TResult, TPayload>(string url, TPayload payload)
        {
            // Create the content from the payload
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            // Send the request
            var response = await SendRequestAsync(HttpMethod.Post, url, jsonContent);

            // Read the response content
            var json = await response.Content.ReadAsStringAsync();

            // Deserialize and return the result
            #pragma warning disable CS8603 // Possible null reference return.
            return JsonSerializer.Deserialize<TResult>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            #pragma warning restore CS8603 // Possible null reference return.
        }

        public async Task<IDocument> fetchHTML(string url)
        {
            // Create a new request message to ensure cookies are attached
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await _client.SendAsync(request);

            response.EnsureSuccessStatusCode(); // Will throw if not 2xx
            var html = await response.Content.ReadAsStringAsync();

            return await TextHtmlToDocument(html);
        }


        public async Task<IDocument> TextHtmlToDocument(string html)
        {
            var context = BrowsingContext.New(Configuration.Default);
            var document = await context.OpenAsync(req => req.Content(html));
            return document;
        }

        public async Task<T> fetchJson<T>(string url)
        {
            // Create a new request message to ensure cookies are attached
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await _client.SendAsync(request);

            response.EnsureSuccessStatusCode(); // Will throw if not 2xx
            var json = await response.Content.ReadAsStringAsync();
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
        }

        /// <summary>
        /// Sets a cookie in the cookie container for subsequent requests
        /// </summary>
        /// <param name="name">The name of the cookie</param>
        /// <param name="value">The value of the cookie</param>
        /// <param name="path">The path for the cookie (defaults to "/")</param>
        /// <param name="domain">Optional domain for the cookie</param>
        public void SetCookie(string name, string value, string path = "/", string domain = null)
        {
            // URL encode the value to handle special characters
            var encodedValue = HttpUtility.UrlEncode(value);

            // For localhost, we need to use a different approach
            var uri = new Uri(_baseUrl);

            // Create a cookie and add it directly to the CookieContainer with the URI
            _cookieContainer.Add(uri, new Cookie(name, encodedValue, path));
            
            // Debug output to verify cookie was set
            Console.WriteLine($"Cookie set in container: {name}={encodedValue}");
        }

        /// <summary>
        /// Sets a structured cookie value that will be serialized to JSON
        /// </summary>
        /// <typeparam name="T">The type of the object to serialize</typeparam>
        /// <param name="name">The name of the cookie</param>
        /// <param name="value">The object to serialize as the cookie value</param>
        /// <param name="path">The path for the cookie (defaults to "/")</param>
        public void SetCookie<T>(string name, T value, string path = "/")
        {
            // Serialize the object to JSON
            var jsonValue = JsonSerializer.Serialize(value);

            // URL encode the JSON string to handle special characters
            var encodedValue = HttpUtility.UrlEncode(jsonValue);

            // For localhost, we need to use a different approach
            var uri = new Uri(_baseUrl);

            // Create a cookie and add it directly to the CookieContainer with the URI
            _cookieContainer.Add(uri, new Cookie(name, encodedValue, path));
            
            // Debug output to verify cookie was set
            Console.WriteLine($"Complex cookie set in container: {name}={encodedValue}");
        }

        /// <summary>
        /// Gets a cookie value from the cookie container
        /// </summary>
        /// <param name="name">The name of the cookie to retrieve</param>
        /// <returns>The cookie value or null if not found</returns>
        public string GetCookie(string name)
        {
            var uri = new Uri(_baseUrl);
            var cookies = _cookieContainer.GetCookies(uri);

            foreach (Cookie cookie in cookies)
            {
                if (cookie.Name == name)
                {
                    // URL decode the value to get the original content
                    return HttpUtility.UrlDecode(cookie.Value);
                }
            }

            return null;
        }

        /// <summary>
        /// Gets a cookie value and deserializes it from JSON
        /// </summary>
        /// <typeparam name="T">The type to deserialize to</typeparam>
        /// <param name="name">The name of the cookie</param>
        /// <returns>The deserialized object or default if cookie not found</returns>
        public T GetCookie<T>(string name)
        {
            var cookieValue = GetCookie(name);
            
            if (string.IsNullOrEmpty(cookieValue))
            {
                return default;
            }
            
            return JsonSerializer.Deserialize<T>(cookieValue);
        }

        /// <summary>
        /// Gets all cookies from the response
        /// </summary>
        /// <param name="response">The HTTP response</param>
        /// <returns>Dictionary of cookie names and values</returns>
        public Dictionary<string, string> GetResponseCookies(HttpResponseMessage response)
        {
            var cookies = new Dictionary<string, string>();

            if (response.Headers.TryGetValues("Set-Cookie", out var setCookieValues))
            {
                foreach (var cookieString in setCookieValues)
                {
                    var cookieParts = cookieString.Split(';')[0].Split('=');
                    if (cookieParts.Length == 2)
                    {
                        // URL decode the cookie value
                        cookies[cookieParts[0]] = HttpUtility.UrlDecode(cookieParts[1]);
                    }
                }
            }

            return cookies;
        }

        /// <summary>
        /// Sends an HTTP request with optional cookies and content
        /// </summary>
        /// <param name="method">The HTTP method (GET, POST, etc.)</param>
        /// <param name="url">The URL to request</param>
        /// <param name="content">Optional content for the request (for POST, PUT, etc.)</param>
        /// <returns>The HTTP response</returns>
        public async Task<HttpResponseMessage> SendRequestAsync(HttpMethod method, string url, HttpContent content = null)
        {
            var request = new HttpRequestMessage(method, url);
            
            if (content != null)
            {
                request.Content = content;
            }
            
            // Debug output to help diagnose issues
            var uri = new Uri(_baseUrl);
            var cookieHeader = _cookieContainer.GetCookieHeader(uri);
            Console.WriteLine($"Request to {uri}. Cookies: {cookieHeader}");
            
            return await _client.SendAsync(request);
        }

        /// <summary>
        /// Posts form data to a URL and returns the response
        /// </summary>
        /// <param name="url">The URL to post to</param>
        /// <param name="formData">Dictionary of form field names and values</param>
        /// <returns>The HTTP response</returns>
        public async Task<HttpResponseMessage> PostFormAsync(string url, Dictionary<string, string> formData)
        {
            var content = new FormUrlEncodedContent(formData);
            return await SendRequestAsync(HttpMethod.Post, url, content);
        }
    }

    public class SqliteTestFixture : IDisposable
    {
        private readonly SqliteConnection _connection;

        public SqliteTestFixture()
        {
            Console.WriteLine("Setting up E2E tests with SQLite");

            // Create and open an in-memory SQLite connection
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            // Build the service provider
            var services = new ServiceCollection();

            // Add Identity services
            services.AddIdentity<AppUser, IdentityRole<Guid>>()
                    .AddEntityFrameworkStores<ApplicationDbContext>();

            // Add a DbContext using SQLite
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });

            ServiceProvider = services.BuildServiceProvider();

            // Create the schema and seed data
            using (var scope = ServiceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Ensure database is created
                db.Database.EnsureCreated();
            }
        }

        public IServiceProvider ServiceProvider { get; }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }

    public class CookieHandlingDelegatingHandler : DelegatingHandler
    {
        private readonly CookieContainer _cookieContainer;

        public CookieHandlingDelegatingHandler(CookieContainer cookieContainer, HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
            _cookieContainer = cookieContainer;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Get cookies for this request from the cookie container
            if (_cookieContainer != null && request.RequestUri != null)
            {
                var cookieHeader = _cookieContainer.GetCookieHeader(request.RequestUri);
                if (!string.IsNullOrEmpty(cookieHeader))
                {
                    // Make sure we don't add duplicate Cookie headers
                    if (request.Headers.Contains("Cookie"))
                    {
                        request.Headers.Remove("Cookie");
                    }
                    
                    // Add the Cookie header to the request
                    request.Headers.Add("Cookie", cookieHeader);
                    Console.WriteLine($"Added cookies to request: {cookieHeader}");
                }
            }

            // Send the request to the inner handler
            var response = await base.SendAsync(request, cancellationToken);

            // Extract cookies from the response and store them in the cookie container
            if (_cookieContainer != null && response.Headers.TryGetValues("Set-Cookie", out var setCookieValues))
            {
                foreach (var cookieValue in setCookieValues)
                {
                    if (request.RequestUri != null)
                    {
                        try
                        {
                            _cookieContainer.SetCookies(request.RequestUri, cookieValue);
                            Console.WriteLine($"Set cookie from response: {cookieValue}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error setting cookie: {ex.Message}");
                        }
                    }
                }
            }

            return response;
        }
    }
    public static class TestEnvironmentUtility
    {
        /// <summary>
        /// Configures the test environment with a SQLite database for integration testing
        /// </summary>
        /// <param name="sqliteFixture">The SQLite test fixture containing the test database</param>
        /// <param name="cookieContainer">Optional cookie container for handling cookies in tests</param>
        /// <returns>A tuple containing the WebApplicationFactory and HttpClient configured for testing</returns>
        public static (WebApplicationFactory<atlas_the_public_think_tank.Program> factory, HttpClient client, string baseUrl)
            ConfigureTestEnvironment(SqliteTestFixture sqliteFixture, CookieContainer cookieContainer = null)
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
                        // Set to Warning instead of Error to see more info
                        logging.SetMinimumLevel(LogLevel.Warning);
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
                            .AddApplicationPart(typeof(UnitTestController).Assembly);
                    });
                });

            // Get the server handler
            var innerHandler = factory.Server.CreateHandler();
            
            // Create the custom delegating handler with our cookie container
            var delegatingHandler = new CookieHandlingDelegatingHandler(cookieContainer, innerHandler);
            
            // Create HttpClient with our handler chain
            var client = new HttpClient(delegatingHandler)
            {
                BaseAddress = new Uri(baseUrl)
            };
            
            // Output the base URL for debugging
            baseUrl = client.BaseAddress.ToString().TrimEnd('/');
            Console.WriteLine($"Test server URL: {baseUrl}");

            return (factory, client, baseUrl);
        }
    }
}
