using System.Data;
using Atlas.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Atlas.ConsoleApp.Storage;

/// <summary>Shares one context/transaction across repository calls in the current synchronous operation.</summary>
public static class SqlOperationBoundary
{
    private static readonly AsyncLocal<AtlasDataContext?> Current = new();

    public static T Execute<T>(Func<AtlasDataContext> open, Func<T> operation)
    {
        if (Current.Value is not null)
        {
            using var check = open();
            EnsureSameDatabase(check, Current.Value);
            return operation();
        }
        using var database = open();
        using var transaction = database.Database.BeginTransaction(IsolationLevel.Serializable);
        try
        {
            // A database-scoped transaction lock serializes current MVP write workflows,
            // including check-then-insert and graph topology checks. No schema changes.
            database.Database.ExecuteSqlInterpolated($"DECLARE @result int; EXEC @result = sp_getapplock @Resource={"AtlasWriteWorkflow"}, @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=15000; IF @result < 0 THROW 51000, 'Atlas write lock unavailable', 1;");
            Current.Value = database;
            var result = operation();
            transaction.Commit();
            return result;
        }
        catch (Exception exception) when (IsConflict(exception))
        {
            throw new OperationConflictException("The operation conflicted with another write or a database constraint. Reload and retry.", exception);
        }
        finally { Current.Value = null; }
    }

    internal static ContextLease Open(Func<AtlasDataContext> open)
    {
        var candidate = open();
        if (Current.Value is null) return new ContextLease(candidate, true);
        try { EnsureSameDatabase(candidate, Current.Value); }
        finally { candidate.Dispose(); }
        return new ContextLease(Current.Value, false);
    }

    private static void EnsureSameDatabase(AtlasDataContext candidate, AtlasDataContext current)
    {
        if (candidate.Database.GetConnectionString() != current.Database.GetConnectionString())
            throw new InvalidOperationException("An operation cannot span different Atlas databases.");
    }

    private static bool IsConflict(Exception exception) =>
        exception is DbUpdateConcurrencyException ||
        exception is SqlException sql && sql.Number is 1205 or 2601 or 2627 or 547 or 51000 ||
        exception.InnerException is not null && IsConflict(exception.InnerException);

    internal sealed class ContextLease(AtlasDataContext context, bool owns) : IDisposable
    {
        public AtlasDataContext Context { get; } = context;
        public void Dispose() { if (owns) Context.Dispose(); }
    }
}
