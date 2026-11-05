using Atlas.Admin;
using Atlas.Application.Storage;
using Atlas.Identity;
using Atlas.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

// Atlas.Admin is deliberately non-interactive: local development commands
// use the existing .NET User Secrets store, whereas Azure WebJobs use only
// protected environment configuration and separate managed identities.
if (args is ["--initialize-local-database"] or ["--initialize-local-database", "--seed-demo"])
{
    try
    {
        var localConfiguration = new ConfigurationBuilder()
            .AddUserSecrets<Program>(optional: true)
            .AddEnvironmentVariables()
            .Build();
        await LocalDatabaseCommands.InitializeAsync(localConfiguration, seedDemo: args.Length == 2);
        return 0;
    }
    catch (Exception error)
    {
        Console.Error.WriteLine("Local database initialization failed: " + error.Message);
        return 1;
    }
}

if (args.Length != 1 ||
    args[0] is not ("--bootstrap-cloud-identities" or "--diagnose-cloud-connection" or "--setup-database" or "--seed-demo"))
{
    Console.Error.WriteLine("Usage: Atlas.Admin --initialize-local-database [--seed-demo] (local only)");
    Console.Error.WriteLine("Azure WebJobs: --diagnose-cloud-connection | --bootstrap-cloud-identities | --setup-database | --seed-demo");
    return 2;
}

// Cloud operations intentionally do NOT load development User Secrets.
var configuration = new ConfigurationBuilder().AddEnvironmentVariables().Build();

var phase = "configuration validation";
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
        case "--diagnose-cloud-connection":
            phase = "managed-identity token diagnostic";
            await ManagedIdentityDiagnostic.RunAsync(configuration);
            break;

        case "--bootstrap-cloud-identities":
            phase = "identity bootstrap";
            await CloudDatabaseBootstrap.RunAsync(configuration);
            break;

        case "--setup-database":
            using (var database = new AtlasDataContext(
                new DbContextOptionsBuilder<AtlasDataContext>()
                    .UseSqlServer(connectionString).Options))
            {
                phase = "schema connection opening (managed identity)";
                // WORKAROUND (2026-11-05, see infrastructure/DEPLOYMENT.md):
                // Bootstrap showed first-login resets with a later
                // successful connection. Cause remains unresolved. Retry ONLY
                // opening this context's connection, never the migration work.
                Console.WriteLine("Schema: opening managed-identity SQL connection before database changes (bounded retry workaround; reset cause unresolved).");
                await CloudDatabaseBootstrap.OpenConnectionWithRetryAsync(
                    () => database.Database.OpenConnectionAsync(), operation: "Schema");
                try
                {
                    // Keep this connection open: a separate readiness probe would
                    // not protect the connection actually used by migrations.
                    phase = "schema migrations and supplemental SQL initialization";
                    Console.WriteLine("Schema: connection opened; applying migrations and supplemental SQL objects.");
                    AtlasDatabaseSetup.Apply(database);
                }
                finally
                {
                    await database.Database.CloseConnectionAsync();
                }
            }
            Console.WriteLine("Schema and SQL Full-Text initialization completed; background index population may continue.");
            break;

        case "--seed-demo":
            phase = "demo seeding";
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
    Console.Error.WriteLine($"Atlas.Admin operation failed during {phase}.");
    // Provider messages are useful for distinguishing transport, identity and SQL
    // failures. Redact configured credentials before writing protected job logs.
    var redaction = new SqlConnectionStringBuilder();
    try
    {
        redaction = new SqlConnectionStringBuilder(configuration["ATLAS_SQL_CONNECTION_STRING"]);
    }
    catch (ArgumentException) { /* Configuration validation may itself have failed. */ }
    for (Exception? cause = error; cause is not null; cause = cause.InnerException)
    {
        var message = cause.Message;
        foreach (var key in new[] { "ATLAS_BOOTSTRAP_SQL_PASSWORD", "ATLAS_BOOTSTRAP_SQL_USERNAME", "ATLAS_DEMO_PASSWORD" })
        {
            var secret = configuration[key];
            if (!string.IsNullOrEmpty(secret))
                message = message.Replace(secret, "[redacted]", StringComparison.OrdinalIgnoreCase);
        }
        Console.Error.WriteLine($"{cause.GetType().Name}: {CloudDatabaseBootstrap.RedactDiagnosticMessage(message, redaction)}");
        if (cause is SqlException sqlError)
        {
            Console.Error.WriteLine($"SQL client connection ID: {sqlError.ClientConnectionId}.");
            foreach (SqlError detail in sqlError.Errors)
                Console.Error.WriteLine($"SQL error number={detail.Number}, state={detail.State}, class={detail.Class}.");
        }
        if (cause is System.Net.Sockets.SocketException socket)
            Console.Error.WriteLine($"SQL transport socket error={socket.SocketErrorCode}, native error={socket.NativeErrorCode}.");
    }
    return 1;
}
