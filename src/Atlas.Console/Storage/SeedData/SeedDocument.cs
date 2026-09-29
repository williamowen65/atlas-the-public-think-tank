using Atlas.Persistence;

namespace Atlas.ConsoleApp.Storage;

internal static partial class DemoData
{
    private static void SeedDocument(AtlasDataContext database)
    {
        database.DocumentRows.AddRange(new DocumentRow[]
        {
            new DocumentRow
            {
                Id = Guid.Parse("b2000000-0000-4000-8000-000000000001"),
                BlockIds = [ Guid.Parse("b3000000-0000-4000-8000-000000000001") ],
                CreatedAt = DateTimeOffset.Parse("2026-09-28T15:20:00+00:00", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T15:20:00+00:00", System.Globalization.CultureInfo.InvariantCulture),
            },
            new DocumentRow
            {
                Id = Guid.Parse("b2000000-0000-4000-8000-000000000002"),
                BlockIds = [ Guid.Parse("b3000000-0000-4000-8000-000000000002") ],
                CreatedAt = DateTimeOffset.Parse("2026-09-28T15:20:00+00:00", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T15:20:00+00:00", System.Globalization.CultureInfo.InvariantCulture),
            },
            new DocumentRow
            {
                Id = Guid.Parse("b2000000-0000-4000-8000-000000000003"),
                BlockIds = [ Guid.Parse("b3000000-0000-4000-8000-000000000003") ],
                CreatedAt = DateTimeOffset.Parse("2026-09-28T15:20:00+00:00", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T15:20:00+00:00", System.Globalization.CultureInfo.InvariantCulture),
            },
            new DocumentRow
            {
                Id = Guid.Parse("b2000000-0000-4000-8000-000000000004"),
                BlockIds = [ Guid.Parse("b3000000-0000-4000-8000-000000000004") ],
                CreatedAt = DateTimeOffset.Parse("2026-09-28T15:20:00+00:00", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T15:20:00+00:00", System.Globalization.CultureInfo.InvariantCulture),
            },
            new DocumentRow
            {
                Id = Guid.Parse("b2000000-0000-4000-8000-000000000005"),
                BlockIds = [ Guid.Parse("b3000000-0000-4000-8000-000000000005") ],
                CreatedAt = DateTimeOffset.Parse("2026-09-28T15:20:00+00:00", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T15:20:00+00:00", System.Globalization.CultureInfo.InvariantCulture),
            },
            new DocumentRow
            {
                Id = Guid.Parse("b2000000-0000-4000-8000-000000000006"),
                BlockIds = [ Guid.Parse("b3000000-0000-4000-8000-000000000006") ],
                CreatedAt = DateTimeOffset.Parse("2026-09-28T15:20:00+00:00", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T15:20:00+00:00", System.Globalization.CultureInfo.InvariantCulture),
            },
            new DocumentRow
            {
                Id = Guid.Parse("b2000000-0000-4000-8000-000000000007"),
                BlockIds = [ Guid.Parse("b3000000-0000-4000-8000-000000000007") ],
                CreatedAt = DateTimeOffset.Parse("2026-09-28T16:47:04.525Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T16:47:04.525Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new DocumentRow
            {
                Id = Guid.Parse("b2000000-0000-4000-8000-000000000008"),
                BlockIds = [ Guid.Parse("b3000000-0000-4000-8000-000000000008") ],
                CreatedAt = DateTimeOffset.Parse("2026-09-28T16:47:04.525Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T16:47:04.525Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new DocumentRow
            {
                Id = Guid.Parse("b2000000-0000-4000-8000-000000000009"),
                BlockIds = [ Guid.Parse("b3000000-0000-4000-8000-000000000009") ],
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:12:40.758Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:12:40.758Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new DocumentRow
            {
                Id = Guid.Parse("b2000000-0000-4000-8000-000000000010"),
                BlockIds = [ Guid.Parse("b3000000-0000-4000-8000-000000000010") ],
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:12:40.758Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:12:40.758Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new DocumentRow
            {
                Id = Guid.Parse("b2000000-0000-4000-8000-000000000011"),
                BlockIds = [ Guid.Parse("b3000000-0000-4000-8000-000000000011") ],
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:12:40.758Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:12:40.758Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new DocumentRow
            {
                Id = Guid.Parse("b2000000-0000-4000-8000-000000000012"),
                BlockIds = [ Guid.Parse("b3000000-0000-4000-8000-000000000012") ],
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:12:40.758Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:12:40.758Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new DocumentRow
            {
                Id = Guid.Parse("b2000000-0000-4000-8000-000000000013"),
                BlockIds = [ Guid.Parse("b3000000-0000-4000-8000-000000000013") ],
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:19:22.364Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:19:22.364Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new DocumentRow
            {
                Id = Guid.Parse("b2000000-0000-4000-8000-000000000014"),
                BlockIds = [ Guid.Parse("b3000000-0000-4000-8000-000000000014") ],
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:36:56.094Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:36:56.094Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new DocumentRow
            {
                Id = Guid.Parse("b2000000-0000-4000-8000-000000000015"),
                BlockIds = [ Guid.Parse("b3000000-0000-4000-8000-000000000015") ],
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:36:56.094Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:36:56.094Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new DocumentRow
            {
                Id = Guid.Parse("b2000000-0000-4000-8000-000000000016"),
                BlockIds = [ Guid.Parse("b3000000-0000-4000-8000-000000000016") ],
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:36:56.094Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:36:56.094Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new DocumentRow
            {
                Id = Guid.Parse("b2000000-0000-4000-8000-000000000017"),
                BlockIds = [ Guid.Parse("b3000000-0000-4000-8000-000000000017") ],
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:36:56.094Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:36:56.094Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new DocumentRow
            {
                Id = Guid.Parse("b2000000-0000-4000-8000-000000000018"),
                BlockIds = [ Guid.Parse("b3000000-0000-4000-8000-000000000018") ],
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:48:51.338Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:48:51.338Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new DocumentRow
            {
                Id = Guid.Parse("b2000000-0000-4000-8000-000000000019"),
                BlockIds = [ Guid.Parse("b3000000-0000-4000-8000-000000000019") ],
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:48:51.338Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:48:51.338Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new DocumentRow
            {
                Id = Guid.Parse("b2000000-0000-4000-8000-000000000020"),
                BlockIds = [ Guid.Parse("b3000000-0000-4000-8000-000000000020") ],
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:48:51.338Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:48:51.338Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new DocumentRow
            {
                Id = Guid.Parse("b2000000-0000-4000-8000-000000000021"),
                BlockIds = [ Guid.Parse("b3000000-0000-4000-8000-000000000021") ],
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:48:51.338Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:48:51.338Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new DocumentRow
            {
                Id = Guid.Parse("b2000000-0000-4000-8000-000000000022"),
                BlockIds = [ Guid.Parse("b3000000-0000-4000-8000-000000000022") ],
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:48:51.338Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:48:51.338Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new DocumentRow
            {
                Id = Guid.Parse("b2000000-0000-4000-8000-000000000023"),
                BlockIds = [ Guid.Parse("b3000000-0000-4000-8000-000000000023") ],
                CreatedAt = DateTimeOffset.Parse("2026-09-28T18:02:45.096Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T18:02:45.096Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new DocumentRow
            {
                Id = Guid.Parse("b2000000-0000-4000-8000-000000000024"),
                BlockIds = [ Guid.Parse("b3000000-0000-4000-8000-000000000024") ],
                CreatedAt = DateTimeOffset.Parse("2026-09-28T18:02:45.096Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T18:02:45.096Z", System.Globalization.CultureInfo.InvariantCulture),
            },
        });
    }
}
