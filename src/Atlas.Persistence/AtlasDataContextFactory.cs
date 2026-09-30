using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Atlas.Persistence;

/// <summary>Creates the model for EF tooling without running the console or migrating a database.</summary>
public sealed class AtlasDataContextFactory : IDesignTimeDbContextFactory<AtlasDataContext>
{
    public AtlasDataContext CreateDbContext(string[] args)
    {
        // Use the same secrets ID and provider precedence as the console.
        // Building the model does not connect to the database or run startup migrations.
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<AtlasDataContextFactory>(optional: true)
            .AddEnvironmentVariables()
            .Build();
        var connection = configuration["ATLAS_SQL_CONNECTION_STRING"];
        if (string.IsNullOrWhiteSpace(connection))
            throw new InvalidOperationException(
                "Set ATLAS_SQL_CONNECTION_STRING in shared user secrets or the environment before using EF tooling.");
        var options = new DbContextOptionsBuilder<AtlasDataContext>().UseSqlServer(connection).Options;
        return new AtlasDataContext(options);
    }
}
