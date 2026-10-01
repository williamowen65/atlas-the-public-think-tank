using Atlas.Persistence;

namespace Atlas.ConsoleApp.Storage;

internal static partial class DemoData
{
    private static void SeedVote(AtlasDataContext database)
    {
        database.VoteRows.AddRange(new VoteRow[]
        {
            new VoteRow
            {
                Id = Guid.Parse("d2000000-0000-4000-8000-000000000001"),
                ParticipantId = DemoParticipants.User02Id,
                TargetId = Guid.Parse("d1000000-0000-4000-8000-000000000001"),
                TargetType = "NodeReaction",
                Value = 1,
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new VoteRow
            {
                Id = Guid.Parse("d2000000-0000-4000-8000-000000000002"),
                ParticipantId = DemoParticipants.User03Id,
                TargetId = Guid.Parse("d1000000-0000-4000-8000-000000000001"),
                TargetType = "NodeReaction",
                Value = 1,
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new VoteRow
            {
                Id = Guid.Parse("d2000000-0000-4000-8000-000000000003"),
                ParticipantId = DemoParticipants.User03Id,
                TargetId = Guid.Parse("d1000000-0000-4000-8000-000000000002"),
                TargetType = "NodeReaction",
                Value = 1,
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new VoteRow
            {
                Id = Guid.Parse("d2000000-0000-4000-8000-000000000004"),
                ParticipantId = DemoParticipants.User01Id,
                TargetId = Guid.Parse("d1000000-0000-4000-8000-000000000003"),
                TargetType = "NodeReaction",
                Value = 1,
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new VoteRow
            {
                Id = Guid.Parse("d2000000-0000-4000-8000-000000000005"),
                ParticipantId = DemoParticipants.User05Id,
                TargetId = Guid.Parse("d1000000-0000-4000-8000-000000000003"),
                TargetType = "NodeReaction",
                Value = 1,
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new VoteRow
            {
                Id = Guid.Parse("d2000000-0000-4000-8000-000000000006"),
                ParticipantId = DemoParticipants.User04Id,
                TargetId = Guid.Parse("d1000000-0000-4000-8000-000000000004"),
                TargetType = "NodeReaction",
                Value = 1,
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new VoteRow
            {
                Id = Guid.Parse("d2000000-0000-4000-8000-000000000007"),
                ParticipantId = DemoParticipants.User02Id,
                TargetId = Guid.Parse("d1000000-0000-4000-8000-000000000005"),
                TargetType = "NodeReaction",
                Value = 1,
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new VoteRow
            {
                Id = Guid.Parse("d2000000-0000-4000-8000-000000000008"),
                ParticipantId = DemoParticipants.User04Id,
                TargetId = Guid.Parse("d1000000-0000-4000-8000-000000000005"),
                TargetType = "NodeReaction",
                Value = 1,
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new VoteRow
            {
                Id = Guid.Parse("d2000000-0000-4000-8000-000000000009"),
                ParticipantId = DemoParticipants.User01Id,
                TargetId = Guid.Parse("d1000000-0000-4000-8000-000000000006"),
                TargetType = "NodeReaction",
                Value = 1,
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new VoteRow
            {
                Id = Guid.Parse("d2000000-0000-4000-8000-000000000010"),
                ParticipantId = DemoParticipants.User03Id,
                TargetId = Guid.Parse("d1000000-0000-4000-8000-000000000007"),
                TargetType = "NodeReaction",
                Value = 1,
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new VoteRow
            {
                Id = Guid.Parse("d2000000-0000-4000-8000-000000000011"),
                ParticipantId = DemoParticipants.User02Id,
                TargetId = Guid.Parse("d1000000-0000-4000-8000-000000000008"),
                TargetType = "NodeReaction",
                Value = 1,
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new VoteRow
            {
                Id = Guid.Parse("d2000000-0000-4000-8000-000000000012"),
                ParticipantId = DemoParticipants.User05Id,
                TargetId = Guid.Parse("d1000000-0000-4000-8000-000000000008"),
                TargetType = "NodeReaction",
                Value = 1,
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new VoteRow
            {
                Id = Guid.Parse("d2000000-0000-4000-8000-000000000013"),
                ParticipantId = DemoParticipants.User03Id,
                TargetId = Guid.Parse("d1000000-0000-4000-8000-000000000009"),
                TargetType = "NodeReaction",
                Value = 1,
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new VoteRow
            {
                Id = Guid.Parse("d2000000-0000-4000-8000-000000000014"),
                ParticipantId = DemoParticipants.User01Id,
                TargetId = Guid.Parse("d1000000-0000-4000-8000-000000000009"),
                TargetType = "NodeReaction",
                Value = 1,
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new VoteRow
            {
                Id = Guid.Parse("d2000000-0000-4000-8000-000000000015"),
                ParticipantId = DemoParticipants.User01Id,
                TargetId = Guid.Parse("d1000000-0000-4000-8000-000000000010"),
                TargetType = "NodeReaction",
                Value = 1,
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new VoteRow
            {
                Id = Guid.Parse("d2000000-0000-4000-8000-000000000016"),
                ParticipantId = DemoParticipants.User04Id,
                TargetId = Guid.Parse("d1000000-0000-4000-8000-000000000010"),
                TargetType = "NodeReaction",
                Value = 1,
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new VoteRow
            {
                Id = Guid.Parse("d2000000-0000-4000-8000-000000000017"),
                ParticipantId = DemoParticipants.User02Id,
                TargetId = Guid.Parse("d1000000-0000-4000-8000-000000000011"),
                TargetType = "NodeReaction",
                Value = 1,
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new VoteRow
            {
                Id = Guid.Parse("d2000000-0000-4000-8000-000000000018"),
                ParticipantId = DemoParticipants.User03Id,
                TargetId = Guid.Parse("d1000000-0000-4000-8000-000000000011"),
                TargetType = "NodeReaction",
                Value = 1,
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new VoteRow
            {
                Id = Guid.Parse("d2000000-0000-4000-8000-000000000019"),
                ParticipantId = DemoParticipants.User05Id,
                TargetId = Guid.Parse("d1000000-0000-4000-8000-000000000011"),
                TargetType = "NodeReaction",
                Value = -1,
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new VoteRow
            {
                Id = Guid.Parse("d2000000-0000-4000-8000-000000000020"),
                ParticipantId = DemoParticipants.User03Id,
                TargetId = Guid.Parse("d1000000-0000-4000-8000-000000000012"),
                TargetType = "NodeReaction",
                Value = 1,
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new VoteRow
            {
                Id = Guid.Parse("d2000000-0000-4000-8000-000000000021"),
                ParticipantId = DemoParticipants.User02Id,
                TargetId = Guid.Parse("d1000000-0000-4000-8000-000000000013"),
                TargetType = "NodeReaction",
                Value = 1,
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
            },
        });
    }
}
