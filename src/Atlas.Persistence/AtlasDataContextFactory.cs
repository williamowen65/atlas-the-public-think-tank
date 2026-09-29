using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Atlas.Persistence;

/// <summary>Creates the model for EF tooling without running the console or migrating a database.</summary>
public sealed class AtlasDataContextFactory : IDesignTimeDbContextFactory<AtlasDataContext>
{
    public AtlasDataContext CreateDbContext(string[] args)
    {
        // Scaffolding needs the SQL Server provider, but does not connect to this database.
        // Database update requires the actual connection through --connection or the environment.
        var connection = Environment.GetEnvironmentVariable("ATLAS_SQL_CONNECTION_STRING")
            ?? "Server=localhost;Database=Atlas;Integrated Security=True;TrustServerCertificate=True";
        var options = new DbContextOptionsBuilder<AtlasDataContext>().UseSqlServer(connection).Options;
        return new AtlasDataContext(options);
    }
}
