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
                AppliedByParticipantId = Guid.Parse("22222222-2222-4222-8222-222222222222"),
                LifecycleState = "Active",
                Disposition = "Community",
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:20.490Z", System.Globalization.CultureInfo.InvariantCulture),
                RemovedAt = null,
                AuditHistory = [ new ReactionAuditRow
                {
                    Action = "Applied",
                    ActorParticipantId = Guid.Parse("22222222-2222-4222-8222-222222222222"),
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
                AppliedByParticipantId = Guid.Parse("33333333-3333-4333-8333-333333333333"),
                LifecycleState = "Active",
                Disposition = "Community",
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:20.490Z", System.Globalization.CultureInfo.InvariantCulture),
                RemovedAt = null,
                AuditHistory = [ new ReactionAuditRow
                {
                    Action = "Applied",
                    ActorParticipantId = Guid.Parse("33333333-3333-4333-8333-333333333333"),
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
                AppliedByParticipantId = Guid.Parse("11111111-1111-4111-8111-111111111111"),
                LifecycleState = "Active",
                Disposition = "Community",
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:20.490Z", System.Globalization.CultureInfo.InvariantCulture),
                RemovedAt = null,
                AuditHistory = [ new ReactionAuditRow
                {
                    Action = "Applied",
                    ActorParticipantId = Guid.Parse("11111111-1111-4111-8111-111111111111"),
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
                AppliedByParticipantId = Guid.Parse("44444444-4444-4444-8444-444444444444"),
                LifecycleState = "Active",
                Disposition = "Community",
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:20.490Z", System.Globalization.CultureInfo.InvariantCulture),
                RemovedAt = null,
                AuditHistory = [ new ReactionAuditRow
                {
                    Action = "Applied",
                    ActorParticipantId = Guid.Parse("44444444-4444-4444-8444-444444444444"),
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
                AppliedByParticipantId = Guid.Parse("22222222-2222-4222-8222-222222222222"),
                LifecycleState = "Active",
                Disposition = "Community",
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:20.490Z", System.Globalization.CultureInfo.InvariantCulture),
                RemovedAt = null,
                AuditHistory = [ new ReactionAuditRow
                {
                    Action = "Applied",
                    ActorParticipantId = Guid.Parse("22222222-2222-4222-8222-222222222222"),
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
                AppliedByParticipantId = Guid.Parse("11111111-1111-4111-8111-111111111111"),
                LifecycleState = "Active",
                Disposition = "Community",
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:20.490Z", System.Globalization.CultureInfo.InvariantCulture),
                RemovedAt = null,
                AuditHistory = [ new ReactionAuditRow
                {
                    Action = "Applied",
                    ActorParticipantId = Guid.Parse("11111111-1111-4111-8111-111111111111"),
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
                AppliedByParticipantId = Guid.Parse("33333333-3333-4333-8333-333333333333"),
                LifecycleState = "Active",
                Disposition = "Community",
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:20.490Z", System.Globalization.CultureInfo.InvariantCulture),
                RemovedAt = null,
                AuditHistory = [ new ReactionAuditRow
                {
                    Action = "Applied",
                    ActorParticipantId = Guid.Parse("33333333-3333-4333-8333-333333333333"),
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
                AppliedByParticipantId = Guid.Parse("22222222-2222-4222-8222-222222222222"),
                LifecycleState = "Active",
                Disposition = "Community",
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:20.490Z", System.Globalization.CultureInfo.InvariantCulture),
                RemovedAt = null,
                AuditHistory = [ new ReactionAuditRow
                {
                    Action = "Applied",
                    ActorParticipantId = Guid.Parse("22222222-2222-4222-8222-222222222222"),
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
                AppliedByParticipantId = Guid.Parse("33333333-3333-4333-8333-333333333333"),
                LifecycleState = "Active",
                Disposition = "Community",
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:20.490Z", System.Globalization.CultureInfo.InvariantCulture),
                RemovedAt = null,
                AuditHistory = [ new ReactionAuditRow
                {
                    Action = "Applied",
                    ActorParticipantId = Guid.Parse("33333333-3333-4333-8333-333333333333"),
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
                AppliedByParticipantId = Guid.Parse("11111111-1111-4111-8111-111111111111"),
                LifecycleState = "Active",
                Disposition = "Community",
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:20.490Z", System.Globalization.CultureInfo.InvariantCulture),
                RemovedAt = null,
                AuditHistory = [ new ReactionAuditRow
                {
                    Action = "Applied",
                    ActorParticipantId = Guid.Parse("11111111-1111-4111-8111-111111111111"),
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
                AppliedByParticipantId = Guid.Parse("22222222-2222-4222-8222-222222222222"),
                LifecycleState = "Active",
                Disposition = "Community",
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:20.490Z", System.Globalization.CultureInfo.InvariantCulture),
                RemovedAt = null,
                AuditHistory = [ new ReactionAuditRow
                {
                    Action = "Applied",
                    ActorParticipantId = Guid.Parse("22222222-2222-4222-8222-222222222222"),
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
                AppliedByParticipantId = Guid.Parse("33333333-3333-4333-8333-333333333333"),
                LifecycleState = "Active",
                Disposition = "Community",
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:20.490Z", System.Globalization.CultureInfo.InvariantCulture),
                RemovedAt = null,
                AuditHistory = [ new ReactionAuditRow
                {
                    Action = "Applied",
                    ActorParticipantId = Guid.Parse("33333333-3333-4333-8333-333333333333"),
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
                AppliedByParticipantId = Guid.Parse("22222222-2222-4222-8222-222222222222"),
                LifecycleState = "Active",
                Disposition = "Community",
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:20.490Z", System.Globalization.CultureInfo.InvariantCulture),
                RemovedAt = null,
                AuditHistory = [ new ReactionAuditRow
                {
                    Action = "Applied",
                    ActorParticipantId = Guid.Parse("22222222-2222-4222-8222-222222222222"),
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
