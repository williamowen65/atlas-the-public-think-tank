using System.Diagnostics.Metrics;
using Atlas.Contracts.Observability;
using Atlas.Observability;

namespace Atlas.Api;

/// <summary>
/// Provider-neutral, allowlisted API telemetry instrument. In deployment,
/// Microsoft OpenTelemetry exports ILogger, ActivitySource and Meter signals to
/// the Application Insights resource configured by Terraform.
/// </summary>
/// <remarks>
/// The SQL audit record and outbox remain authoritative for required audit events.
/// Logger/metric exporter delivery is best effort, never an outbox acknowledgement.
/// </remarks>
public sealed class CloudTelemetry : ITelemetryObserver, ICacheTelemetryObserver, IDisposable
{
    private const string MeterName = "Atlas.Observability";
    private readonly Meter _meter = new(MeterName);
    private readonly Counter<long> _events;
    private readonly Counter<long> _cache;
    private readonly Histogram<double> _durations;
    private readonly ILogger<CloudTelemetry> _logger;
    private readonly string _variant;

    public CloudTelemetry(ILogger<CloudTelemetry> logger, string? deploymentVariant)
    {
        _logger = logger;
        _variant = deploymentVariant is "demo" or "live" ? deploymentVariant : "unknown";
        _events = _meter.CreateCounter<long>("atlas.events.count");
        _cache = _meter.CreateCounter<long>("atlas.discovery.cache.lookups");
        _durations = _meter.CreateHistogram<double>("atlas.operation.duration", "ms");
    }

    public void Record(TelemetryEvent value)
    {
        // The existing serializer validates the closed vocabulary and privacy rules
        // before anything is passed to a cloud logging provider.
        _ = EventJson.Serialize(value);
        var name = EventJson.Name(value.Kind);
        var outcome = value.Outcome.ToString().ToLowerInvariant();
        var reason = value.ReasonCode.ToString().ToLowerInvariant();
        _events.Add(1, new("operation", name), new("outcome", outcome), new("variant", _variant));
        if (value.DurationMs > 0)
            _durations.Record(value.DurationMs, new("operation", name), new("outcome", outcome), new("variant", _variant));

        // Never log body text, SQL, URL, credentials, actor IDs or target IDs.
        _logger.LogInformation(
            "Atlas event {EventName} {Outcome} {ReasonCode} {DeploymentVariant}",
            name, outcome, reason, _variant);
    }

    public void CacheLookup(bool detail, bool hit)
    {
        _cache.Add(1,
            new("layer", detail ? "detail" : "page"),
            new("outcome", hit ? "hit" : "miss"),
            new("variant", _variant));
    }

    public void Dispose() => _meter.Dispose();
}
