using Atlas.Persistence;

namespace Atlas.Application.Storage;

internal static partial class DemoData
{
    private static void SeedModerationCase(AtlasDataContext database)
    {
        // Disposable demo-only cases. Use distinct nodes so pending reports, a
        // hidden node awaiting an author edit, and a requested review can be tested.
        // Status: 0 = Submitted, 2 = Actioned; public reason: 1 = Spam, 4 = OffTopic.
        var reportedAt = DateTimeOffset.Parse("2026-10-20T12:00:00Z");
        database.ModerationCaseRows.AddRange(new ModerationCaseRow[]
        {
            new()
            {
                Id = Guid.Parse("c4000000-0000-4000-8000-000000000001"),
                NodeId = Guid.Parse("b1000000-0000-4000-8000-000000000002"),
                ReporterId = DemoParticipants.User01Id,
                Reason = "Demo: review whether this node is off topic",
                Explanation = "Pending report for testing the moderator queue and dismiss/hide actions.",
                ReportedTitle = "Homelessness",
                CreatedAt = reportedAt,
                Status = 0
            },
            new()
            {
                Id = Guid.Parse("c4000000-0000-4000-8000-000000000002"),
                NodeId = Guid.Parse("b1000000-0000-4000-8000-000000000003"),
                ReporterId = DemoParticipants.User02Id,
                Reason = "Demo: review reported content",
                ReportedTitle = "Make essential services accessible to everyone",
                CreatedAt = reportedAt,
                Status = 2,
                ReviewerId = DemoParticipants.User05Id,
                DecisionReason = "Demo moderation decision for author revision testing.",
                DecidedAt = reportedAt.AddHours(1),
                PublicReason = 4
            },
            new()
            {
                Id = Guid.Parse("c4000000-0000-4000-8000-000000000003"),
                NodeId = Guid.Parse("b1000000-0000-4000-8000-000000000004"),
                ReporterId = DemoParticipants.User03Id,
                Reason = "Demo: review reported content",
                ReportedTitle = "Learn from solutions that already work",
                CreatedAt = reportedAt,
                Status = 2,
                ReviewerId = DemoParticipants.User05Id,
                DecisionReason = "Demo moderation decision for restoration testing.",
                DecidedAt = reportedAt.AddHours(1),
                PublicReason = 1,
                ReviewRequestedAt = reportedAt.AddHours(2)
            },
            new()
            {
                // Extra hidden contribution by Demo User 04 for profile layout testing.
                Id = Guid.Parse("c4000000-0000-4000-8000-000000000005"),
                NodeId = Guid.Parse("b1000000-0000-4000-8000-000000000013"),
                ReporterId = DemoParticipants.User02Id,
                Reason = "Demo: verify claims and supporting evidence",
                ReportedTitle = "Make internet access free and available everywhere",
                CreatedAt = reportedAt,
                Status = 2,
                ReviewerId = DemoParticipants.User05Id,
                DecisionReason = "Demo moderation: claims need more supporting evidence before publication.",
                DecidedAt = reportedAt.AddHours(1),
                PublicReason = 4
            },
            new()
            {
                // Another hidden contribution by Demo User 04, with a different reason.
                Id = Guid.Parse("c4000000-0000-4000-8000-000000000006"),
                NodeId = Guid.Parse("b1000000-0000-4000-8000-000000000016"),
                ReporterId = DemoParticipants.User01Id,
                Reason = "Demo: review discussion quality",
                ReportedTitle = "Public funding could leave some communities with inadequate service",
                CreatedAt = reportedAt,
                Status = 2,
                ReviewerId = DemoParticipants.User05Id,
                DecisionReason = "Demo moderation: revise the framing to encourage constructive discussion.",
                DecidedAt = reportedAt.AddHours(1),
                PublicReason = 1
            },
            new()
            {
                // A hidden grandchild with a real two-step breadcrumb:
                // What ideas could help make the world better? > Use ranked voting...
                NodeId = Guid.Parse("b1000000-0000-4000-8000-000000000008"),
                Id = Guid.Parse("c4000000-0000-4000-8000-000000000004"),
                ReporterId = DemoParticipants.User03Id,
                Reason = "Demo: hidden descendant for breadcrumb verification",
                ReportedTitle = "Would ranked voting encourage more people to run for office?",
                CreatedAt = reportedAt,
                Status = 2,
                ReviewerId = DemoParticipants.User05Id,
                DecisionReason = "Demo-only hidden descendant for inspecting authorized breadcrumbs.",
                DecidedAt = reportedAt.AddHours(1),
                PublicReason = 4
            }
        });
    }
}
