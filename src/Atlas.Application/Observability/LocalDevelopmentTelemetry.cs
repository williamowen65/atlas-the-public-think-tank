using Atlas.Contracts.Observability;
using Atlas.Observability;
using Atlas.Persistence;
using Atlas.Persistence.Observability;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Application.Observability;

/// <summary>Owns local files and a recoverable SQL-to-file delivery loop. Cloud wiring remains a separate host adapter.</summary>
/// <remarks>
/// Future production wiring should live in a separate file/class rather than mixing
/// local and production implementations here. Keep this local file-based option available
/// alongside the production option, with the host selecting the appropriate implementation.
/// This development-only implementation is sufficient for the current host; introduce
/// the separate production implementation when deployment requires it.
/// </remarks>
public sealed class LocalDevelopmentTelemetry : IDisposable
{
    private readonly LocalTelemetry _local;
    private readonly Action<string>? _reportFailure;
    private readonly IDisposable _observer;
    private readonly CancellationTokenSource _stop = new();
    private readonly Task _delivery;
    public string DirectoryPath { get; }

    public LocalDevelopmentTelemetry(IConfiguration configuration, Func<AtlasDataContext> open, Action<string>? reportFailure = null)
    {
        _reportFailure = reportFailure;
        // 1. This host wires development output only; deployment needs its own adapter.
        var environment = configuration["DOTNET_ENVIRONMENT"] ?? "Development";
        if (!string.Equals(environment, "Development", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("This local-development adapter currently supports Development telemetry only. Configure a durable cloud audit destination in the deployment host before production use.");
        // 2. Resolve the local destination and per-stream rotation/retention settings.
        DirectoryPath = Path.GetFullPath(configuration["ATLAS_TELEMETRY_DIRECTORY"] ?? Path.Combine(FindRoot(), "logs", "observability"));
        var maxBytes = ReadPositive(configuration["ATLAS_TELEMETRY_FILE_BYTES"], 5 * 1024 * 1024);
        var retained = ReadPositive(configuration["ATLAS_TELEMETRY_RETAINED_FILES"], 5);
        // 3. Create the adapter, install it in the current async flow, then emit startup.
        // Keep the returned scope so shutdown can restore the previous observer.
        _local = new(DirectoryPath, maxBytes, retained);
        _observer = Telemetry.UseObserver(_local);
        Telemetry.Record(Telemetry.Create(EventKind.HostStarted));
        // 4. SQL owns committed audit evidence. This worker delivers inspection copies
        // outside the business transaction; AuditDelivery owns leases and retry state.
        var delivery = new AuditDelivery(open, new LocalAuditDestination(new JsonLineFile(DirectoryPath, "audit", maxBytes, retained)));
        // Task.Run inherits the execution context (including our AsyncLocal observer).
        _delivery = Task.Run(async () =>
        {
            var nextHealthCheck = DateTimeOffset.MinValue;
            long lastOutputFailures = 0;
            while (!_stop.IsCancellationRequested)
            {
                try
                {
                    // Periodically measure the pending local backlog independently of delivery.
                    if (DateTimeOffset.UtcNow >= nextHealthCheck)
                    {
                        using var health = open();
                        var pending = health.AuditOutbox.Where(row => row.Destination == "local" && row.DeliveredAtUtc == null);
                        var count = await pending.LongCountAsync(_stop.Token);
                        var oldest = await (from item in pending
                            join record in health.AuditRecords on item.AuditRecordId equals record.Id
                            select (DateTimeOffset?)record.OccurredAtUtc).MinAsync(_stop.Token);
                        _local.QueueState(count, oldest is null ? 0 : Math.Max(0, (DateTimeOffset.UtcNow - oldest.Value).TotalSeconds));
                        if (_local.OutputFailures > lastOutputFailures)
                        {
                            reportFailure?.Invoke("Telemetry file output failed; check disk capacity and permissions. SQL audit records remain pending/recoverable.");
                            lastOutputFailures = _local.OutputFailures;
                        }
                        nextHealthCheck = DateTimeOffset.UtcNow.AddSeconds(10);
                    }
                    // Keep draining due entries; pause when none are currently deliverable.
                    if (!await delivery.DeliverOneAsync(_stop.Token))
                        await Task.Delay(500, _stop.Token);
                }
                catch (OperationCanceledException) when (_stop.IsCancellationRequested) { break; }
                // Report only a bounded failure reason, then retry without exception text.
                catch (Exception)
                {
                    Telemetry.Record(Telemetry.Create(EventKind.AuditDelivery, outcome: EventOutcome.Failed, reason: EventReason.DeliveryUnavailable));
                    try { await Task.Delay(1000, _stop.Token); }
                    catch (OperationCanceledException) { break; }
                }
            }
        });
    }

    private static int ReadPositive(string? value, int fallback) => value is null ? fallback
        : int.TryParse(value, out var result) && result > 0 ? result
        : throw new InvalidOperationException("Telemetry size/retention must be positive integers.");

    private static string FindRoot()
    {
        foreach (var start in new[] { Environment.CurrentDirectory, AppContext.BaseDirectory })
            for (var directory = new DirectoryInfo(start); directory is not null; directory = directory.Parent)
                if (File.Exists(Path.Combine(directory.FullName, "Atlas.sln"))) return directory.FullName;
        return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    }

    public void Dispose()
    {
        // Stop and join the worker before removing its output adapter.
        _stop.Cancel();
        _delivery.GetAwaiter().GetResult();
        // Emit the final event while the observer is still installed.
        Telemetry.Record(Telemetry.Create(EventKind.HostStopped));
        if (_local.OutputFailures > 0 || Telemetry.ObserverFailures > 0)
            _reportFailure?.Invoke("Some diagnostic output failed. SQL audit records remain authoritative; inspect file permissions and disk capacity.");
        // Restore the enclosing observer, release listeners/meters, then the stop token.
        _observer.Dispose();
        _local.Dispose();
        _stop.Dispose();
    }
}
