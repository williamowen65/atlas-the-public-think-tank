using System.Linq.Expressions;
using Atlas.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Atlas.ConsoleApp.Storage;

/// <summary>Provides scoped EF queries and saves only the addressed entity.</summary>
/// <param name="contextFactory">Optional factory for fresh contexts, commonly supplied by SQL tests.
/// When omitted, SqlStorage.Open uses the connection configured by the host. All repositories joining
/// one operation must produce contexts with exactly the same connection string.</param>
public abstract class SqlRepository(Func<AtlasDataContext>? contextFactory = null) : IOperationBoundary, IReferenceLookup
{
    /// <summary>Optional named lock requested before this repository's operation callback.</summary>
    /// <remarks>Most writes rely on serializable isolation and constraints. Overrides must use a shared
    /// resource for the invariant being protected, rather than one arbitrary name per repository instance.</remarks>
    protected virtual string? WriteLockResource => null;

    /// <summary>Requests named coordination for a specific operation, joining the current transaction when nested.</summary>
    protected T ExecuteWithLock<T>(
        Func<T> operation,
        string resource
    ) =>
        SqlOperationBoundary.Execute(
            Open,
            operation,
            resource
        );

    /// <summary>Creates a candidate context using the injected factory, or the host-configured SqlStorage factory.</summary>
    /// <remarks>
    /// This factory does not itself join a transaction; SqlOperationBoundary.Open determines which context the caller uses.
    /// </remarks>
    private AtlasDataContext Open() => contextFactory is null ? SqlStorage.Open() : contextFactory();
    /// <summary>Implements IOperationBoundary by delegating the callback and context factory to the SQL helper.</summary>
    /// <remarks>
    /// Nested calls join the outer operation, so repository saves cannot commit that operation early.
    /// </remarks>
    public T Execute<T>(Func<T> operation) => SqlOperationBoundary.Execute(
        Open,
        operation,
        WriteLockResource
    );
    /// <summary>Answers reference existence and optional availability checks using the operation context.</summary>
    /// <remarks>
    /// When requireActive is false, inactive, archived or hidden records may still satisfy existence.
    /// Documents and blocks have existence-only checks; unsupported kinds return false. This does not authorize the caller.
    /// </remarks>
    public bool IsAvailable(
        string kind,
        Guid id,
        bool requireActive
    )
    {
        using var lease = SqlOperationBoundary.Open(Open);
        var db = lease.Context;
        return kind switch
        {
            "Participant" => db.ParticipantRows.Any(row => row.Id == id && (!requireActive || row.IsActive)),
            "Node" => db.NodeRows.Any(row => row.Id == id && (!requireActive || row.Status == "Active")) &&
                (!requireActive || !db.ModerationCaseRows.Any(row => row.NodeId == id && row.Status == (int)Atlas.Moderation.ModerationStatus.Actioned && row.VisibilityRestoredAt == null)),
            "Document" => db.DocumentRows.Any(row => row.Id == id),
            "Block" => db.BlockRows.Any(row => row.Id == id),
            "NodeType" => db.NodeTypeRows.Any(row => row.Id == id && (!requireActive || !row.IsArchived)),
            "Community" => db.CommunityRows.Any(row => row.Id == id && (!requireActive || row.Status == "Active")),
            "NodeReaction" => db.NodeReactionRows.Any(row => row.Id == id && (!requireActive ||
                row.LifecycleState == "Active" && db.NodeRows.Any(node => node.Id == row.NodeId && node.Status == "Active") &&
                !db.ModerationCaseRows.Any(report => report.NodeId == row.NodeId && report.Status == (int)Atlas.Moderation.ModerationStatus.Actioned && report.VisibilityRestoredAt == null))),
            _ => false
        };
    }

