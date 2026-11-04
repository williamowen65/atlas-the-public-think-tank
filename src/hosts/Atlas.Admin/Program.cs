using Atlas.Admin;
using Atlas.Application.Storage;
using Atlas.Identity;
using Atlas.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

// Dedicated, non-interactive cloud administration entry point. These are the
// only three currently exposed commands; no user-facing Atlas.Console menu
// or web endpoints are included in this deployed executable.
if (args.Length != 1 ||
    args[0] is not ("--bootstrap-cloud-identities" or "--setup-database" or "--seed-demo"))
{
    Console.Error.WriteLine("Expected exactly one supported Atlas.Admin maintenance command.");
    return 2;
}

var configuration = new ConfigurationBuilder().AddEnvironmentVariables().Build();

try
{
    string Required(string key) =>
        !string.IsNullOrWhiteSpace(configuration[key]) ? configuration[key]! :
        throw new InvalidOperationException("Missing protected configuration: " + key);

    var variant = Required("ATLAS_DEPLOYMENT_VARIANT");
    if (variant is not ("live" or "demo"))
        throw new InvalidOperationException("Expected a live or demo database variant.");

    var expectedDatabase = Required("ATLAS_EXPECTED_DATABASE");
    if (!expectedDatabase.EndsWith("-" + variant, StringComparison.Ordinal))
        throw new InvalidOperationException("The expected database name must match its variant.");

    var connectionString = Required("ATLAS_SQL_CONNECTION_STRING");
    var sql = new SqlConnectionStringBuilder(connectionString);
    var adminClientId = Guid.Parse(Required("ATLAS_ADMIN_MI_CLIENT_ID"));
    if (!string.Equals(sql.InitialCatalog, expectedDatabase, StringComparison.Ordinal)
        || !sql.DataSource.EndsWith(".database.windows.net,1433", StringComparison.OrdinalIgnoreCase)
        || !string.Equals(sql.UserID, adminClientId.ToString(), StringComparison.OrdinalIgnoreCase)
        || !connectionString.Contains("Authentication=Active Directory Managed Identity", StringComparison.OrdinalIgnoreCase)
        || sql.TrustServerCertificate || sql.Encrypt != SqlConnectionEncryptOption.Mandatory)
        throw new InvalidOperationException("Cloud administration requires the expected Azure SQL database and managed identity.");

    switch (args[0])
    {
        case "--bootstrap-cloud-identities":
            await CloudDatabaseBootstrap.RunAsync(configuration);
            break;

        case "--setup-database":
            using (var database = new AtlasDataContext(
                new DbContextOptionsBuilder<AtlasDataContext>()
                    .UseSqlServer(connectionString).Options))
            {
                AtlasDatabaseSetup.Apply(database);
            }
            Console.WriteLine("Schema and SQL Full-Text initialization completed; background index population may continue.");
            break;

        case "--seed-demo":
            if (variant != "demo")
                throw new InvalidOperationException("Demo seeding is forbidden in live.");
            _ = Required("ATLAS_DEMO_PASSWORD");
            // Schema changes belong exclusively to --setup-database, not the seed job.
            SqlStorage.Configure(connectionString, applyMigrations: false);
            using (var services = new ServiceCollection()
                .AddAtlasIdentity(connectionString)
                .BuildServiceProvider())
            {
                await SqlStorage.SeedDemoDataAsync(services.GetRequiredService<IServiceScopeFactory>());
            }
            Console.WriteLine("Demo data initialized once; existing data is never overwritten.");
            break;
    }

    return 0;
}
catch (Exception error)
{
    // Error details may contain SQL metadata; don't output exception messages
    // from a privileged hosted command to the public Actions log.
    Console.Error.WriteLine("Atlas.Admin operation failed. Check Azure WebJob diagnostic logs and database configuration.");
    Console.Error.WriteLine(error.GetType().Name);
    return 1;
}
