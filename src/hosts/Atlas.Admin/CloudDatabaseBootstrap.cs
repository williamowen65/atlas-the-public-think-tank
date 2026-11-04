using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Atlas.Admin;

/// <summary>
/// First-time Azure SQL authorization, invoked ONLY by the dedicated
/// Atlas.Admin WebJob with temporary SQL provisioner credentials. Never by the
/// public API or the local interactive Atlas.Console.
/// Creates contained Microsoft Entra users without Microsoft Graph directory
/// lookup (Azure SQL CREATE USER WITH SID = ..., TYPE = E).
/// </summary>
internal static class CloudDatabaseBootstrap
{
    public static async Task RunAsync(IConfiguration configuration)
    {
        var variant = configuration["ATLAS_DEPLOYMENT_VARIANT"];
        if (variant is not ("live" or "demo"))
            throw new InvalidOperationException("Bootstrap requires a live/demo deployment variant.");

        var expectedDatabase = Required(configuration, "ATLAS_EXPECTED_DATABASE");
        if (!expectedDatabase.EndsWith("-" + variant, StringComparison.Ordinal)
            || !string.Equals(expectedDatabase, expectedDatabase.ToLowerInvariant(), StringComparison.Ordinal))
            throw new InvalidOperationException("Invalid or unexpected deployment database name.");

        var runtime = new SqlConnectionStringBuilder(Required(configuration, "ATLAS_SQL_CONNECTION_STRING"));
        if (!string.Equals(runtime.InitialCatalog, expectedDatabase, StringComparison.Ordinal)
            || !runtime.DataSource.Contains(".database.windows.net", StringComparison.OrdinalIgnoreCase)
            || runtime.TrustServerCertificate)
            throw new InvalidOperationException("Bootstrap database or SQL server did not match the protected environment.");

        var runtimeClientId = Guid.Parse(Required(configuration, "ATLAS_RUNTIME_MI_CLIENT_ID"));
        var adminClientId = Guid.Parse(Required(configuration, "ATLAS_ADMIN_MI_CLIENT_ID"));
        if (runtimeClientId == Guid.Empty || adminClientId == Guid.Empty || runtimeClientId == adminClientId)
            throw new InvalidOperationException("Each SQL role requires a distinct nonempty managed identity client ID.");

        var admin = new SqlConnectionStringBuilder
        {
            DataSource = runtime.DataSource,
            InitialCatalog = expectedDatabase,
            UserID = Required(configuration, "ATLAS_BOOTSTRAP_SQL_USERNAME"),
            Password = Required(configuration, "ATLAS_BOOTSTRAP_SQL_PASSWORD"),
            Encrypt = SqlConnectionEncryptOption.Mandatory,
            TrustServerCertificate = false,
            ConnectTimeout = 30
        };

        await using var connection = new SqlConnection(admin.ConnectionString);
        System.Console.WriteLine("Opening the Azure SQL bootstrap connection.");
        await connection.OpenAsync();
        System.Console.WriteLine("Connected to Azure SQL. Configuring managed-identity database users.");

        // No passwords, SQL statement bodies or account identifiers are printed.
        await EnsureUserAndRoleAsync(connection, "atlas-" + variant + "-runtime", runtimeClientId, "db_datareader");
        await EnsureUserAndRoleAsync(connection, "atlas-" + variant + "-runtime", runtimeClientId, "db_datawriter");
        await EnsureUserAndRoleAsync(connection, "atlas-" + variant + "-admin", adminClientId, "db_owner");
        await using var execute = new SqlCommand(
            "GRANT EXECUTE TO [" + "atlas-" + variant + "-runtime" + "]", connection);
        await execute.ExecuteNonQueryAsync();

        System.Console.WriteLine("Azure SQL contained managed-identity users verified for " + variant + ".");
        System.Console.WriteLine("Remove ATLAS_BOOTSTRAP_SQL_* settings by disabling enable_sql_bootstrap and applying Terraform.");
    }

    private static string Required(IConfiguration configuration, string key)
        => !string.IsNullOrWhiteSpace(configuration[key]) ? configuration[key]!
            : throw new InvalidOperationException("Missing required bootstrap setting: " + key);

    private static async Task EnsureUserAndRoleAsync(
        SqlConnection connection, string name, Guid clientId, string role)
    {
        // All values are SQL parameters; only the two known roles are supported.
        if (role is not ("db_datareader" or "db_datawriter" or "db_owner"))
            throw new ArgumentOutOfRangeException(nameof(role));

        const string sql = """
            IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = @Principal)
            BEGIN
                DECLARE @Sid VARBINARY(16) = CONVERT(VARBINARY(16), @ClientId);
                DECLARE @Create NVARCHAR(MAX) = N'CREATE USER ' + QUOTENAME(@Principal) +
                    N' WITH SID = ' + CONVERT(VARCHAR(MAX), @Sid, 1) + N', TYPE = E;';
                EXEC (@Create);
            END;
            IF NOT EXISTS (
                SELECT 1 FROM sys.database_role_members rm
                JOIN sys.database_principals r ON r.principal_id = rm.role_principal_id
                JOIN sys.database_principals u ON u.principal_id = rm.member_principal_id
                WHERE r.name = @Role AND u.name = @Principal
            )
            BEGIN
                DECLARE @Grant NVARCHAR(MAX) =
                    N'ALTER ROLE ' + QUOTENAME(@Role) + N' ADD MEMBER ' + QUOTENAME(@Principal) + N';';
                EXEC (@Grant);
            END;
            """;
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Principal", name);
        command.Parameters.AddWithValue("@ClientId", clientId);
        command.Parameters.AddWithValue("@Role", role);
        await command.ExecuteNonQueryAsync();

        // An old DB user with the same name but a different SID must never
        // silently receive permissions meant for a new Azure managed identity.
        await using var verify = new SqlCommand("""
            SELECT CASE WHEN sid = CONVERT(VARBINARY(16), @ClientId) AND type = 'E'
                THEN 1 ELSE 0 END
            FROM sys.database_principals
            WHERE name = @Principal
            """, connection);
        verify.Parameters.AddWithValue("@Principal", name);
        verify.Parameters.AddWithValue("@ClientId", clientId);
        var mapping = await verify.ExecuteScalarAsync();
        if (mapping is not int valid || valid != 1)
            throw new InvalidOperationException("An existing Azure SQL principal does not match the intended managed identity.");
    }
}