    /// <summary>Materializes all rows and mapped child collections without tracking for domain reconstitution.</summary>
    /// <remarks>
    /// Uses the outer context when present, otherwise a short-lived standalone context.
    /// </remarks>
    protected List<T> ReadRows<T>() where T : class
    {
        using var lease = SqlOperationBoundary.Open(Open);
        var database = lease.Context;
        return IncludeChildren(
            database,
            database.Set<T>().AsNoTracking()
        ).ToList();
    }
    /// <summary>Applies an EF-translatable predicate and optional query composition before materializing untracked rows.</summary>
    /// <remarks>
    /// Includes mapped child collections; callers can add ordering or other query operators through compose.
    /// </remarks>
    protected List<T> QueryRows<T>(
        Expression<Func<T, bool>> predicate,
        Func<IQueryable<T>, IQueryable<T>>? compose = null
    ) where T : class
    {
        using var lease = SqlOperationBoundary.Open(Open);
        var database = lease.Context;
        var query = IncludeChildren(
            database,
            database.Set<T>().AsNoTracking()
        ).Where(predicate);
        return (compose is null ? query : compose(query)).ToList();
    }
    /// <summary>Finds a row by its ordered primary-key values and loads mapped child collections.</summary>
    /// <remarks>
    /// EF Find may return an already tracked row from the shared context; returns null when no row exists.
    /// </remarks>
    protected T? FindRow<T>(params object[] keys) where T : class
    {
        using var lease = SqlOperationBoundary.Open(Open);
        var database = lease.Context;
        var row = database.Set<T>().Find(keys);
        if (row is not null)
            foreach (var navigation in database.Entry(row).Metadata.GetNavigations().Where(navigation => navigation.IsCollection))
                database.Entry(row).Collection(navigation.Name).Load();
        return row;
    }
    /// <summary>Starts or joins an operation boundary before inserting or replacing the addressed row.</summary>
    protected void SaveRow<T>(
        T row,
        params string[] preservedCollections
    ) where T : class =>
        Execute(() => { SaveRowCore(
            row,
            preservedCollections
        ); return true; });
    /// <summary>Prepares row values, finds the existing primary key, and inserts or updates that entity.</summary>
    /// <remarks>
    /// Mapped child collections are replaced after their old rows are removed, except explicitly preserved collections. SaveChanges flushes SQL within the current transaction; it does not commit the outer operation.
    /// </remarks>
    private void SaveRowCore<T>(
        T row,
        string[] preservedCollections
    ) where T : class
    {
        using var lease = SqlOperationBoundary.Open(Open);
        var database = lease.Context;
        AtlasDataContext.PrepareRow(row);
        var entry = database.Entry(row);
        var primaryKey = entry.Metadata.FindPrimaryKey() ?? throw new InvalidOperationException("Entity has no primary key.");
        var keys = primaryKey.Properties.Select(property => entry.Property(property.Name).CurrentValue).ToArray();
        var existing = database.Set<T>().Find(keys);

        if (existing is null) database.Set<T>().Add(row);
        else
        {
            var existingEntry = database.Entry(existing);
            var collections = existingEntry.Metadata.GetNavigations().Where(navigation => navigation.IsCollection && !preservedCollections.Contains(navigation.Name)).ToList();
            foreach (var navigation in collections)
            {
                existingEntry.Collection(navigation.Name).Load();
                var children = (System.Collections.IEnumerable)navigation.PropertyInfo!.GetValue(existing)!;
                database.RemoveRange(children.Cast<object>());
            }
            // Flush child removals before attaching replacement rows with the same composite keys.
            if (collections.Count > 0) database.SaveChanges();
            existingEntry.CurrentValues.SetValues(row);
            foreach (var navigation in collections)
                navigation.PropertyInfo!.SetValue(
                    existing,
                    navigation.PropertyInfo.GetValue(row)
                );
        }
        database.SaveChanges();

    }
    /// <summary>Adds eager loading for every mapped collection navigation needed to reconstitute the domain object.</summary>
    /// <remarks>
    /// Rejects row types that are not part of the EF model.
    /// </remarks>
    private static IQueryable<T> IncludeChildren<T>(
        AtlasDataContext database,
        IQueryable<T> query
    ) where T : class
    {
        var entity = database.Model.FindEntityType(typeof(T)) ?? throw new InvalidOperationException("Unknown EF entity.");
        foreach (var navigation in entity.GetNavigations().Where(navigation => navigation.IsCollection))
            query = query.Include(navigation.Name);
        return query;
    }
    /// <summary>Starts or joins an operation boundary before deleting a row by its primary-key values.</summary>
    protected void DeleteRow<T>(params object[] keys) where T : class => Execute(() => { DeleteRowCore<T>(keys); return true; });
    /// <summary>Deletes the addressed row when present and flushes within the current transaction.</summary>
    /// <remarks>
    /// A missing row is a no-op; mapped database constraints and delete rules still apply.
    /// </remarks>
    private void DeleteRowCore<T>(params object[] keys) where T : class
    {
        using var lease = SqlOperationBoundary.Open(Open);
        var database = lease.Context;
        var row = database.Set<T>().Find(keys);
        if (row is null) return;
        database.Set<T>().Remove(row);
        database.SaveChanges();
    }
}
