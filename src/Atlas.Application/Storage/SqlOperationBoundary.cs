using System.Data;
using System.Diagnostics;
using Atlas.Contracts.Observability;
using Atlas.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Application.Storage;

/// <summary>Shares one context/transaction across repository calls in the current synchronous operation.</summary>
public static class SqlOperationBoundary
{
    // Ambient context for this execution flow, not a global context shared by all users.
    // EF contexts are not thread-safe; this synchronous workflow must not fan out parallel DB calls.
    private static readonly AsyncLocal<AtlasDataContext?> Current = new();

    /// <summary>Runs the complete callback in one SQL transaction, or joins the current operation.</summary>
    /// <remarks>
    /// Only the outermost call owns commit and rollback. Exceptions leave the transaction uncommitted; disposal rolls it back.
    /// Optional named coordination is independent of transaction atomicity.
    /// The callback is synchronous and must not launch parallel repository work using the shared context.
    /// </remarks>
    public static T Execute<T>(
        Func<AtlasDataContext> open,
        Func<T> operation,
        string? lockResource = null
    )
    {
        if (Current.Value is not null)
        {
            using var check = open();
            EnsureSameDatabase(
                check,
                Current.Value
            );
            AcquireLock(
                Current.Value,
                lockResource
            );
            return operation();
        }
        using var correlation = Telemetry.JoinOperation();
        using var span = Atlas.Observability.LocalTelemetry.Activities.StartActivity("storage.transaction");
        using var database = open();
        // Serializable isolation protects the transaction's reads and writes until completion.
        using var transaction = database.Database.BeginTransaction(IsolationLevel.Serializable);
        var started = Stopwatch.GetTimestamp();
        var committing = false;
        try
        {
            AcquireLock(
                database,
                lockResource
            );
            Current.Value = database;
            var result = operation();
            committing = true;
            transaction.Commit();
            database.PublishCommittedTelemetry(transaction.TransactionId);
            Telemetry.Record(Telemetry.Create(EventKind.StorageWrite, durationMs: Stopwatch.GetElapsedTime(started).TotalMilliseconds));
            return result;
        }
        catch (Exception exception) when (IsConflict(exception))
        {
            Telemetry.Record(Telemetry.Create(EventKind.StorageWrite, outcome: EventOutcome.Conflict, reason: EventReason.StorageConflict));
            throw new OperationConflictException(
                "The operation conflicted with another write or a database constraint. Reload and retry.",
                exception
            );
        }
        catch (Exception error)
        {
            Telemetry.Failure(EventKind.StorageWrite, error,
                committing ? EventReason.CommitOutcomeUnknown : Telemetry.Reason(error));
            throw;
        }
        finally { Current.Value = null; }
    }

    /// <summary>Acquires optional named coordination inside the current transaction before the callback runs.</summary>
    /// <remarks>
    /// Null means ordinary serializable transaction protection only. Node topology operations request the shared
    /// AtlasGraphTopology resource because ancestry checks can span multiple nodes. Nested calls also
    /// request their required lock, even when the outer operation began through another repository.
    /// SQL Server permits repeated acquisition by the owning transaction and releases its locks at completion.
    /// Exclusive applies only to this resource name. Timeout is 15000 milliseconds; failure raises SQL
    /// error 51000 for translation to OperationConflictException. Resource values are SQL parameters.
    /// </remarks>
    private static void AcquireLock(
        AtlasDataContext database,
        string? lockResource
    )
    {
        if (lockResource is null) return;
        database.Database.ExecuteSqlInterpolated($"DECLARE @result int; EXEC @result = sp_getapplock @Resource={lockResource}, @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=15000; IF @result < 0 THROW 51000, 'Atlas write lock unavailable', 1;");
    }

    /// <summary>Returns a lease on the current operation context, or owns a fresh context for a standalone call.</summary>
    /// <remarks>
    /// A temporary candidate checks connection configuration and is disposed before borrowing the shared context.
    /// </remarks>
    internal static ContextLease Open(Func<AtlasDataContext> open)
    {
        var candidate = open();
        if (Current.Value is null) return new ContextLease(
            candidate,
            true
        );
        try { EnsureSameDatabase(
            candidate,
            Current.Value
        ); }
        finally { candidate.Dispose(); }
        return new ContextLease(
            Current.Value,
            false
        );
    }

    /// <summary>Rejects joining repositories with different connection strings.</summary>
    /// <remarks>
    /// This deliberately compares configured strings exactly, rather than assuming equivalent strings identify the same database.
    /// </remarks>
    private static void EnsureSameDatabase(
        AtlasDataContext candidate,
        AtlasDataContext current
    )
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
    internal sealed class ContextLease(
        AtlasDataContext context,
        bool owns
    ) : IDisposable
    {
        public AtlasDataContext Context { get; } = context;
        /// <summary>Disposes only an owned standalone context; borrowed contexts remain alive until the outer boundary exits.</summary>
        public void Dispose() { if (owns) Context.Dispose(); }
    }
}
