using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text.RegularExpressions;
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
    public static async Task RunDiagnosticAsync(IConfiguration configuration)
    {
        const int attempts = 5;
        var succeeded = 0;
        for (var attempt = 1; attempt <= attempts; attempt++)
        {
            System.Console.WriteLine($"SQL diagnostic attempt {attempt}/{attempts} at {DateTimeOffset.UtcNow:O}; bootstrap connection settings; default pooling enabled; no master probe.");
            try
            {
                // First login targets the same database as bootstrap, without a
                // preceding control connection. Later opens may reuse its pool.
                await RunAsync(configuration, connectionOnly: true);
                succeeded++;
                System.Console.WriteLine($"SQL diagnostic attempt {attempt}/{attempts}: succeeded.");
            }
            catch (SqlException)
            {
                // RunAsync already emits redacted details. Continue collecting
                // evidence rather than mistaking one later success for stability.
                System.Console.Error.WriteLine($"SQL diagnostic attempt {attempt}/{attempts}: failed.");
            }
            if (attempt < attempts)
                await Task.Delay(TimeSpan.FromSeconds(10));
        }
        System.Console.WriteLine($"SQL diagnostic summary: {succeeded}/{attempts} target connections succeeded; {attempts - succeeded} failed. No database changes performed.");
        if (succeeded != attempts)
            throw new InvalidOperationException("SQL diagnostic detected connection failures; inspect the per-attempt results.");
    }

    public static async Task RunAsync(IConfiguration configuration, bool connectionOnly = false)
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
        // Limit target diagnostics to the expected Azure hostname and database name formats.
        var server = Regex.Match(admin.DataSource, @"\A(?:tcp:)?([a-z0-9-]+\.database\.windows\.net)(?:,1433)?\z",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        var database = Regex.IsMatch(expectedDatabase, @"\A[a-z0-9-]+\z") ? expectedDatabase : "(unrecognized)";
        var hostname = server.Success ? server.Groups[1].Value : "(unrecognized)";
        System.Console.WriteLine($"Azure SQL connection target. Host: {hostname}, database: {database}.");

        if (connectionOnly && server.Success)
        {
            // Run inside the actual WebJob process, rather than a separate Kudu console.
            var addresses = await Dns.GetHostAddressesAsync(hostname)
                .WaitAsync(TimeSpan.FromSeconds(10));
            foreach (var address in addresses)
                System.Console.WriteLine($"SQL DNS address: {address}.");
        }

        System.Console.WriteLine($"Opening target database connection: {database}.");
        var timer = Stopwatch.StartNew();
        try
        {
            if (connectionOnly)
                await connection.OpenAsync();
            else
                await OpenConnectionWithRetryAsync(() => connection.OpenAsync());
        }
        catch (SqlException error)
        {
            if (connectionOnly)
            {
                // Only the explicit diagnostic prints provider text. Redact before
                // flattening or truncating, including every nested exception message.
                for (Exception? detail = error; detail is not null; detail = detail.InnerException)
                    System.Console.Error.WriteLine($"SQL diagnostic {detail.GetType().Name}: {RedactDiagnosticMessage(detail.Message, admin)}");
            }
            // Classify provider text without printing account names or connection strings.
            var message = error.Message;
            var phase = message.Contains("pre-login", StringComparison.OrdinalIgnoreCase)
                ? "pre-login handshake"
                : message.Contains("login failed", StringComparison.OrdinalIgnoreCase)
                    ? "login rejected" : "undetermined";
            System.Console.Error.WriteLine($"SQL connection phase: {phase}; client connection ID: {error.ClientConnectionId}.");
            foreach (SqlError detail in error.Errors)
                System.Console.Error.WriteLine($"SQL connection code: number={detail.Number}, state={detail.State}, class={detail.Class}.");
            // Provider messages can include login details. Emit structured codes only.
            for (var cause = error.InnerException; cause is not null; cause = cause.InnerException)
            {
                var code = cause switch
                {
                    SocketException socket => $"socket error={socket.SocketErrorCode}, native error={socket.NativeErrorCode}",
                    AuthenticationException => $"TLS authentication HRESULT=0x{cause.HResult:X8}",
                    Win32Exception native => $"native error={native.NativeErrorCode}",
                    _ => $"HRESULT=0x{cause.HResult:X8}"
                };
                System.Console.Error.WriteLine($"SQL connection cause: {cause.GetType().Name}, {code}.");
            }
            throw;
        }
        finally
        {
            System.Console.WriteLine($"Azure SQL connection attempt took {timer.ElapsedMilliseconds} ms.");
        }
        if (connectionOnly)
        {
            System.Console.WriteLine($"Target connection opened; client connection ID: {connection.ClientConnectionId}.");
            // No user creation, grants, schema changes or seed data in this operation.
            await using var probe = new SqlCommand("SELECT 1", connection) { CommandTimeout = 10 };
            await probe.ExecuteScalarAsync();
            System.Console.WriteLine("SQL connection diagnostic succeeded (SELECT 1); no database changes performed.");
            return;
        }
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

    internal static async Task OpenConnectionWithRetryAsync(
        Func<Task> open, Func<TimeSpan, Task>? delay = null, string operation = "Bootstrap")
    {
        // WORKAROUND (2026-11-05): Azure WebJob diagnostics showed a first-login
        // connection reset followed by a successful login with identical settings.
        // The root cause is unresolved; do not describe this as a confirmed fix
        // for serverless resume. Retry connection opening ONLY, before any SQL
        // user creation, grants or schema work. Never replay the administrative
        // operation. See infrastructure/DEPLOYMENT.md for evidence and scope.
        const int maxAttempts = 3;
        delay ??= duration => Task.Delay(duration);
        for (var attempt = 1; ; attempt++)
        {
            System.Console.WriteLine($"{operation} SQL connection attempt {attempt}/{maxAttempts}.");
            try
            {
                await open();
                return;
            }
            catch (Exception error) when (attempt < maxAttempts && IsRetryableConnectionFailure(error))
            {
                System.Console.Error.WriteLine($"{operation} SQL connection attempt {attempt} failed ({error.GetType().Name}); retrying in 10 seconds. No database changes have run.");
                await delay(TimeSpan.FromSeconds(10));
            }
        }
    }

    internal static bool IsRetryableConnectionFailure(Exception error)
    {
        if (error is SqlException sql && sql.Number != 0)
            return sql.Number is -2 or 40613 or 40197 or 40501;

        for (Exception? cause = error; cause is not null; cause = cause.InnerException)
            if (cause is SocketException { SocketErrorCode: SocketError.ConnectionReset })
                return true;
        return false;
    }

    internal static string RedactDiagnosticMessage(string message, SqlConnectionStringBuilder connection)
    {
        // Replace exact configured values first: passwords can contain delimiters,
        // quotes and line breaks that a connection-string pattern cannot safely parse.
        foreach (var secret in new[] { connection.ConnectionString, connection.Password, connection.UserID }
            .Where(value => !string.IsNullOrEmpty(value)).OrderByDescending(value => value.Length))
            message = message.Replace(secret, "[redacted]", StringComparison.OrdinalIgnoreCase);

        // Also cover quoted/escaped credentials if the provider formatted them.
        message = Regex.Replace(message,
            "(?i)\\b(password|pwd|user\\s*id|uid|access\\s*token)\\s*=\\s*(?:\"(?:\"\"|[^\"])*\"|'(?:''|[^'])*'|[^;\\r\\n]*)",
            "$1=[redacted]", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));
        message = message.Replace('\r', ' ').Replace('\n', ' ');
        return message.Length <= 4096 ? message : message[..4096] + " [truncated]";
    }

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
