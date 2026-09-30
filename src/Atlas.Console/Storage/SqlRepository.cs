using System.Linq.Expressions;
using Atlas.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Atlas.ConsoleApp.Storage;

/// <summary>Provides scoped EF queries and saves only the addressed entity.</summary>
public abstract class SqlRepository(Func<AtlasDataContext>? contextFactory = null)
{
    private AtlasDataContext Open() => contextFactory is null ? SqlStorage.Open() : contextFactory();
    protected List<T> ReadRows<T>() where T : class
    {
        using var database = Open();
        return IncludeChildren(database, database.Set<T>().AsNoTracking()).ToList();
    }
    protected List<T> QueryRows<T>(Expression<Func<T, bool>> predicate,
        Func<IQueryable<T>, IQueryable<T>>? compose = null) where T : class
    {
        using var database = Open();
        var query = IncludeChildren(database, database.Set<T>().AsNoTracking()).Where(predicate);
        return (compose is null ? query : compose(query)).ToList();
    }
    protected T? FindRow<T>(params object[] keys) where T : class
    {
        using var database = Open();
        var row = database.Set<T>().Find(keys);
        if (row is not null)
            foreach (var navigation in database.Entry(row).Metadata.GetNavigations().Where(navigation => navigation.IsCollection))
                database.Entry(row).Collection(navigation.Name).Load();
        return row;
    }
    protected void SaveRow<T>(T row) where T : class
    {
        using var database = Open();
        AtlasDataContext.PrepareRow(row);
        var entry = database.Entry(row);
        var primaryKey = entry.Metadata.FindPrimaryKey() ?? throw new InvalidOperationException("Entity has no primary key.");
        var keys = primaryKey.Properties.Select(property => entry.Property(property.Name).CurrentValue).ToArray();
        var existing = database.Set<T>().Find(keys);
        using var transaction = database.Database.BeginTransaction();
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
        transaction.Commit();
    }
    private static IQueryable<T> IncludeChildren<T>(AtlasDataContext database, IQueryable<T> query) where T : class
    {
        var entity = database.Model.FindEntityType(typeof(T)) ?? throw new InvalidOperationException("Unknown EF entity.");
        foreach (var navigation in entity.GetNavigations().Where(navigation => navigation.IsCollection))
            query = query.Include(navigation.Name);
        return query;
    }
    protected void DeleteRow<T>(params object[] keys) where T : class
    {
        using var database = Open();
        var row = database.Set<T>().Find(keys);
        if (row is null) return;
        database.Set<T>().Remove(row);
        database.SaveChanges();
    }
}
