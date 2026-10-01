using Atlas.Persistence;

namespace Atlas.ConsoleApp.Storage;

internal static partial class DemoData
{
    private static void SeedNodeReaction(AtlasDataContext database)
    {
        var rows = new NodeReactionRow[]
        {
            new NodeReactionRow
            {
                Id = Guid.Parse("d1000000-0000-4000-8000-000000000001"),
                NodeId = Guid.Parse("b1000000-0000-4000-8000-000000000007"),
                ReactionDefinitionId = Guid.Parse("40000000-0000-4000-8000-000000000001"),
                AppliedByParticipantId = DemoParticipants.User02Id,
                LifecycleState = "Active",
                Disposition = "Community",
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:20.490Z", System.Globalization.CultureInfo.InvariantCulture),
                RemovedAt = null,
                AuditHistory = [ new ReactionAuditRow
                {
                    Action = "Applied",
                    ActorParticipantId = DemoParticipants.User02Id,
                    OccurredAt = DateTimeOffset.Parse("2026-09-28T17:58:20.490Z", System.Globalization.CultureInfo.InvariantCulture),
                    LifecycleState = "Active",
                    Disposition = "Community",
                    RelatedNodeReactionId = null,
                } ],
            },
            new NodeReactionRow
            {
                Id = Guid.Parse("d1000000-0000-4000-8000-000000000002"),
                NodeId = Guid.Parse("b1000000-0000-4000-8000-000000000007"),
                ReactionDefinitionId = Guid.Parse("40000000-0000-4000-8000-000000000012"),
                AppliedByParticipantId = DemoParticipants.User03Id,
                LifecycleState = "Active",
                Disposition = "Community",
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:20.490Z", System.Globalization.CultureInfo.InvariantCulture),
                RemovedAt = null,
                AuditHistory = [ new ReactionAuditRow
                {
                    Action = "Applied",
                    ActorParticipantId = DemoParticipants.User03Id,
                    OccurredAt = DateTimeOffset.Parse("2026-09-28T17:58:20.490Z", System.Globalization.CultureInfo.InvariantCulture),
                    LifecycleState = "Active",
                    Disposition = "Community",
                    RelatedNodeReactionId = null,
                } ],
            },
            new NodeReactionRow
            {
                Id = Guid.Parse("d1000000-0000-4000-8000-000000000003"),
                NodeId = Guid.Parse("b1000000-0000-4000-8000-000000000009"),
                ReactionDefinitionId = Guid.Parse("40000000-0000-4000-8000-000000000003"),
                AppliedByParticipantId = DemoParticipants.User01Id,
                LifecycleState = "Active",
                Disposition = "Community",
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:20.490Z", System.Globalization.CultureInfo.InvariantCulture),
                RemovedAt = null,
                AuditHistory = [ new ReactionAuditRow
                {
                    Action = "Applied",
                    ActorParticipantId = DemoParticipants.User01Id,
                    OccurredAt = DateTimeOffset.Parse("2026-09-28T17:58:20.490Z", System.Globalization.CultureInfo.InvariantCulture),
                    LifecycleState = "Active",
                    Disposition = "Community",
                    RelatedNodeReactionId = null,
                } ],
            },
            new NodeReactionRow
            {
                Id = Guid.Parse("d1000000-0000-4000-8000-000000000004"),
                NodeId = Guid.Parse("b1000000-0000-4000-8000-000000000010"),
                ReactionDefinitionId = Guid.Parse("40000000-0000-4000-8000-000000000012"),
                AppliedByParticipantId = DemoParticipants.User04Id,
                LifecycleState = "Active",
                Disposition = "Community",
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:20.490Z", System.Globalization.CultureInfo.InvariantCulture),
                RemovedAt = null,
                AuditHistory = [ new ReactionAuditRow
                {
                    Action = "Applied",
                    ActorParticipantId = DemoParticipants.User04Id,
                    OccurredAt = DateTimeOffset.Parse("2026-09-28T17:58:20.490Z", System.Globalization.CultureInfo.InvariantCulture),
                    LifecycleState = "Active",
                    Disposition = "Community",
                    RelatedNodeReactionId = null,
                } ],
            },
            new NodeReactionRow
            {
                Id = Guid.Parse("d1000000-0000-4000-8000-000000000005"),
                NodeId = Guid.Parse("b1000000-0000-4000-8000-000000000013"),
                ReactionDefinitionId = Guid.Parse("40000000-0000-4000-8000-000000000008"),
                AppliedByParticipantId = DemoParticipants.User02Id,
                LifecycleState = "Active",
                Disposition = "Community",
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:20.490Z", System.Globalization.CultureInfo.InvariantCulture),
                RemovedAt = null,
                AuditHistory = [ new ReactionAuditRow
                {
                    Action = "Applied",
                    ActorParticipantId = DemoParticipants.User02Id,
                    OccurredAt = DateTimeOffset.Parse("2026-09-28T17:58:20.490Z", System.Globalization.CultureInfo.InvariantCulture),
                    LifecycleState = "Active",
                    Disposition = "Community",
                    RelatedNodeReactionId = null,
                } ],
            },
            new NodeReactionRow
            {
                Id = Guid.Parse("d1000000-0000-4000-8000-000000000006"),
                NodeId = Guid.Parse("b1000000-0000-4000-8000-000000000013"),
                ReactionDefinitionId = Guid.Parse("40000000-0000-4000-8000-000000000010"),
                AppliedByParticipantId = DemoParticipants.User01Id,
                LifecycleState = "Active",
                Disposition = "Community",
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:20.490Z", System.Globalization.CultureInfo.InvariantCulture),
                RemovedAt = null,
                AuditHistory = [ new ReactionAuditRow
                {
                    Action = "Applied",
                    ActorParticipantId = DemoParticipants.User01Id,
                    OccurredAt = DateTimeOffset.Parse("2026-09-28T17:58:20.490Z", System.Globalization.CultureInfo.InvariantCulture),
                    LifecycleState = "Active",
                    Disposition = "Community",
                    RelatedNodeReactionId = null,
                } ],
            },
            new NodeReactionRow
            {
                Id = Guid.Parse("d1000000-0000-4000-8000-000000000007"),
                NodeId = Guid.Parse("b1000000-0000-4000-8000-000000000014"),
                ReactionDefinitionId = Guid.Parse("40000000-0000-4000-8000-000000000012"),
                AppliedByParticipantId = DemoParticipants.User03Id,
                LifecycleState = "Active",
                Disposition = "Community",
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:20.490Z", System.Globalization.CultureInfo.InvariantCulture),
                RemovedAt = null,
                AuditHistory = [ new ReactionAuditRow
                {
                    Action = "Applied",
                    ActorParticipantId = DemoParticipants.User03Id,
                    OccurredAt = DateTimeOffset.Parse("2026-09-28T17:58:20.490Z", System.Globalization.CultureInfo.InvariantCulture),
                    LifecycleState = "Active",
                    Disposition = "Community",
                    RelatedNodeReactionId = null,
                } ],
            },
            new NodeReactionRow
            {
                Id = Guid.Parse("d1000000-0000-4000-8000-000000000008"),
                NodeId = Guid.Parse("b1000000-0000-4000-8000-000000000015"),
                ReactionDefinitionId = Guid.Parse("40000000-0000-4000-8000-000000000001"),
                AppliedByParticipantId = DemoParticipants.User02Id,
                LifecycleState = "Active",
                Disposition = "Community",
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:20.490Z", System.Globalization.CultureInfo.InvariantCulture),
                RemovedAt = null,
                AuditHistory = [ new ReactionAuditRow
                {
                    Action = "Applied",
                    ActorParticipantId = DemoParticipants.User02Id,
                    OccurredAt = DateTimeOffset.Parse("2026-09-28T17:58:20.490Z", System.Globalization.CultureInfo.InvariantCulture),
                    LifecycleState = "Active",
                    Disposition = "Community",
                    RelatedNodeReactionId = null,
                } ],
            },
            new NodeReactionRow
            {
                Id = Guid.Parse("d1000000-0000-4000-8000-000000000009"),
                NodeId = Guid.Parse("b1000000-0000-4000-8000-000000000016"),
                ReactionDefinitionId = Guid.Parse("40000000-0000-4000-8000-000000000004"),
                AppliedByParticipantId = DemoParticipants.User03Id,
                LifecycleState = "Active",
                Disposition = "Community",
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:20.490Z", System.Globalization.CultureInfo.InvariantCulture),
                RemovedAt = null,
                AuditHistory = [ new ReactionAuditRow
                {
                    Action = "Applied",
                    ActorParticipantId = DemoParticipants.User03Id,
                    OccurredAt = DateTimeOffset.Parse("2026-09-28T17:58:20.490Z", System.Globalization.CultureInfo.InvariantCulture),
                    LifecycleState = "Active",
                    Disposition = "Community",
                    RelatedNodeReactionId = null,
                } ],
            },
            new NodeReactionRow
            {
                Id = Guid.Parse("d1000000-0000-4000-8000-000000000010"),
                NodeId = Guid.Parse("b1000000-0000-4000-8000-000000000018"),
                ReactionDefinitionId = Guid.Parse("40000000-0000-4000-8000-000000000001"),
                AppliedByParticipantId = DemoParticipants.User01Id,
                LifecycleState = "Active",
                Disposition = "Community",
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:20.490Z", System.Globalization.CultureInfo.InvariantCulture),
                RemovedAt = null,
                AuditHistory = [ new ReactionAuditRow
                {
                    Action = "Applied",
                    ActorParticipantId = DemoParticipants.User01Id,
                    OccurredAt = DateTimeOffset.Parse("2026-09-28T17:58:20.490Z", System.Globalization.CultureInfo.InvariantCulture),
                    LifecycleState = "Active",
                    Disposition = "Community",
                    RelatedNodeReactionId = null,
                } ],
            },
            new NodeReactionRow
            {
                Id = Guid.Parse("d1000000-0000-4000-8000-000000000011"),
                NodeId = Guid.Parse("b1000000-0000-4000-8000-000000000018"),
                ReactionDefinitionId = Guid.Parse("40000000-0000-4000-8000-000000000007"),
                AppliedByParticipantId = DemoParticipants.User02Id,
                LifecycleState = "Active",
                Disposition = "Community",
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:20.490Z", System.Globalization.CultureInfo.InvariantCulture),
                RemovedAt = null,
                AuditHistory = [ new ReactionAuditRow
                {
                    Action = "Applied",
                    ActorParticipantId = DemoParticipants.User02Id,
                    OccurredAt = DateTimeOffset.Parse("2026-09-28T17:58:20.490Z", System.Globalization.CultureInfo.InvariantCulture),
                    LifecycleState = "Active",
                    Disposition = "Community",
                    RelatedNodeReactionId = null,
                } ],
            },
            new NodeReactionRow
            {
                Id = Guid.Parse("d1000000-0000-4000-8000-000000000012"),
                NodeId = Guid.Parse("b1000000-0000-4000-8000-000000000019"),
                ReactionDefinitionId = Guid.Parse("40000000-0000-4000-8000-000000000011"),
                AppliedByParticipantId = DemoParticipants.User03Id,
                LifecycleState = "Active",
                Disposition = "Community",
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:20.490Z", System.Globalization.CultureInfo.InvariantCulture),
                RemovedAt = null,
                AuditHistory = [ new ReactionAuditRow
                {
                    Action = "Applied",
                    ActorParticipantId = DemoParticipants.User03Id,
                    OccurredAt = DateTimeOffset.Parse("2026-09-28T17:58:20.490Z", System.Globalization.CultureInfo.InvariantCulture),
                    LifecycleState = "Active",
                    Disposition = "Community",
                    RelatedNodeReactionId = null,
                } ],
            },
            new NodeReactionRow
            {
                Id = Guid.Parse("d1000000-0000-4000-8000-000000000013"),
                NodeId = Guid.Parse("b1000000-0000-4000-8000-000000000022"),
                ReactionDefinitionId = Guid.Parse("40000000-0000-4000-8000-000000000005"),
                AppliedByParticipantId = DemoParticipants.User02Id,
                LifecycleState = "Active",
                Disposition = "Community",
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:20.490Z", System.Globalization.CultureInfo.InvariantCulture),
                RemovedAt = null,
                AuditHistory = [ new ReactionAuditRow
                {
                    Action = "Applied",
                    ActorParticipantId = DemoParticipants.User02Id,
                    OccurredAt = DateTimeOffset.Parse("2026-09-28T17:58:20.490Z", System.Globalization.CultureInfo.InvariantCulture),
                    LifecycleState = "Active",
                    Disposition = "Community",
                    RelatedNodeReactionId = null,
                } ],
            },
        };
        foreach (var row in rows) AtlasDataContext.PrepareRow(row);
        database.NodeReactionRows.AddRange(rows);
    }
}
