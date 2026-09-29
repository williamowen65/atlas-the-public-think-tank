using Atlas.Persistence;

namespace Atlas.ConsoleApp.Storage;

internal static partial class DemoData
{
    private static void SeedCommunityNode(AtlasDataContext database)
    {
        database.CommunityNodeRows.AddRange(new CommunityNodeRow[]
        {
            new CommunityNodeRow
            {
                CommunityId = Guid.Parse("c5000000-0000-4000-8000-000000000001"),
                NodeId = Guid.Parse("b1000000-0000-4000-8000-000000000009"),
                AssociatedByParticipantId = Guid.Parse("11111111-1111-4111-8111-111111111111"),
                AssociatedAt = DateTimeOffset.Parse("2026-09-28T17:53:53.392Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new CommunityNodeRow
            {
                CommunityId = Guid.Parse("c5000000-0000-4000-8000-000000000001"),
                NodeId = Guid.Parse("b1000000-0000-4000-8000-000000000010"),
                AssociatedByParticipantId = Guid.Parse("11111111-1111-4111-8111-111111111111"),
                AssociatedAt = DateTimeOffset.Parse("2026-09-28T17:53:53.392Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new CommunityNodeRow
            {
                CommunityId = Guid.Parse("c5000000-0000-4000-8000-000000000001"),
                NodeId = Guid.Parse("b1000000-0000-4000-8000-000000000011"),
                AssociatedByParticipantId = Guid.Parse("11111111-1111-4111-8111-111111111111"),
                AssociatedAt = DateTimeOffset.Parse("2026-09-28T17:53:53.392Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new CommunityNodeRow
            {
                CommunityId = Guid.Parse("c5000000-0000-4000-8000-000000000001"),
                NodeId = Guid.Parse("b1000000-0000-4000-8000-000000000012"),
                AssociatedByParticipantId = Guid.Parse("11111111-1111-4111-8111-111111111111"),
                AssociatedAt = DateTimeOffset.Parse("2026-09-28T17:53:53.392Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new CommunityNodeRow
            {
                CommunityId = Guid.Parse("c5000000-0000-4000-8000-000000000002"),
                NodeId = Guid.Parse("b1000000-0000-4000-8000-000000000013"),
                AssociatedByParticipantId = Guid.Parse("22222222-2222-4222-8222-222222222222"),
                AssociatedAt = DateTimeOffset.Parse("2026-09-28T17:53:53.392Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new CommunityNodeRow
            {
                CommunityId = Guid.Parse("c5000000-0000-4000-8000-000000000002"),
                NodeId = Guid.Parse("b1000000-0000-4000-8000-000000000014"),
                AssociatedByParticipantId = Guid.Parse("22222222-2222-4222-8222-222222222222"),
                AssociatedAt = DateTimeOffset.Parse("2026-09-28T17:53:53.392Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new CommunityNodeRow
            {
                CommunityId = Guid.Parse("c5000000-0000-4000-8000-000000000002"),
                NodeId = Guid.Parse("b1000000-0000-4000-8000-000000000015"),
                AssociatedByParticipantId = Guid.Parse("22222222-2222-4222-8222-222222222222"),
                AssociatedAt = DateTimeOffset.Parse("2026-09-28T17:53:53.392Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new CommunityNodeRow
            {
                CommunityId = Guid.Parse("c5000000-0000-4000-8000-000000000002"),
                NodeId = Guid.Parse("b1000000-0000-4000-8000-000000000016"),
                AssociatedByParticipantId = Guid.Parse("22222222-2222-4222-8222-222222222222"),
                AssociatedAt = DateTimeOffset.Parse("2026-09-28T17:53:53.392Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new CommunityNodeRow
            {
                CommunityId = Guid.Parse("c5000000-0000-4000-8000-000000000002"),
                NodeId = Guid.Parse("b1000000-0000-4000-8000-000000000017"),
                AssociatedByParticipantId = Guid.Parse("22222222-2222-4222-8222-222222222222"),
                AssociatedAt = DateTimeOffset.Parse("2026-09-28T17:53:53.392Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new CommunityNodeRow
            {
                CommunityId = Guid.Parse("c5000000-0000-4000-8000-000000000002"),
                NodeId = Guid.Parse("b1000000-0000-4000-8000-000000000018"),
                AssociatedByParticipantId = Guid.Parse("22222222-2222-4222-8222-222222222222"),
                AssociatedAt = DateTimeOffset.Parse("2026-09-28T17:53:53.392Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new CommunityNodeRow
            {
                CommunityId = Guid.Parse("c5000000-0000-4000-8000-000000000003"),
                NodeId = Guid.Parse("b1000000-0000-4000-8000-000000000007"),
                AssociatedByParticipantId = Guid.Parse("33333333-3333-4333-8333-333333333333"),
                AssociatedAt = DateTimeOffset.Parse("2026-09-28T17:53:53.392Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new CommunityNodeRow
            {
                CommunityId = Guid.Parse("c5000000-0000-4000-8000-000000000003"),
                NodeId = Guid.Parse("b1000000-0000-4000-8000-000000000008"),
                AssociatedByParticipantId = Guid.Parse("33333333-3333-4333-8333-333333333333"),
                AssociatedAt = DateTimeOffset.Parse("2026-09-28T17:53:53.392Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new CommunityNodeRow
            {
                CommunityId = Guid.Parse("c5000000-0000-4000-8000-000000000004"),
                NodeId = Guid.Parse("b1000000-0000-4000-8000-000000000002"),
                AssociatedByParticipantId = Guid.Parse("44444444-4444-4444-8444-444444444444"),
                AssociatedAt = DateTimeOffset.Parse("2026-09-28T17:53:53.392Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new CommunityNodeRow
            {
                CommunityId = Guid.Parse("c5000000-0000-4000-8000-000000000005"),
                NodeId = Guid.Parse("b1000000-0000-4000-8000-000000000019"),
                AssociatedByParticipantId = Guid.Parse("55555555-5555-4555-8555-555555555555"),
                AssociatedAt = DateTimeOffset.Parse("2026-09-28T17:53:53.392Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new CommunityNodeRow
            {
                CommunityId = Guid.Parse("c5000000-0000-4000-8000-000000000005"),
                NodeId = Guid.Parse("b1000000-0000-4000-8000-000000000020"),
                AssociatedByParticipantId = Guid.Parse("55555555-5555-4555-8555-555555555555"),
                AssociatedAt = DateTimeOffset.Parse("2026-09-28T17:53:53.392Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new CommunityNodeRow
            {
                CommunityId = Guid.Parse("c5000000-0000-4000-8000-000000000005"),
                NodeId = Guid.Parse("b1000000-0000-4000-8000-000000000021"),
                AssociatedByParticipantId = Guid.Parse("55555555-5555-4555-8555-555555555555"),
                AssociatedAt = DateTimeOffset.Parse("2026-09-28T17:53:53.392Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new CommunityNodeRow
            {
                CommunityId = Guid.Parse("c5000000-0000-4000-8000-000000000005"),
                NodeId = Guid.Parse("b1000000-0000-4000-8000-000000000022"),
                AssociatedByParticipantId = Guid.Parse("55555555-5555-4555-8555-555555555555"),
                AssociatedAt = DateTimeOffset.Parse("2026-09-28T17:53:53.392Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new CommunityNodeRow
            {
                CommunityId = Guid.Parse("c5000000-0000-4000-8000-000000000005"),
                NodeId = Guid.Parse("b1000000-0000-4000-8000-000000000023"),
                AssociatedByParticipantId = Guid.Parse("55555555-5555-4555-8555-555555555555"),
                AssociatedAt = DateTimeOffset.Parse("2026-09-28T18:03:05.125Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new CommunityNodeRow
            {
                CommunityId = Guid.Parse("c5000000-0000-4000-8000-000000000005"),
                NodeId = Guid.Parse("b1000000-0000-4000-8000-000000000024"),
                AssociatedByParticipantId = Guid.Parse("55555555-5555-4555-8555-555555555555"),
                AssociatedAt = DateTimeOffset.Parse("2026-09-28T18:03:05.125Z", System.Globalization.CultureInfo.InvariantCulture),
            },
        });
    }
}
