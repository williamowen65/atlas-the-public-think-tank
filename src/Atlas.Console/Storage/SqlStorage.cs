using System.Text.Json;
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

    private static AtlasDataContext Open()
    {
        var options = new DbContextOptionsBuilder<AtlasDataContext>()
            .UseSqlServer(_connectionString ?? throw new InvalidOperationException("SQL storage has not been configured."))
            .Options;
        return new AtlasDataContext(options);
    }

    private static string Name(string collectionKey) => Path.GetFileNameWithoutExtension(collectionKey);

    public static bool Exists(string collectionKey) =>
        _connectionString is null ? File.Exists(collectionKey) : KnownCollections.Contains(Name(collectionKey));

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

    public static string ReadText(string collectionKey)
    {
        if (_connectionString is null) return File.ReadAllText(collectionKey);
        using var database = Open();
        return ReadRows(database, Name(collectionKey));
    }

    private static string ReadRows(AtlasDataContext database, string name) => name switch
    {
        "nodes" => JsonSerializer.Serialize(database.NodeRows.AsNoTracking().ToList()),
        "node-types" => JsonSerializer.Serialize(database.NodeTypeRows.AsNoTracking().ToList()),
        "documents" => JsonSerializer.Serialize(database.DocumentRows.AsNoTracking().ToList()),
        "blocks" => JsonSerializer.Serialize(database.BlockRows.AsNoTracking().ToList()),
        "participants" => JsonSerializer.Serialize(database.ParticipantRows.AsNoTracking().ToList()),
        "votes" => JsonSerializer.Serialize(database.VoteRows.AsNoTracking().ToList()),
        "reaction-definitions" => JsonSerializer.Serialize(database.ReactionDefinitionRows.AsNoTracking().ToList()),
        "node-reactions" => JsonSerializer.Serialize(database.NodeReactionRows.AsNoTracking().ToList()),
        "communities" => JsonSerializer.Serialize(database.CommunityRows.AsNoTracking().ToList()),
        "community-memberships" => JsonSerializer.Serialize(database.CommunityMembershipRows.AsNoTracking().ToList()),
        "community-nodes" => JsonSerializer.Serialize(database.CommunityNodeRows.AsNoTracking().ToList()),
        "comments" => JsonSerializer.Serialize(database.CommentRows.AsNoTracking().ToList()),
        "moderation-cases" => JsonSerializer.Serialize(database.ModerationCaseRows.AsNoTracking().ToList()),
        "notifications" => JsonSerializer.Serialize(database.NotificationRows.AsNoTracking().ToList()),
        "notification-preferences" => JsonSerializer.Serialize(database.NotificationPreferencesRows.AsNoTracking().ToList()),
        _ => throw new ArgumentOutOfRangeException(nameof(name), name, "Unknown Atlas collection")
    };

    public static void WriteText(string collectionKey, string contents)
    {
        if (_connectionString is null)
        {
            var directory = Path.GetDirectoryName(collectionKey);
            if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
            File.WriteAllText(collectionKey, contents);
            return;
        }
        using var database = Open();
        using var transaction = database.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);
        ReplaceRows(database, Name(collectionKey), contents);
        database.SaveChanges();
        transaction.Commit();
    }

    private static void ReplaceRows(AtlasDataContext database, string name, string contents)
    {
        switch (name)
        {
            case "nodes":
                Replace(database.NodeRows, JsonSerializer.Deserialize<List<NodeRow>>(contents) ?? []);
                break;
            case "node-types":
                Replace(database.NodeTypeRows, JsonSerializer.Deserialize<List<NodeTypeRow>>(contents) ?? []);
                break;
            case "documents":
                Replace(database.DocumentRows, JsonSerializer.Deserialize<List<DocumentRow>>(contents) ?? []);
                break;
            case "blocks":
                Replace(database.BlockRows, JsonSerializer.Deserialize<List<BlockRow>>(contents) ?? []);
                break;
            case "participants":
                Replace(database.ParticipantRows, JsonSerializer.Deserialize<List<ParticipantRow>>(contents) ?? []);
                break;
            case "votes":
                Replace(database.VoteRows, JsonSerializer.Deserialize<List<VoteRow>>(contents) ?? []);
                break;
            case "reaction-definitions":
                Replace(database.ReactionDefinitionRows, JsonSerializer.Deserialize<List<ReactionDefinitionRow>>(contents) ?? []);
                break;
            case "node-reactions":
                Replace(database.NodeReactionRows, JsonSerializer.Deserialize<List<NodeReactionRow>>(contents) ?? []);
                break;
            case "communities":
                Replace(database.CommunityRows, JsonSerializer.Deserialize<List<CommunityRow>>(contents) ?? []);
                break;
            case "community-memberships":
                Replace(database.CommunityMembershipRows, JsonSerializer.Deserialize<List<CommunityMembershipRow>>(contents) ?? []);
                break;
            case "community-nodes":
                Replace(database.CommunityNodeRows, JsonSerializer.Deserialize<List<CommunityNodeRow>>(contents) ?? []);
                break;
            case "comments":
                Replace(database.CommentRows, JsonSerializer.Deserialize<List<CommentRow>>(contents) ?? []);
                break;
            case "moderation-cases":
                Replace(database.ModerationCaseRows, JsonSerializer.Deserialize<List<ModerationCaseRow>>(contents) ?? []);
                break;
            case "notifications":
                Replace(database.NotificationRows, JsonSerializer.Deserialize<List<NotificationRow>>(contents) ?? []);
                break;
            case "notification-preferences":
                Replace(database.NotificationPreferencesRows, JsonSerializer.Deserialize<List<NotificationPreferencesRow>>(contents) ?? []);
                break;
            default: throw new ArgumentOutOfRangeException(nameof(name), name, "Unknown Atlas collection");
        }
    }

    private static void Replace<T>(DbSet<T> table, List<T> records) where T : class
    {
        table.ExecuteDelete();
        table.AddRange(records);
    }

    /// <summary>Insert the stable demo rows only into a completely empty Atlas database.</summary>
    public static void SeedDemoData()
    {
        using var database = Open();
        using var transaction = database.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);
        if (KnownCollections.Any(name => CollectionHasRows(database, name)))
            throw new InvalidOperationException("Demo seed requires an empty Atlas database; SQL data was not changed.");
        foreach (var (name, payload) in DemoData.Collections)
            ReplaceRows(database, Name(name), payload);
        database.SaveChanges();
        transaction.Commit();
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
