using Atlas.Api;
using Atlas.Contracts.Observability;
using Microsoft.Extensions.Logging;

namespace Atlas.Api.Tests;

[TestClass]
public sealed class CloudTelemetryTests
{
    private sealed class CaptureLogger : ILogger<CloudTelemetry>
    {
        public List<string> Messages { get; } = new();
        public IDisposable BeginScope<TState>(TState state) where TState : notnull => Noop.Instance;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter) => Messages.Add(formatter(state, exception));

        private sealed class Noop : IDisposable
        {
            public static readonly Noop Instance = new();
            public void Dispose() { }
        }
    }

    [TestMethod]
    public void Cloud_observer_exposes_only_bounded_diagnostic_labels()
    {
        var logger = new CaptureLogger();
        var actorId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        using var observer = new CloudTelemetry(logger, "demo");
        observer.Record(new TelemetryEvent(
            Guid.NewGuid(), DateTimeOffset.UtcNow, Guid.NewGuid(), EventKind.ModerationChanged,
            ActorKind.Participant, actorId, targetId, EventOutcome.Succeeded, EventReason.None,
            DurationMs: 25, Host: TelemetryHost.Api));
        observer.CacheLookup(detail: true, hit: false);

        Assert.AreEqual(1, logger.Messages.Count);
        StringAssert.Contains(logger.Messages.Single(), "moderation.changed");
        StringAssert.Contains(logger.Messages.Single(), "demo");
        Assert.IsFalse(logger.Messages.Single().Contains(actorId.ToString(), StringComparison.OrdinalIgnoreCase));
        Assert.IsFalse(logger.Messages.Single().Contains(targetId.ToString(), StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void Cloud_observer_does_not_export_an_invalid_privacy_envelope()
    {
        var logger = new CaptureLogger();
        using var observer = new CloudTelemetry(logger, "live");
        var invalid = new TelemetryEvent(
            Guid.NewGuid(), DateTimeOffset.UtcNow, Guid.NewGuid(), EventKind.NodeView,
            ActorKind.Participant, Guid.NewGuid(), Guid.NewGuid(),
            EventOutcome.Succeeded, EventReason.None, Host: TelemetryHost.Api);
        Assert.ThrowsExactly<ArgumentException>(() => observer.Record(invalid));
        Assert.AreEqual(0, logger.Messages.Count);
    }
}
