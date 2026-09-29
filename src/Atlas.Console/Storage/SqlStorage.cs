using Microsoft.EntityFrameworkCore;

namespace Atlas.ConsoleApp.Storage;

/// <summary>
/// SQL Server backing store for the console's existing persistence records.
/// Each collection retains its serialized representation while the repositories
/// and domain interfaces stay stable. This is an initial SQL migration boundary.
/// </summary>
public static class SqlStorage
{
    private static string? _connectionString;

    public static void Configure(string connectionString)
    {
        _connectionString = connectionString;
        using var database = Open();
        database.Database.Migrate();
    }

    private static AtlasDataContext Open() => new(
        _connectionString ?? throw new InvalidOperationException("SQL storage has not been configured."));

    private static string Name(string path) => Path.GetFileName(path);

    public static bool Exists(string path)
    {
        if (_connectionString is null) return File.Exists(path);
        using var database = Open();
        var name = Name(path);
        return database.Collections.Any(collection => collection.Name == name);
    }

    public static string ReadText(string path)
    {
        if (_connectionString is null) return File.ReadAllText(path);
        using var database = Open();
        var name = Name(path);
        return database.Collections.AsNoTracking()
            .Where(collection => collection.Name == name)
            .Select(collection => collection.Payload)
            .SingleOrDefault() ?? string.Empty;
    }

    public static void WriteText(string path, string contents)
    {
        if (_connectionString is null)
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
            File.WriteAllText(path, contents);
            return;
        }
        using var database = Open();
        var name = Name(path);
        var existing = database.Collections.SingleOrDefault(collection => collection.Name == name);
        if (existing is null)
            database.Collections.Add(new SqlCollection { Name = name, Payload = contents });
        else
            existing.Payload = contents;
        database.SaveChanges();
    }

    /// <summary>Seed stable demo collections exactly once into an empty database.</summary>
    public static void SeedDemoData()
    {
        using var database = Open();
        using var transaction = database.Database.BeginTransaction();
        if (database.Collections.Any())
            throw new InvalidOperationException("Import requires an empty Atlas database; no existing SQL data was changed.");

        foreach (var (name, payload) in DemoData.Collections)
            database.Collections.Add(new SqlCollection
            {
                Name = name,
                Payload = payload
            });

        database.SaveChanges();
        transaction.Commit();
    }
}

internal sealed class AtlasDataContext(string connectionString) : DbContext
{
    public DbSet<SqlCollection> Collections => Set<SqlCollection>();

    protected override void OnConfiguring(DbContextOptionsBuilder options) =>
        options.UseSqlServer(connectionString);

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<SqlCollection>(entity =>
        {
            entity.ToTable("AtlasCollections");
            entity.HasKey(collection => collection.Name);
            entity.Property(collection => collection.Name).HasMaxLength(128);
            entity.Property(collection => collection.Payload).HasColumnType("nvarchar(max)").IsRequired();
            entity.Property(collection => collection.Version).IsRowVersion();
        });
    }
}

internal sealed class SqlCollection
{
    public string Name { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public byte[] Version { get; set; } = [];
}
