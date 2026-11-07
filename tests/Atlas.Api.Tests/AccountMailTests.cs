using Atlas.Api;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Api.Tests;

[TestClass]
public sealed class AccountMailTests
{
    [TestMethod]
    public async Task Recovery_test_host_can_exercise_more_than_twenty_auth_steps()
    {
        using var factory = new ApiFactory(disableAuthLimiter: true);
        using var client = factory.Browser();
        for (var step = 0; step < 25; step++)
            Assert.AreEqual(HttpStatusCode.OK, (await client.GetAsync("/api/auth/csrf")).StatusCode);
    }

    // Exercise the deployed ASP.NET host, not just Vite's permissive SPA fallback.
    [TestMethod]
    public async Task Email_routes_serve_the_spa_but_unknown_routes_stay_404()
    {
        var directory = Path.Combine(Path.GetTempPath(), "atlas-email-route-" + Guid.NewGuid());
        Directory.CreateDirectory(directory);
        await File.WriteAllTextAsync(Path.Combine(directory, "index.html"), "<html>email-route-test</html>");
        try
        {
            using var root = new ApiFactory();
            using var factory = root.WithWebHostBuilder(builder => builder.UseWebRoot(directory));
            using var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
                { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false });
            foreach (var path in new[] { "/confirm-email", "/forgot-password", "/reset-password" })
            {
                var response = await client.GetAsync(path);
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, path);
                StringAssert.Contains(await response.Content.ReadAsStringAsync(), "email-route-test");
            }
            Assert.AreEqual(HttpStatusCode.NotFound, (await client.GetAsync("/unknown-email-page")).StatusCode);
            Assert.AreEqual(HttpStatusCode.NotFound, (await client.GetAsync("/api/unknown-email-endpoint")).StatusCode);
        }
        finally { Directory.Delete(directory, recursive: true); }
    }

    [TestMethod]
    [DataRow("Development", "http://localhost:5173", true)]
    [DataRow("Development", "http://127.0.0.1:5173", true)]
    [DataRow("Development", "http://example.com", false)]
    [DataRow("Production", "http://localhost:5173", false)]
    public void Http_mail_links_are_allowed_only_for_development_loopback(string environment, string origin, bool expected)
    {
        var config = new Microsoft.Extensions.Configuration.ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["Atlas:AccountMail:Transport"] = "Smtp",
                ["Atlas:AccountMail:Host"] = "smtp.example",
                ["Atlas:AccountMail:Username"] = "atlas@example.com",
                ["Atlas:AccountMail:From"] = "atlas@example.com",
                ["Atlas:AccountMail:Password"] = "test-only",
                ["Atlas:AccountMail:PublicOrigin"] = origin
            }).Build();
        var host = new MailEnvironment { EnvironmentName = environment };
        var mail = new AccountMail(config, new DevelopmentAccountMail(host, config), host);
        Assert.AreEqual(expected, mail.IsAvailable);
    }

    private sealed class MailEnvironment : IWebHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Development";
        public string ApplicationName { get; set; } = "Atlas.Api";
        public string ContentRootPath { get; set; } = ".";
        public string WebRootPath { get; set; } = ".";
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = new Microsoft.Extensions.FileProviders.NullFileProvider();
        public Microsoft.Extensions.FileProviders.IFileProvider WebRootFileProvider { get; set; } = new Microsoft.Extensions.FileProviders.NullFileProvider();
    }

    // A weak password must produce useful policy feedback without touching SQL.
    [TestMethod]
    public async Task Registration_returns_password_requirements_before_database_access()
    {
        using var factory = new ApiFactory();
        using var client = factory.Browser();
        var csrf = await client.GetFromJsonAsync<JsonElement>("/api/auth/csrf");
        client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", csrf.GetProperty("token").GetString());
        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "test@example.com", password = "abc", displayName = "Test"
        });
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        var errors = problem.GetProperty("errors").GetProperty("Password").EnumerateArray()
            .Select(error => error.GetString()).ToArray();
        Assert.IsTrue(errors.Any(error => error!.Contains("12")));
        Assert.IsTrue(errors.Length > 1);
    }

    // Invalid explicit SMTP configuration must never silently fall back to local files.
    [TestMethod]
    [DataRow("http://atlas.example", false)]
    [DataRow("https://atlas.example/path", false)]
    [DataRow("https://atlas.example/?redirect=elsewhere", false)]
    [DataRow("https://atlas.example", true)]
    public void Smtp_requires_a_configured_https_origin(string origin, bool expected)
    {
        using var root = new ApiFactory();
        using var factory = root.WithWebHostBuilder(b => b
            .UseSetting("Atlas:AccountMail:Transport", "Smtp")
            .UseSetting("Atlas:AccountMail:Host", "smtp.example")
            .UseSetting("Atlas:AccountMail:Username", "test")
            .UseSetting("Atlas:AccountMail:Password", "not-a-real-secret")
            .UseSetting("Atlas:AccountMail:From", "atlas@example.com")
            .UseSetting("Atlas:AccountMail:PublicOrigin", origin));
        using var scope = factory.Services.CreateScope();
        Assert.AreEqual(expected, scope.ServiceProvider.GetRequiredService<AccountMail>().IsAvailable);
    }
}
