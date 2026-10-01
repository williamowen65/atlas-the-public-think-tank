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
                AssociatedByParticipantId = DemoParticipants.User01Id,
                AssociatedAt = DateTimeOffset.Parse("2026-09-28T17:53:53.392Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new CommunityNodeRow
            {
                CommunityId = Guid.Parse("c5000000-0000-4000-8000-000000000001"),
                NodeId = Guid.Parse("b1000000-0000-4000-8000-000000000010"),
                AssociatedByParticipantId = DemoParticipants.User01Id,
                AssociatedAt = DateTimeOffset.Parse("2026-09-28T17:53:53.392Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new CommunityNodeRow
            {
                CommunityId = Guid.Parse("c5000000-0000-4000-8000-000000000001"),
                NodeId = Guid.Parse("b1000000-0000-4000-8000-000000000011"),
                AssociatedByParticipantId = DemoParticipants.User01Id,
                AssociatedAt = DateTimeOffset.Parse("2026-09-28T17:53:53.392Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new CommunityNodeRow
            {
                CommunityId = Guid.Parse("c5000000-0000-4000-8000-000000000001"),
                NodeId = Guid.Parse("b1000000-0000-4000-8000-000000000012"),
                AssociatedByParticipantId = DemoParticipants.User01Id,
                AssociatedAt = DateTimeOffset.Parse("2026-09-28T17:53:53.392Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new CommunityNodeRow
            {
                CommunityId = Guid.Parse("c5000000-0000-4000-8000-000000000002"),
                NodeId = Guid.Parse("b1000000-0000-4000-8000-000000000013"),
                AssociatedByParticipantId = DemoParticipants.User02Id,
                AssociatedAt = DateTimeOffset.Parse("2026-09-28T17:53:53.392Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new CommunityNodeRow
            {
                CommunityId = Guid.Parse("c5000000-0000-4000-8000-000000000002"),
                NodeId = Guid.Parse("b1000000-0000-4000-8000-000000000014"),
                AssociatedByParticipantId = DemoParticipants.User02Id,
                AssociatedAt = DateTimeOffset.Parse("2026-09-28T17:53:53.392Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new CommunityNodeRow
            {
                CommunityId = Guid.Parse("c5000000-0000-4000-8000-000000000002"),
                NodeId = Guid.Parse("b1000000-0000-4000-8000-000000000015"),
                AssociatedByParticipantId = DemoParticipants.User02Id,
                AssociatedAt = DateTimeOffset.Parse("2026-09-28T17:53:53.392Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new CommunityNodeRow
            {
                CommunityId = Guid.Parse("c5000000-0000-4000-8000-000000000002"),
                NodeId = Guid.Parse("b1000000-0000-4000-8000-000000000016"),
                AssociatedByParticipantId = DemoParticipants.User02Id,
                AssociatedAt = DateTimeOffset.Parse("2026-09-28T17:53:53.392Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new CommunityNodeRow
            {
                CommunityId = Guid.Parse("c5000000-0000-4000-8000-000000000002"),
                NodeId = Guid.Parse("b1000000-0000-4000-8000-000000000017"),
                AssociatedByParticipantId = DemoParticipants.User02Id,
                AssociatedAt = DateTimeOffset.Parse("2026-09-28T17:53:53.392Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new CommunityNodeRow
            {
                CommunityId = Guid.Parse("c5000000-0000-4000-8000-000000000002"),
                NodeId = Guid.Parse("b1000000-0000-4000-8000-000000000018"),
                AssociatedByParticipantId = DemoParticipants.User02Id,
                AssociatedAt = DateTimeOffset.Parse("2026-09-28T17:53:53.392Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new CommunityNodeRow
            {
                CommunityId = Guid.Parse("c5000000-0000-4000-8000-000000000003"),
                NodeId = Guid.Parse("b1000000-0000-4000-8000-000000000007"),
                AssociatedByParticipantId = DemoParticipants.User03Id,
                AssociatedAt = DateTimeOffset.Parse("2026-09-28T17:53:53.392Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new CommunityNodeRow
            {
                CommunityId = Guid.Parse("c5000000-0000-4000-8000-000000000003"),
                NodeId = Guid.Parse("b1000000-0000-4000-8000-000000000008"),
                AssociatedByParticipantId = DemoParticipants.User03Id,
                AssociatedAt = DateTimeOffset.Parse("2026-09-28T17:53:53.392Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new CommunityNodeRow
            {
                CommunityId = Guid.Parse("c5000000-0000-4000-8000-000000000004"),
                NodeId = Guid.Parse("b1000000-0000-4000-8000-000000000002"),
                AssociatedByParticipantId = DemoParticipants.User04Id,
                AssociatedAt = DateTimeOffset.Parse("2026-09-28T17:53:53.392Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new CommunityNodeRow
            {
                CommunityId = Guid.Parse("c5000000-0000-4000-8000-000000000005"),
                NodeId = Guid.Parse("b1000000-0000-4000-8000-000000000019"),
                AssociatedByParticipantId = DemoParticipants.User05Id,
                AssociatedAt = DateTimeOffset.Parse("2026-09-28T17:53:53.392Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new CommunityNodeRow
            {
                CommunityId = Guid.Parse("c5000000-0000-4000-8000-000000000005"),
                NodeId = Guid.Parse("b1000000-0000-4000-8000-000000000020"),
                AssociatedByParticipantId = DemoParticipants.User05Id,
                AssociatedAt = DateTimeOffset.Parse("2026-09-28T17:53:53.392Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new CommunityNodeRow
            {
                CommunityId = Guid.Parse("c5000000-0000-4000-8000-000000000005"),
                NodeId = Guid.Parse("b1000000-0000-4000-8000-000000000021"),
                AssociatedByParticipantId = DemoParticipants.User05Id,
                AssociatedAt = DateTimeOffset.Parse("2026-09-28T17:53:53.392Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new CommunityNodeRow
            {
                CommunityId = Guid.Parse("c5000000-0000-4000-8000-000000000005"),
                NodeId = Guid.Parse("b1000000-0000-4000-8000-000000000022"),
                AssociatedByParticipantId = DemoParticipants.User05Id,
                AssociatedAt = DateTimeOffset.Parse("2026-09-28T17:53:53.392Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new CommunityNodeRow
            {
                CommunityId = Guid.Parse("c5000000-0000-4000-8000-000000000005"),
                NodeId = Guid.Parse("b1000000-0000-4000-8000-000000000023"),
                AssociatedByParticipantId = DemoParticipants.User05Id,
                AssociatedAt = DateTimeOffset.Parse("2026-09-28T18:03:05.125Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new CommunityNodeRow
            {
                CommunityId = Guid.Parse("c5000000-0000-4000-8000-000000000005"),
                NodeId = Guid.Parse("b1000000-0000-4000-8000-000000000024"),
                AssociatedByParticipantId = DemoParticipants.User05Id,
                AssociatedAt = DateTimeOffset.Parse("2026-09-28T18:03:05.125Z", System.Globalization.CultureInfo.InvariantCulture),
            },
        });
    }
}
