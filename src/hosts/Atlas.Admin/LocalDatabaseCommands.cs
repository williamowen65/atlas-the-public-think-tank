using Atlas.Application.Storage;
using Atlas.Identity;
using Atlas.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Admin;

/// <summary>
/// Explicit local development database initialization. This is never called
/// by the Azure WebJob entry point; cloud commands have independent guards.
/// </summary>
internal static class LocalDatabaseCommands
{
    public static async Task InitializeAsync(IConfiguration configuration, bool seedDemo)
    {
        if (string.Equals(configuration["DOTNET_ENVIRONMENT"], "Production", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(configuration["ASPNETCORE_ENVIRONMENT"], "Production", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Local initialization is not available in a Production environment.");

        var connectionString = configuration["ATLAS_SQL_CONNECTION_STRING"];
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                "Set ATLAS_SQL_CONNECTION_STRING in the shared .NET User Secrets store or local environment.");

        var sql = new SqlConnectionStringBuilder(connectionString);
        if (string.IsNullOrWhiteSpace(sql.DataSource) || string.IsNullOrWhiteSpace(sql.InitialCatalog))
            throw new InvalidOperationException("Local SQL connection must specify a server and database.");

        // An explicit local developer command must never silently target
        // private or public Azure SQL through a stale connection string.
        if (sql.DataSource.Contains(".database.windows.net", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                "Local initialization refuses Azure SQL. Use the protected AtlasAdmin GitHub workflow.");

        Console.WriteLine("Initializing local Atlas SQL schema and Full-Text Search.");
        using (var database = new AtlasDataContext(
            new DbContextOptionsBuilder<AtlasDataContext>()
                .UseSqlServer(connectionString).Options))
        {
            AtlasDatabaseSetup.Apply(database);
        }
        Console.WriteLine("Local schema setup complete. Full-Text index population may continue in the background.");

        if (!seedDemo)
            return;

        // Seed requires an empty database. Never overwrite existing rows.
        // Passing the configured password explicitly ensures that .NET User
        // Secrets work; the seed implementation needn't access environment vars.
        SqlStorage.Configure(connectionString, applyMigrations: false);
        using var services = new ServiceCollection()
            .AddAtlasIdentity(connectionString)
            .BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        await SqlStorage.SeedDemoDataAsync(
            services.GetRequiredService<IServiceScopeFactory>(),
            configuration["ATLAS_DEMO_PASSWORD"]);
        Console.WriteLine("Local demo accounts and content seeded successfully.");
    }
}
