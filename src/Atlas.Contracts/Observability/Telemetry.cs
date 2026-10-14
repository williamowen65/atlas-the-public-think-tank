using System.Diagnostics;

namespace Atlas.Contracts.Observability;

/// <summary>Closed event vocabulary: callers cannot insert payloads, credentials or arbitrary properties.</summary>
public enum EventKind
{
    HostStarted, HostStopped, SignIn, SignOut, Registration, SessionRejected,
    NodeChanged, NodeTypeChanged, ContentChanged, VoteChanged, ReactionChanged,
    CommunityChanged, MembershipChanged, CommunityLinkChanged, CommentChanged,
    ModerationChanged, ProfileChanged, AccountSecurityChanged, RoleChanged, RoleDefinitionChanged,
    NotificationChanged, DiscoveryRead, StorageWrite, AuditDelivery
}
public enum EventOutcome { Succeeded, Denied, Failed, Conflict }
public enum EventReason { None, NotAuthorized, InvalidInput, CredentialsRejected, LockedOut, NotAllowed, SecondFactorRequired, StorageConflict, InfrastructureFailure, CommitOutcomeUnknown, DeliveryUnavailable }
public enum ActorKind { Anonymous, Participant, System }
public enum MutationAction { None, Created, Updated, Deleted, Hidden, Restored, Dismissed, ReviewRequested, RoleAssigned, RoleRemoved, PasswordChanged, EmailConfirmed, LockoutChanged, Linked, Unlinked }

/// <summary>Allowlisted event envelope. No free-form text or extension dictionary is accepted.</summary>
public sealed record TelemetryEvent(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    Guid ObservabilityOperationId,
    EventKind Kind,
    ActorKind ActorKind,
    Guid? ActorId,
    Guid? TargetId,
    EventOutcome Outcome,
    EventReason ReasonCode,
    double DurationMs = 0,
    string? TraceId = null,
    string? SpanId = null,
    MutationAction Action = MutationAction.None,
    Guid? RelatedTargetId = null)
{
    public int SchemaVersion => 1;
}

/// <summary>Host-owned output port; domain/application code never selects files or cloud vendors.</summary>
public interface ICacheTelemetryObserver
{
    void CacheLookup(bool detail, bool hit);
}

public interface ITelemetryObserver
{
    void Record(TelemetryEvent value);
}

/// <summary>Operation-local context. IDs correlate evidence; they never establish authentication.</summary>
public static class Telemetry
{
    public static readonly ActivitySource Activities = new("Atlas.Contracts");
    private static readonly AsyncLocal<Context?> Ambient = new();
    private sealed record Context(ITelemetryObserver? Observer, Guid OperationId, Guid? ActorId, ActorKind ActorKind)
    {
        public HashSet<Exception> ReportedFailures { get; } = new(ReferenceEqualityComparer.Instance);
    }

    /// <summary>Install a host/test observer in this async flow; disposal restores its predecessor.</summary>
    public static IDisposable UseObserver(ITelemetryObserver observer)
    {
        var previous = Ambient.Value;
        Ambient.Value = new(observer, Guid.Empty, null, ActorKind.Anonymous);
        return new Restore(() => Ambient.Value = previous);
    }

    /// <summary>Start a command/request, with an actor already verified by the host.</summary>
    public static IDisposable BeginOperation(Guid? verifiedActorId = null, bool system = false)
    {
        if (verifiedActorId == Guid.Empty || system && verifiedActorId is not null) throw new ArgumentException("Actor cannot be empty.");
        var previous = Ambient.Value;
        Ambient.Value = new(previous?.Observer, Guid.NewGuid(), verifiedActorId,
            system ? ActorKind.System : verifiedActorId is null ? ActorKind.Anonymous : ActorKind.Participant);
        var activity = Activities.StartActivity("atlas.command");
        return new Restore(() => { activity?.Dispose(); Ambient.Value = previous; });
    }

    /// <summary>Observe a reusable workflow without copying inputs, search terms or exception messages.</summary>
    public static T Observe<T>(EventKind kind, Func<T> operation)
    {
        using var activity = Activities.StartActivity(kind.ToString());
        var started = Stopwatch.GetTimestamp();
        try
        {
            var result = operation();
            Record(Create(kind, durationMs: Stopwatch.GetElapsedTime(started).TotalMilliseconds));
            return result;
        }
        catch (Exception error)
        {
            Failure(kind, error);
            throw;
        }
    }

    public static void CacheLookup(bool detail, bool hit)
    {
        try { (Ambient.Value?.Observer as ICacheTelemetryObserver)?.CacheLookup(detail, hit); }
        catch (Exception) { Interlocked.Increment(ref _observerFailures); }
    }

    public static Guid? OperationId => Ambient.Value is { OperationId: var id } && id != Guid.Empty ? id : null;

    /// <summary>Enrich the current operation only after authentication completes; preserve its correlation ID.</summary>
    public static IDisposable UseVerifiedActor(Guid actorId)
    {
        if (actorId == Guid.Empty) throw new ArgumentException("Actor cannot be empty.");
        var previous = Ambient.Value;
        Ambient.Value = previous is null ? new(null, Guid.NewGuid(), actorId, ActorKind.Participant)
            : previous with { ActorId = actorId, ActorKind = ActorKind.Participant };
        return new Restore(() => Ambient.Value = previous);
    }

    public static IDisposable JoinOperation() => Ambient.Value is { OperationId: var id } && id != Guid.Empty
        ? new Restore(() => { }) : BeginOperation(system: true);

    public static TelemetryEvent Create(EventKind kind, Guid? targetId = null,
        EventOutcome outcome = EventOutcome.Succeeded, EventReason reason = EventReason.None, double durationMs = 0)
    {
        if (!Enum.IsDefined(kind) || !Enum.IsDefined(outcome) || !Enum.IsDefined(reason))
            throw new ArgumentOutOfRangeException(nameof(kind));
        var context = Ambient.Value;
        return new(Guid.NewGuid(), DateTimeOffset.UtcNow, context is { OperationId: var id } && id != Guid.Empty ? id : Guid.NewGuid(), kind,
            context?.ActorKind ?? ActorKind.System, context?.ActorId, targetId, outcome, reason, durationMs,
            Activity.Current?.TraceId.ToString(), Activity.Current?.SpanId.ToString());
    }

    /// <summary>Diagnostic output is best effort. A failing destination must not alter business results.</summary>
    public static void Record(TelemetryEvent value)
    {
        try { Ambient.Value?.Observer?.Record(value); }
        catch (Exception) { Interlocked.Increment(ref _observerFailures); }
    }
    private static long _observerFailures;
    public static long ObserverFailures => Interlocked.Read(ref _observerFailures);

    /// <summary>One diagnostic owner per exception in an operation; never serialize exception text.</summary>
    public static void Failure(EventKind kind, Exception error, EventReason? reason = null)
    {
        var context = Ambient.Value;
        if (context is not null)
            lock (context.ReportedFailures)
                if (!context.ReportedFailures.Add(error)) return;
        Record(Create(kind, outcome: error is UnauthorizedAccessException ? EventOutcome.Denied : EventOutcome.Failed,
            reason: reason ?? Reason(error)));
    }

    public static EventReason Reason(Exception error) => error switch
    {
        UnauthorizedAccessException => EventReason.NotAuthorized,
        ArgumentException => EventReason.InvalidInput,
        _ => EventReason.InfrastructureFailure
    };

    private sealed class Restore(Action restore) : IDisposable
    {
        private Action? _restore = restore;
        public void Dispose() => Interlocked.Exchange(ref _restore, null)?.Invoke();
    }
}
