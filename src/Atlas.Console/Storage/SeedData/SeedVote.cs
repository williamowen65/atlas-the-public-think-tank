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
                ParticipantId = Guid.Parse("22222222-2222-4222-8222-222222222222"),
                TargetId = Guid.Parse("d1000000-0000-4000-8000-000000000001"),
                TargetType = "NodeReaction",
                Value = 1,
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new VoteRow
            {
                Id = Guid.Parse("d2000000-0000-4000-8000-000000000002"),
                ParticipantId = Guid.Parse("33333333-3333-4333-8333-333333333333"),
                TargetId = Guid.Parse("d1000000-0000-4000-8000-000000000001"),
                TargetType = "NodeReaction",
                Value = 1,
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new VoteRow
            {
                Id = Guid.Parse("d2000000-0000-4000-8000-000000000003"),
                ParticipantId = Guid.Parse("33333333-3333-4333-8333-333333333333"),
                TargetId = Guid.Parse("d1000000-0000-4000-8000-000000000002"),
                TargetType = "NodeReaction",
                Value = 1,
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new VoteRow
            {
                Id = Guid.Parse("d2000000-0000-4000-8000-000000000004"),
                ParticipantId = Guid.Parse("11111111-1111-4111-8111-111111111111"),
                TargetId = Guid.Parse("d1000000-0000-4000-8000-000000000003"),
                TargetType = "NodeReaction",
                Value = 1,
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new VoteRow
            {
                Id = Guid.Parse("d2000000-0000-4000-8000-000000000005"),
                ParticipantId = Guid.Parse("55555555-5555-4555-8555-555555555555"),
                TargetId = Guid.Parse("d1000000-0000-4000-8000-000000000003"),
                TargetType = "NodeReaction",
                Value = 1,
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new VoteRow
            {
                Id = Guid.Parse("d2000000-0000-4000-8000-000000000006"),
                ParticipantId = Guid.Parse("44444444-4444-4444-8444-444444444444"),
                TargetId = Guid.Parse("d1000000-0000-4000-8000-000000000004"),
                TargetType = "NodeReaction",
                Value = 1,
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new VoteRow
            {
                Id = Guid.Parse("d2000000-0000-4000-8000-000000000007"),
                ParticipantId = Guid.Parse("22222222-2222-4222-8222-222222222222"),
                TargetId = Guid.Parse("d1000000-0000-4000-8000-000000000005"),
                TargetType = "NodeReaction",
                Value = 1,
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new VoteRow
            {
                Id = Guid.Parse("d2000000-0000-4000-8000-000000000008"),
                ParticipantId = Guid.Parse("44444444-4444-4444-8444-444444444444"),
                TargetId = Guid.Parse("d1000000-0000-4000-8000-000000000005"),
                TargetType = "NodeReaction",
                Value = 1,
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new VoteRow
            {
                Id = Guid.Parse("d2000000-0000-4000-8000-000000000009"),
                ParticipantId = Guid.Parse("11111111-1111-4111-8111-111111111111"),
                TargetId = Guid.Parse("d1000000-0000-4000-8000-000000000006"),
                TargetType = "NodeReaction",
                Value = 1,
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new VoteRow
            {
                Id = Guid.Parse("d2000000-0000-4000-8000-000000000010"),
                ParticipantId = Guid.Parse("33333333-3333-4333-8333-333333333333"),
                TargetId = Guid.Parse("d1000000-0000-4000-8000-000000000007"),
                TargetType = "NodeReaction",
                Value = 1,
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new VoteRow
            {
                Id = Guid.Parse("d2000000-0000-4000-8000-000000000011"),
                ParticipantId = Guid.Parse("22222222-2222-4222-8222-222222222222"),
                TargetId = Guid.Parse("d1000000-0000-4000-8000-000000000008"),
                TargetType = "NodeReaction",
                Value = 1,
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new VoteRow
            {
                Id = Guid.Parse("d2000000-0000-4000-8000-000000000012"),
                ParticipantId = Guid.Parse("55555555-5555-4555-8555-555555555555"),
                TargetId = Guid.Parse("d1000000-0000-4000-8000-000000000008"),
                TargetType = "NodeReaction",
                Value = 1,
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new VoteRow
            {
                Id = Guid.Parse("d2000000-0000-4000-8000-000000000013"),
                ParticipantId = Guid.Parse("33333333-3333-4333-8333-333333333333"),
                TargetId = Guid.Parse("d1000000-0000-4000-8000-000000000009"),
                TargetType = "NodeReaction",
                Value = 1,
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new VoteRow
            {
                Id = Guid.Parse("d2000000-0000-4000-8000-000000000014"),
                ParticipantId = Guid.Parse("11111111-1111-4111-8111-111111111111"),
                TargetId = Guid.Parse("d1000000-0000-4000-8000-000000000009"),
                TargetType = "NodeReaction",
                Value = 1,
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new VoteRow
            {
                Id = Guid.Parse("d2000000-0000-4000-8000-000000000015"),
                ParticipantId = Guid.Parse("11111111-1111-4111-8111-111111111111"),
                TargetId = Guid.Parse("d1000000-0000-4000-8000-000000000010"),
                TargetType = "NodeReaction",
                Value = 1,
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new VoteRow
            {
                Id = Guid.Parse("d2000000-0000-4000-8000-000000000016"),
                ParticipantId = Guid.Parse("44444444-4444-4444-8444-444444444444"),
                TargetId = Guid.Parse("d1000000-0000-4000-8000-000000000010"),
                TargetType = "NodeReaction",
                Value = 1,
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new VoteRow
            {
                Id = Guid.Parse("d2000000-0000-4000-8000-000000000017"),
                ParticipantId = Guid.Parse("22222222-2222-4222-8222-222222222222"),
                TargetId = Guid.Parse("d1000000-0000-4000-8000-000000000011"),
                TargetType = "NodeReaction",
                Value = 1,
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new VoteRow
            {
                Id = Guid.Parse("d2000000-0000-4000-8000-000000000018"),
                ParticipantId = Guid.Parse("33333333-3333-4333-8333-333333333333"),
                TargetId = Guid.Parse("d1000000-0000-4000-8000-000000000011"),
                TargetType = "NodeReaction",
                Value = 1,
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new VoteRow
            {
                Id = Guid.Parse("d2000000-0000-4000-8000-000000000019"),
                ParticipantId = Guid.Parse("55555555-5555-4555-8555-555555555555"),
                TargetId = Guid.Parse("d1000000-0000-4000-8000-000000000011"),
                TargetType = "NodeReaction",
                Value = -1,
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new VoteRow
            {
                Id = Guid.Parse("d2000000-0000-4000-8000-000000000020"),
                ParticipantId = Guid.Parse("33333333-3333-4333-8333-333333333333"),
                TargetId = Guid.Parse("d1000000-0000-4000-8000-000000000012"),
                TargetType = "NodeReaction",
                Value = 1,
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new VoteRow
            {
                Id = Guid.Parse("d2000000-0000-4000-8000-000000000021"),
                ParticipantId = Guid.Parse("22222222-2222-4222-8222-222222222222"),
                TargetId = Guid.Parse("d1000000-0000-4000-8000-000000000013"),
                TargetType = "NodeReaction",
                Value = 1,
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:58:33.243Z", System.Globalization.CultureInfo.InvariantCulture),
            },
        });
    }
}
