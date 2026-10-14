using System.Text.Json;
using System.Text.Json.Serialization;
using Atlas.Contracts.Observability;

namespace Atlas.Observability;

/// <summary>One explicit JSON shape shared by local files and future export adapters.</summary>
public static class EventJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower) }
    };

    // Anonymous projection is deliberate: adding a property to a domain/request object or
    // even the contract cannot silently extend the serialized privacy boundary.
    public static string Serialize(TelemetryEvent value)
    {
        if (!Enum.IsDefined(value.Kind) || !Enum.IsDefined(value.Outcome) || !Enum.IsDefined(value.ReasonCode)
            || !Enum.IsDefined(value.ActorKind) || !Enum.IsDefined(value.Action)
            || value.ModerationReason is { } moderation && !Enum.IsDefined(moderation)
            || value.EventId == Guid.Empty || value.ObservabilityOperationId == Guid.Empty
            || !double.IsFinite(value.DurationMs) || value.DurationMs < 0
            || value.ActorKind == ActorKind.Participant && (value.ActorId is null || value.ActorId == Guid.Empty)
            || value.ActorKind != ActorKind.Participant && value.ActorId is not null
            || !ValidHex(value.TraceId, 32) || !ValidHex(value.SpanId, 16))
            throw new ArgumentException("Invalid telemetry envelope.");
        return JsonSerializer.Serialize(new
    {
        schema_version = value.SchemaVersion,
        service = "atlas-console",
        environment = "development",
        app_version = typeof(EventJson).Assembly.GetName().Version?.ToString(),
        event_id = value.EventId,
        occurred_at_utc = value.OccurredAtUtc,
        observability_operation_id = value.ObservabilityOperationId,
        event_name = Name(value.Kind),
        actor_kind = value.ActorKind,
        actor_id = value.ActorId,
        target_id = value.TargetId,
        target_kind = Target(value.Kind),
        action = value.Action,
        related_target_id = value.RelatedTargetId,
        moderation_reason = value.ModerationReason,
        outcome = value.Outcome,
        reason_code = value.ReasonCode,
        duration_ms = value.DurationMs,
        trace_id = value.TraceId,
        span_id = value.SpanId
    }, Options);
    }

    private static bool ValidHex(string? value, int size) => value is null || value.Length == size && value.All(Uri.IsHexDigit);

    private static string? Target(EventKind kind) => kind switch
    {
        EventKind.NodeChanged => "node", EventKind.NodeTypeChanged => "node_type",
        EventKind.ModerationChanged => "moderation_case", EventKind.RoleDefinitionChanged => "role", EventKind.RoleChanged or EventKind.AccountSecurityChanged or EventKind.ProfileChanged => "participant",
        EventKind.ContentChanged => "content", EventKind.VoteChanged => "vote", EventKind.ReactionChanged => "reaction",
        EventKind.CommunityChanged or EventKind.MembershipChanged or EventKind.CommunityLinkChanged => "community",
        EventKind.CommentChanged => "comment", EventKind.NotificationChanged => "notification",
        EventKind.AuditDelivery => "audit_record", _ => null
    };

    public static string Name(EventKind kind) => kind switch
    {
        EventKind.HostStarted => "host.started", EventKind.HostStopped => "host.stopped",
        EventKind.SignIn => "identity.sign_in_completed", EventKind.SignOut => "identity.sign_out_completed",
        EventKind.Registration => "identity.registration_completed", EventKind.SessionRejected => "identity.session_rejected",
        EventKind.NodeChanged => "node.changed", EventKind.NodeTypeChanged => "node_type.changed",
        EventKind.ContentChanged => "content.changed", EventKind.VoteChanged => "vote.changed",
        EventKind.ReactionChanged => "reaction.changed", EventKind.CommunityChanged => "community.changed",
        EventKind.MembershipChanged => "community.membership_changed", EventKind.CommunityLinkChanged => "community.node_link_changed",
        EventKind.CommentChanged => "comment.changed", EventKind.ModerationChanged => "moderation.changed",
        EventKind.ProfileChanged => "participant.profile_changed", EventKind.AccountSecurityChanged => "identity.account_security_changed",
        EventKind.RoleChanged => "identity.role_changed", EventKind.RoleDefinitionChanged => "identity.role_definition_changed", EventKind.NotificationChanged => "notification.changed",
        EventKind.DiscoveryRead => "discovery.read_completed", EventKind.StorageWrite => "storage.write_completed",
        EventKind.AuditDelivery => "audit.delivery_completed", _ => throw new ArgumentOutOfRangeException(nameof(kind))
    };
}
