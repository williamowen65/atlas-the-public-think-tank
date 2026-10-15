using System.Diagnostics;

namespace Atlas.Contracts.Observability;

/// <summary>Closed event vocabulary: callers cannot insert payloads, credentials or arbitrary properties.</summary>
public enum EventKind
{
    HostStarted, HostStopped, SignIn, SignOut, Registration, SessionRejected,
    NodeChanged, NodeTypeChanged, ContentChanged, VoteChanged, ReactionChanged,
    CommunityChanged, MembershipChanged, CommunityLinkChanged, CommentChanged,
    ModerationChanged, ProfileChanged, AccountSecurityChanged, RoleChanged, RoleDefinitionChanged,
    NotificationChanged, DiscoveryRead, StorageWrite, AuditDelivery, NodeView
}
public enum EventOutcome { Succeeded, Denied, Failed, Conflict }
public enum EventReason { None, NotAuthorized, InvalidInput, CredentialsRejected, LockedOut, NotAllowed, SecondFactorRequired, StorageConflict, InfrastructureFailure, CommitOutcomeUnknown, DeliveryUnavailable }
public enum PrivacyPolicy { Standard, ReadNoIdentityOrTarget }
public enum ActorKind { Anonymous, Participant, System }
public enum MutationAction { None, Created, Updated, Deleted, Hidden, Restored, Dismissed, ReviewRequested, RoleAssigned, RoleRemoved, PasswordChanged, EmailConfirmed, LockoutChanged, Linked, Unlinked, Archived, Activated, Deactivated, Renamed, TypeChanged }
public enum AuditModerationReason { Other, Spam, Harassment, UnsafeContent, OffTopic }

