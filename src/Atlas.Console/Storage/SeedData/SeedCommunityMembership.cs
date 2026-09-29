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
                ParticipantId = Guid.Parse("11111111-1111-4111-8111-111111111111"),
                JoinedAt = DateTimeOffset.Parse("2026-09-28T17:53:35.160Z", System.Globalization.CultureInfo.InvariantCulture),
                LeftAt = null,
            },
            new CommunityMembershipRow
            {
                CommunityId = Guid.Parse("c5000000-0000-4000-8000-000000000002"),
                ParticipantId = Guid.Parse("22222222-2222-4222-8222-222222222222"),
                JoinedAt = DateTimeOffset.Parse("2026-09-28T17:53:35.160Z", System.Globalization.CultureInfo.InvariantCulture),
                LeftAt = null,
            },
            new CommunityMembershipRow
            {
                CommunityId = Guid.Parse("c5000000-0000-4000-8000-000000000003"),
                ParticipantId = Guid.Parse("33333333-3333-4333-8333-333333333333"),
                JoinedAt = DateTimeOffset.Parse("2026-09-28T17:53:35.160Z", System.Globalization.CultureInfo.InvariantCulture),
                LeftAt = null,
            },
            new CommunityMembershipRow
            {
                CommunityId = Guid.Parse("c5000000-0000-4000-8000-000000000004"),
                ParticipantId = Guid.Parse("44444444-4444-4444-8444-444444444444"),
                JoinedAt = DateTimeOffset.Parse("2026-09-28T17:53:35.160Z", System.Globalization.CultureInfo.InvariantCulture),
                LeftAt = null,
            },
            new CommunityMembershipRow
            {
                CommunityId = Guid.Parse("c5000000-0000-4000-8000-000000000005"),
                ParticipantId = Guid.Parse("55555555-5555-4555-8555-555555555555"),
                JoinedAt = DateTimeOffset.Parse("2026-09-28T17:53:35.160Z", System.Globalization.CultureInfo.InvariantCulture),
                LeftAt = null,
            },
        });
    }
}
