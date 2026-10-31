using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Text.Json;
using Atlas.Contracts.Observability;

namespace Atlas.Observability;

/// <summary>Host-side diagnostic destination and standard .NET instrumentation. No user payloads are accepted.</summary>
public sealed class LocalTelemetry : ITelemetryObserver, ICacheTelemetryObserver, IDisposable
{
    // Standard .NET APIs: ActivitySource produces timed trace spans; Meter owns
    // numeric instruments. Neither chooses a storage vendor or file by itself.
    public const string SourceName = "Atlas.Observability";
    public static readonly ActivitySource Activities = new(SourceName);
    private readonly Meter _meter = new(SourceName);
    // Counters accumulate increments; a histogram records individual measurements
    // that a collector can aggregate into distributions (for example latency).
    private readonly Counter<long> _events;
    private readonly Counter<long> _cache;
    private readonly Histogram<double> _duration;
    private readonly JsonLineFile _diagnostics;
    private readonly JsonLineFile _metrics;
    private readonly JsonLineFile _traces;
    private readonly ActivityListener _listener;
    // ActivityListener callbacks can overlap disposal on another thread.
    // One gate protects the lifetime of all file output, not just individual writes.
    private readonly object _outputGate = new();
    private bool _disposed;
    private long _failures;
    private long _pendingAudit;
    private double _oldestAuditSeconds;
    public long OutputFailures => Interlocked.Read(ref _failures);

    public LocalTelemetry(string directory, long maxBytes = 5 * 1024 * 1024, int retainedFiles = 5)
    {
        _diagnostics = new(directory, "diagnostics", maxBytes, retainedFiles);
        _metrics = new(directory, "metrics", maxBytes, retainedFiles);
        _traces = new(directory, "traces", maxBytes, retainedFiles);
        _events = _meter.CreateCounter<long>("atlas.events.count");
        _cache = _meter.CreateCounter<long>("atlas.discovery.cache.lookups");
        _duration = _meter.CreateHistogram<double>("atlas.operation.duration", "ms");
        // Observable gauges expose the latest value when a metric collector polls.
        _meter.CreateObservableGauge("atlas.audit.pending", () => Interlocked.Read(ref _pendingAudit), "{record}");
        _meter.CreateObservableGauge("atlas.audit.oldest_pending_age", () => Volatile.Read(ref _oldestAuditSeconds), "s");
        // Subscribe to the two Atlas tracing sources and request their spans.
        // When a span stops, export its allowed timing/relationship fields to JSONL.
        _listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name is SourceName or "Atlas.Contracts",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = activity =>
            {
                // Source names alone are not a privacy boundary: never export a future
                // caller's arbitrary operation label (for example a search string).
                if (activity.OperationName is not ("atlas.command" or "storage.transaction")
                    && !Enum.GetNames<EventKind>().Contains(activity.OperationName, StringComparer.Ordinal)) return;
                TryOutput(() => _traces.Write(JsonSerializer.Serialize(new
                {
                    signal = "trace",
                    observability_id = Telemetry.OperationId,
                    trace_id = activity.TraceId.ToString(),
                    span_id = activity.SpanId.ToString(),
                    parent_span_id = activity.ParentSpanId.ToString(),
                    operation = activity.OperationName,
                    privacy_policy = activity.OperationName == nameof(EventKind.NodeView)
                        ? "read_no_identity_or_target" : "standard",
                    started_at_utc = activity.StartTimeUtc,
                    duration_ms = activity.Duration.TotalMilliseconds
                })));
            }
        };
        ActivitySource.AddActivityListener(_listener);
    }

    public void Record(TelemetryEvent value)
    {
        // The same validated event feeds diagnostics and .NET numeric instruments.
        // JSONL metric examples below are written explicitly, not by a Meter listener.
        var json = EventJson.Serialize(value); // Validate before recording any signal or dimension.
        var name = EventJson.Name(value.Kind);
        var outcome = value.Outcome.ToString().ToLowerInvariant();
        _events.Add(1, new("operation", name), new("outcome", outcome));
        if (value.DurationMs > 0)
            _duration.Record(value.DurationMs, new("operation", name), new("outcome", outcome));
        TryOutput(() => _diagnostics.Write(json));
        // Local metric examples are observations/deltas, not lifetime totals presented as new counts.
        TryOutput(() => _metrics.Write(JsonSerializer.Serialize(new
        {
            signal = "metric", observed_at_utc = DateTimeOffset.UtcNow,
            name = "atlas.events.count", unit = "{event}", value = 1,
            temporality = "delta", operation = name, outcome
        })));

        if (value.DurationMs > 0)
            TryOutput(() => _metrics.Write(JsonSerializer.Serialize(new
            {
                signal = "metric", observed_at_utc = DateTimeOffset.UtcNow,
                name = "atlas.operation.duration", unit = "ms", value = value.DurationMs,
                operation = name, outcome
            })));
    }

    public void CacheLookup(bool detail, bool hit)
    {
        var layer = detail ? "detail" : "page";
        var outcome = hit ? "hit" : "miss";
        _cache.Add(1, new("layer", layer), new("outcome", outcome));
        TryOutput(() => _metrics.Write(JsonSerializer.Serialize(new
        {
            signal = "metric", observed_at_utc = DateTimeOffset.UtcNow,
            name = "atlas.discovery.cache.lookups", unit = "{lookup}", value = 1,
            temporality = "delta", layer, outcome
        })));
    }

    public void QueueState(long pending, double oldestAgeSeconds)
    {
        Interlocked.Exchange(ref _pendingAudit, pending);
        Volatile.Write(ref _oldestAuditSeconds, oldestAgeSeconds);
        TryOutput(() => _metrics.Write(JsonSerializer.Serialize(new
        {
            signal = "metric", observed_at_utc = DateTimeOffset.UtcNow,
            name = "atlas.audit.pending", unit = "{record}", value = pending,
            oldest_pending_age_seconds = oldestAgeSeconds
        })));
    }

    /// <summary>
    /// Serialize local output with disposal. A callback already handed out by
    /// ActivityListener may arrive after unsubscription; it must not write then.
    /// </summary>
    private void TryOutput(Action action)
    {
        lock (_outputGate)
        {
            if (_disposed) return;
            try { action(); }
            catch (Exception) { Interlocked.Increment(ref _failures); }
        }
    }

    /// <summary>
    /// Wait for the current synchronous file write and reject further output
    /// before releasing instrumentation. Once this returns, callers may read
    /// or remove the output directory without racing a late activity callback.
    /// </summary>
    public void Dispose()
    {
        lock (_outputGate)
        {
            if (_disposed) return;
            _disposed = true;
        }
        // Do not hold the output gate while unsubscribing instrumentation.
        _listener.Dispose();
        _meter.Dispose();
    }
}
