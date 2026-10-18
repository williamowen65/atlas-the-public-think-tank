using Atlas.Persistence;

namespace Atlas.Application.Storage;

internal static partial class DemoData
{
    private static void SeedComment(AtlasDataContext database)
    {
        database.CommentRows.AddRange(new CommentRow[]
        {
        });
    }
}
