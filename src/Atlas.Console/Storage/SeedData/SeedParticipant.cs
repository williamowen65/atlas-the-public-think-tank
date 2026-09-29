using Atlas.Persistence;

namespace Atlas.ConsoleApp.Storage;

internal static partial class DemoData
{
    private static void SeedParticipant(AtlasDataContext database)
    {
        database.ParticipantRows.AddRange(new ParticipantRow[]
        {
            new ParticipantRow
            {
                Id = Guid.Parse("11111111-1111-4111-8111-111111111111"),
                DisplayName = "Demo User 01",
                Bio = "The default Atlas participant used when the console first starts.",
                IsActive = true,
                CreatedAt = DateTimeOffset.Parse("2026-08-01T15:00:00+00:00", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-08-01T15:00:00+00:00", System.Globalization.CultureInfo.InvariantCulture),
            },
            new ParticipantRow
            {
                Id = Guid.Parse("22222222-2222-4222-8222-222222222222"),
                DisplayName = "Demo User 02",
                Bio = "Tacoma neighborhood planner interested in housing, public space, and practical civic experiments.",
                IsActive = true,
                CreatedAt = DateTimeOffset.Parse("2026-08-02T16:00:00+00:00", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-08-02T16:00:00+00:00", System.Globalization.CultureInfo.InvariantCulture),
            },
            new ParticipantRow
            {
                Id = Guid.Parse("33333333-3333-4333-8333-333333333333"),
                DisplayName = "Demo User 03",
                Bio = "Ferry commuter and volunteer emergency-preparedness coordinator from Kitsap County.",
                IsActive = true,
                CreatedAt = DateTimeOffset.Parse("2026-08-03T17:00:00+00:00", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-08-03T17:00:00+00:00", System.Globalization.CultureInfo.InvariantCulture),
            },
            new ParticipantRow
            {
                Id = Guid.Parse("44444444-4444-4444-8444-444444444444"),
                DisplayName = "Demo User 04",
                Bio = "Marine ecologist who works with community groups on shoreline resilience and habitat restoration.",
                IsActive = true,
                CreatedAt = DateTimeOffset.Parse("2026-08-04T18:00:00+00:00", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-08-04T18:00:00+00:00", System.Globalization.CultureInfo.InvariantCulture),
            },
            new ParticipantRow
            {
                Id = Guid.Parse("55555555-5555-4555-8555-555555555555"),
                DisplayName = "Demo User 05",
                Bio = "Middle-school science teacher and daily bus rider who likes projects students can test locally.",
                IsActive = true,
                CreatedAt = DateTimeOffset.Parse("2026-08-05T19:00:00+00:00", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-08-05T19:00:00+00:00", System.Globalization.CultureInfo.InvariantCulture),
            },
        });
    }
}
