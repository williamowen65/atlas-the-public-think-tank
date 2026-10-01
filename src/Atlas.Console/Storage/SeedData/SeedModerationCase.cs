using Atlas.Persistence;

namespace Atlas.ConsoleApp.Storage;

internal static partial class DemoData
{
    private static void SeedModerationCase(AtlasDataContext database)
    {
        database.ModerationCaseRows.AddRange(new ModerationCaseRow[]
        {
        });
    }
}
