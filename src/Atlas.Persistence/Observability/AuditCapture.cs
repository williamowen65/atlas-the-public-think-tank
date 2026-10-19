using Atlas.Contracts.Observability;
using Atlas.Contracts.Operations;
using Atlas.Observability;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Atlas.Persistence.Identity;

namespace Atlas.Persistence.Observability;

/// <summary>
/// Bridges Entity Framework tracked changes and structured telemetry. During a save,
/// Stage translates explicitly mapped entity transitions into bounded events, so business
/// logic need not construct those persistence-change logs manually. Only approved IDs
/// and enum values enter events; raw rows, navigation payloads and private text are not serialized.
/// </summary>
/// <remarks>
/// Selected security-relevant transitions also stage audit evidence and outbox delivery
/// intent in the same context as the business changes. Stage does not save, commit or emit
/// diagnostics: the save/transaction owner persists the rows and publishes events only
/// after a confirmed commit. Unmapped entities and direct SQL writes are not captured.
/// </remarks>
internal static class AuditCapture
{
    /// <summary>Return staged diagnostic events and add required audit/outbox rows for the upcoming EF save.</summary>
    public static List<TelemetryEvent> Stage(AtlasDataContext db)
    {
        // Refresh EF states before classifying changes. Snapshot the entries because
        // staging audit/outbox rows below adds more entities to this same tracker.
        db.ChangeTracker.DetectChanges();
        var events = new List<TelemetryEvent>();
        foreach (var entry in db.ChangeTracker.Entries().ToArray())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted)) continue;
            if (entry.Entity is AuditRecordRow && entry.State is EntityState.Modified or EntityState.Deleted)
                throw new InvalidOperationException("Audit records are append-only; use a separately authorized retention process.");
            // Explicit allowlist: an entity type maps to an event category; unknown types
            // are skipped rather than having their fields automatically dumped into logs.
            var kind = entry.Entity switch
            {
                NodeRow or NodeParentRow => EventKind.NodeChanged, NodeTypeRow => EventKind.NodeTypeChanged,
                DocumentRow or BlockRow => EventKind.ContentChanged, VoteRow => EventKind.VoteChanged,
                NodeReactionRow or ReactionDefinitionRow => EventKind.ReactionChanged,
                CommunityRow => EventKind.CommunityChanged, CommunityMembershipRow => EventKind.MembershipChanged,
                CommunityNodeRow => EventKind.CommunityLinkChanged, CommentRow => EventKind.CommentChanged,
                ModerationCaseRow => EventKind.ModerationChanged, ParticipantRow => EventKind.ProfileChanged,
                NotificationRow or NotificationPreferencesRow => EventKind.NotificationChanged,
                IdentityUserRole<Guid> => EventKind.RoleChanged, IdentityRole<Guid> => EventKind.RoleDefinitionChanged,
                AtlasIdentityUser => EventKind.AccountSecurityChanged,
                _ => (EventKind?)null
            };
            if (kind is null) continue;
            // Only identifiers are selected; absent composite IDs are intentionally not guessed.
            var id = entry.Metadata.FindPrimaryKey()?.Properties
                .Where(property => property.ClrType == typeof(Guid))
                .Select(property => entry.Property(property.Name).CurrentValue).OfType<Guid>().FirstOrDefault();
            // Start with the EF lifecycle action, then refine selected transitions below
            // (for example hidden/restored, role assignment or password changed).
            var action = entry.State == EntityState.Added ? MutationAction.Created
                : entry.State == EntityState.Deleted ? MutationAction.Deleted : MutationAction.Updated;
            Guid? related = null;
            AuditModerationReason? publicReason = null;
            if (entry.Entity is ModerationCaseRow moderation)
            {
                related = moderation.NodeId;
                publicReason = moderation.PublicReason switch
                {
                    1 => AuditModerationReason.Spam, 2 => AuditModerationReason.Harassment,
                    3 => AuditModerationReason.UnsafeContent, 4 => AuditModerationReason.OffTopic,
                    _ => AuditModerationReason.Other
                };
                if (entry.State == EntityState.Modified)
                    action = Changed(nameof(ModerationCaseRow.VisibilityRestoredAt)) && moderation.VisibilityRestoredAt is not null ? MutationAction.Restored
                        : Changed(nameof(ModerationCaseRow.ReviewRequestedAt)) ? MutationAction.ReviewRequested
                        : moderation.Status == 2 ? MutationAction.Hidden : moderation.Status == 1 ? MutationAction.Dismissed : MutationAction.Updated;
            }
            if (entry.Entity is NodeParentRow parent)
            {
                id = parent.NodeId;
                related = parent.ParentNodeId;
                action = entry.State == EntityState.Deleted ? MutationAction.Unlinked : MutationAction.Linked;
            }
            if (entry.Entity is IdentityUserRole<Guid> membership)
            {
                id = membership.UserId;
                related = membership.RoleId;
                action = entry.State == EntityState.Deleted ? MutationAction.RoleRemoved : MutationAction.RoleAssigned;
            }
            if (entry.Entity is AtlasIdentityUser user && entry.State == EntityState.Modified)
                // Report the transition into an active lockout explicitly. Counter updates,
                // clearing a lockout, or extending one already active retain LockoutChanged.
                // Give this transition precedence if Identity updates several fields together.
                action = Changed(nameof(AtlasIdentityUser.LockoutEnd))
                    && user.LockoutEnd > AtlasTime.UtcNow
                    && (entry.Property(nameof(AtlasIdentityUser.LockoutEnd)).OriginalValue is not DateTimeOffset previousEnd
                        || previousEnd <= AtlasTime.UtcNow) ? MutationAction.LockoutStarted
                    : Changed(nameof(AtlasIdentityUser.PasswordHash)) ? MutationAction.PasswordChanged
                    : Changed(nameof(AtlasIdentityUser.EmailConfirmed)) && user.EmailConfirmed ? MutationAction.EmailConfirmed
                    : Changed(nameof(AtlasIdentityUser.LockoutEnd)) || Changed(nameof(AtlasIdentityUser.AccessFailedCount)) ? MutationAction.LockoutChanged
                    : MutationAction.Updated;
            if (entry.Entity is NodeRow node && entry.State == EntityState.Modified)
                action = Changed(nameof(NodeRow.Status)) ? node.Status == "Archived" ? MutationAction.Archived : MutationAction.Restored
                    : Changed(nameof(NodeRow.Title)) ? MutationAction.Renamed
                    : Changed(nameof(NodeRow.TypeId)) ? MutationAction.TypeChanged : MutationAction.Updated;
            if (entry.Entity is ParticipantRow participant && entry.State == EntityState.Modified && Changed(nameof(ParticipantRow.IsActive)))
                action = participant.IsActive ? MutationAction.Activated : MutationAction.Deactivated;
            // Capture the current operation/actor context and approved transition metadata.
            // Inspecting sensitive properties above selects an action, never their logged values.
            var value = Telemetry.Create(kind.Value, id == Guid.Empty ? null : id) with
            {
                Action = action, RelatedTargetId = related, ModerationReason = publicReason
            };
            bool Changed(string property) => !Equals(entry.Property(property).OriginalValue, entry.Property(property).CurrentValue);
            events.Add(value);
            // Every mapped transition produces a staged diagnostic. Only these selected
            // transitions additionally require durable audit evidence and delivery intent.
            var required = kind is EventKind.ModerationChanged or EventKind.RoleChanged or EventKind.RoleDefinitionChanged or EventKind.AccountSecurityChanged
                || (entry.Entity is ParticipantRow && entry.State == EntityState.Modified && entry.Property(nameof(ParticipantRow.IsActive)).IsModified);
            if (!required) continue;
            var record = new AuditRecordRow
            {
                Id = value.EventId, OccurredAtUtc = value.OccurredAtUtc,
                ObservabilityOperationId = value.ObservabilityOperationId, ActorId = value.ActorId,
                TargetId = value.TargetId, EventName = EventJson.Name(value.Kind), PayloadJson = EventJson.Serialize(value)
            };
            // These are tracked additions, not immediate SQL or file writes. The caller's
            // EF save persists them atomically with the business rows in its transaction.
            db.AuditRecords.Add(record);
            db.AuditOutbox.Add(new AuditOutboxRow { Id = Guid.NewGuid(), AuditRecordId = record.Id });
            // Cloud delivery is required in deployment, but no cloud client is silently selected here.
            // Local delivery and cloud acceptance have separate acknowledgements.
            if (db.RequireCloudAuditDelivery)
                db.AuditOutbox.Add(new AuditOutboxRow { Id = Guid.NewGuid(), AuditRecordId = record.Id, Destination = "cloud" });
        }
        return events;
    }
}
