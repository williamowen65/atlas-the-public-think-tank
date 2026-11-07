using Atlas.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Api.Tests;

[TestClass]
public sealed class AccountMailTests
{
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
