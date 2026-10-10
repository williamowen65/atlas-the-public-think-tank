using System.Linq.Expressions;
using Atlas.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Atlas.ConsoleApp.Storage;

/// <summary>Provides scoped EF queries and saves only the addressed entity.</summary>
public abstract class SqlRepository(Func<AtlasDataContext>? contextFactory = null) : IOperationBoundary, IReferenceLookup
{
    private AtlasDataContext Open() => contextFactory is null ? SqlStorage.Open() : contextFactory();
    public T Execute<T>(Func<T> operation) => SqlOperationBoundary.Execute(Open, operation);
    public bool IsAvailable(string kind, Guid id, bool requireActive)
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

    protected List<T> ReadRows<T>() where T : class
    {
        using var lease = SqlOperationBoundary.Open(Open);
        var database = lease.Context;
        return IncludeChildren(database, database.Set<T>().AsNoTracking()).ToList();
    }
    protected List<T> QueryRows<T>(Expression<Func<T, bool>> predicate,
        Func<IQueryable<T>, IQueryable<T>>? compose = null) where T : class
    {
        using var lease = SqlOperationBoundary.Open(Open);
        var database = lease.Context;
        var query = IncludeChildren(database, database.Set<T>().AsNoTracking()).Where(predicate);
        return (compose is null ? query : compose(query)).ToList();
    }
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
    protected void SaveRow<T>(T row) where T : class => Execute(() => { SaveRowCore(row); return true; });
    private void SaveRowCore<T>(T row) where T : class
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
            var collections = existingEntry.Metadata.GetNavigations().Where(navigation => navigation.IsCollection).ToList();
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
                navigation.PropertyInfo!.SetValue(existing, navigation.PropertyInfo.GetValue(row));
        }
        database.SaveChanges();

    }
    private static IQueryable<T> IncludeChildren<T>(AtlasDataContext database, IQueryable<T> query) where T : class
    {
        var entity = database.Model.FindEntityType(typeof(T)) ?? throw new InvalidOperationException("Unknown EF entity.");
        foreach (var navigation in entity.GetNavigations().Where(navigation => navigation.IsCollection))
            query = query.Include(navigation.Name);
        return query;
    }
    protected void DeleteRow<T>(params object[] keys) where T : class => Execute(() => { DeleteRowCore<T>(keys); return true; });
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
