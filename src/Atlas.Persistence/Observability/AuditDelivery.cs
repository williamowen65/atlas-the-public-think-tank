using Microsoft.EntityFrameworkCore;
using Atlas.Contracts.Observability;
using Atlas.Observability;

namespace Atlas.Persistence.Observability;

/// <summary>Delivery adapters acknowledge only durable acceptance. Future cloud adapters implement this port.</summary>
public interface IAuditDestination
{
    // Matches AuditOutboxRow.Destination; local and future cloud delivery have
    // independent rows/acknowledgements even when they reference the same audit event.
    string Name { get; }
    // Return successfully only after durable acceptance, not just queueing in memory.
    // Throw on failure so the outbox entry remains eligible for retry.
    Task DeliverAsync(AuditRecordRow record, CancellationToken cancellationToken);
}

/// <summary>Local inspection copy; SQL remains the authoritative history after file rotation.</summary>
public sealed class LocalAuditDestination(JsonLineFile file) : IAuditDestination
{
    public string Name => "local";
    public Task DeliverAsync(AuditRecordRow record, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        // Deliver the payload captured at business commit time, not a reconstruction
        // from today's entity state. Write returns after flushing the file to disk.
        // This synchronous file write is not interrupted by cancellation once started.
        file.Write(record.PayloadJson);
        // The interface is asynchronous for other adapters; this file adapter has
        // already completed its synchronous work, so no Task.Run is needed.
        return Task.CompletedTask;
    }
}

