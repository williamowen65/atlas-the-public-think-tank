using System.Collections;
using Atlas.Identity;
using Atlas.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Atlas.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Atlas.ConsoleApp.Storage;

/// <summary>Routes the console adapters to EF Core rows in SQL Server.</summary>
public static class SqlStorage
{
    private static string? _connectionString;
    public static void Configure(string connectionString)
    {
        _connectionString = connectionString;
        using var database = Open();
        database.Database.Migrate();
    }

    public static AtlasDataContext Open()
    {
        var options = new DbContextOptionsBuilder<AtlasDataContext>()
            .UseSqlServer(_connectionString ?? throw new InvalidOperationException("SQL storage has not been configured."))
            .Options;
        return new AtlasDataContext(options);
    }

    private static string Name(string collectionKey) => Path.GetFileNameWithoutExtension(collectionKey);

    private static readonly HashSet<string> KnownCollections = new(StringComparer.OrdinalIgnoreCase)
    {
        "nodes",
        "node-types",
        "documents",
        "blocks",
        "participants",
        "votes",
        "reaction-definitions",
        "node-reactions",
        "communities",
        "community-memberships",
        "community-nodes",
        "comments",
        "moderation-cases",
        "notifications",
        "notification-preferences",
    };

    public static string DescribeCollection(string collectionKey)
    {
        using var database = Open();
        return ReadRows(database, Name(collectionKey));
    }

    private static string ReadRows(AtlasDataContext database, string name) => name switch
    {
        "nodes" => FormatRows(database.NodeRows.Include(row => row.Parents).Include(row => row.RequestedTypes).AsNoTracking().ToList()),
        "node-types" => FormatRows(database.NodeTypeRows.AsNoTracking().ToList()),
        "documents" => FormatRows(database.DocumentRows.Include(row => row.Blocks).AsNoTracking().ToList()),
        "blocks" => FormatRows(database.BlockRows.AsNoTracking().ToList()),
        "participants" => FormatRows(database.ParticipantRows.AsNoTracking().ToList()),
        "votes" => FormatRows(database.VoteRows.AsNoTracking().ToList()),
        "reaction-definitions" => FormatRows(database.ReactionDefinitionRows.AsNoTracking().ToList()),
        "node-reactions" => FormatRows(database.NodeReactionRows.Include(row => row.AuditHistory).AsNoTracking().ToList()),
        "communities" => FormatRows(database.CommunityRows.AsNoTracking().ToList()),
        "community-memberships" => FormatRows(database.CommunityMembershipRows.AsNoTracking().ToList()),
        "community-nodes" => FormatRows(database.CommunityNodeRows.AsNoTracking().ToList()),
        "comments" => FormatRows(database.CommentRows.AsNoTracking().ToList()),
        "moderation-cases" => FormatRows(database.ModerationCaseRows.AsNoTracking().ToList()),
        "notifications" => FormatRows(database.NotificationRows.Include(row => row.DeliveryAttempts).AsNoTracking().ToList()),
        "notification-preferences" => FormatRows(database.NotificationPreferencesRows.AsNoTracking().ToList()),
        _ => throw new ArgumentOutOfRangeException(nameof(name), name, "Unknown Atlas collection")
    };

    private static string FormatRows<T>(IEnumerable<T> rows) => string.Join(Environment.NewLine + Environment.NewLine,
        rows.Select(row => FormatRecord(row!)));
    private static string FormatRecord(object row) => string.Join(" | ", row.GetType().GetProperties().Select(property =>
    {
        var value = property.GetValue(row);
        var text = value is IEnumerable items && value is not string
            ? "[" + string.Join("; ", items.Cast<object>().Select(item => item.GetType().IsValueType ? item.ToString() : FormatRecord(item))) + "]"
            : value?.ToString() ?? "(none)";
        return $"{property.Name}: {text}";
    }));

    /// <summary>Create demo accounts through Identity, then seed dependent Atlas records with their generated IDs.</summary>
    public static async Task SeedDemoDataAsync(IServiceScopeFactory scopes)
    {
        using (var database = Open())
        {
            if (KnownCollections.Any(name => CollectionHasRows(database, name)) || database.Users.Any())
                throw new InvalidOperationException("Demo seed requires an empty Atlas database; SQL data was not changed.");
        }

        var ids = new List<Guid>(DemoParticipants.Accounts.Length);
        foreach (var demo in DemoParticipants.Accounts)
        {
            using var scope = scopes.CreateScope();
            var accounts = scope.ServiceProvider.GetRequiredService<AtlasAccounts>();
            var registration = await accounts.RegisterAsync(demo.Email, DemoParticipants.Password, demo.DisplayName);
            if (!registration.Result.Succeeded || registration.ParticipantId is null)
                throw new InvalidOperationException($"Could not create demo account {demo.Email}: " +
                    string.Join("; ", registration.Result.Errors.Select(error => error.Description)));

            var users = scope.ServiceProvider.GetRequiredService<UserManager<AtlasIdentityUser>>();
            var user = await users.FindByIdAsync(registration.ParticipantId.Value.Value.ToString());
            if (user is null)
                throw new InvalidOperationException($"Demo Identity user {demo.Email} was not persisted.");
            var confirmation = await users.GenerateEmailConfirmationTokenAsync(user);
            var confirmed = await users.ConfirmEmailAsync(user, confirmation);
            if (!confirmed.Succeeded)
                throw new InvalidOperationException($"Could not confirm demo account {demo.Email}.");

            ids.Add(registration.ParticipantId.Value.Value);
        }

        DemoParticipants.SetIds(ids);
        using var seedDatabase = Open();
        DemoData.Seed(seedDatabase);
        seedDatabase.SaveChanges();
    }

    private static bool CollectionHasRows(AtlasDataContext database, string name) => name switch
    {
        "nodes" => database.NodeRows.Any(),
        "node-types" => database.NodeTypeRows.Any(),
        "documents" => database.DocumentRows.Any(),
        "blocks" => database.BlockRows.Any(),
        "participants" => database.ParticipantRows.Any(),
        "votes" => database.VoteRows.Any(),
        "reaction-definitions" => database.ReactionDefinitionRows.Any(),
        "node-reactions" => database.NodeReactionRows.Any(),
        "communities" => database.CommunityRows.Any(),
        "community-memberships" => database.CommunityMembershipRows.Any(),
        "community-nodes" => database.CommunityNodeRows.Any(),
        "comments" => database.CommentRows.Any(),
        "moderation-cases" => database.ModerationCaseRows.Any(),
        "notifications" => database.NotificationRows.Any(),
        "notification-preferences" => database.NotificationPreferencesRows.Any(),
        _ => throw new ArgumentOutOfRangeException(nameof(name), name, "Unknown Atlas collection")
    };
}
