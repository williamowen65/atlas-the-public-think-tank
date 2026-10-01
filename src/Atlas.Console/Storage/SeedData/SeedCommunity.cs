using Atlas.Persistence;

namespace Atlas.ConsoleApp.Storage;

internal static partial class DemoData
{
    private static void SeedCommunity(AtlasDataContext database)
    {
        database.CommunityRows.AddRange(new CommunityRow[]
        {
            new CommunityRow
            {
                Id = Guid.Parse("c5000000-0000-4000-8000-000000000001"),
                Name = "Southern Resident Orca Recovery",
                Description = "Questions, evidence, and ideas to support the recovery of Southern Resident orcas.",
                OwnerParticipantId = DemoParticipants.User01Id,
                Status = "Active",
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:53:35.160Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:53:35.160Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new CommunityRow
            {
                Id = Guid.Parse("c5000000-0000-4000-8000-000000000002"),
                Name = "Digital Access",
                Description = "Explore ways to make reliable internet access available and useful to everyone.",
                OwnerParticipantId = DemoParticipants.User02Id,
                Status = "Active",
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:53:35.160Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:53:35.160Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new CommunityRow
            {
                Id = Guid.Parse("c5000000-0000-4000-8000-000000000003"),
                Name = "Democracy and Elections",
                Description = "Ideas and questions about how elections could give people meaningful choices and representation.",
                OwnerParticipantId = DemoParticipants.User03Id,
                Status = "Active",
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:53:35.160Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:53:35.160Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new CommunityRow
            {
                Id = Guid.Parse("c5000000-0000-4000-8000-000000000004"),
                Name = "Housing Stability",
                Description = "Explore homelessness, stable housing, and responses shaped by people affected.",
                OwnerParticipantId = DemoParticipants.User04Id,
                Status = "Active",
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:53:35.160Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:53:35.160Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new CommunityRow
            {
                Id = Guid.Parse("c5000000-0000-4000-8000-000000000005"),
                Name = "Building Atlas",
                Description = "Help shape how Atlas organizes ideas, questions, disagreement, and collaboration.",
                OwnerParticipantId = DemoParticipants.User05Id,
                Status = "Active",
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:53:35.160Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:53:35.160Z", System.Globalization.CultureInfo.InvariantCulture),
            },
        });
    }
}
