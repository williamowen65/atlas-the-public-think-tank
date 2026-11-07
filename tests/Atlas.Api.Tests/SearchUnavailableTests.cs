using System.Net;
using System.Reflection;
using Microsoft.Data.SqlClient;
using System.Text.Json;
using Atlas.Discovery;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Api.Tests;

[TestClass]
public sealed class SearchUnavailableTests
{
    [TestMethod]
    public async Task Search_dependency_failure_returns_safe_503_problem_instead_of_empty_results()
    {
        using var root = new ApiFactory();
        using var factory = root.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
            services.AddSingleton<IDiscoveryReadQuery>(new UnavailableSearch())));
        using var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false });
        var response = await client.GetAsync("/api/nodes?SearchText=salmon");
        Assert.AreEqual(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        using var problem = JsonDocument.Parse(body);
        Assert.AreEqual("search_unavailable", problem.RootElement.GetProperty("code").GetString());
        StringAssert.Contains(problem.RootElement.GetProperty("title").GetString()!, "Text search is temporarily unavailable");
        Assert.IsFalse(body.Contains("private SQL diagnostic", StringComparison.Ordinal));
    }

    [TestMethod]
    [DataRow(40613, false, "database_resuming", 503)]
    [DataRow(40613, true, "database_resuming", 503)]
    [DataRow(18456, false, "request_failed", 500)]
    [DataRow(-2, false, "request_failed", 500)]
    [DataRow(40501, false, "request_failed", 500)]
    public async Task Only_database_unavailable_errors_receive_resume_feedback(
        int sqlNumber, bool wrapped, string expectedCode, int expectedStatus)
    {
        using var root = new ApiFactory();
        using var factory = root.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
            services.AddSingleton<IDiscoveryReadQuery>(new UnavailableSearch(() =>
                wrapped ? new SearchUnavailableException(SqlFailure(sqlNumber)) : SqlFailure(sqlNumber)))));
        using var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
            { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false });
        var response = await client.GetAsync("/api/nodes");
        Assert.AreEqual(expectedStatus, (int)response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        using var problem = JsonDocument.Parse(body);
        Assert.AreEqual(expectedCode, problem.RootElement.GetProperty("code").GetString());
        if (sqlNumber == 40613) Assert.AreEqual(TimeSpan.FromSeconds(60), response.Headers.RetryAfter?.Delta);
        else Assert.IsNull(response.Headers.RetryAfter);
        Assert.IsFalse(body.Contains("private SQL diagnostic", StringComparison.Ordinal));
    }

    // SqlClient has no public constructor for server errors. Build its error collection
    // without a database connection so these HTTP checks stay deterministic.
    private static SqlException SqlFailure(int number)
    {
        var constructor = typeof(SqlError).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
            .First(ctor => ctor.GetParameters().Length == 9);
        var sqlError = (SqlError)constructor.Invoke(
            [number, (byte)0, (byte)17, "private-server", "private SQL diagnostic", "", 0, (uint)0, null]);
        var errors = (SqlErrorCollection)Activator.CreateInstance(typeof(SqlErrorCollection), nonPublic: true)!;
        typeof(SqlErrorCollection).GetMethod("Add", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(errors, [sqlError]);
        return (SqlException)typeof(SqlException).GetMethod("CreateException",
            BindingFlags.Static | BindingFlags.NonPublic, null, [typeof(SqlErrorCollection), typeof(string)], null)!
            .Invoke(null, [errors, "16.0"])!;
    }

    private sealed class UnavailableSearch(Func<Exception>? failure = null) : IDiscoveryReadQuery
    {
        private Exception Failure() => failure?.Invoke() ?? new SearchUnavailableException(new InvalidOperationException("private SQL diagnostic"));
        public DiscoveryPage DiscoverPage(DiscoveryQuery query) => throw Failure();
        public DiscoveryPage DiscoverAuthoredPage(DiscoveryQuery query, IReadOnlyCollection<Guid> authoredNodeIds) => throw Failure();
        public IReadOnlyList<RankedDiscoveryItem> Discover(DiscoveryQuery query) => throw Failure();
        public IReadOnlyList<RankedDiscoveryItem> DiscoverAuthored(DiscoveryQuery query, IReadOnlyCollection<Guid> authoredNodeIds) => throw Failure();
    }
}