/// <summary>Leases one row at a time across processes. A crash or lost acknowledgement can duplicate, never discard, delivery.</summary>
// Constructor inputs are collaborators: open creates a fresh context per attempt;
// destination decides how to deliver. The host owns repetition, delays and shutdown.
public sealed class AuditDelivery(Func<AtlasDataContext> open, IAuditDestination destination)
{
    /// <summary>Attempt one due outbox entry for this destination; no business transaction is held during delivery.</summary>
    /// <returns>
    /// False when no entry is currently due. True when a candidate was found, including
    /// a lost lease race or a handled delivery failure; true does not mean delivered.
    /// Cancellation and unhandled database failures propagate to the host loop.
    /// </returns>
    public async Task<bool> DeliverOneAsync(CancellationToken cancellationToken = default)
    {
        // 1. Delivery is new system work, not a continuation impersonating the original actor.
        // The stored payload retains the original actor/operation; delivery diagnostics use
        // this new scope and reference the audit event ID. using cleans up on every exit.
        using var operation = Telemetry.BeginOperation(system: true);
        using var db = open();
        var now = DateTimeOffset.UtcNow;
        // 2. Select pending work whose retry time has arrived and whose lease is absent
        // or expired. AsNoTracking avoids attaching read-only snapshots to EF's tracker.
        // Example: a local worker ignores a cloud entry for the very same audit event.
        // Ordering prefers the earliest due time; Id breaks ties deterministically.
        var candidate = await db.AuditOutbox.AsNoTracking()
            .Where(row => row.Destination == destination.Name && row.DeliveredAtUtc == null &&
                row.NextAttemptAtUtc <= now && (row.LeaseUntilUtc == null || row.LeaseUntilUtc < now))
            .OrderBy(row => row.NextAttemptAtUtc).ThenBy(row => row.Id)
            .Select(row => (Guid?)row.Id).FirstOrDefaultAsync(cancellationToken);
        // Nullable Guid projection distinguishes no row from an actual row ID.
        // No due entry does not mean the queue is empty: others may be delayed/leased.
        if (candidate is null) return false;
        var owner = Guid.NewGuid();
        // 3. Reading a candidate does not reserve it. Recheck eligibility in ONE SQL
        // UPDATE and record a unique ownership token; only one contender can claim it.
        // Example: workers A and B read row R. A updates it first; B's UPDATE affects
        // zero rows because R now has an active lease, so B must not deliver R.
        // ExecuteUpdateAsync writes directly to SQL without a later SaveChanges call.
        // A crashed worker's two-minute lease expires, allowing a later attempt.
        // The lease is not renewed here; slow delivery can overlap a later claimant.
        var claimed = await db.AuditOutbox.Where(row => row.Id == candidate && row.Destination == destination.Name &&
            row.DeliveredAtUtc == null && row.NextAttemptAtUtc <= now &&
            (row.LeaseUntilUtc == null || row.LeaseUntilUtc < now))
            .ExecuteUpdateAsync(update => update.SetProperty(row => row.LeaseOwner, owner)
                .SetProperty(row => row.Attempts, row => row.Attempts + 1)
                .SetProperty(row => row.LeaseUntilUtc, now.AddMinutes(2)), cancellationToken);
        // Lost race: report that a candidate existed so the host can try another immediately.
        if (claimed != 1) return true;
        // 4. Reload after claiming: Attempts now includes this attempt. The outbox row
        // holds mutable delivery state; AuditRecords holds the original stored evidence.
        // A read failure here propagates; the claimed lease can recover by expiry.
        var pending = await db.AuditOutbox.AsNoTracking().SingleAsync(row => row.Id == candidate, cancellationToken);
        var record = await db.AuditRecords.AsNoTracking().SingleAsync(row => row.Id == pending.AuditRecordId, cancellationToken);
        try
        {
            // 5. Deliver outside a business SQL transaction, then acknowledge acceptance.
            // Example: the file flush succeeds but the process crashes before the SQL ACK.
            // The next worker can write the same event again. Its stable record.Id/event_id
            // allows consumers to deduplicate; this is at-least-once, not exactly-once.
            await destination.DeliverAsync(record, cancellationToken);
            // Only the current lease owner may acknowledge this destination's entry.
            // If another worker has reclaimed it, the token no longer matches: emit Conflict.
            // Setting DeliveredAtUtc preserves the row; it does not delete audit evidence
            // or acknowledge a separate cloud entry.
            var acknowledged = await db.AuditOutbox.Where(row => row.Id == candidate && row.LeaseOwner == owner)
                .ExecuteUpdateAsync(update => update.SetProperty(row => row.DeliveredAtUtc, DateTimeOffset.UtcNow)
                    .SetProperty(row => row.LeaseOwner, (Guid?)null).SetProperty(row => row.LeaseUntilUtc, (DateTimeOffset?)null), cancellationToken);
            Telemetry.Record(Telemetry.Create(EventKind.AuditDelivery, record.Id,
                acknowledged == 1 ? EventOutcome.Succeeded : EventOutcome.Conflict,
                acknowledged == 1 ? EventReason.None : EventReason.StorageConflict));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Host shutdown/requested cancellation: propagate without claiming success.
            // Leave the lease to expire; a later process can replay this record even
            // if the destination accepted it just before cancellation was observed.
            throw;
        }
        catch (Exception)
        {
            // Store only retry state, never exception text/credentials. Attempts are bounded in
            // delay, not in total count: exhausting an arbitrary limit must not delete evidence.
            // 6. Schedule another attempt rather than sleeping while holding a lease.
            // With incremented Attempts: 1 -> 2s, 2 -> 4s, 3 -> 8s, ... 8+ -> 256s.
            // The exponent cap makes 256s the current effective maximum, below the
            // additional 300s ceiling. There is no maximum total number of attempts.
            var delay = TimeSpan.FromSeconds(Math.Min(300, Math.Pow(2, Math.Min(pending.Attempts, 8))));
            // Release only our own lease. If this SQL update itself fails, it propagates
            // to the host; lease expiry still provides a later recovery path.
            await db.AuditOutbox.Where(row => row.Id == candidate && row.LeaseOwner == owner)
                .ExecuteUpdateAsync(update => update.SetProperty(row => row.NextAttemptAtUtc, DateTimeOffset.UtcNow.Add(delay))
                    .SetProperty(row => row.LeaseOwner, (Guid?)null).SetProperty(row => row.LeaseUntilUtc, (DateTimeOffset?)null), cancellationToken);
            Telemetry.Record(Telemetry.Create(EventKind.AuditDelivery, record.Id, EventOutcome.Failed, EventReason.DeliveryUnavailable));
        }
        return true;
    }
}
