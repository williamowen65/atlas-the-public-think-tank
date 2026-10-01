using Atlas.Persistence;

namespace Atlas.ConsoleApp.Storage;

internal static partial class DemoData
{
    private static void SeedCommunityMembership(AtlasDataContext database)
    {
        database.CommunityMembershipRows.AddRange(new CommunityMembershipRow[]
        {
            new CommunityMembershipRow
            {
                CommunityId = Guid.Parse("c5000000-0000-4000-8000-000000000001"),
                ParticipantId = DemoParticipants.User01Id,
                JoinedAt = DateTimeOffset.Parse("2026-09-28T17:53:35.160Z", System.Globalization.CultureInfo.InvariantCulture),
                LeftAt = null,
            },
            new CommunityMembershipRow
            {
                CommunityId = Guid.Parse("c5000000-0000-4000-8000-000000000002"),
                ParticipantId = DemoParticipants.User02Id,
                JoinedAt = DateTimeOffset.Parse("2026-09-28T17:53:35.160Z", System.Globalization.CultureInfo.InvariantCulture),
                LeftAt = null,
            },
            new CommunityMembershipRow
            {
                CommunityId = Guid.Parse("c5000000-0000-4000-8000-000000000003"),
                ParticipantId = DemoParticipants.User03Id,
                JoinedAt = DateTimeOffset.Parse("2026-09-28T17:53:35.160Z", System.Globalization.CultureInfo.InvariantCulture),
                LeftAt = null,
            },
            new CommunityMembershipRow
            {
                CommunityId = Guid.Parse("c5000000-0000-4000-8000-000000000004"),
                ParticipantId = DemoParticipants.User04Id,
                JoinedAt = DateTimeOffset.Parse("2026-09-28T17:53:35.160Z", System.Globalization.CultureInfo.InvariantCulture),
                LeftAt = null,
            },
            new CommunityMembershipRow
            {
                CommunityId = Guid.Parse("c5000000-0000-4000-8000-000000000005"),
                ParticipantId = DemoParticipants.User05Id,
                JoinedAt = DateTimeOffset.Parse("2026-09-28T17:53:35.160Z", System.Globalization.CultureInfo.InvariantCulture),
                LeftAt = null,
            },
        });
    }
}
