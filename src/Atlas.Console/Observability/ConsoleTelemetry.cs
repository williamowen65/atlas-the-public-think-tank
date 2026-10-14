using Atlas.Contracts.Observability;
using Atlas.Observability;
using Atlas.Persistence;
using Atlas.Persistence.Observability;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Atlas.ConsoleApp.Observability;

/// <summary>Owns local files and a recoverable SQL-to-file delivery loop. Cloud wiring remains a separate host adapter.</summary>
internal sealed class ConsoleTelemetry : IDisposable
{
    private readonly LocalTelemetry _local;
    private readonly IDisposable _observer;
    private readonly CancellationTokenSource _stop = new();
    private readonly Task _delivery;
    public string DirectoryPath { get; }

    public ConsoleTelemetry(IConfiguration configuration, Func<AtlasDataContext> open)
    {
        var environment = configuration["DOTNET_ENVIRONMENT"] ?? "Development";
        if (environment != "Development")
            throw new InvalidOperationException("This Console host currently supports Development telemetry only. Configure a durable cloud audit destination in the deployment host before production use.");
        DirectoryPath = Path.GetFullPath(configuration["ATLAS_TELEMETRY_DIRECTORY"] ?? Path.Combine(FindRoot(), "logs", "observability"));
        var maxBytes = ReadPositive(configuration["ATLAS_TELEMETRY_FILE_BYTES"], 5 * 1024 * 1024);
        var retained = ReadPositive(configuration["ATLAS_TELEMETRY_RETAINED_FILES"], 5);
        _local = new(DirectoryPath, maxBytes, retained);
        _observer = Telemetry.UseObserver(_local);
        Telemetry.Record(Telemetry.Create(EventKind.HostStarted));
        var delivery = new AuditDelivery(open, new LocalAuditDestination(new JsonLineFile(DirectoryPath, "audit", maxBytes, retained)));
        _delivery = Task.Run(async () =>
        {
            var nextHealthCheck = DateTimeOffset.MinValue;
            long lastOutputFailures = 0;
            while (!_stop.IsCancellationRequested)
            {
                try
                {
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
                            System.Console.Error.WriteLine("Telemetry file output failed; check disk capacity and permissions. SQL audit records remain pending/recoverable.");
                            lastOutputFailures = _local.OutputFailures;
                        }
                        nextHealthCheck = DateTimeOffset.UtcNow.AddSeconds(10);
                    }
                    if (!await delivery.DeliverOneAsync(_stop.Token))
                        await Task.Delay(500, _stop.Token);
                }
                catch (OperationCanceledException) when (_stop.IsCancellationRequested) { break; }
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
        _stop.Cancel();
        _delivery.GetAwaiter().GetResult();
        Telemetry.Record(Telemetry.Create(EventKind.HostStopped));
        if (_local.OutputFailures > 0 || Telemetry.ObserverFailures > 0)
            System.Console.Error.WriteLine("Some diagnostic output failed. SQL audit records remain authoritative; inspect file permissions and disk capacity.");
        _observer.Dispose();
        _local.Dispose();
        _stop.Dispose();
    }
}
