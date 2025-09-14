using AngleSharp;
using AngleSharp.Dom;
using atlas_the_public_think_tank.Data;
using atlas_the_public_think_tank.Data.DatabaseEntities.Users;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace CloudTests.TestingSetup
{
    public class TestEnvironment
    {
        private WebApplicationFactory<atlas_the_public_think_tank.Program> _factory;
        private IServiceScope _scope;
        private string _baseUrl;
        public HttpClient _client;
        public ApplicationDbContext _db;
        public CookieContainer _cookieContainer;
        public string _connectionString; // dynamic testing connection string

        public TestEnvironment() : this(null)
        { 
            // overload
        }

        public TestEnvironment(string appSettingsOverride)
        {
            _cookieContainer = new CookieContainer();

            (_factory, _client, _baseUrl, _connectionString) = TestEnvironmentUtility.ConfigureTestEnvironment(appSettingsOverride, _cookieContainer);

            // Create a scope from the factory's service provider and resolve the SQL Server DbContext
            _scope = _factory.Services.CreateScope();
            _db = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Apply migrations to the test database (safe if already applied)
            _db.Database.Migrate();
        }

        public async Task<TResult> fetchPost<TResult, TPayload>(string url, TPayload payload)
        {
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            var response = await SendRequestAsync(HttpMethod.Post, url, jsonContent);
            var json = await response.Content.ReadAsStringAsync();

#pragma warning disable CS8603
            return JsonSerializer.Deserialize<TResult>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
#pragma warning restore CS8603
        }

        public async Task<IDocument> fetchHTML(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();
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
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
        }

        public void SetCookie(string name, string value, string path = "/", string domain = null)
        {
            var encodedValue = HttpUtility.UrlEncode(value);
            var uri = new Uri(_baseUrl);
            _cookieContainer.Add(uri, new Cookie(name, encodedValue, path));
            Console.WriteLine($"Cookie set in container: {name}={encodedValue}");
        }

        public void SetCookie<T>(string name, T value, string path = "/")
        {
            var jsonValue = JsonSerializer.Serialize(value);
            var encodedValue = HttpUtility.UrlEncode(jsonValue);
            var uri = new Uri(_baseUrl);
            _cookieContainer.Add(uri, new Cookie(name, encodedValue, path));
            Console.WriteLine($"Complex cookie set in container: {name}={encodedValue}");
        }

        public string GetCookie(string name)
        {
            var uri = new Uri(_baseUrl);
            var cookies = _cookieContainer.GetCookies(uri);
            foreach (Cookie cookie in cookies)
            {
                if (cookie.Name == name)
                {
                    return HttpUtility.UrlDecode(cookie.Value);
                }
            }
            return null;
        }

        public T GetCookie<T>(string name)
        {
            var cookieValue = GetCookie(name);
            if (string.IsNullOrEmpty(cookieValue)) return default;
            return JsonSerializer.Deserialize<T>(cookieValue);
        }

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
                        cookies[cookieParts[0]] = HttpUtility.UrlDecode(cookieParts[1]);
                    }
                }
            }
            return cookies;
        }

        public async Task<HttpResponseMessage> SendRequestAsync(HttpMethod method, string url, HttpContent content = null)
        {
            var request = new HttpRequestMessage(method, url);
            if (content != null)
            {
                request.Content = content;
            }
            var uri = new Uri(_baseUrl);
            var cookieHeader = _cookieContainer.GetCookieHeader(uri);
            Console.WriteLine($"Request to {uri}. Cookies: {cookieHeader}");
            return await _client.SendAsync(request);
        }

        public async Task<HttpResponseMessage> PostFormAsync(string url, List<KeyValuePair<string, string>> formData)
        {
            var content = new FormUrlEncodedContent(formData);
            return await SendRequestAsync(HttpMethod.Post, url, content);
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
            if (_cookieContainer != null && request.RequestUri != null)
            {
                var cookieHeader = _cookieContainer.GetCookieHeader(request.RequestUri);
                if (!string.IsNullOrEmpty(cookieHeader))
                {
                    if (request.Headers.Contains("Cookie"))
                    {
                        request.Headers.Remove("Cookie");
                    }
                    request.Headers.Add("Cookie", cookieHeader);
                    Console.WriteLine($"Added cookies to request: {cookieHeader}");
                }
            }

            var response = await base.SendAsync(request, cancellationToken);

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
        public static (WebApplicationFactory<atlas_the_public_think_tank.Program> factory, HttpClient client, string baseUrl, string connectionString)
            ConfigureTestEnvironment(string appSettingsOverride, CookieContainer cookieContainer = null)
        {
            string baseUrl = "https://localhost:5501";

            // Build dynamic database name per test run
            var dbName = $"atlas_the_public_think_tank-testing-{Guid.NewGuid()}";

            // Local default (LocalDB, Windows auth)
            var localConnection =
                $"Server=(localdb)\\mssqllocaldb;Database={dbName};Trusted_Connection=True;MultipleActiveResultSets=true";

            // GitHub Actions / container SQL (SA user)
            var actionsPassword = Environment.GetEnvironmentVariable("TEST_SQL_SA_PASSWORD") ?? "Aa123456!";
            var actionsConnection =
                $"Server=localhost,1433;Database={dbName};User Id=sa;Password={actionsPassword};TrustServerCertificate=True;Encrypt=False;";

            // Detect CI / GitHub Actions
            var githubFlag =
                Environment.GetEnvironmentVariable("GITHUB_ACTIONS") ?? null;

            var runningInGithub = string.Equals(githubFlag, "true", StringComparison.OrdinalIgnoreCase);

            var connectionString = runningInGithub ? actionsConnection : localConnection;

            Console.WriteLine($"[TestEnvironment] RunningInGitHub={runningInGithub}. Using dynamic test database: {dbName}");
            Console.WriteLine($"[TestEnvironment] Connection (sanitized): {(runningInGithub ? "Actions SQL" : "LocalDB")}");

            var factory = new WebApplicationFactory<atlas_the_public_think_tank.Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.UseUrls(baseUrl);
                    builder.UseEnvironment("Testing");


                    // Set the builder.Configuration variable based on applySeedData arg
                    builder.ConfigureAppConfiguration((context, config) =>
                    {
                        if (!string.IsNullOrEmpty(appSettingsOverride))
                        {
                            using var doc = JsonDocument.Parse(appSettingsOverride);
                            var dict = new Dictionary<string, string?>();

                            foreach (var prop in doc.RootElement.EnumerateObject())
                            {
                                if (prop.Value.ValueKind == JsonValueKind.Object)
                                {
                                    foreach (var subProp in prop.Value.EnumerateObject())
                                    {
                                        dict[$"{prop.Name}:{subProp.Name}"] = subProp.Value.ToString();
                                    }
                                }
                                else
                                {
                                    dict[prop.Name] = prop.Value.ToString();
                                }
                            }

                            config.AddInMemoryCollection(dict);
                        }
                    });


                    builder.ConfigureLogging(logging =>
                    {
                        logging.ClearProviders();
                        logging.SetMinimumLevel(LogLevel.Warning);
                    });

                    builder.ConfigureServices(services =>
                    {
                        // Remove existing DbContext registrations added in Program
                        var toRemove = services
                            .Where(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) ||
                                        d.ServiceType == typeof(ApplicationDbContext))
                            .ToList();
                        foreach (var descriptor in toRemove)
                        {
                            services.Remove(descriptor);
                        }

                        services.AddDbContext<ApplicationDbContext>(options =>
                            options.UseSqlServer(connectionString));

                        services.AddControllers()
                                .AddApplicationPart(typeof(TestController).Assembly);
                    });
                });

            var innerHandler = factory.Server.CreateHandler();
            var delegatingHandler = new CookieHandlingDelegatingHandler(cookieContainer, innerHandler);

            var client = new HttpClient(delegatingHandler)
            {
                BaseAddress = new Uri(baseUrl)
            };

            baseUrl = client.BaseAddress.ToString().TrimEnd('/');
            Console.WriteLine($"Test server URL: {baseUrl}");

            return (factory, client, baseUrl, connectionString);
        }
    }
}