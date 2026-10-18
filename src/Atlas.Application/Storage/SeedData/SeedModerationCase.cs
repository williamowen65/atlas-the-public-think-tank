using Atlas.Persistence;

namespace Atlas.Application.Storage;

internal static partial class DemoData
{
    private static void SeedModerationCase(AtlasDataContext database)
    {
        database.ModerationCaseRows.AddRange(new ModerationCaseRow[]
        {
        });
    }
}
