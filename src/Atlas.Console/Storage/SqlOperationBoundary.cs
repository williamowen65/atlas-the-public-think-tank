using System.Data;
using Atlas.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Atlas.ConsoleApp.Storage;

/// <summary>Shares one context/transaction across repository calls in the current synchronous operation.</summary>
public static class SqlOperationBoundary
{
    // Ambient context for this execution flow, not a global context shared by all users.
    // EF contexts are not thread-safe; this synchronous workflow must not fan out parallel DB calls.
    private static readonly AsyncLocal<AtlasDataContext?> Current = new();

    /// <summary>Runs the complete callback in one SQL transaction, or joins the current operation.</summary>
    /// <remarks>
    /// Only the outermost call owns commit and rollback. Exceptions leave the transaction uncommitted; disposal rolls it back.
    /// The callback is synchronous and must not launch parallel repository work using the shared context.
    /// </remarks>
    public static T Execute<T>(Func<AtlasDataContext> open, Func<T> operation)
    {
        if (Current.Value is not null)
        {
            using var check = open();
            EnsureSameDatabase(check, Current.Value);
            return operation();
        }
        using var database = open();
        // Serializable isolation protects the transaction's reads and writes until completion.
        using var transaction = database.Database.BeginTransaction(IsolationLevel.Serializable);
        try
        {
            // A database-scoped transaction lock serializes current MVP write workflows,
            // including check-then-insert and graph topology checks. No schema changes.
            // AtlasWriteWorkflow is one common resource name: all cooperating writes take turns.
            // Exclusive prevents a second holder; Transaction releases the lock on commit/rollback.
            // LockTimeout is 15000 milliseconds. Negative return codes become custom SQL error 51000.
            // These are fixed implementation settings, not user input or separate app configuration.
            // ExecuteSqlInterpolated binds the resource name as a SQL parameter.
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

    /// <summary>Returns a lease on the current operation context, or owns a fresh context for a standalone call.</summary>
    /// <remarks>
    /// A temporary candidate checks connection configuration and is disposed before borrowing the shared context.
    /// </remarks>
    internal static ContextLease Open(Func<AtlasDataContext> open)
    {
        var candidate = open();
        if (Current.Value is null) return new ContextLease(candidate, true);
        try { EnsureSameDatabase(candidate, Current.Value); }
        finally { candidate.Dispose(); }
        return new ContextLease(Current.Value, false);
    }

    /// <summary>Rejects joining repositories with different connection strings.</summary>
    /// <remarks>
    /// This deliberately compares configured strings exactly, rather than assuming equivalent strings identify the same database.
    /// </remarks>
    private static void EnsureSameDatabase(AtlasDataContext candidate, AtlasDataContext current)
    {
        if (candidate.Database.GetConnectionString() != current.Database.GetConnectionString())
            throw new InvalidOperationException("An operation cannot span different Atlas databases.");
    }

    /// <summary>Recognizes EF concurrency errors and SQL conflicts, including wrapped inner exceptions.</summary>
    /// <remarks>
    /// SQL codes cover deadlock (1205), duplicate keys (2601/2627), constraint failure (547), and the application lock failure (51000).
    /// </remarks>
    private static bool IsConflict(Exception exception) =>
        exception is DbUpdateConcurrencyException ||
        exception is SqlException sql && sql.Number is 1205 or 2601 or 2627 or 547 or 51000 ||
        exception.InnerException is not null && IsConflict(exception.InnerException);

    /// <summary>Tracks whether this caller owns its context or only borrows the outer operation context.</summary>
    internal sealed class ContextLease(AtlasDataContext context, bool owns) : IDisposable
    {
        public AtlasDataContext Context { get; } = context;
        /// <summary>Disposes only an owned standalone context; borrowed contexts remain alive until the outer boundary exits.</summary>
        public void Dispose() { if (owns) Context.Dispose(); }
    }
}
