using Microsoft.EntityFrameworkCore;
using Atlas.Contracts.Observability;
using Atlas.Observability;

namespace Atlas.Persistence.Observability;

/// <summary>Delivery adapters acknowledge only durable acceptance. Future cloud adapters implement this port.</summary>
public interface IAuditDestination
{
    string Name { get; }
    Task DeliverAsync(AuditRecordRow record, CancellationToken cancellationToken);
}

/// <summary>Local inspection copy; SQL remains the authoritative history after file rotation.</summary>
public sealed class LocalAuditDestination(JsonLineFile file) : IAuditDestination
{
    public string Name => "local";
    public Task DeliverAsync(AuditRecordRow record, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        file.Write(record.PayloadJson);
        return Task.CompletedTask;
    }
}

/// <summary>Leases one row at a time across processes. A crash or lost acknowledgement can duplicate, never discard, delivery.</summary>
public sealed class AuditDelivery(Func<AtlasDataContext> open, IAuditDestination destination)
{
    public async Task<bool> DeliverOneAsync(CancellationToken cancellationToken = default)
    {
        using var operation = Telemetry.BeginOperation(system: true);
        using var db = open();
        var now = DateTimeOffset.UtcNow;
        var candidate = await db.AuditOutbox.AsNoTracking()
            .Where(row => row.Destination == destination.Name && row.DeliveredAtUtc == null &&
                row.NextAttemptAtUtc <= now && (row.LeaseUntilUtc == null || row.LeaseUntilUtc < now))
            .OrderBy(row => row.NextAttemptAtUtc).ThenBy(row => row.Id)
            .Select(row => (Guid?)row.Id).FirstOrDefaultAsync(cancellationToken);
        if (candidate is null) return false;
        var owner = Guid.NewGuid();
        // Compare-and-set lease: only one competing worker wins. An expired lease is recoverable.
        var claimed = await db.AuditOutbox.Where(row => row.Id == candidate && row.Destination == destination.Name &&
            row.DeliveredAtUtc == null && row.NextAttemptAtUtc <= now &&
            (row.LeaseUntilUtc == null || row.LeaseUntilUtc < now))
            .ExecuteUpdateAsync(update => update.SetProperty(row => row.LeaseOwner, owner)
                .SetProperty(row => row.Attempts, row => row.Attempts + 1)
                .SetProperty(row => row.LeaseUntilUtc, now.AddMinutes(2)), cancellationToken);
        if (claimed != 1) return true;
        var pending = await db.AuditOutbox.AsNoTracking().SingleAsync(row => row.Id == candidate, cancellationToken);
        var record = await db.AuditRecords.AsNoTracking().SingleAsync(row => row.Id == pending.AuditRecordId, cancellationToken);
        try
        {
            await destination.DeliverAsync(record, cancellationToken);
            var acknowledged = await db.AuditOutbox.Where(row => row.Id == candidate && row.LeaseOwner == owner)
                .ExecuteUpdateAsync(update => update.SetProperty(row => row.DeliveredAtUtc, DateTimeOffset.UtcNow)
                    .SetProperty(row => row.LeaseOwner, (Guid?)null).SetProperty(row => row.LeaseUntilUtc, (DateTimeOffset?)null), cancellationToken);
            Telemetry.Record(Telemetry.Create(EventKind.AuditDelivery, record.Id,
                acknowledged == 1 ? EventOutcome.Succeeded : EventOutcome.Conflict,
                acknowledged == 1 ? EventReason.None : EventReason.StorageConflict));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Leave the lease to expire; a later process can replay this record.
            throw;
        }
        catch (Exception)
        {
            // Store only retry state, never exception text/credentials. Attempts are bounded in
            // delay, not in total count: exhausting an arbitrary limit must not delete evidence.
            var delay = TimeSpan.FromSeconds(Math.Min(300, Math.Pow(2, Math.Min(pending.Attempts, 8))));
            await db.AuditOutbox.Where(row => row.Id == candidate && row.LeaseOwner == owner)
                .ExecuteUpdateAsync(update => update.SetProperty(row => row.NextAttemptAtUtc, DateTimeOffset.UtcNow.Add(delay))
                    .SetProperty(row => row.LeaseOwner, (Guid?)null).SetProperty(row => row.LeaseUntilUtc, (DateTimeOffset?)null), cancellationToken);
            Telemetry.Record(Telemetry.Create(EventKind.AuditDelivery, record.Id, EventOutcome.Failed, EventReason.DeliveryUnavailable));
        }
        return true;
    }
}