/// <summary>Allowlisted event envelope. No free-form text or extension dictionary is accepted.</summary>
// A positional record generates properties and a constructor from these parameters.
// Optional parameters below supply constructor defaults; sealed prevents inheritance.
// The bounded fields (plus output validation), rather than sealed itself, limit logged data.
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
    Guid? RelatedTargetId = null,
    AuditModerationReason? ModerationReason = null,
    PrivacyPolicy PrivacyPolicy = PrivacyPolicy.Standard)
{
    // Computed, read-only property: every envelope currently uses schema version 1.
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
    // Standard .NET tracing API (System.Diagnostics), not a file or cloud destination.
    // The string names this instrumentation source so a host listener can subscribe.
    // Contracts owns the shared vocabulary/context; host adapters own signal export.
    public static readonly ActivitySource Activities = new("Atlas.Contracts");
    // A field holding a context slot, not a method. Its Value follows the logical
    // execution flow across await, even when execution resumes on another thread.
    // "Ambient" means helpers can read the current scope without a context parameter.
    // Child flows inherit the value; this does not deep-copy the Context object.
    private static readonly AsyncLocal<Context?> Ambient = new();
    // The header generates four properties: observer, operation ID and actor identity.
    // The body adds per-context state; this record is not just a failure tracker.
    private sealed record Context(ITelemetryObserver? Observer, Guid OperationId, Guid? ActorId, ActorKind ActorKind)
    {
        // A new set per Context. Reference equality identifies the same exception
        // passing through nested catch blocks, so it is reported only once.
        // A record `with` copy retains this set while changing actor properties.
        public HashSet<Exception> ReportedFailures { get; } = new(ReferenceEqualityComparer.Instance);
    }

    /// <summary>Install a host/test observer in this async flow; disposal restores its predecessor.</summary>
    public static IDisposable UseObserver(ITelemetryObserver observer)
    {
        ArgumentNullException.ThrowIfNull(observer);
        // Save the enclosing scope, then install the host-selected output adapter.
        // Target-typed new(...) here constructs Context from the assigned field type.
        var previous = Ambient.Value;
        Ambient.Value = new(observer, Guid.Empty, null, ActorKind.Anonymous);
        // using/Dispose invokes this callback to put the enclosing context back.
        return new Restore(() => Ambient.Value = previous);
    }

    /// <summary>Start a command/request, with an actor already verified by the host.</summary>
    public static IDisposable BeginOperation(Guid? verifiedActorId = null, bool system = false)
    {
        if (verifiedActorId == Guid.Empty || system && verifiedActorId is not null) throw new ArgumentException("Actor cannot be empty.");
        var previous = Ambient.Value;
        Ambient.Value = new(previous?.Observer, Guid.NewGuid(), verifiedActorId,
            system ? ActorKind.System : verifiedActorId is null ? ActorKind.Anonymous : ActorKind.Participant);
        // Begin a timed trace span. With no interested listener, this may return null.
        // Nested activities normally inherit Activity.Current as their parent.
        var activity = Activities.StartActivity("atlas.command");
        // Stop the span before restoring context: the stop listener still needs
        // this operation ID when it exports the completed span.
        return new Restore(() => { activity?.Dispose(); Ambient.Value = previous; });
    }

    /// <summary>Observe a reusable workflow without copying inputs, search terms or exception messages.</summary>
    public static T Observe<T>(EventKind kind, Func<T> operation)
    {
        // Unlike BeginOperation (a scope around caller code), Observe invokes the
        // supplied delegate and measures it here. using ends the span on return/throw.
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

    /// <summary>
    /// Measure detail loading/rendering without building a reading history. A new root trace
    /// and correlation ID prevent joining this read to an authenticated parent command.
    /// This scope never spans the interactive menu or a subsequent mutation.
    /// </summary>
    public static void ObserveNodeView(Action render)
    {
        var previous = Ambient.Value;
        var parentActivity = Activity.Current;
        Activity.Current = null;
        Ambient.Value = new(previous?.Observer, Guid.NewGuid(), null, ActorKind.System);
        try
        {
            using var activity = Activities.StartActivity(nameof(EventKind.NodeView));
            var started = Stopwatch.GetTimestamp();
            try
            {
                render();
                Record(Create(EventKind.NodeView, durationMs: Stopwatch.GetElapsedTime(started).TotalMilliseconds));
            }
            catch (Exception error)
            {
                Record(Create(EventKind.NodeView,
                    outcome: error is UnauthorizedAccessException ? EventOutcome.Denied : EventOutcome.Failed,
                    reason: Reason(error), durationMs: Stopwatch.GetElapsedTime(started).TotalMilliseconds));
                throw;
            }
        }
        finally
        {
            Ambient.Value = previous;
            Activity.Current = parentActivity;
        }
    }

    public static void CacheLookup(bool detail, bool hit)
    {
        // Cache metrics are an optional capability of the installed observer.
        // Ordinary event observers need not implement this separate interface.
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

    // Reuse an existing command context; otherwise own a temporary system scope.
    // The no-op disposable lets both cases use the same using pattern.
    public static IDisposable JoinOperation() => Ambient.Value is { OperationId: var id } && id != Guid.Empty
        ? new Restore(() => { }) : BeginOperation(system: true);

    public static TelemetryEvent Create(EventKind kind, Guid? targetId = null,
        EventOutcome outcome = EventOutcome.Succeeded, EventReason reason = EventReason.None, double durationMs = 0)
    {
        if (!Enum.IsDefined(kind) || !Enum.IsDefined(outcome) || !Enum.IsDefined(reason))
            throw new ArgumentOutOfRangeException(nameof(kind));
        // Build evidence from the active scope; constructing an event does not emit it.
        var context = Ambient.Value;
        return new(Guid.NewGuid(), DateTimeOffset.UtcNow, context is { OperationId: var id } && id != Guid.Empty ? id : Guid.NewGuid(), kind,
            context?.ActorKind ?? ActorKind.System, context?.ActorId, targetId, outcome, reason, durationMs,
            Activity.Current?.TraceId.ToString(), Activity.Current?.SpanId.ToString(),
            PrivacyPolicy: kind == EventKind.NodeView ? PrivacyPolicy.ReadNoIdentityOrTarget : PrivacyPolicy.Standard);
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

    // Our own nested helper, not an imported framework type. The primary-constructor
    // parameter is a callback, captured in a field until cleanup is requested.
    // IDisposable provides deterministic scope cleanup, not garbage collection:
    // using calls Dispose even when the scope exits through an exception.
    private sealed class Restore(Action restore) : IDisposable
    {
        private Action? _restore = restore;
        // Clear and take the callback atomically; repeated disposal runs it only once.
        public void Dispose() => Interlocked.Exchange(ref _restore, null)?.Invoke();
    }
}
