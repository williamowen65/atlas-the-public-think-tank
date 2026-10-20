using Atlas.Contracts.Observability;
using Atlas.Observability;
using Atlas.Persistence;
using Atlas.Persistence.Observability;
namespace Atlas.Api;

/// <summary>Local inspection output for development. Production exporters remain PTT-136.</summary>
public sealed class ApiTelemetry : IDisposable
{
    public LocalTelemetry? Observer { get; }
    public string? DirectoryPath { get; }
    public ApiTelemetry(IConfiguration configuration, IWebHostEnvironment environment)
    {
        if (!environment.IsDevelopment() || configuration.GetValue<bool?>("Atlas:Telemetry:Enabled") == false) return;
        DirectoryPath = configuration["ATLAS_TELEMETRY_DIRECTORY"] ?? Path.Combine(AppContext.BaseDirectory, "logs", "observability");
        Observer = new LocalTelemetry(DirectoryPath);
    }
    public void Dispose() => Observer?.Dispose();
}

/// <summary>One asynchronous delivery loop per API process; leases coordinate overlapping processes.</summary>
public sealed class ApiAuditWorker(ApiTelemetry telemetry, Func<AtlasDataContext> open, ILogger<ApiAuditWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (telemetry.DirectoryPath is null) return;
        using var host = Telemetry.UseHost(TelemetryHost.Api);
        using var observer = Telemetry.UseObserver(telemetry.Observer!);
        var delivery = new AuditDelivery(open, new LocalAuditDestination(new JsonLineFile(telemetry.DirectoryPath, "audit")));
        while (!stoppingToken.IsCancellationRequested)
        {
            try { if (!await delivery.DeliverOneAsync(stoppingToken)) await Task.Delay(500, stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception) { logger.LogWarning("Local audit delivery failed; committed SQL evidence remains available for retry."); await Task.Delay(1000, stoppingToken); }
        }
    }
}
