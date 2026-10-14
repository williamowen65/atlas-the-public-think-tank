using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using Atlas.Contracts.Observability;
using Atlas.Contracts.Operations;
using Atlas.Observability;

namespace Atlas.ConsoleApp.Tests;

[TestClass]
public sealed class ObservabilityTests
{
    [TestMethod]
    public void Events_allow_only_reviewed_fields_and_bounded_values()
    {
        using var operation = Telemetry.BeginOperation(Guid.NewGuid());
        var json = EventJson.Serialize(Telemetry.Create(EventKind.NodeChanged, Guid.NewGuid()));
        using var document = JsonDocument.Parse(json);
        CollectionAssert.AreEquivalent(new[] { "schema_version", "service", "environment", "app_version", "event_id", "occurred_at_utc",
            "observability_operation_id", "event_name", "actor_kind", "actor_id", "target_id", "target_kind", "action", "related_target_id", "moderation_reason",
            "outcome", "reason_code", "duration_ms", "trace_id", "span_id" }, document.RootElement.EnumerateObject().Select(p => p.Name).ToArray());
        Assert.AreEqual("node.changed", document.RootElement.GetProperty("event_name").GetString());
        Assert.Throws<ArgumentException>(() => EventJson.Serialize(Telemetry.Create(EventKind.SignIn) with { TraceId = "password-canary" }));
        Assert.Throws<ArgumentException>(() => EventJson.Serialize(Telemetry.Create(EventKind.SignIn) with { DurationMs = double.NaN }));
        Assert.Throws<ArgumentException>(() => EventJson.Serialize(Telemetry.Create(EventKind.SignIn) with { Action = (MutationAction)999 }));
    }

    [TestMethod]
    public void Exception_secrets_are_never_serialized_and_duplicate_failure_has_one_owner()
    {
        var sink = new RecordingSink();
        using var observer = Telemetry.UseObserver(sink);
        using var operation = Telemetry.BeginOperation();
        var error = new UnauthorizedAccessException("PASSWORD-canary cookie=canary body=canary\nforged log entry");
        Telemetry.Failure(EventKind.StorageWrite, error);
        Telemetry.Failure(EventKind.StorageWrite, error);
        var value = sink.Events.Single();
        Assert.AreEqual(EventReason.NotAuthorized, value.ReasonCode);
        Assert.DoesNotContain("canary", EventJson.Serialize(value));
        Assert.AreEqual(EventOutcome.Denied, value.Outcome);
    }

    [TestMethod]
    public async Task Parallel_operations_keep_actors_and_correlation_isolated()
    {
        var sink = new RecordingSink();
        using var observer = Telemetry.UseObserver(sink);
        var actors = new[] { Guid.NewGuid(), Guid.NewGuid() };
        await Task.WhenAll(actors.Select(async actor =>
        {
            using var operation = Telemetry.BeginOperation(actor);
            var first = Telemetry.Create(EventKind.ProfileChanged);
            await Task.Yield();
            var second = Telemetry.Create(EventKind.CommentChanged);
            Assert.AreEqual(first.ObservabilityOperationId, second.ObservabilityOperationId);
            Assert.AreEqual(actor, second.ActorId);
            Telemetry.Record(first);
        }));
        CollectionAssert.AreEquivalent(actors, sink.Events.Select(value => value.ActorId!.Value).ToArray());
        Assert.AreEqual(2, sink.Events.Select(value => value.ObservabilityOperationId).Distinct().Count());
        Assert.IsNull(Telemetry.Create(EventKind.SignIn).ActorId);
    }

    [TestMethod]
    public void Failed_diagnostic_sink_does_not_change_workflow_result()
    {
        using var observer = Telemetry.UseObserver(new BrokenSink());
        var result = Telemetry.Observe(EventKind.StorageWrite, () => 42);
        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public void Real_files_capture_three_signal_shapes_and_rotate_without_cloud()
    {
        var directory = Path.Combine(Path.GetTempPath(), "AtlasTelemetryTests-" + Guid.NewGuid());
        try
        {
            using (var local = new LocalTelemetry(directory, maxBytes: 1024, retainedFiles: 2))
            using (Telemetry.UseObserver(local))
            using (Telemetry.BeginOperation(Guid.NewGuid()))
            {
                Telemetry.Observe(EventKind.DiscoveryRead, () => 7);
                local.CacheLookup(detail: false, hit: true);
                using (Telemetry.Activities.StartActivity("private-canary-search")) { }
            }
            var files = Directory.GetFiles(directory);
            Assert.IsTrue(files.Any(path => Path.GetFileName(path).StartsWith("diagnostics-")));
            Assert.IsTrue(files.Any(path => Path.GetFileName(path).StartsWith("metrics-")));
            Assert.IsTrue(files.Any(path => Path.GetFileName(path).StartsWith("traces-")));
            foreach (var line in files.SelectMany(File.ReadLines))
            {
                Assert.DoesNotContain("canary", line);
                using (JsonDocument.Parse(line)) { }
            }
            var rotating = new JsonLineFile(directory, "diagnostics", maxBytes: 1024, retainedFiles: 2);
            for (var i = 0; i < 10; i++) rotating.Write(JsonSerializer.Serialize(new { padding = new string('x', 800) }));
            Assert.IsLessThanOrEqualTo(2, Directory.GetFiles(directory, "diagnostics-*.jsonl").Length);
        }
        finally { if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true); }
    }

    private sealed class RecordingSink : ITelemetryObserver
    {
        public ConcurrentBag<TelemetryEvent> Events { get; } = [];
        public void Record(TelemetryEvent value) => Events.Add(value);
    }
    private sealed class BrokenSink : ITelemetryObserver
    {
        public void Record(TelemetryEvent value) => throw new IOException("secret-canary");
    }
}
